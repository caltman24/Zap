import { Link } from "@remix-run/react";

export const handle = {
    breadcrumb: () => <Link to="/projects/new">New</Link>,
};

export default function NewProjectRoute() {
    return (
        <div className="text-center w-full bg-base-300 h-full p-6">
            <h1 className="text-3xl font-bold">Create New Project</h1>
        </div>
    );
}
