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
  logoUrl?: string;
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

export type CreateProjectRequest = {
  name: string;
  description: string;
  priority: string;
  dueDate: string;
};

export type ProjectResponse = {
  id: string;
  name: string;
  description: string;
  priority: string;
  dueDate: string; // ISO date string format
  members: { name: string; avatarUrl: string }[];
};

export type CompanyProjectsResponse = {
  id: string;
  name: string;
  priority: string;
  dueDate: string; // ISO date string format
  memberCount: number;
  avatarUrls: string[];
};
