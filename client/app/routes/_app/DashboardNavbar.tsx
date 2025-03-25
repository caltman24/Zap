import { Form, useMatches } from "@remix-run/react";



export default function DashboardNavbar({ avatarUrl }: { avatarUrl: string }) {
    const matches = useMatches();

    return (
        <div className="bg-base-200 shadow-sm sticky left-0 w-full z-10">
            <nav className="relative left-0 navbar px-10">
                <div className="flex-1 breadcrumbs">
                    <ul>
                        {matches
                            .filter(
                                (match) =>
                                    match.handle && (match.handle as any).breadcrumb
                            )
                            .map((match, index) => (
                                <li key={index} className={`${index === 0 ? "font-medium" : ""}`}>
                                    {(match.handle as any).breadcrumb(match)}
                                </li>
                            ))}
                    </ul>
                </div>
                <div className="flex gap-2 items-center">
                    <div className="dropdown dropdown-end">
                        <div tabIndex={0} role="button" className="btn btn-ghost btn-circle avatar">
                            <div className="w-full rounded-full">
                                <img
                                    alt="Tailwind CSS Navbar component"
                                    src={avatarUrl} />
                            </div>
                        </div>
                        <ul
                            tabIndex={0}
                            className="menu menu-md dropdown-content bg-base-200 rounded-box z-1 mt-3 w-52 p-2 shadow">
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