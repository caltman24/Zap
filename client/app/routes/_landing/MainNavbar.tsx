import { Form, Link } from "@remix-run/react";

export default function MainNavbar({ isAuthenticated }: { isAuthenticated: boolean }) {
    return (
        <div className="bg-base-200 shadow-sm">
            <nav className="navbar max-w-7xl mx-auto">
                <div className="flex-1">
                    <Link to="/" className="btn btn-ghost text-2xl">ZAP</Link>
                </div>
                <div className="flex gap-2 items-center">
                    <ul className="menu menu-horizontal px-1 2xl:text-lg">
                        <li>
                            <Link to="/">Home</Link>
                        </li>
                        <li>
                            <Link to="/setup">Setup</Link>
                        </li>
                        {
                            isAuthenticated ?
                                <li>
                                    <Link to="/dashboard">Dashboard</Link>
                                </li>
                                :
                                (
                                    <>
                                        <li>
                                            <Link to="/login">Login</Link>
                                        </li>
                                        <li>
                                            <Link to="/register">Register</Link>
                                        </li>
                                    </>
                                )
                        }
                    </ul>
                    {
                        isAuthenticated && (
                            <div className="dropdown dropdown-end">
                                <div tabIndex={0} role="button" className="btn btn-ghost btn-circle avatar">
                                    <div className="w-10 rounded-full">
                                        <img
                                            alt="Tailwind CSS Navbar component"
                                            src="https://img.daisyui.com/images/stock/photo-1534528741775-53994a69daeb.webp" />
                                    </div>
                                </div>
                                <ul
                                    tabIndex={0}
                                    className="menu menu-sm dropdown-content bg-base-200 rounded-box z-1 mt-3 w-52 p-2 shadow">
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
                        )
                    }
                </div>
            </nav>
        </div>
    )
}