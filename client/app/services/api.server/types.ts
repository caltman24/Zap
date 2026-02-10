import { RoleName } from "~/data/roles";

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
  role: RoleName;
  avatarUrl: string;
  companyId?: string;
  memberId: string;
};
export type BasicUserInfo = {
  id: string;
  name: string;
  avatarUrl: string;
  role: RoleName;
};
export type ProjectManagerInfo = {
  id: string;
  name: string;
  avatarUrl: string;
  role: RoleName;
  assigned: boolean;
};

export type CompanyInfoResponse = {
  name: string;
  description: string;
  logoUrl?: string;
  members: CompanyMemberPerRole;
};

export type CompanyMemberPerRole = {
  [key: string]: { id: string; name: string; avatarUrl: string }[];
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
  isArchived: boolean;
  projectManager: {
    id: string;
    name: string;
    avatarUrl: string;
    role: RoleName;
  } | null;
  members: {
    id: string;
    name: string;
    avatarUrl: string;
    role: RoleName;
  }[];
  tickets: BasicTicketInfo[];
};

export type BasicProjectResponse = {
  id: string;
  name: string;
};

export type CompanyProjectsResponse = {
  id: string;
  name: string;
  priority: string;
  dueDate: string; // ISO date string format
  memberCount: number;
  isArchived: boolean;
  avatarUrls: string[];
};

export type CreateTicketRequest = {
  name: string;
  description: string;
  priority: string;
  status: string;
  type: string;
  projectId: string;
};

export type BasicTicketInfo = {
  id: string;
  name: string;
  description: string;
  priority: string;
  status: string;
  type: string;
  projectId: string;
  submitter: BasicUserInfo;
  assignee: BasicUserInfo | null;
  isArchived: boolean;
  projectIsArchived: boolean;
  createdAt: string;
  updatedAt: string | null;
};

export type CreateTicketResult = {
  id: string;
};

export type TicketComment = {
  id: string;
  ticketId: string;
  sender: BasicUserInfo;
  message: string;
  createdAt: string;
  updatedAt: string | null;
};
