import { Link, Outlet, redirect } from "@remix-run/react";

export const handle = {
    breadcrumb: () => <Link to="/tickets">Tickets</Link>,
};
