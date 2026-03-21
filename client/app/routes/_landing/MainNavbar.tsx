import { Link, useLocation } from "@remix-run/react";

export default function MainNavbar({ isAuthenticated }: { isAuthenticated: boolean }) {
    const location = useLocation();
    const isLoginPage = location.pathname === "/login";

    const secondaryLink = { to: "/", label: "Home" };
    const primaryLink = isAuthenticated
        ? { to: "/dashboard", label: "Dashboard" }
        : isLoginPage
            ? { to: "/register", label: "Register" }
            : { to: "/login", label: "Sign In" };
    const primaryLinkClassName = isAuthenticated
        ? "landing-button landing-button-primary landing-nav-button"
        : "landing-button landing-button-secondary landing-nav-button";

    return (
        <header className="fixed inset-x-0 top-0 z-50 border-b border-white/5 bg-[color:var(--landing-surface)]/80 backdrop-blur-xl">
            <nav className="mx-auto flex w-full max-w-7xl items-center justify-between gap-6 px-6 py-3.5 sm:px-8 sm:py-4">
                <Link className="landing-headline text-2xl font-black tracking-[-0.08em] text-[var(--landing-on-surface)]" to="/">
                    Zap
                </Link>

                <div className="flex items-center gap-3">
                    <Link className="landing-button landing-button-secondary landing-nav-button" to={secondaryLink.to}>
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
