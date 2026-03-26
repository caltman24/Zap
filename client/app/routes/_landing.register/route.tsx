import { ActionFunctionArgs, LoaderFunctionArgs, redirect } from "@remix-run/node";
import { Form, Link, useActionData, useNavigation } from "@remix-run/react";
import { useMemo, useState } from "react";
import apiClient from "~/services/api.server/apiClient";
import { getSession } from "~/services/sessions.server";
import setSession from "../_landing.login/setSession";
import tryCatch from "~/utils/tryCatch";
import { TokenResponse } from "~/services/api.server/types";

const registerHighlights = [
    {
        title: "Real account flow",
        description: "Create a normal account and drop straight into the setup path you already built.",
    },
    {
        title: "Strong password rules",
        description: "Client-side validation stays intact so the visual refresh does not weaken the existing constraints.",
    },
] as const;

const headlineClass = "[font-family:Manrope,sans-serif]";
const buttonBaseClass = "inline-flex items-center justify-center rounded-full text-sm font-bold leading-none transition duration-200 hover:-translate-y-px";
const primaryButtonClass = "bg-[linear-gradient(135deg,var(--landing-primary)_0%,var(--landing-primary-container)_100%)] text-[#1000a9] shadow-[0_14px_28px_rgba(128,131,255,0.2)]";
const authCardClass = "relative mx-auto w-full max-w-[32rem] rounded-[1.75rem] border border-[var(--landing-outline-variant)] bg-[rgba(28,27,27,0.92)] p-[1.35rem] shadow-[0_32px_80px_rgba(0,0,0,0.28)] backdrop-blur-[20px] sm:p-8 lg:min-h-[41.5rem] lg:p-[2.1rem]";
const authInputLabelClass = "block text-xs font-semibold uppercase tracking-[0.18em] text-[var(--landing-on-surface-variant)]";
const authInputWrapClass = "flex items-center gap-3 rounded-2xl border border-[var(--landing-outline-variant)] bg-[rgba(14,14,14,0.92)] px-4 py-[0.95rem] transition duration-200 focus-within:border-[rgba(192,193,255,0.42)] focus-within:bg-[rgba(28,27,27,0.96)] focus-within:shadow-[0_0_0_4px_rgba(192,193,255,0.08)]";
const authInputWrapErrorClass = "border-[rgba(255,180,171,0.34)]";
const authInputClass = "w-full border-0 bg-transparent text-[0.95rem] text-[var(--landing-on-surface)] outline-none placeholder:text-[rgba(199,196,215,0.55)]";
const authInputIconClass = "material-symbols-outlined text-lg text-[var(--landing-on-surface-variant)]";
const authToggleClass = "inline-flex items-center justify-center text-[var(--landing-on-surface-variant)] transition-colors duration-200 hover:text-[var(--landing-on-surface)]";
const authHelperClass = "text-[0.8125rem] leading-7 text-[var(--landing-on-surface-variant)]";
const authAlertClass = "rounded-2xl border border-[rgba(255,180,171,0.18)] bg-[rgba(147,0,10,0.18)] px-4 py-3.5 text-sm leading-6 text-[#ffdad6]";

function getActionMessage(message: unknown) {
    if (typeof message === "string") {
        return message;
    }

    if (message && typeof message === "object") {
        const firstValue = Object.values(message)[0];

        if (typeof firstValue === "string") {
            return firstValue;
        }
    }

    return null;
}

export async function action({ request }: ActionFunctionArgs) {
    const formData = await request.formData();
    const firstName = formData.get("firstName");
    const lastName = formData.get("lastName");
    const email = formData.get("email");
    const password = formData.get("password");
    const confirmPassword = formData.get("confirmPassword");

    if (password !== confirmPassword) {
        return Response.json({ message: "Passwords do not match" });
    }

    const { data: res, error } = await tryCatch(
        apiClient.auth.registerAccount({
            firstName: firstName as string,
            lastName: lastName as string,
            email: email as string,
            password: password as string,
        }),
    );

    if (error) {
        return Response.json({ message: "Failed to register. Please try again later." });
    }

    if (res.ok) {
        const tokenResonse: TokenResponse = await res.json();

        return await setSession(request, tokenResonse, "/dashboard");
    }

    if (res.status === 400) {
        return Response.json({ message: await res.json() });
    }

    return Response.json({ message: "Failed to register. Please try again later." });
}

export async function loader({ request }: LoaderFunctionArgs) {
    const session = await getSession(request);

    if (session.get("user")) {
        return redirect("/dashboard");
    }

    return null;
}

export default function RegisterAccount() {
    const navigation = useNavigation();
    const isSubmitting = navigation.state === "submitting";
    const actionData = useActionData<typeof action>();
    const actionMessage = getActionMessage(actionData?.message);
    const [password, setPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");
    const [showPassword, setShowPassword] = useState(false);
    const [showConfirmPassword, setShowConfirmPassword] = useState(false);

    const passwordsMatch = useMemo(() => {
        if (password.length === 0 && confirmPassword.length === 0) {
            return true;
        }

        return password === confirmPassword;
    }, [confirmPassword, password]);

    return (
        <div className="landing-page min-h-screen px-6 pb-14 pt-24 sm:px-8 sm:pb-18 sm:pt-28 lg:pt-32">
            <div className="mx-auto grid max-w-7xl gap-8 lg:grid-cols-[minmax(0,1fr)_30rem] lg:items-center lg:gap-14 xl:grid-cols-[minmax(0,1fr)_31rem]">
                <section className="order-2 space-y-7 lg:order-1 lg:pr-10">
                    <div className="inline-flex items-center gap-2 rounded-full border border-white/10 bg-white/4 px-3 py-1 text-xs font-medium uppercase tracking-[0.18em] text-[var(--landing-on-surface-variant)]">
                        <span className="material-symbols-outlined text-base text-[var(--landing-primary)]">person_add</span>
                        Account Setup
                    </div>

                    <div className="space-y-5">
                        <h1 className={`${headlineClass} max-w-2xl text-4xl font-extrabold tracking-[-0.04em] text-[var(--landing-on-surface)] sm:text-5xl lg:text-6xl`}>
                            Create your account and start building your workspace.
                        </h1>
                        <p className="max-w-xl text-lg leading-8 text-[var(--landing-on-surface-variant)]">
                            Register with the same production-minded auth flow that powers the app, then move directly into company setup and project onboarding.
                        </p>
                    </div>

                    <div className="grid gap-4 sm:grid-cols-2">
                        {registerHighlights.map((highlight) => (
                            <article key={highlight.title} className="rounded-3xl bg-[var(--landing-surface-container-low)] p-6">
                                <h2 className={`${headlineClass} text-lg font-bold tracking-[-0.02em] text-[var(--landing-on-surface)]`}>
                                    {highlight.title}
                                </h2>
                                <p className="mt-3 text-sm leading-7 text-[var(--landing-on-surface-variant)]">
                                    {highlight.description}
                                </p>
                            </article>
                        ))}
                    </div>

                    <div className="rounded-[1.75rem] bg-[color:var(--landing-surface-container-high)]/45 p-6 sm:p-8">
                        <p className="text-xs font-semibold uppercase tracking-[0.18em] text-[var(--landing-primary)]">What happens next</p>
                        <div className="mt-4 space-y-3 text-sm leading-7 text-[var(--landing-on-surface-variant)]">
                            <p>1. Create your account with a strong password.</p>
                            <p>2. Enter the setup flow and attach yourself to a company.</p>
                            <p>3. Start managing projects, tickets, and role-aware workflows.</p>
                        </div>
                    </div>
                </section>

                <section className="relative order-1 w-full lg:order-2">
                    <div className="absolute inset-x-5 top-10 h-40 rounded-full bg-[rgba(192,193,255,0.14)] blur-[64px]" />
                    <div className={authCardClass}>
                        <div className="space-y-3 pb-4">
                            <p className="text-xs font-semibold uppercase tracking-[0.18em] text-[var(--landing-primary)]">New Account</p>
                            <h2 className={`${headlineClass} text-3xl font-extrabold tracking-[-0.03em] text-[var(--landing-on-surface)]`}>
                                Register Account
                            </h2>
                            <p className="text-sm leading-6 text-[var(--landing-on-surface-variant)]">
                                Create a full account, then continue into setup without changing the auth behavior already in place.
                            </p>
                        </div>

                        {actionMessage && <div className={authAlertClass}>{actionMessage}</div>}

                        <Form className="space-y-5" method="post">
                            <fieldset className="space-y-5" disabled={isSubmitting}>
                                <div className="grid gap-5 sm:grid-cols-2">
                                    <label className="block space-y-2">
                                        <span className={authInputLabelClass}>First Name</span>
                                        <span className={authInputWrapClass}>
                                            <span aria-hidden="true" className={authInputIconClass}>badge</span>
                                            <input className={authInputClass} maxLength={50} name="firstName" placeholder="John" required type="text" />
                                        </span>
                                    </label>

                                    <label className="block space-y-2">
                                        <span className={authInputLabelClass}>Last Name</span>
                                        <span className={authInputWrapClass}>
                                            <span aria-hidden="true" className={authInputIconClass}>badge</span>
                                            <input className={authInputClass} maxLength={50} name="lastName" placeholder="Doe" required type="text" />
                                        </span>
                                    </label>
                                </div>

                                <label className="block space-y-2">
                                    <span className={authInputLabelClass}>Email</span>
                                    <span className={authInputWrapClass}>
                                        <span aria-hidden="true" className={authInputIconClass}>mail</span>
                                        <input className={authInputClass} maxLength={75} name="email" placeholder="mail@site.com" required type="email" />
                                    </span>
                                </label>

                                <label className="block space-y-2">
                                    <span className={authInputLabelClass}>Password</span>
                                    <span className={authInputWrapClass}>
                                        <span aria-hidden="true" className={authInputIconClass}>key</span>
                                        <input
                                            className={authInputClass}
                                            maxLength={50}
                                            minLength={6}
                                            name="password"
                                            onChange={(event) => setPassword(event.target.value)}
                                            pattern="(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z0-9]).{6,}"
                                            placeholder="Password"
                                            required
                                            title="Must be more than 6 characters, including number, lowercase letter, uppercase letter, and non alphanumeric character"
                                            type={showPassword ? "text" : "password"}
                                            value={password}
                                        />
                                        <button
                                            className={authToggleClass}
                                            onClick={() => setShowPassword(!showPassword)}
                                            type="button"
                                        >
                                            <span aria-hidden="true" className="material-symbols-outlined text-lg">
                                                {showPassword ? "visibility_off" : "visibility"}
                                            </span>
                                        </button>
                                    </span>
                                </label>

                                <p className={authHelperClass}>
                                    Use at least one number, one lowercase letter, one uppercase letter, and one non-alphanumeric character.
                                </p>

                                <label className="block space-y-2">
                                    <span className={authInputLabelClass}>Confirm Password</span>
                                    <span className={`${authInputWrapClass} ${passwordsMatch ? "" : authInputWrapErrorClass}`.trim()}>
                                        <span aria-hidden="true" className={authInputIconClass}>verified_user</span>
                                        <input
                                            className={authInputClass}
                                            maxLength={50}
                                            minLength={6}
                                            name="confirmPassword"
                                            onChange={(event) => setConfirmPassword(event.target.value)}
                                            placeholder="Confirm Password"
                                            required
                                            type={showConfirmPassword ? "text" : "password"}
                                            value={confirmPassword}
                                        />
                                        <button
                                            className={authToggleClass}
                                            onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                                            type="button"
                                        >
                                            <span aria-hidden="true" className="material-symbols-outlined text-lg">
                                                {showConfirmPassword ? "visibility_off" : "visibility"}
                                            </span>
                                        </button>
                                    </span>
                                </label>

                                {!passwordsMatch && (
                                    <p className="text-sm text-[#ffdad6]">
                                        Passwords must match before you can continue.
                                    </p>
                                )}

                                <button className={`${buttonBaseClass} ${primaryButtonClass} min-h-[3.25rem] w-full px-5 text-base disabled:opacity-70 sm:min-h-14`} type="submit">
                                    {isSubmitting ? (
                                        <span className="flex items-center gap-3">
                                            <span className="inline-flex h-4 w-4 animate-spin rounded-full border-2 border-current border-r-transparent" />
                                            Creating Account
                                        </span>
                                    ) : (
                                        "Register"
                                    )}
                                </button>
                            </fieldset>
                        </Form>

                        <p className="text-sm text-[var(--landing-on-surface-variant)]">
                            Already have an account?{" "}
                            <Link className="font-semibold text-[var(--landing-primary)] transition-colors duration-200 hover:text-[var(--landing-on-surface)]" to="/login">
                                Sign in here
                            </Link>
                        </p>
                    </div>
                </section>
            </div>
        </div>
    );
}
