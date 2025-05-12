import roleNames from "./roles";

export type MenuLink = {
  name: string;
  to: string;
  matchId: string;
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
        matchId: "routes/_app.dashboard",
        materialIcon: "dashboard",
        roles: [],
      },
      {
        name: "Company Details",
        to: "/company",
        matchId: "routes/_app.company",
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
        matchId: "routes/_app.projects",
        materialIcon: "folder",
        roles: [],
      },
      {
        name: "My Projects",
        to: "/projects/myprojects",
        matchId: "routes/_app.projects._index",
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
        matchId: "routes/_app.projects.archived._index",
        materialIcon: "folder_open",
        roles: [roleNames.admin, roleNames.projectManager],
      },
      {
        name: "Create Project",
        to: "/projects/new",
        matchId: "routes/_app.projects.new",
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
        matchId: "routes/_app.tickets._index",
        materialIcon: "assignment",
        roles: [],
      },
      {
        name: "My Tickets",
        to: "/tickets/assigned",
        matchId: "routes/_app.tickets.assigned",
        materialIcon: "assignment_ind",
        roles: [],
      },
      {
        name: "Resolved Tickets",
        to: "/tickets/resolved",
        matchId: "routes/_app.tickets.resolved",
        materialIcon: "assignment_turned_in",
        roles: [],
      },
      {
        name: "Archived Tickets",
        to: "/tickets/archived",
        matchId: "routes/_app.tickets.resolved",
        materialIcon: "assignment_returned",
        roles: [],
      },
      {
        name: "Submit Ticket",
        to: "/tickets/new",
        matchId: "routes/_app.tickets.new",
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
