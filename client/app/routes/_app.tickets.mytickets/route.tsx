import { Link, useLocation } from "@remix-run/react";

function MyTicketsBreadcrumb() {
    const location = useLocation();

    return <Link to={{ pathname: "/tickets/mytickets", search: location.search }}>My Tickets</Link>;
}

export const handle = {
    breadcrumb: () => <MyTicketsBreadcrumb />,
};
