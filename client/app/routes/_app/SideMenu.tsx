import { Link, useLocation } from "@remix-run/react";
import { useState } from "react";
import { MenuGroup, MenuRoutes } from "~/data/routes";


export default function SideMenu({ menuRoutes: menuRoutes }: { menuRoutes: MenuRoutes }) {
    const location = useLocation();
    const [expandedMenus, setExpandedMenus] = useState<{ [key: string]: boolean }>({
        "Company": true // Default expanded menu
    });

    const toggleMenu = (menuName: string) => {
        setExpandedMenus(prev => ({
            ...prev,
            [menuName]: !prev[menuName]
        }));
    };

    const MenuItem = (item: MenuGroup) => {
        const isExpanded = expandedMenus[item.name] || false;
        const hasActiveLink = item.links.some(link => location.pathname.startsWith(link.to));

        return (
            <div>
                <div
                    className={`font-bold text-lg mb-2 flex items-center justify-between cursor-pointer hover:bg-base-300 px-4 py-3 rounded ${hasActiveLink ? "text-primary" : ""}`}
                    onClick={() => toggleMenu(item.name)}
                >
                    <span>{item.name}</span>
                    <span className={`material-symbols-outlined transition-transform ${isExpanded ? "rotate-90" : ""}`}>
                        chevron_right
                    </span>
                </div>

                {isExpanded && (
                    <ul className="flex flex-col gap-2 pl-2 transition-all">
                        {item.links.map((link, index) => {
                            const isActive = location.pathname === link.to;
                            return (
                                <li key={index}>
                                    <Link
                                        to={link.to}
                                        className={`hover:bg-base-300 p-2 ml-2 rounded w-full flex gap-2 items-center ${isActive
                                            ? "bg-base-100 text-primary font-bold"
                                            : ""
                                            }`}
                                    >
                                        {link.materialIcon && (
                                            <span className="material-symbols-outlined">
                                                {link.materialIcon}
                                            </span>
                                        )}
                                        {link.name}
                                    </Link>
                                </li>
                            )
                        })}
                    </ul>
                )}
            </div>
        )
    }

    return (
        <div className="flex flex-col bg-base-200 text-content 2xl:flex-1/5 xl:flex-1/4 lg:flex-1/3 flex-1/2 border-r border-base-content/10">
            <div className="">
                <Link to={"/"} className="text-4xl block text-center p-6 font-bold tracking-wider hover:text-base-content">ZAP</Link>
            </div>
            {menuRoutes.map((item, index) => (
                <MenuItem key={index} {...item} />
            ))}
        </div>

    )
}
