import { Link, useLocation } from "@remix-run/react";

function TicketsBreadcrumb() {
    const location = useLocation();

    return <Link to={{ pathname: "/tickets", search: location.search }}>Tickets</Link>;
}

export const handle = {
    breadcrumb: () => <TicketsBreadcrumb />,
};
