import { ActionFunctionArgs, LoaderFunctionArgs, redirect } from "@remix-run/node";
import { Form, Link, useActionData, useNavigation } from "@remix-run/react";
import { useEffect, useRef } from "react";
import DemoUserLoginHandler from "./_handlers/DemoUserLoginHandler";
import PwdLoginHandler from "./_handlers/PwdLoginHandler";
import TestUserLoginHandler from "./_handlers/TestUserLoginHandler";
import { getSession } from "~/services/sessions.server";

const demoActions = [
    { intent: "demo:admin", label: "Demo as Admin" },
    { intent: "demo:pm", label: "Demo as Project Manager" },
    { intent: "demo:dev", label: "Demo as Developer" },
    { intent: "demo:submitter", label: "Demo as Submitter" },
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
            return Response.json({ message: "Invalid demo role selected." }, { status: 400 });
        }

        return await DemoUserLoginHandler(request, role);
    }
}

export async function loader({ request }: LoaderFunctionArgs) {
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
            <div className="mx-auto grid max-w-7xl gap-8 lg:grid-cols-[minmax(0,1fr)_29rem] lg:items-center lg:gap-14 xl:grid-cols-[minmax(0,1fr)_30rem]">
                <section className="order-2 space-y-7 lg:order-1 lg:pr-10">
                    <div className="inline-flex items-center gap-2 rounded-full border border-white/10 bg-white/4 px-3 py-1 text-xs font-medium uppercase tracking-[0.18em] text-[var(--landing-on-surface-variant)]">
                        <span className="material-symbols-outlined text-base text-[var(--landing-primary)]">lock</span>
                        Secure Access
                    </div>

                    <div className="space-y-5">
                        <h1 className="landing-headline max-w-2xl text-4xl font-extrabold tracking-[-0.04em] text-[var(--landing-on-surface)] sm:text-5xl lg:text-6xl">
                            Sign in and pick up where your team left off.
                        </h1>
                        <p className="max-w-xl text-lg leading-8 text-[var(--landing-on-surface-variant)]">
                            Use your account credentials to access projects, tickets, and comments with the same server-first permission model that powers the rest of Zap.
                        </p>
                    </div>

                    <div className="grid gap-4 sm:grid-cols-2">
                        {authHighlights.map((highlight) => (
                            <article key={highlight.title} className="rounded-3xl bg-[var(--landing-surface-container-low)] p-6">
                                <h2 className="landing-headline text-lg font-bold tracking-[-0.02em] text-[var(--landing-on-surface)]">
                                    {highlight.title}
                                </h2>
                                <p className="mt-3 text-sm leading-7 text-[var(--landing-on-surface-variant)]">
                                    {highlight.description}
                                </p>
                            </article>
                        ))}
                    </div>

                    <div className="rounded-[1.75rem] bg-[color:var(--landing-surface-container-high)]/45 p-6 sm:p-8">
                        <p className="text-xs font-semibold uppercase tracking-[0.18em] text-[var(--landing-primary)]">Guided Access</p>
                        <p className="mt-3 max-w-xl leading-8 text-[var(--landing-on-surface-variant)]">
                            Need a quick walkthrough? The demo roles and test account stay available below the main form, so you can validate the product surface without changing the real sign-in flow.
                        </p>
                    </div>
                </section>

                <section className="landing-auth-panel order-1 lg:order-2">
                    <div className="landing-auth-card">
                        <div className="space-y-3 pb-4">
                            <p className="text-xs font-semibold uppercase tracking-[0.18em] text-[var(--landing-primary)]">Welcome Back</p>
                            <h2 className="landing-headline text-3xl font-extrabold tracking-[-0.03em] text-[var(--landing-on-surface)]">
                                Sign into Zap
                            </h2>
                            <p className="text-sm leading-6 text-[var(--landing-on-surface-variant)]">
                                Access your tickets, project views, and workflow history with one focused login surface.
                            </p>
                        </div>

                        {actionMessage && <div className="landing-auth-alert">{actionMessage}</div>}

                        <Form className="space-y-5" method="post" ref={formRef}>
                            <fieldset className="space-y-5" disabled={isSubmitting}>
                                <label className="block space-y-2">
                                    <span className="landing-auth-label">Email</span>
                                    <span className="landing-auth-input-wrap">
                                        <span aria-hidden="true" className="landing-auth-input-icon material-symbols-outlined">mail</span>
                                        <input className="landing-auth-input" name="email" placeholder="mail@site.com" required type="email" />
                                    </span>
                                </label>

                                <label className="block space-y-2">
                                    <span className="landing-auth-label">Password</span>
                                    <span className="landing-auth-input-wrap">
                                        <span aria-hidden="true" className="landing-auth-input-icon material-symbols-outlined">key</span>
                                        <input className="landing-auth-input" minLength={6} name="password" placeholder="Password" required type="password" />
                                    </span>
                                </label>

                                <button className="landing-button landing-button-primary w-full min-h-13 text-base disabled:opacity-70 sm:min-h-14" name="intent" type="submit" value="pwd">
                                    {isSubmitting && currentIntent === "pwd" ? (
                                        <span className="flex items-center gap-3">
                                            <span className="landing-spinner" />
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
                                <div className="h-px flex-1 bg-white/8" />
                                <span className="text-center text-[11px] mt-4 font-medium uppercase tracking-[0.18em] text-[var(--landing-on-surface-variant)]">or continue with a guided account</span>
                                <div className="h-px flex-1 bg-white/8" />
                            </div>

                            <Form className="grid gap-3 sm:grid-cols-2" method="post">
                                {demoActions.map((demoAction) => (
                                    <button
                                        key={demoAction.intent}
                                        className="landing-auth-choice disabled:opacity-70"
                                        disabled={isSubmitting}
                                        name="intent"
                                        type="submit"
                                        value={demoAction.intent}
                                    >
                                        {isSubmitting && currentIntent === demoAction.intent ? "Opening..." : demoAction.label}
                                    </button>
                                ))}

                                <button
                                    className="landing-auth-choice sm:col-span-2 disabled:opacity-70"
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
                            <Link className="font-semibold text-[var(--landing-primary)] transition-colors duration-200 hover:text-[var(--landing-on-surface)]" to="/register">
                                Register here
                            </Link>
                        </p>
                    </div>
                </section>
            </div>
        </div>
    );
}
