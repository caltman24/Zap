import RouteLayout from "~/layouts/RouteLayout";
import { Link } from "@remix-run/react";

export const handle = {
    breadcrumb: () => <Link to="/projects/archived">Archived</Link>,
};

export default function ArchivedProjectsRoute() {
    return (
        <RouteLayout className="text-center w-full bg-base-300 h-full p-6">
            <h1 className="text-3xl font-bold">Archived Projects</h1>
        </RouteLayout>
    );
}
