import { Session, SessionData } from "@remix-run/node";
import { commitSession } from "../sessions.server";
import { BaseApiClient } from "./baseClient";
import { AuthenticationError } from "./errors";
import { RegisterUserRequest, TokenResponse } from "./types";

export class AuthClient extends BaseApiClient {
  private readonly refreshBuffer = 1000 * 60 * 2; // 2 minutes
  private prefix: string;

  constructor(baseUrl: string) {
    super(baseUrl);
    this.prefix = baseUrl + "/auth";
  }

  public async signInUser(email: string, password: string): Promise<Response> {
    return fetch(`${this.prefix}/signin`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email, password }),
    });
  }

  public async signInTestUser(): Promise<Response> {
    return fetch(`${this.prefix}/signin-test`, { method: "POST" });
  }

  public async registerAccount(data: RegisterUserRequest): Promise<Response> {
    return fetch(`${this.prefix}/register`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data),
    });
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
          } as HeadersInit,
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
        headers: undefined as HeadersInit | undefined,
      };
    }
  }

  private async refreshTokens(refreshToken: string): Promise<TokenResponse> {
    return this.requestJson<TokenResponse>(this.prefix + "/refreshtokens", {
      method: "POST",
      body: JSON.stringify({ refreshToken }),
    });
  }
}
