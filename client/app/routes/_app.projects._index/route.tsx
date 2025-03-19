import { Link } from "@remix-run/react";

export const handle = {
    breadcrumb: () => <Link to="/projects">All Projects</Link>,
};

export default function ProjectsRoute() {
    return (
        <div className="text-center w-full bg-base-300 h-full p-6">
            <h1 className="text-3xl font-bold">All Projects</h1>
        </div>
    );
}
