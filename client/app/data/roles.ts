const roleNames: {
  admin: RoleName;
  projectManager: RoleName;
  developer: RoleName;
  submitter: RoleName;
} = {
  admin: "admin",
  projectManager: "projectmanager",
  developer: "developer",
  submitter: "submitter",
};

export type RoleName = "admin" | "projectmanager" | "developer" | "submitter";

export default roleNames;
