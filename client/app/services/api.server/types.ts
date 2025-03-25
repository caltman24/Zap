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
  avatarUrl: string;
  companyId?: string;
};

export type CompanyInfoResponse = {
  name: string;
  description: string;
  members: { [key: string]: { name: string; avatarUrl: string }[] };
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
