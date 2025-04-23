import tryCatch from "~/utils/tryCatch";
import { ApiError, AuthenticationError } from "./errors";

export class BaseApiClient {
  protected readonly baseUrl: string;

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl;
  }

  // Helper methods
  public expiresInToIso(expiresIn: number): string {
    return new Date(Date.now() + expiresIn * 1000).toISOString();
  }

  // Base HTTP methods
  protected async requestJson<T>(
    endpoint: string,
    options?: RequestInit,
    token?: string,
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
    return this.handleJsonResponse<T>(response, error, "GET");
  }
  protected async handleResponse(
    response: Response | null,
    error: Error | null,
    method: string,
  ): Promise<Response> {
    // Error from fetch
    if (error) {
      console.error(error);
      throw new Error("Unexpected error: Failed to fetch", { cause: error });
    }

    if (!response) {
      console.error("Unexpected error: No response");
      throw new Error("No response");
    }

    if (response.status === 401) {
      throw new AuthenticationError("Unauthorized");
    }

    if (!response.ok) {
      this.logResponse(response, method);
      throw new ApiError(response.statusText, response.status);
    }

    this.logResponse(response, method);

    return response;
  }
  protected async handleJsonResponse<T>(
    response: Response | null,
    error: Error | null,
    method: string,
  ): Promise<T> {
    if (error) {
      console.error(error);
      throw new Error("Unexpected error: Failed to fetch", { cause: error });
    }

    if (!response) {
      console.error("Unexpected error: No response");
      throw new Error("No response");
    }

    if (response.status === 401) {
      throw new AuthenticationError("Unauthorized");
    }

    if (!response.ok) {
      this.logResponse(response, method);
      throw new ApiError(response.statusText, response.status);
    }

    this.logResponse(response, method);

    return (await response.json()) as T;
  }

  private logResponse(response: Response, method: string) {
    if (process.env.NODE_ENV !== "production") {
      console.log(method, response.url, response.status, response.statusText);
    }
  }
}
