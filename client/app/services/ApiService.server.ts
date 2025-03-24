import { Session, SessionData } from "@remix-run/node";
import { commitSession } from "~/services/sessions.server";
import tryCatch from "~/utils/tryCatch";

// Custom error classes for better error handling

// Thrown when the user is not authenticated / No Tokens
export class AuthenticationError extends Error {
  constructor(message = "Authentication required") {
    super(message);
    this.name = "AuthenticationError";
  }
}

// Thrown when the token refresh fails -> redirect to login
export class TokenRefreshError extends Error {
  constructor(message = "Failed to refresh token") {
    super(message);
    this.name = "TokenRefreshError";
  }
}

// Thrown when the API returns an error
export class ApiError extends Error {
  public status: number;

  constructor(message: string, status: number) {
    super(message);
    this.name = "ApiError";
    this.status = status;
  }
}

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

export class ApiService {
  public readonly BaseUrl!: string;

  constructor(baseUrl: string) {
    this.BaseUrl = baseUrl;
  }

  // Base wrapper to make requests
  private async fetchApi(url: string, options?: RequestInit) {
    return await fetch(this.BaseUrl + url, options);
  }

  public expiresInToIso(expiresIn: number) {
    return new Date(Date.now() + expiresIn * 1000).toISOString();
  }

  // Gets token from session and refreshes if necessary
  // Returns token and headers if the token was refreshed -> append headers to reponse from loader or action
  // Returns a rejected promise if the token is invalid or expired -> logout
  public async getValidToken(session: Session<SessionData, SessionData>) {
    console.log("getValidToken");
    const tokens = session.get("tokens");
    if (!tokens) {
      return Promise.reject(new AuthenticationError("Unauthorized"));
    }
    const refreshBuffer = 1000 * 60 * 2; // 2 minutes
    const expiresAt = new Date(tokens.expiresIn).getTime() - refreshBuffer;
    console.log("expiresAt", expiresAt);
    console.log("Date.now()", Date.now());

    if (expiresAt <= Date.now()) {
      console.log("refreshing token");
      const { data: res, error } = await tryCatch(
        this.refreshTokens(tokens.refreshToken)
      );

      if (error) {
        console.error(error);
        return Promise.reject(new TokenRefreshError("Failed to refresh token"));
      }

      if (res.status === 401) {
        console.log("Expired refresh token -> logout");
        return Promise.reject(new AuthenticationError("Unauthorized"));
      }
      if (!res.ok) {
        console.error(res);
        return Promise.reject(new ApiError(res.statusText, res.status));
      }

      const tokenResponse: TokenResponse = await res.json();

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
    } else {
      // either proceed with request or return tokens
      return {
        token: tokens.accessToken,
        headers: undefined as ResponseInit | undefined,
      };
    }
  }

  public async refreshTokens(refreshToken: string): Promise<Response> {
    return await this.fetchApi("/refresh", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ refreshToken }),
    });
  }

  public async signInUser(email: string, password: string): Promise<Response> {
    return await this.fetchApi("/signin", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ email, password }),
    });
  }

  public async signInTestUser(): Promise<Response> {
    return await this.fetchApi("/signin/testuser", {
      method: "POST",
    });
  }

  public async registerAccount({
    firstName,
    lastName,
    email,
    password,
  }: {
    firstName: string;
    lastName: string;
    email: string;
    password: string;
  }): Promise<Response> {
    return await this.fetchApi("/register/user", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ firstName, lastName, email, password }),
    });
  }

  public async registerCompany(
    {
      name,
      description,
    }: {
      name: string;
      description: string;
    },
    accessToken: string
  ): Promise<Response> {
    return await this.fetchApi("/register/company", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${accessToken}`,
      },
      body: JSON.stringify({ name, description }),
    });
  }

  // Maybe pass in the session instead of tokens. This way we can get and set the tokens directly here
  public async getUserInfo(accessToken: string): Promise<UserInfoResponse> {
    const { data: res, error } = await tryCatch(
      this.fetchApi("/user/info", {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${accessToken}`,
        },
      })
    );

    if (error) {
      console.error(error);
      return Promise.reject(
        new ApiError("Server error: Failed to get user info", 500)
      );
    }

    // Dont need to handle this if using getValidToken before
    if (res.status === 401) {
      return Promise.reject(new AuthenticationError("Unauthorized"));
    }

    if (!res.ok) {
      console.error(res);
      return Promise.reject(new ApiError(res.statusText, res.status));
    }

    const data: {
      id: string;
      email: string;
      firstName: string;
      lastName: string;
      role: string;
      companyId?: string;
    } = await res.json();

    return data;
  }
}

const apiService = new ApiService("http://localhost:5090");

export default apiService;
