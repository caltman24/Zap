import { Session, SessionData } from "@remix-run/node";
import { commitSession } from "~/services/sessions.server";
import tryCatch from "~/utils/tryCatch";

// Error classes
export class AuthenticationError extends Error {
  constructor(message = "Authentication required") {
    super(message);
    this.name = "AuthenticationError";
  }
}

export class TokenRefreshError extends Error {
  constructor(message = "Failed to refresh token") {
    super(message);
    this.name = "TokenRefreshError";
  }
}

export class ApiError extends Error {
  public status: number;

  constructor(message: string, status: number) {
    super(message);
    this.name = "ApiError";
    this.status = status;
  }
}

// Response types
export type ValidateAccountResponse = {
  result: "company" | "user" | "none";
};

export type TokenResponse = {
  tokenType: string;
  accessToken: string;
  expiresIn: number;
  refreshToken: string;
};

export type UserInfoResponse = {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  companyId?: string;
};

export type CompanyInfoResponse = {
  name: string;
  description: string;
};

export type RegisterUserRequest = {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
};

export type RegisterCompanyRequest = {
  name: string;
  description: string;
};

export class ApiService {
  private readonly baseUrl: string;
  private readonly refreshBuffer = 1000 * 60 * 2; // 2 minutes

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl;
  }

  // Helper methods
  public expiresInToIso(expiresIn: number): string {
    return new Date(Date.now() + expiresIn * 1000).toISOString();
  }

  // Base HTTP methods
  private async requestJson<T>(
    endpoint: string,
    options?: RequestInit,
    token?: string
  ): Promise<T> {
    const url = `${this.baseUrl}${endpoint}`;
    const headers: any = {
      "Content-Type": "application/json",
      ...options?.headers,
    };

    if (token) {
      headers.Authorization = `Bearer ${token}`;
    }

    const config: RequestInit = {
      ...options,
      headers,
    };

    const { data: response, error } = await tryCatch(fetch(url, config));
    return this.handleResponse<T>(response, error);
  }

  private async handleResponse<T>(
    response: Response | null,
    error: Error | null
  ): Promise<T> {
    if (error) {
      console.error(error);
      throw new ApiError("Server error", 500);
    }

    if (!response) {
      console.error("No response");
      throw new ApiError("No response", 500);
    }

    if (response.status === 401) {
      throw new AuthenticationError("Unauthorized");
    }

    if (!response.ok) {
      console.error(response.url, response.status, response.statusText);
      throw new ApiError(response.statusText, response.status);
    }

    return (await response.json()) as T;
  }

  // Authentication
  public async getValidToken(session: Session<SessionData, SessionData>) {
    const tokens = session.get("tokens");
    if (!tokens) {
      throw new AuthenticationError("Unauthorized");
    }

    const expiresAt = new Date(tokens.expiresIn).getTime() - this.refreshBuffer;

    if (expiresAt <= Date.now()) {
      console.log("refreshing token");
      try {
        const tokenResponse = await this.refreshTokens(tokens.refreshToken);

        session.set("tokens", {
          accessToken: tokenResponse.accessToken,
          expiresIn: this.expiresInToIso(tokenResponse.expiresIn),
          refreshToken: tokenResponse.refreshToken,
        });

        return {
          token: tokenResponse.accessToken,
          headers: {
            "Set-Cookie": await commitSession(session),
          } as ResponseInit,
        };
      } catch (error) {
        console.error(error);
        if (error instanceof AuthenticationError) {
          console.log("Expired refresh token -> logout");
        }
        throw error;
      }
    } else {
      return {
        token: tokens.accessToken,
        headers: undefined as ResponseInit | undefined,
      };
    }
  }

  public async refreshTokens(refreshToken: string): Promise<TokenResponse> {
    return this.requestJson<TokenResponse>("/refresh", {
      method: "POST",
      body: JSON.stringify({ refreshToken }),
    });
  }

  // Auth endpoints
  public async signInUser(email: string, password: string): Promise<Response> {
    return fetch(`${this.baseUrl}/signin`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email, password }),
    });
  }

  public async signInTestUser(): Promise<Response> {
    return fetch(`${this.baseUrl}/signin/testuser`, { method: "POST" });
  }

  // User endpoints
  public async registerAccount(data: RegisterUserRequest): Promise<Response> {
    return fetch(`${this.baseUrl}/register/user`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data),
    });
  }

  public async getUserInfo(accessToken: string): Promise<UserInfoResponse> {
    return this.requestJson<UserInfoResponse>(
      "/user/info",
      { method: "GET" },
      accessToken
    );
  }

  // Company endpoints
  public async registerCompany(
    data: RegisterCompanyRequest,
    accessToken: string
  ): Promise<Response> {
    return fetch(`${this.baseUrl}/register/company`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${accessToken}`,
      },
      body: JSON.stringify(data),
    });
  }

  public async getCompanyInfo(
    accessToken: string
  ): Promise<CompanyInfoResponse> {
    return this.requestJson<CompanyInfoResponse>(
      "/company/info",
      { method: "GET" },
      accessToken
    );
  }
}

// Create and export a singleton instance
const apiService = new ApiService(
  process.env.API_BASE_URL || "http://localhost:5090"
);
export default apiService;
