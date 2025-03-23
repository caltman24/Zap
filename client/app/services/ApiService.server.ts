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

  private async fetchWithAuth(
    url: string,
    session: Session<SessionData, SessionData>,
    options?: RequestInit
  ) {
    const tokens = session.get("tokens");
    // if no tokens throw AuthenticationError to redirect to login
    if (!tokens) {
      return Promise.reject(new AuthenticationError("Unauthorized"));
    }
    const res = await this.fetchApi(url, {
      ...options,
      headers: {
        ...options?.headers,
        Authorization: `Bearer ${tokens.accessToken}`,
      },
    });

    if (res.status === 401) {
      const { data: refreshRes, error } = await tryCatch(
        this.RefreshTokens(tokens.refreshToken)
      );

      if (error) {
        return Promise.reject(new TokenRefreshError("Failed to refresh token"));
      }

      const { accessToken, refreshToken } =
        (await refreshRes.json()) as TokenResponse;

      session.set("tokens", {
        accessToken,
        refreshToken,
      });

      return await this.fetchApi(url, {
        ...options,
        headers: {
          ...options?.headers,
          Authorization: `Bearer ${accessToken}`,
        },
      });
    }

    return res;
  }

  public async RefreshTokens(refreshToken: string): Promise<Response> {
    return await this.fetchApi("/refresh", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ refreshToken }),
    });
  }

  public async SignInUser(email: string, password: string): Promise<Response> {
    return await this.fetchApi("/signin", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ email, password }),
    });
  }

  public async SignInTestUser(): Promise<Response> {
    return await this.fetchApi("/signin/testuser", {
      method: "POST",
    });
  }

  public async RegisterAccount({
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

  public async RegisterCompany(
    {
      name,
      description,
    }: {
      name: string;
      description: string;
    },
    session: Session<SessionData, SessionData>
  ): Promise<Response> {
    const tokens = session.get("tokens");
    if (!tokens) {
      return Promise.reject(new AuthenticationError("Unauthorized"));
    }
    return await this.fetchApi("/register/company", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${tokens.accessToken}`,
      },
      body: JSON.stringify({ name, description }),
    });
  }

  // Maybe pass in the session instead of tokens. This way we can get and set the tokens directly here
  public async GetUserInfo(
    session?: Session<SessionData, SessionData>,
    accessToken?: string
  ): Promise<UserInfoResponse> {
    const tokens = session?.get("tokens") || { accessToken };
    if (!tokens) {
      return Promise.reject(new AuthenticationError("Unauthorized"));
    }
    const { data: res, error } = await tryCatch(
      this.fetchApi("/user/info", {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${tokens.accessToken}`,
        },
      })
    );

    if (error) {
      console.error(error);
      return Promise.reject(
        new ApiError("Server error: Failed to get user info", 500)
      );
    }

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

// fetch with auth

// fetch(API)
// 200 -> return
// 401 -> getSessionToken -> RefreshToken -> 401 -> clearSession -> return
// 401 -> getSessionToken -> RefreshToken -> 200 -> setTokens -> return
