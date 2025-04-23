import { ApiError, AuthenticationError } from "~/services/api.server/errors";
import tryCatch from "./tryCatch";

export const DEV_URL = "http://localhost:5090";

export function expiresInToIso(expiresIn: number): string {
  return new Date(Date.now() + expiresIn * 1000).toISOString();
}

// Base HTTP methods
export async function requestJson<T>(
  endpoint: string,
  options?: RequestInit,
  token?: string,
): Promise<T> {
  const url = `${DEV_URL}${endpoint}`;
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
  return handleJsonResponse<T>(response, error, "GET");
}
export async function handleResponse(
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
    logResponse(response, method);
    throw new ApiError(response.statusText, response.status);
  }

  logResponse(response, method);

  return response;
}
export async function handleJsonResponse<T>(
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
    logResponse(response, method);
    throw new ApiError(response.statusText, response.status);
  }

  logResponse(response, method);

  return (await response.json()) as T;
}

export function logResponse(response: Response, method: string) {
  if (process.env.NODE_ENV !== "production") {
    console.log(method, response.url, response.status, response.statusText);
  }
}
