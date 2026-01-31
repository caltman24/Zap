import tryCatch from "~/utils/tryCatch";
import { AuthClient } from "./authClient";
import { BaseApiClient } from "./baseClient";
import {
  CompanyInfoResponse,
  CompanyMemberPerRole,
  CompanyProjectsResponse,
  CreateProjectRequest,
  ProjectResponse,
  RegisterCompanyRequest,
  UserInfoResponse,
} from "./types";

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
    return fetch(`${this.baseUrl}/company/register`, {
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
    const method = "PUT";
    const { data: response, error } = await tryCatch(
      fetch(`${this.baseUrl}/company/info`, {
        method,
        headers: {
          Authorization: `Bearer ${accessToken}`,
        },
        body: formData,
      })
    );

    return this.handleResponse(response, error, method);
  }

  public async getCompanyProjects(
    accessToken: string
  ): Promise<CompanyProjectsResponse[]> {
    return this.requestJson<CompanyProjectsResponse[]>(
      "/company/projects?isArchived=false",
      { method: "GET" },
      accessToken
    );
  }

  public async getCompanyArchivedProjects(
    accessToken: string
  ): Promise<CompanyProjectsResponse[]> {
    return this.requestJson<CompanyProjectsResponse[]>(
      "/company/projects?isArchived=true",
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

  public async updateProject(
    projectId: string,
    projectData: any,
    accessToken: string
  ): Promise<Response> {
    const method = "PUT";
    const { data: response, error } = await tryCatch(
      fetch(`${this.baseUrl}/projects/${projectId}`, {
        method,
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${accessToken}`,
        },
        body: JSON.stringify(projectData),
      })
    );

    return this.handleResponse(response, error, method);
  }

  public async toggleArchiveProject(projectId: string, accessToken: string) {
    const method = "PUT";
    const { data: response, error } = await tryCatch(
      fetch(`${this.baseUrl}/projects/${projectId}/archive`, {
        method,
        headers: {
          Authorization: `Bearer ${accessToken}`,
        },
      })
    );

    return this.handleResponse(response, error, method);
  }
}

// Create and export a singleton instance
// Use a runtime-safe API base URL. In Remix server/runtime code `process.env` is available,
// but when bundling for the browser this falls back to the relative path so the frontend
// can call the same origin. Keep a default that points to the local API port used during
// development / docker smoke tests.
const apiClient = new ApiService(
  typeof process !== "undefined" && process.env?.API_BASE_URL
    ? process.env.API_BASE_URL
    : ""
);
export default apiClient;
