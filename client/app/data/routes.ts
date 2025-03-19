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
        name: "Archived Projects",
        to: "/projects/archived",
        materialIcon: "folder_open",
        roles: [],
      },
      {
        name: "Create Project",
        to: "/projects/new",
        materialIcon: "add_circle",
        roles: [],
      },
    ],
  },
  {
    name: "Tickets",
    links: [
      {
        name: "Open Tickets",
        to: "/tickets/open",
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

export function createRouteNameMap(
  menuRoutes: MenuRoutes
): Record<string, string> {
  return menuRoutes.reduce((acc, group) => {
    group.links.forEach((link) => {
      acc[link.to] = link.name;
    });
    return acc;
  }, {} as Record<string, string>);
}
