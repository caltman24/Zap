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
  protected async handleResponse<T>(
    response: Response | null,
    error: Error | null
  ): Promise<T> {
    if (error) {
      console.error(error);
      throw new ApiError("Failed to fetch", 500);
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
}
