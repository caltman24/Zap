import {ActionFunctionArgs, LoaderFunctionArgs, redirect} from "@remix-run/node";
import {Form, Link, useActionData, useNavigation} from "@remix-run/react";
import {useEffect, useRef} from "react";
import DemoUserLoginHandler from "./_handlers/DemoUserLoginHandler";
import PwdLoginHandler from "./_handlers/PwdLoginHandler";
import TestUserLoginHandler from "./_handlers/TestUserLoginHandler";
import {getSession} from "~/services/sessions.server";

const demoActions = [
    {intent: "demo:admin", label: "Demo as Admin"},
    {intent: "demo:pm", label: "Demo as Project Manager"},
    {intent: "demo:dev", label: "Demo as Developer"},
    {intent: "demo:submitter", label: "Demo as Submitter"},
] as const;

const authHighlights = [
    {
        title: "Server-first access",
        description: "Project and ticket visibility is still enforced on the backend after login.",
    },
    {
        title: "Fast demo paths",
        description: "Jump into seeded roles without changing the real auth flows you already built.",
    },
] as const;

const headlineClass = "[font-family:Manrope,sans-serif]";
const buttonBaseClass = "inline-flex items-center justify-center rounded-full text-sm font-bold leading-none transition duration-200 hover:-translate-y-px";
const primaryButtonClass = "bg-[linear-gradient(135deg,var(--landing-primary)_0%,var(--landing-primary-container)_100%)] text-[#1000a9] shadow-[0_14px_28px_rgba(128,131,255,0.2)]";
const authCardClass = "relative mx-auto w-full max-w-[32rem] rounded-[1.75rem] border border-[var(--landing-outline-variant)] bg-[rgba(28,27,27,0.92)] p-[1.35rem] shadow-[0_32px_80px_rgba(0,0,0,0.28)] backdrop-blur-[20px] sm:p-8 lg:min-h-[41.5rem] lg:p-[2.1rem]";
const authInputLabelClass = "block text-xs font-semibold uppercase tracking-[0.18em] text-[var(--landing-on-surface-variant)]";
const authInputWrapClass = "flex items-center gap-3 rounded-2xl border border-[var(--landing-outline-variant)] bg-[rgba(14,14,14,0.92)] px-4 py-[0.95rem] transition duration-200 focus-within:border-[rgba(192,193,255,0.42)] focus-within:bg-[rgba(28,27,27,0.96)] focus-within:shadow-[0_0_0_4px_rgba(192,193,255,0.08)]";
const authInputClass = "w-full border-0 bg-transparent text-[0.95rem] text-[var(--landing-on-surface)] outline-none placeholder:text-[rgba(199,196,215,0.55)]";
const authInputIconClass = "material-symbols-outlined text-lg text-[var(--landing-on-surface-variant)]";
const authAlertClass = "rounded-2xl border border-[rgba(255,180,171,0.18)] bg-[rgba(147,0,10,0.18)] px-4 py-3.5 text-sm leading-6 text-[#ffdad6]";
const authChoiceClass = "inline-flex min-h-[3.25rem] w-full cursor-pointer items-center justify-center rounded-full border border-[var(--landing-outline-variant)] bg-[rgba(53,53,52,0.42)] px-4 py-3.5 text-center text-sm font-semibold text-[var(--landing-on-surface)] transition duration-200 hover:-translate-y-px hover:border-[rgba(192,193,255,0.28)] hover:bg-[rgba(53,53,52,0.62)] disabled:opacity-70 sm:w-auto backdrop-blur-xl";

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

export async function action({request}: ActionFunctionArgs) {
    const formData = await request.formData();
    const intent = formData.get("intent");

    if (intent === "pwd") {
        return await PwdLoginHandler(request, formData);
    }

    if (intent === "test:user") {
        return await TestUserLoginHandler(request);
    }

    if (intent?.toString().startsWith("demo:")) {
        const roleMap = {
            "demo:admin": "admin",
            "demo:pm": "projectManager",
            "demo:dev": "developer",
            "demo:submitter": "submitter",
        } as const;

        const role = roleMap[intent.toString() as keyof typeof roleMap];

        if (!role) {
            return Response.json({message: "Invalid demo role selected."}, {status: 400});
        }

        return await DemoUserLoginHandler(request, role);
    }
}

export async function loader({request}: LoaderFunctionArgs) {
    const session = await getSession(request);

    if (session.get("user")) {
        return redirect("/setup");
    }

    return null;
}

export default function Login() {
    const actionData = useActionData<typeof action>();
    const navigation = useNavigation();
    const isSubmitting = navigation.state === "submitting";
    const currentIntent = navigation.formData?.get("intent")?.toString();
    const actionMessage = getActionMessage(actionData?.message);
    const formRef = useRef<HTMLFormElement | null>(null);

    useEffect(() => {
        formRef.current?.reset();
    }, [actionData]);

    return (
        <div className="landing-page min-h-screen px-6 pb-14 pt-24 sm:px-8 sm:pb-18 sm:pt-28 lg:pt-32">
            <div
                className="mx-auto grid max-w-7xl gap-8 lg:grid-cols-[minmax(0,1fr)_29rem] lg:items-center lg:gap-14 xl:grid-cols-[minmax(0,1fr)_30rem]">
                <section className="order-2 space-y-7 lg:order-1 lg:pr-10">
                    <div
                        className="inline-flex items-center gap-2 rounded-full border border-white/10 bg-white/4 px-3 py-1 text-xs font-medium uppercase tracking-[0.18em] text-[var(--landing-on-surface-variant)]">
                        <span className="material-symbols-outlined text-base text-[var(--landing-primary)]">lock</span>
                        Secure Access
                    </div>

                    <div className="space-y-5">
                        <h1 className={`${headlineClass} max-w-2xl text-4xl font-extrabold tracking-[-0.04em] text-[var(--landing-on-surface)] sm:text-5xl lg:text-6xl`}>
                            Sign in and pick up where your team left off.
                        </h1>
                        <p className="max-w-xl text-lg leading-8 text-[var(--landing-on-surface-variant)]">
                            Use your account credentials to access projects, tickets, and comments with the same
                            server-first permission model that powers the rest of Zap.
                        </p>
                    </div>

                    <div className="grid gap-4 sm:grid-cols-2">
                        {authHighlights.map((highlight) => (
                            <article key={highlight.title}
                                     className="rounded-3xl bg-[var(--landing-surface-container-low)] p-6">
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
                        <p className="text-xs font-semibold uppercase tracking-[0.18em] text-[var(--landing-primary)]">Guided
                            Access</p>
                        <p className="mt-3 max-w-xl leading-8 text-[var(--landing-on-surface-variant)]">
                            Need a quick walkthrough? The demo roles and test account stay available below the main
                            form, so you can validate the product surface without changing the real sign-in flow.
                        </p>
                    </div>
                </section>

                <section className="relative order-1 w-full lg:order-2">
                    <div
                        className="absolute inset-x-5 top-10 h-40 rounded-full bg-[rgba(192,193,255,0.14)] blur-[64px]"/>
                    <div className={authCardClass}>
                        <div className="space-y-3 pb-4">
                            <p className="text-xs font-semibold uppercase tracking-[0.18em] text-[var(--landing-primary)]">Welcome
                                Back</p>
                            <h2 className={`${headlineClass} text-3xl font-extrabold tracking-[-0.03em] text-[var(--landing-on-surface)]`}>
                                Sign into Zap
                            </h2>
                            <p className="text-sm leading-6 text-[var(--landing-on-surface-variant)]">
                                Access your tickets, project views, and workflow history with one focused login surface.
                            </p>
                        </div>

                        {actionMessage && <div className={authAlertClass}>{actionMessage}</div>}

                        <Form className="space-y-5" method="post" ref={formRef}>
                            <fieldset className="space-y-5" disabled={isSubmitting}>
                                <label className="block space-y-2">
                                    <span className={authInputLabelClass}>Email</span>
                                    <span className={authInputWrapClass}>
                                        <span aria-hidden="true" className={authInputIconClass}>mail</span>
                                        <input className={authInputClass} name="email" placeholder="mail@site.com"
                                               required type="email"/>
                                    </span>
                                </label>

                                <label className="block space-y-2">
                                    <span className={authInputLabelClass}>Password</span>
                                    <span className={authInputWrapClass}>
                                        <span aria-hidden="true" className={authInputIconClass}>key</span>
                                        <input className={authInputClass} minLength={6} name="password"
                                               placeholder="Password" required type="password"/>
                                    </span>
                                </label>

                                <button
                                    className={`${buttonBaseClass} ${primaryButtonClass} min-h-[3.25rem] w-full px-5 text-base disabled:opacity-70 sm:min-h-14`}
                                    name="intent" type="submit" value="pwd">
                                    {isSubmitting && currentIntent === "pwd" ? (
                                        <span className="flex items-center gap-3">
                                            <span
                                                className="inline-flex h-4 w-4 animate-spin rounded-full border-2 border-current border-r-transparent"/>
                                            Signing In
                                        </span>
                                    ) : (
                                        "Sign In"
                                    )}
                                </button>
                            </fieldset>
                        </Form>

                        <div className="space-y-4">
                            <div className="flex items-center gap-4">
                                <div className="h-px flex-1 bg-white/8"/>
                                <span
                                    className="text-center text-[11px] mt-4 font-medium uppercase tracking-[0.18em] text-[var(--landing-on-surface-variant)]">or continue with a guided account</span>
                                <div className="h-px flex-1 bg-white/8"/>
                            </div>

                            <Form className="grid gap-3 sm:grid-cols-2" method="post">
                                {demoActions.map((demoAction) => (
                                    <button
                                        key={demoAction.intent}
                                        className={authChoiceClass}
                                        disabled={isSubmitting}
                                        name="intent"
                                        type="submit"
                                        value={demoAction.intent}
                                    >
                                        {isSubmitting && currentIntent === demoAction.intent ? "Opening..." : demoAction.label}
                                    </button>
                                ))}

                                <button
                                    className={`${authChoiceClass} sm:col-span-2`}
                                    disabled={isSubmitting}
                                    name="intent"
                                    type="submit"
                                    value="test:user"
                                >
                                    {isSubmitting && currentIntent === "test:user" ? "Opening test account..." : "Test User"}
                                </button>
                            </Form>
                        </div>

                        <p className="text-sm text-[var(--landing-on-surface-variant)]">
                            Need an account?{" "}
                            <Link
                                className="font-semibold text-[var(--landing-primary)] transition-colors duration-200 hover:text-[var(--landing-on-surface)]"
                                to="/register">
                                Register here
                            </Link>
                        </p>
                    </div>
                </section>
            </div>
        </div>
    );
}
