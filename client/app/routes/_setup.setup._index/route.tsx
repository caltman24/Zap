import { Link, useOutletContext } from "@remix-run/react";

const choiceCardClassName =
  "group relative block overflow-hidden rounded-[1.75rem] bg-[var(--app-surface-container-low)] p-6 text-left outline outline-1 outline-[var(--app-outline-variant-soft)] transition-all duration-200 hover:-translate-y-1 hover:outline-[var(--app-primary-fixed)]/30";

export default function SetupRoute() {
  const contextData = useOutletContext() as { firstName: string };

  return (
    <div className="app-shell min-h-screen overflow-hidden">
      <div className="mx-auto flex min-h-screen w-full max-w-6xl items-center px-6 py-12 sm:px-10">
        <div className="relative w-full overflow-hidden rounded-[2rem] bg-[radial-gradient(circle_at_top,rgba(192,193,255,0.14),transparent_32%),linear-gradient(180deg,rgba(255,255,255,0.02),rgba(255,255,255,0)),var(--app-surface)] p-8 outline outline-1 outline-[var(--app-outline-variant-soft)] sm:p-12">
          <div className="absolute inset-x-0 top-0 h-px bg-[linear-gradient(90deg,transparent,rgba(192,193,255,0.45),transparent)]" />

          <div className="grid gap-10 lg:grid-cols-[minmax(0,1.1fr)_minmax(0,0.9fr)] lg:items-center">
            <div className="max-w-2xl">
              <p className="app-shell-mono text-xs uppercase tracking-[0.28em] text-[var(--app-outline)]">Company Setup</p>
              <h1 className="mt-4 text-4xl font-bold tracking-[-0.04em] text-[var(--app-on-surface)] sm:text-5xl">
                Welcome, <span className="text-[var(--app-primary)]">{contextData.firstName}</span>.
              </h1>
              <p className="mt-4 max-w-xl text-base leading-7 text-[var(--app-on-surface-variant)] sm:text-lg">
                Attach your account to a company before you enter the workspace. You can join an existing team or create the first company space for your organization.
              </p>

              <div className="mt-8 flex flex-wrap items-center gap-3 text-sm text-[var(--app-on-surface-variant)]">
                <span className="inline-flex items-center gap-2 rounded-full bg-[var(--app-surface-container-high)] px-3 py-1.5 outline outline-1 outline-[var(--app-outline-variant-soft)]">
                  <span className="material-symbols-outlined text-base text-[var(--app-primary)]">workspace_premium</span>
                  Secure account already active
                </span>
                <span className="inline-flex items-center gap-2 rounded-full bg-[var(--app-surface-container-high)] px-3 py-1.5 outline outline-1 outline-[var(--app-outline-variant-soft)]">
                  <span className="material-symbols-outlined text-base text-[var(--app-tertiary)]">bolt</span>
                  Continue in under a minute
                </span>
              </div>
            </div>

            <div className="grid gap-4">
              <div className={choiceCardClassName}>
                <div className="absolute right-5 top-5 rounded-full bg-[var(--app-surface-container-high)] p-3 text-[var(--app-outline)] outline outline-1 outline-[var(--app-outline-variant-soft)]">
                  <span className="material-symbols-outlined text-xl">mail</span>
                </div>
                <p className="app-shell-mono text-[10px] uppercase tracking-[0.24em] text-[var(--app-outline)]">Join Existing Company</p>
                <h2 className="mt-4 text-2xl font-bold tracking-[-0.03em] text-[var(--app-on-surface)]">Use an invite link</h2>
                <p className="mt-3 max-w-md text-sm leading-6 text-[var(--app-on-surface-variant)]">
                  Accept an invite from your team lead or admin and drop straight into the company workspace.
                </p>
                <div className="mt-6 inline-flex items-center gap-2 rounded-full bg-[var(--app-surface-container-high)] px-3 py-1.5 text-xs font-semibold uppercase tracking-[0.18em] text-[var(--app-outline)]">
                  Coming Soon
                </div>
              </div>

              <Link className={`${choiceCardClassName} cursor-pointer`} to="/setup/company">
                <div className="absolute right-5 top-5 rounded-full bg-[var(--app-primary-fixed)]/15 p-3 text-[var(--app-primary)] outline outline-1 outline-[var(--app-primary-fixed)]/15 transition-transform duration-200 group-hover:scale-105">
                  <span className="material-symbols-outlined text-xl">domain_add</span>
                </div>
                <p className="app-shell-mono text-[10px] uppercase tracking-[0.24em] text-[var(--app-primary)]">Create Company</p>
                <h2 className="mt-4 text-2xl font-bold tracking-[-0.03em] text-[var(--app-on-surface)]">Register a new workspace</h2>
                <p className="mt-3 max-w-md text-sm leading-6 text-[var(--app-on-surface-variant)]">
                  Set up the first company profile, invite your team later, and start organizing projects right away.
                </p>
                <div className="mt-6 inline-flex items-center gap-2 text-sm font-semibold text-[var(--app-primary)] transition-transform duration-200 group-hover:translate-x-1">
                  Continue
                  <span className="material-symbols-outlined text-base">arrow_forward</span>
                </div>
              </Link>
            </div>
          </div>

          <div className="mt-10 border-t border-[var(--app-outline-variant)]/10 pt-6">
            <Link
              className="inline-flex items-center gap-2 rounded-xl px-4 py-2.5 text-sm font-medium text-[var(--app-on-surface-variant)] outline outline-1 outline-[var(--app-outline-variant-soft)] transition-colors hover:bg-[var(--app-hover-overlay)] hover:text-[var(--app-on-surface)]"
              to="/logout"
            >
              <span className="material-symbols-outlined text-base">logout</span>
              Logout
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}
