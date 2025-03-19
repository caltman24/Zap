import { Link } from "@remix-run/react";

export const handle = {
    breadcrumb: () => <Link to="/tickets/archived">Archived</Link>,
};

export default function ArchivedTicketsRoute() {
    return (
        <div className="text-center w-full bg-base-300 h-full p-6">
            <h1 className="text-3xl font-bold">Archived Tickets</h1>
        </div>
    );
}
