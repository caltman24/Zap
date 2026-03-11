export type MenuLink = {
  name: string;
  to: string;
  matchId: string;
  materialIcon?: string;
  requiredPermission?: string;
  hiddenRoles?: string[];
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
      },
      {
        name: "Company Details",
        to: "/company",
        matchId: "routes/_app.company",
        materialIcon: "domain",
        requiredPermission: "company.edit",
      },
    ],
  },
  {
    name: "Projects",
    links: [
      {
        name: "All Projects",
        to: "/projects",
        matchId: "routes/_app.projects._index",
        materialIcon: "folder",
        requiredPermission: "project.viewAll",
      },
      {
        name: "My Projects",
        to: "/projects/myprojects",
        matchId: "routes/_app.projects.myprojects._index",
        materialIcon: "folder_shared",
        requiredPermission: "project.viewAssigned",
        hiddenRoles: ["admin"],
      },
      {
        name: "Archived Projects",
        to: "/projects/archived",
        matchId: "routes/_app.projects.archived._index",
        materialIcon: "folder_open",
        requiredPermission: "project.viewArchived",
      },
      {
        name: "Create Project",
        to: "/projects/new",
        matchId: "routes/_app.projects.new",
        materialIcon: "add_circle",
        requiredPermission: "project.create",
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
      },
      {
        name: "My Tickets",
        to: "/tickets/mytickets",
        matchId: "routes/_app.tickets.mytickets",
        materialIcon: "assignment_ind",
      },
      {
        name: "Resolved Tickets",
        to: "/tickets/resolved",
        matchId: "routes/_app.tickets.resolved",
        materialIcon: "assignment_turned_in",
      },
      {
        name: "Archived Tickets",
        to: "/tickets/archived",
        matchId: "routes/_app.tickets.resolved",
        materialIcon: "assignment_returned",
      },
      {
        name: "Submit Ticket",
        to: "/tickets/new",
        matchId: "routes/_app.tickets.new",
        materialIcon: "assignment_late",
        requiredPermission: "ticket.create",
      },
    ],
  },
];

export function filterMenuRoutesByPermissions(
  menuRoutes: MenuRoutes,
  permissions: string[],
  role?: string
): MenuRoutes {
  return menuRoutes.map((menuGroup) => ({
    ...menuGroup,
    links: menuGroup.links.filter(
      (link) =>
        (!link.requiredPermission || permissions.includes(link.requiredPermission)) &&
        !(link.hiddenRoles?.includes((role ?? "").toLowerCase()))
    ),
  }));
}
