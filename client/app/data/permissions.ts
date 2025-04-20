import roleNames, { RoleName } from "./roles";

const permissions: ActionPermissions = {
  project: {
    edit: [roleNames.admin, roleNames.projectManager],
    delete: [roleNames.admin, roleNames.projectManager],
    create: [roleNames.admin, roleNames.projectManager],
  },
  company: {
    edit: [roleNames.admin],
    delete: [roleNames.admin],
  },
};

export type ActionPermissions = {
  project: {
    edit: RoleName[];
    delete: RoleName[];
    create: RoleName[];
  };
  company: {
    edit: RoleName[];
    delete: RoleName[];
  };
};

export default permissions;
