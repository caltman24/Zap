import roleNames from "./roles";

export type MenuLink = {
  name: string;
  to: string;
  materialIcon?: string;
  roles: string[];
};

export type MenuGroup = {
  name: string;
  links: MenuLink[];
};

export type MenuRoutes = MenuGroup[];

export const menuRoutes: MenuRoutes = [
  {
    name: "Company",
    links: [
      {
        name: "Dashboard",
        to: "/dashboard",
        materialIcon: "dashboard",
        roles: [],
      },
      {
        name: "Company Details",
        to: "/company",
        materialIcon: "domain",
        roles: [],
      },
    ],
  },
  {
    name: "Projects",
    links: [
      {
        name: "All Projects",
        to: "/projects",
        materialIcon: "folder",
        roles: [],
      },
      {
        name: "My Projects",
        to: "/projects/myprojects",
        materialIcon: "folder",
        roles: [
          roleNames.submitter,
          roleNames.developer,
          roleNames.projectManager,
        ],
      },
      {
        name: "Archived Projects",
        to: "/projects/archived",
        materialIcon: "folder_open",
        roles: [roleNames.admin, roleNames.projectManager],
      },
      {
        name: "Create Project",
        to: "/projects/new",
        materialIcon: "add_circle",
        roles: [roleNames.admin, roleNames.projectManager],
      },
    ],
  },
  {
    name: "Tickets",
    links: [
      {
        name: "Open Tickets",
        to: "/tickets",
        materialIcon: "assignment",
        roles: [],
      },
      {
        name: "My Tickets",
        to: "/tickets/assigned",
        materialIcon: "assignment_ind",
        roles: [],
      },
      {
        name: "Resolved Tickets",
        to: "/tickets/resolved",
        materialIcon: "assignment_turned_in",
        roles: [],
      },
      {
        name: "Archived Tickets",
        to: "/tickets/archived",
        materialIcon: "assignment_returned",
        roles: [],
      },
      {
        name: "Submit Ticket",
        to: "/tickets/new",
        materialIcon: "assignment_late",
        roles: [],
      },
    ],
  },
];

export function filterMenuRoutesByRoles(
  menuRoutes: MenuRoutes,
  roles: string[]
): MenuRoutes {
  return menuRoutes.map((menuGroup) => ({
    ...menuGroup,
    links: menuGroup.links.filter(
      (link) =>
        link.roles.some((role) => roles.includes(role)) ||
        link.roles.length === 0
    ),
  }));
}

export function getRolesByRouteName(routeName: string) {
  return menuRoutes.flatMap((menuGroup) =>
    menuGroup.links
      .filter((link) => link.name === routeName)
      .flatMap((link) => link.roles)
  );
}
