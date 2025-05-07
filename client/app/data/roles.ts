const roleNames: {
  admin: RoleName;
  projectManager: RoleName;
  developer: RoleName;
  submitter: RoleName;
} = {
  admin: "admin",
  projectManager: "project manager",
  developer: "developer",
  submitter: "submitter",
};

export type RoleName = "admin" | "project manager" | "developer" | "submitter";

export default roleNames;
