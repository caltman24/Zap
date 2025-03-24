import { AuthClient } from "./authClient";
import { BaseApiClient } from "./baseClient";
import {
  CompanyInfoResponse,
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
}

// Create and export a singleton instance
const apiClient = new ApiService(
  process.env.API_BASE_URL || "http://localhost:5090"
);
export default apiClient;
