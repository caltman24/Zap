import { Link, Outlet } from "@remix-run/react";
// this route is only for breadcrumbs
export const handle = {
    breadcrumb: () => <Link to="/projects">Projects</Link>,
};

export default function ProjectsRootRoute() {
    return <Outlet />;
}
