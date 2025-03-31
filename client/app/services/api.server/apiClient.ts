import tryCatch from "~/utils/tryCatch";
import { AuthClient } from "./authClient";
import { BaseApiClient } from "./baseClient";
import {
  CompanyInfoResponse,
  CompanyProjectsResponse,
  CreateProjectRequest,
  ProjectResponse,
  RegisterCompanyRequest,
  UserInfoResponse,
} from "./types";
import { ApiError } from "./errors";

export class ApiService extends BaseApiClient {
  constructor(baseUrl: string) {
    super(baseUrl);
  }

  public auth: AuthClient = new AuthClient(this.baseUrl);

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

  public async updateCompanyInfo(
    formData: FormData,
    accessToken: string
  ): Promise<Response> {
    const { data: response, error } = await tryCatch(
      fetch(`${this.baseUrl}/company/info`, {
        method: "POST",
        headers: {
          Authorization: `Bearer ${accessToken}`,
        },
        body: formData,
      })
    );

    if (error || !response) {
      console.error(error);
      return Promise.reject(new ApiError("Failed to update company info", 500));
    }

    if (response.status === 401) {
      return Promise.reject(new ApiError("Unauthorized", 401));
    }

    if (!response.ok) {
      console.error(response.url, response.status, response.statusText);
      return Promise.reject(new ApiError(response.statusText, response.status));
    }

    return response;
  }

  public async getCompanyProjects(
    accessToken: string
  ): Promise<CompanyProjectsResponse[]> {
    return this.requestJson<CompanyProjectsResponse[]>(
      "/company/projects",
      { method: "GET" },
      accessToken
    );
  }

  public async getProjectById(
    id: string,
    accessToken: string
  ): Promise<ProjectResponse> {
    return this.requestJson<ProjectResponse>(
      `/projects/${id}`,
      { method: "GET" },
      accessToken
    );
  }

  public async createProject(
    data: CreateProjectRequest,
    accessToken: string
  ): Promise<Response> {
    return fetch(`${this.baseUrl}/projects`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${accessToken}`,
      },
      body: JSON.stringify(data),
    });
  }
}

// Create and export a singleton instance
const apiClient = new ApiService(
  process.env.API_BASE_URL || "http://localhost:5090"
);
export default apiClient;
