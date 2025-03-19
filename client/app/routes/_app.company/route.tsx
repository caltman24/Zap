import { Link } from "@remix-run/react";

export const handle = {
    breadcrumb: () => <Link to="/company">Company</Link>,
};

export default function CompanyRoute() {
    return (
        <div className="text-center w-full bg-base-300 h-full p-6">
            <h1 className="text-3xl font-bold">Company Details</h1>
        </div>
    );
}
