import { Link } from "@remix-run/react";
import RouteLayout from "~/layouts/RouteLayout";

export const handle = {
    breadcrumb: () => <Link to="/tickets/assigned">My Tickets</Link>,
};

export default function AssignedTicketsRoute() {
    return (
        <RouteLayout className="text-center w-full bg-base-300 h-full p-6">
            <h1 className="text-3xl font-bold">My Tickets</h1>
        </RouteLayout>
    );
}
