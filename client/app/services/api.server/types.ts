// Response types
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

export type CompanyInfoResponse = {
  name: string;
  description: string;
};

export type RegisterUserRequest = {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
};

export type RegisterCompanyRequest = {
  name: string;
  description: string;
};
