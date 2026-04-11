import {Link, useLocation} from "@remix-run/react";

const headlineClass = "[font-family:Manrope,sans-serif]";
const buttonBaseClass = "inline-flex items-center justify-center rounded-full text-sm font-bold leading-none transition duration-200 hover:-translate-y-px";
const navButtonSizeClass = "min-h-11 px-[1.1rem] max-sm:min-h-10 max-sm:px-[0.9rem] max-sm:text-[0.8125rem]";
const secondaryButtonClass = "border border-[var(--landing-outline-variant)] bg-[rgba(53,53,52,0.42)] text-[var(--landing-on-surface)] backdrop-blur-xl";
const primaryButtonClass = "bg-[linear-gradient(135deg,var(--landing-primary)_0%,var(--landing-primary-container)_100%)] text-[#1000a9] shadow-[0_14px_28px_rgba(128,131,255,0.2)]";

export default function MainNavbar({isAuthenticated}: { isAuthenticated: boolean }) {
    const location = useLocation();
    const isLoginPage = location.pathname === "/login";

    const secondaryLink = {to: "/", label: "Home"};
    const primaryLink = isAuthenticated
        ? {to: "/dashboard", label: "Dashboard"}
        : isLoginPage
            ? {to: "/register", label: "Register"}
            : {to: "/login", label: "Sign In"};
    const primaryLinkClassName = isAuthenticated
        ? `${buttonBaseClass} ${navButtonSizeClass} ${primaryButtonClass}`
        : `${buttonBaseClass} ${navButtonSizeClass} ${secondaryButtonClass}`;

    return (
        <header
            className="fixed inset-x-0 top-0 z-50 border-b border-white/5 bg-[color:var(--landing-surface)]/80 backdrop-blur-xl">
            <nav
                className="mx-auto flex w-full max-w-7xl items-center justify-between gap-6 px-6 py-3.5 sm:px-8 sm:py-4">
                <Link
                    className={`${headlineClass} text-2xl font-black tracking-[-0.08em] text-[var(--landing-on-surface)]`}
                    to="/">
                    Zap
                </Link>

                <div className="flex items-center gap-3">
                    <Link className={`${buttonBaseClass} ${navButtonSizeClass} ${secondaryButtonClass}`}
                          to={secondaryLink.to}>
                        {secondaryLink.label}
                    </Link>
                    <Link className={primaryLinkClassName} to={primaryLink.to}>
                        {primaryLink.label}
                    </Link>
                </div>
            </nav>
        </header>
    );
}
