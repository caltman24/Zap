import { Link } from "@remix-run/react";

export const handle = {
    breadcrumb: () => <Link to="/projects/archived">Archived</Link>,
};

export default function ArchivedProjectsRoute() {
    return (
        <div className="text-center w-full bg-base-300 h-full p-6">
            <h1 className="text-3xl font-bold">Archived Projects</h1>
        </div>
    );
}
