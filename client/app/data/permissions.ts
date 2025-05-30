import roleNames, { RoleName } from "./roles";

const permissions: ActionPermissions = {
  project: {
    edit: [roleNames.admin, roleNames.projectManager],
    delete: [roleNames.admin],
    create: [roleNames.admin, roleNames.projectManager],
    assignPM: [roleNames.admin],
    myprojects: [
      roleNames.submitter,
      roleNames.developer,
      roleNames.projectManager,
    ],
  },
  company: {
    edit: [roleNames.admin],
    delete: [roleNames.admin],
  },
  ticket: {
    create: [roleNames.admin, roleNames.projectManager, roleNames.submitter],
  },
};

export type ActionPermissions = {
  project: {
    edit: RoleName[];
    delete: RoleName[];
    create: RoleName[];
    assignPM: RoleName[];
    myprojects: RoleName[];
  };
  company: {
    edit: RoleName[];
    delete: RoleName[];
  };
  ticket: {
    create: RoleName[];
  };
};

export default permissions;
