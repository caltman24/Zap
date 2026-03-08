import { Form, useMatches } from "@remix-run/react";
import type { UIMatch } from "@remix-run/react";
import type { ReactNode } from "react";

type BreadcrumbMatch = UIMatch<unknown, {
    breadcrumb?: (match: UIMatch) => ReactNode;
    breadcrumbLabel?: string | ((match: UIMatch) => string);
}>;

function normalizeBreadcrumbLabel(label: string | null) {
    return label?.trim().toLowerCase() ?? null;
}

function getBreadcrumbLabel(match: BreadcrumbMatch) {
    const breadcrumbLabel = match.handle?.breadcrumbLabel;

    if (typeof breadcrumbLabel === "function") {
        return breadcrumbLabel(match);
    }

    return breadcrumbLabel ?? match.pathname;
}


export default function DashboardNavbar({ avatarUrl }: { avatarUrl: string }) {
    const matches = useMatches() as BreadcrumbMatch[];
    const breadcrumbMatches = matches
        .filter(
            (match) =>
                match.handle && match.handle.breadcrumb
        )
        .filter((match, index, allMatches) => {
            if (index === 0) {
                return true;
            }

            return normalizeBreadcrumbLabel(getBreadcrumbLabel(match)) !== normalizeBreadcrumbLabel(getBreadcrumbLabel(allMatches[index - 1]));
        });

    return (
        <div className="bg-base-200 shadow-sm sticky left-0 w-full z-20">
            <nav className="relative left-0 navbar px-10">
                <div className="flex-1 breadcrumbs">
                    <ul>
                        {breadcrumbMatches
                            .map((match, index) => (
                                <li key={`${match.id}-${getBreadcrumbLabel(match)}`} className={`${index === 0 ? "font-medium" : ""}`}>
                                    {match.handle?.breadcrumb?.(match)}
                                </li>
                            ))}
                    </ul>
                </div>
                <div className="flex gap-2 items-center">
                    <div className="dropdown dropdown-end">
                        <div tabIndex={0} role="button" className="btn btn-ghost btn-circle avatar shadow-sm">
                            <div className="w-full rounded-full ring ring-primary ring-offset-base-100">
                                <div className="">
                                    <img
                                        alt="Tailwind CSS Navbar component"
                                        src={avatarUrl} />
                                </div>
                            </div>
                        </div>
                        <ul
                            tabIndex={0}
                            className="menu menu-md dropdown-content bg-base-200 rounded-box z-20 mt-3 w-52 p-2 shadow">
                            <li>
                                <a className="justify-between">
                                    Profile
                                </a>
                            </li>
                            <li><a>Settings</a></li>
                            <Form method="post">
                                <li><button type="submit" formAction="/logout">Logout</button></li>
                            </Form>
                        </ul>
                    </div>
                </div>
            </nav>
        </div>
    )
}
