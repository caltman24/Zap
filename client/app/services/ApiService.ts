import tryCatch from "~/utils/tryCatch";

// Custom error classes for better error handling
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

  public async GetUserInfo(tokens: {
    accessToken: string;
    refreshToken: string;
  }): Promise<UserInfoResponse> {
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
      return Promise.reject(new Error("Server error: Failed to get user info"));
    }

    if (res.status === 401) {
      const refreshRes = await this.RefreshTokens(tokens.refreshToken);

      if (refreshRes.ok) {
        const newTokens: TokenResponse = await refreshRes.json();
        return await this.GetUserInfo(newTokens);
      }

      return Promise.reject(new TokenRefreshError("Unauthorized"));
    }

    if (!res.ok) {
      console.error(res);
      return Promise.reject(
        new ApiError("Failed to sign in. No user info found", res.status)
      );
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
