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
