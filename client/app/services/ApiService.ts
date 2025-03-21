export type ValidateAccountResponse = {
  result: "company" | "user" | "none";
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

  public async SignInUser(email: string, password: string): Promise<Response> {
    return await this.fetchApi("/signin/company", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ email, password }),
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
}

const apiService = new ApiService("http://localhost:5090");

export default apiService;
