import roleNames, { RoleName } from "./roles";

const permissions: ActionPermissions = {
  comment: {
    create: [
      roleNames.admin,
      roleNames.projectManager,
      roleNames.developer,
      roleNames.submitter,
    ],
    editOwn: [
      roleNames.admin,
      roleNames.projectManager,
      roleNames.developer,
      roleNames.submitter,
    ],
    deleteOwn: [
      roleNames.admin,
      roleNames.projectManager,
      roleNames.developer,
      roleNames.submitter,
    ],
  },
};

export type ActionPermissions = {
  comment: {
    create: RoleName[];
    editOwn: RoleName[];
    deleteOwn: RoleName[];
  };
};

export default permissions;
