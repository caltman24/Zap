import { Link, MetaFunction, useOutletContext } from "@remix-run/react";
import { useEffect, useState } from "react";
import type { MouseEvent } from "react";

type LandingContext = {
    isAuthenticated: boolean;
};

const workflowFeature = {
    eyebrow: "Workflow",
    title: "Structured ticket management",
    description: "High-performance list views designed for scanning and rapid triage. No bloated boards, just fast decision-making.",
    tickets: [
        { id: "ZAP-104", label: "Fix authentication middleware latency", status: "In Progress", active: true },
        { id: "ZAP-105", label: "Implement secure file storage", status: "Todo", active: false },
    ],
} as const;

const featureCards = [
    {
        icon: "lock",
        title: "Role-based permissions",
        description: "Granular access control for Admins, Members, and Viewers. Secure by design.",
    },
    {
        icon: "folder_shared",
        title: "Project-based access",
        description: "Isolate environments and tickets by project. Keep teams focused and data private.",
    },
    {
        icon: "assignment_turned_in",
        title: "Status tracking",
        description: "Real-time ticket updates and audit history for every issue lifecycle.",
    },
    {
        icon: "cloud_upload",
        title: "Secure attachments",
        description: "Permission-aware file handling that keeps uploads tied to the right tickets.",
    },
] as const;

const stackItems = [
    { icon: "api", label: "ASP.NET Core" },
    { icon: "database", label: "PostgreSQL" },
    { monogram: "TS", label: "TypeScript" },
    { icon: "code", label: "Remix SSR" },
    { icon: "shield", label: "RBAC Auth" },
    { icon: "hub", label: "REST API" },
] as const;

const navItems = [
    { href: "#features", label: "Features" },
    { href: "#product", label: "Product" },
    { href: "#tech", label: "Tech Stack" },
    { href: "#why", label: "Why I Built This" },
] as const;

const headlineClass = "[font-family:Manrope,sans-serif]";
const buttonBaseClass = "inline-flex items-center justify-center rounded-full text-sm font-bold leading-none transition duration-200 hover:-translate-y-px";
const primaryButtonClass = "bg-[linear-gradient(135deg,var(--landing-primary)_0%,var(--landing-primary-container)_100%)] text-[#1000a9] shadow-[0_14px_28px_rgba(128,131,255,0.2)]";
const secondaryButtonClass = "border border-[var(--landing-outline-variant)] bg-[rgba(53,53,52,0.42)] text-[var(--landing-on-surface)] backdrop-blur-xl";
const navButtonSizeClass = "min-h-11 px-[1.1rem] max-sm:min-h-10 max-sm:px-[0.9rem] max-sm:text-[0.8125rem]";
const heroButtonSizeClass = "min-h-14 px-7 text-base";
const previewCardClass = "rounded-[1.5rem] bg-[linear-gradient(180deg,rgba(255,255,255,0.04),rgba(255,255,255,0.01))] p-5 shadow-[0_30px_80px_rgba(229,226,225,0.04)] sm:p-6";
const kineticGradientClass = "bg-[linear-gradient(135deg,var(--landing-primary)_0%,var(--landing-primary-container)_100%)]";

export const meta: MetaFunction = () => {
    return [
        { title: "Zap - Modern Issue Tracking" },
        {
            name: "description",
            content: "A modern issue tracker for developers who want fast workflows, clear ownership, and server-first permission boundaries.",
        },
    ];
};

function MaterialIcon({ icon, className = "" }: { icon: string; className?: string }) {
    return <span aria-hidden="true" className={`material-symbols-outlined ${className}`.trim()}>{icon}</span>;
}

function BotanicalArtwork({ tone = "light" }: { tone?: "light" | "warm" }) {
    return (
        <div className={`landing-botanical ${tone === "warm" ? "landing-botanical-warm" : ""}`.trim()} aria-hidden="true">
            <span className="landing-botanical-stem" />
            <span className="landing-botanical-leaf landing-botanical-leaf-1" />
            <span className="landing-botanical-leaf landing-botanical-leaf-2" />
            <span className="landing-botanical-leaf landing-botanical-leaf-3" />
            <span className="landing-botanical-leaf landing-botanical-leaf-4" />
            <span className="landing-botanical-leaf landing-botanical-leaf-5" />
            <span className="landing-botanical-leaf landing-botanical-leaf-6" />
            <span className="landing-botanical-leaf landing-botanical-leaf-7" />
        </div>
    );
}

export default function Index() {
    const { isAuthenticated } = useOutletContext<LandingContext>();
    const demoHref = isAuthenticated ? "/dashboard" : "/login";
    const demoLabel = isAuthenticated ? "Open Dashboard" : "View Demo";
    const secondaryHref = isAuthenticated ? "/tickets" : "/register";
    const secondaryLabel = isAuthenticated ? "Open Tickets" : "Get Started";
    const navbarSecondaryLink = isAuthenticated
        ? { to: "/tickets", label: "Open Tickets" }
        : { to: "/login", label: "Sign In" };
    const navbarPrimaryLink = isAuthenticated
        ? { to: "/dashboard", label: "Dashboard" }
        : { to: "/register", label: "Register" };
    const currentYear = new Date().getFullYear();
    const [activeNavHref, setActiveNavHref] = useState<(typeof navItems)[number]["href"]>(navItems[0].href);

    useEffect(() => {
        const sectionIds = navItems.map((item) => item.href.replace("#", ""));
        const sections = sectionIds
            .map((id) => document.getElementById(id))
            .filter((section): section is HTMLElement => section !== null);

        if (sections.length === 0) {
            return;
        }

        const updateActiveSection = () => {
            const scrollPosition = window.scrollY + 140;

            let currentSectionId = sections[0].id;

            for (const section of sections) {
                if (section.offsetTop <= scrollPosition) {
                    currentSectionId = section.id;
                }
            }

            setActiveNavHref(`#${currentSectionId}` as (typeof navItems)[number]["href"]);
        };

        updateActiveSection();
        window.addEventListener("scroll", updateActiveSection, { passive: true });
        window.addEventListener("hashchange", updateActiveSection);

        return () => {
            window.removeEventListener("scroll", updateActiveSection);
            window.removeEventListener("hashchange", updateActiveSection);
        };
    }, []);

    const handleSectionNavClick = (event: MouseEvent<HTMLAnchorElement>, href: string) => {
        const sectionId = href.replace("#", "");
        const section = document.getElementById(sectionId);

        if (!section) {
            return;
        }

        event.preventDefault();
        setActiveNavHref(href as (typeof navItems)[number]["href"]);
        section.scrollIntoView({ behavior: "smooth", block: "start" });
        window.history.replaceState(null, "", href);
    };

    return (
        <div className="landing-page min-h-screen bg-[var(--landing-surface)] text-[var(--landing-on-surface)] selection:bg-[color:var(--landing-primary)]/30">
            <header className="fixed inset-x-0 top-0 z-50 border-b border-white/5 bg-[color:var(--landing-surface)]/80 backdrop-blur-xl">
                <nav className="mx-auto flex w-full max-w-7xl items-center justify-between gap-6 px-6 py-4 sm:px-8">
                    <Link className={`${headlineClass} text-2xl font-black tracking-[-0.08em] text-[var(--landing-on-surface)]`} to="/">
                        Zap
                    </Link>

                    <div className="hidden items-center gap-8 md:flex">
                        {navItems.map((item) => (
                            <a
                                key={item.href}
                                className={`border-b pb-1 text-sm transition-colors duration-200 hover:text-[var(--landing-on-surface)] ${activeNavHref === item.href ? "border-[var(--landing-primary)] font-bold text-[var(--landing-primary)]" : "border-transparent text-[var(--landing-on-surface-variant)]"}`}
                                href={item.href}
                                onClick={(event) => handleSectionNavClick(event, item.href)}
                            >
                                {item.label}
                            </a>
                        ))}
                    </div>

                    <div className="flex items-center gap-3">
                        <Link className={`${buttonBaseClass} ${navButtonSizeClass} ${secondaryButtonClass}`} to={navbarSecondaryLink.to}>
                            {navbarSecondaryLink.label}
                        </Link>
                        <Link className={`${buttonBaseClass} ${navButtonSizeClass} ${primaryButtonClass}`} to={navbarPrimaryLink.to}>
                            {navbarPrimaryLink.label}
                        </Link>
                    </div>
                </nav>
            </header>

            <main className="pt-28 sm:pt-32">
                <section className="mx-auto mb-24 max-w-7xl px-6 sm:px-8 lg:mb-32">
                    <div className="grid items-center gap-14 lg:grid-cols-12 lg:gap-12">
                        <div className="space-y-8 lg:col-span-6 lg:pr-4">
                            <div className="space-y-6">
                                <h1 className={`${headlineClass} max-w-xl text-5xl font-extrabold leading-[0.95] tracking-[-0.04em] text-[var(--landing-on-surface)] sm:text-6xl lg:text-7xl`}>
                                    A modern issue tracker built for developers
                                </h1>
                                <p className="max-w-lg text-lg leading-8 text-[var(--landing-on-surface-variant)] sm:text-xl">
                                    Manage projects, track bugs, and collaborate without unnecessary complexity. Zap stays fast, readable, and permission-aware from first ticket to final resolution.
                                </p>
                            </div>

                            <div className="flex flex-wrap gap-4 pt-2">
                                <Link className={`${buttonBaseClass} ${primaryButtonClass} ${heroButtonSizeClass}`} to={demoHref}>
                                    {demoLabel}
                                </Link>
                                <Link className={`${buttonBaseClass} ${secondaryButtonClass} ${heroButtonSizeClass}`} to={secondaryHref}>
                                    {secondaryLabel}
                                </Link>
                            </div>
                        </div>

                        <div className="lg:col-span-6">
                            <div className="relative overflow-hidden rounded-[1.25rem] bg-[linear-gradient(160deg,rgba(204,211,184,0.98)_0%,rgba(188,200,165,0.96)_52%,rgba(220,220,206,0.9)_100%)] p-6 shadow-[0_32px_80px_rgba(229,226,225,0.08)] sm:p-8">
                                <div className="absolute inset-0 bg-[linear-gradient(180deg,rgba(255,255,255,0.15),rgba(255,255,255,0))]" />
                                <div className="absolute inset-[12%_18%] rounded-full bg-[rgba(255,255,255,0.24)] blur-[40px]" />
                                <div className="relative z-[1] mx-auto max-w-xs rounded-[1.35rem] bg-[#f6f2ea] p-5 text-[#211f1f] shadow-[0_20px_50px_rgba(0,0,0,0.22)] sm:max-w-sm sm:p-6">
                                    <div className="mb-5 flex items-center justify-between text-[11px] font-medium uppercase tracking-[0.18em] text-[#767169]">
                                        <span className={`${headlineClass} text-base font-extrabold tracking-[-0.05em] text-[#201d1d]`}>zap</span>
                                        <span>preview</span>
                                    </div>

                                    <div className="rounded-[1.15rem] bg-white px-4 py-5 shadow-[0_12px_30px_rgba(31,28,28,0.12)]">
                                        <div className="mb-4 flex items-center justify-between">
                                            <p className="text-xs uppercase tracking-[0.18em] text-[#8a857d]">Product surface</p>
                                            <span className="h-2 w-2 rounded-full bg-[#b6c48d]" />
                                        </div>

                                        <div className="flex min-h-[15rem] items-center justify-center rounded-[1rem] bg-[#f8f5ef]">
                                            <BotanicalArtwork />
                                        </div>
                                    </div>

                                    <div className="mt-5 flex items-center justify-between rounded-[1.05rem] bg-[#221f1f] px-4 py-3 text-white shadow-[0_12px_28px_rgba(17,13,13,0.2)]">
                                        <div>
                                            <p className="text-[11px] uppercase tracking-[0.16em] text-white/60">Workflow</p>
                                            <p className="mt-1 text-sm font-medium text-white/90">Focused, calm, and fast</p>
                                        </div>
                                        <div className="rounded-full bg-[#c0c1ff] px-3 py-1 text-[11px] font-semibold uppercase tracking-[0.16em] text-[#1000a9]">
                                            Active
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </section>

                <section className="mx-auto max-w-7xl scroll-mt-28 px-6 py-10 sm:scroll-mt-32 sm:px-8 sm:py-20" id="features">
                    <div className="grid grid-cols-1 gap-5 md:grid-cols-3 md:gap-6">
                        <article className="rounded-3xl bg-[var(--landing-surface-container-low)] p-8 md:col-span-2 md:p-10">
                            <div>
                                <span className={`${headlineClass} text-xs font-bold uppercase tracking-[0.22em] text-[var(--landing-primary)]`}>
                                    {workflowFeature.eyebrow}
                                </span>
                                <h2 className={`${headlineClass} mt-4 text-3xl font-bold tracking-[-0.03em] text-[var(--landing-on-surface)] sm:text-[2rem]`}>
                                    {workflowFeature.title}
                                </h2>
                                <p className="mt-5 max-w-xl text-lg leading-8 text-[var(--landing-on-surface-variant)]">
                                    {workflowFeature.description}
                                </p>
                            </div>

                            <div className="mt-10 rounded-[1.15rem] bg-[var(--landing-surface-container-lowest)] p-4">
                                <div className="space-y-3">
                                    {workflowFeature.tickets.map((ticket) => (
                                        <div
                                            key={ticket.id}
                                            className={`flex items-center gap-3 rounded-2xl px-4 py-3 ${ticket.active ? "bg-white/8" : "bg-white/4 opacity-65"}`}
                                        >
                                            <span className={`h-2 w-2 rounded-full ${ticket.active ? "bg-[var(--landing-primary)]" : "bg-[var(--landing-on-surface-variant)]"}`} />
                                            <div className="min-w-0 flex-1">
                                                <p className="truncate font-mono text-sm text-[var(--landing-on-surface-variant)]">
                                                    {ticket.id}: {ticket.label}
                                                </p>
                                            </div>
                                            <span className={`rounded-full px-2.5 py-1 text-[11px] font-semibold uppercase tracking-[0.14em] ${ticket.active ? "bg-[var(--landing-tertiary-fixed)]/14 text-[var(--landing-tertiary-fixed)]" : "bg-white/8 text-[var(--landing-on-surface-variant)]"}`}>
                                                {ticket.status}
                                            </span>
                                        </div>
                                    ))}
                                </div>
                            </div>
                        </article>

                        {featureCards.map((feature) => (
                            <article key={feature.title} className="rounded-3xl bg-[var(--landing-surface-container-high)] p-7 md:p-8">
                                <div className="flex h-12 w-12 items-center justify-center rounded-full bg-[var(--landing-primary)]/10 text-[var(--landing-primary)]">
                                    <MaterialIcon className="text-[22px]" icon={feature.icon} />
                                </div>
                                <h3 className={`${headlineClass} mt-5 text-xl font-bold tracking-[-0.02em] text-[var(--landing-on-surface)]`}>
                                    {feature.title}
                                </h3>
                                <p className="mt-3 text-sm leading-7 text-[var(--landing-on-surface-variant)]">
                                    {feature.description}
                                </p>
                            </article>
                        ))}
                    </div>
                </section>

                <section className="mt-14 scroll-mt-28 bg-[var(--landing-surface-container-lowest)] py-24 sm:scroll-mt-32 sm:py-32" id="product">
                    <div className="mx-auto max-w-7xl px-6 sm:px-8">
                        <div className="mb-16 space-y-4 sm:mb-20">
                            <h2 className={`${headlineClass} text-4xl font-extrabold tracking-[-0.04em] text-[var(--landing-on-surface)] sm:text-5xl`}>
                                Clarity in every view.
                            </h2>
                            <p className="max-w-2xl text-lg leading-8 text-[var(--landing-on-surface-variant)]">
                                Designed for functional engineering over visual clutter, with the same server-first access model that powers the rest of the product.
                            </p>
                        </div>

                        <div className="space-y-20 sm:space-y-28">
                            <div className="grid items-center gap-12 lg:grid-cols-2 lg:gap-20">
                                <div className={`flex min-h-[11rem] items-center ${previewCardClass}`}>
                                    <div className="w-full rounded-[1.25rem] bg-[var(--landing-surface)] p-5 shadow-[0_18px_50px_rgba(0,0,0,0.16)]">
                                        <div className="mb-4 flex items-center gap-2 text-[11px] uppercase tracking-[0.18em] text-[var(--landing-on-surface-variant)]">
                                            <span className="h-2 w-2 rounded-full bg-[var(--landing-primary)]" />
                                            Dashboard View
                                        </div>
                                        <div className="space-y-4">
                                            <div className="h-3 w-32 rounded-full bg-white/10" />
                                            <div className="grid grid-cols-3 gap-3">
                                                {[
                                                    "100%",
                                                    "80%",
                                                    "60%",
                                                ].map((width, index) => (
                                                    <div key={index} className="rounded-2xl bg-white/5 p-3">
                                                        <div className="h-2 rounded-full bg-white/10" style={{ width }} />
                                                        <div className={`mt-3 h-10 rounded-2xl ${kineticGradientClass} opacity-65`} />
                                                    </div>
                                                ))}
                                            </div>
                                            <div className="h-3 rounded-full bg-white/8">
                                                <div className={`h-3 w-3/4 rounded-full ${kineticGradientClass}`} />
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <div className="space-y-6">
                                    <h3 className={`${headlineClass} text-2xl font-bold tracking-[-0.03em] text-[var(--landing-on-surface)] sm:text-3xl`}>
                                        Analytics Dashboard
                                    </h3>
                                    <p className="text-lg leading-8 text-[var(--landing-on-surface-variant)]">
                                        Get a bird&apos;s-eye view of team velocity, project health, and ticket distribution without losing the detail needed to act.
                                    </p>
                                </div>
                            </div>

                            <div className="grid items-center gap-12 lg:grid-cols-2 lg:gap-20">
                                <div className="space-y-6">
                                    <h3 className={`${headlineClass} text-2xl font-bold tracking-[-0.03em] text-[var(--landing-on-surface)] sm:text-3xl`}>
                                        Contextual Detail View
                                    </h3>
                                    <p className="text-lg leading-8 text-[var(--landing-on-surface-variant)]">
                                        Everything needed to resolve an issue lives in one focused workspace: description, comments, assignments, and attachments, separated by tonal depth instead of noise.
                                    </p>
                                </div>

                                <div className={previewCardClass}>
                                    <div className="rounded-[1.25rem] bg-[#f3e8d8] p-6 text-[#231e1e] shadow-[0_18px_50px_rgba(0,0,0,0.16)] sm:p-7">
                                        <div className="mb-4 flex items-center gap-2 text-[11px] uppercase tracking-[0.18em] text-[#8a857d]">
                                            <span className="h-2 w-2 rounded-full bg-[#8da567]" />
                                            Detail View
                                        </div>
                                        <div className="flex min-h-[18rem] items-center justify-center rounded-[1.2rem] bg-[#f7efe3] shadow-[inset_0_1px_0_rgba(255,255,255,0.55)] sm:min-h-[22rem]">
                                            <BotanicalArtwork tone="warm" />
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </section>

                <section className="mx-auto max-w-7xl scroll-mt-28 px-6 py-24 sm:scroll-mt-32 sm:px-8 sm:py-32" id="tech">
                    <div className="mb-14 text-center sm:mb-16">
                        <h2 className={`${headlineClass} text-3xl font-bold tracking-[-0.03em] text-[var(--landing-on-surface)] sm:text-4xl`}>
                            The Stack
                        </h2>
                        <p className="mt-4 text-[var(--landing-on-surface-variant)]">Built with performance and scalability in mind.</p>
                    </div>

                    <div className="grid grid-cols-2 gap-4 md:grid-cols-3 lg:grid-cols-6">
                        {stackItems.map((item) => (
                            <article key={item.label} className="rounded-3xl bg-[var(--landing-surface-container-low)] p-5 text-center transition-colors duration-200 hover:bg-[var(--landing-surface-container-high)] sm:p-6">
                                <div className="mb-3 flex justify-center text-[var(--landing-primary)]">
                                    {"monogram" in item ? (
                                        <span className={`${headlineClass} text-[1.15rem] font-extrabold tracking-[-0.08em] text-[var(--landing-primary)]`}>
                                            {item.monogram}
                                        </span>
                                    ) : (
                                        <MaterialIcon className="text-[2rem]" icon={item.icon} />
                                    )}
                                </div>
                                <p className={`${headlineClass} text-sm font-semibold text-[var(--landing-on-surface)]`}>{item.label}</p>
                            </article>
                        ))}
                    </div>
                </section>

                <section className="mx-auto max-w-4xl scroll-mt-28 px-6 py-6 sm:scroll-mt-32 sm:px-8 sm:py-12" id="why">
                    <div className="relative overflow-hidden rounded-[1.75rem] bg-[color:var(--landing-surface-container-high)]/45 p-8 sm:p-12">
                        <div className="absolute right-0 top-0 p-6 opacity-10 sm:p-8">
                            <MaterialIcon className="text-[5rem] sm:text-[7rem]" icon="psychology" />
                        </div>

                        <div className="relative z-10 max-w-3xl">
                            <h2 className={`${headlineClass} text-3xl font-extrabold tracking-[-0.03em] text-[var(--landing-on-surface)]`}>
                                Why I Built This
                            </h2>
                            <p className="mt-8 text-lg leading-8 text-[var(--landing-on-surface-variant)]">
                                Zap is a portfolio project inspired by tools like Jira and Linear. I built it to demonstrate real-world backend architecture, server-enforced authorization, and full-stack product thinking beyond a surface-level UI clone.
                            </p>
                            <p className="mt-6 leading-8 text-[var(--landing-on-surface-variant)]">
                                The focus is practical engineering: relational data modeling, secure file handling, granular permissions, and a developer-first interface that still feels premium.
                            </p>

                            <div className="mt-10 flex items-center gap-4">
                                <div className="flex h-10 w-10 items-center justify-center rounded-full bg-[var(--landing-primary)]/12 text-[var(--landing-primary)]">
                                    <MaterialIcon className="text-xl" icon="terminal" />
                                </div>
                                <span className="font-mono text-sm text-[var(--landing-on-surface-variant)]">
                                    System Design &amp; Full-Stack Implementation
                                </span>
                            </div>
                        </div>
                    </div>
                </section>
            </main>

            <footer className="mt-20 bg-[var(--landing-surface-container-lowest)] px-6 py-12 sm:px-8">
                <div className="mx-auto flex max-w-7xl flex-col gap-8 md:flex-row md:items-end md:justify-between">
                    <div className="space-y-2 text-center md:text-left">
                        <p className={`${headlineClass} text-lg font-bold tracking-[-0.03em] text-[var(--landing-on-surface)]`}>Zap</p>
                        <p className="max-w-xs text-sm leading-6 text-[var(--landing-on-surface-variant)]">
                            Built as a real-world full-stack project focused on maintainable architecture and production-minded workflows. &copy; {currentYear} Zap.
                        </p>
                    </div>

                    <div className="flex flex-wrap items-center justify-center gap-6 text-xs font-bold uppercase tracking-[0.18em] text-[var(--landing-on-surface-variant)] md:justify-end">
                        <span>GitHub</span>
                        <span>Documentation</span>
                        <span>Privacy</span>
                    </div>
                </div>
            </footer>
        </div>
    );
}
