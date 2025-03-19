type Props = {
    routeName: string;
}

export default function DashboardNavbar({ routeName }: Props) {
    return (
        <div className="bg-base-200 shadow-sm sticky left-0 w-full z-10">
            <nav className="relative left-0 navbar px-10">
                <div className="flex-1">
                    <a className="text-xl font-bold">{routeName}</a>
                </div>
                <div className="flex gap-2 items-center">
                    <div className="dropdown dropdown-end">
                        <div tabIndex={0} role="button" className="btn btn-ghost btn-circle avatar">
                            <div className="w-full rounded-full">
                                <img
                                    alt="Tailwind CSS Navbar component"
                                    src="https://img.daisyui.com/images/stock/photo-1534528741775-53994a69daeb.webp" />
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
                            <li><a>Logout</a></li>
                        </ul>
                    </div>
                </div>
            </nav>
        </div>
    )
}