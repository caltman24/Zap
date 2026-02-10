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
    edit: [roleNames.admin, roleNames.projectManager],
    editStatus: [
      roleNames.admin,
      roleNames.projectManager,
      roleNames.developer,
      roleNames.submitter,
    ],
    editPriority: [
      roleNames.admin,
      roleNames.projectManager,
      roleNames.submitter,
    ],
    editType: [roleNames.admin, roleNames.projectManager, roleNames.submitter],
    delete: [roleNames.admin, roleNames.projectManager],
    archive: [roleNames.admin, roleNames.projectManager],
    assign: [roleNames.admin, roleNames.projectManager],
  },
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
    edit: RoleName[];
    editStatus: RoleName[];
    editPriority: RoleName[];
    editType: RoleName[];
    delete: RoleName[];
    archive: RoleName[];
    assign: RoleName[];
  };
  comment: {
    create: RoleName[];
    editOwn: RoleName[];
    deleteOwn: RoleName[];
  };
};

export default permissions;
