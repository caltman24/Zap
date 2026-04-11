import {ActionFunctionArgs, redirect} from "@remix-run/node";
import {Form, useActionData, useNavigation} from "@remix-run/react";
import BackButton from "~/components/BackButton";
import FormShell, {FormFieldHeader, formInputClassName, formTextareaClassName} from "~/components/FormShell";
import apiClient from "~/services/api.server/apiClient";
import {AuthenticationError} from "~/services/api.server/errors";
import {commitSession, getSession} from "~/services/sessions.server";
import {ActionResponse, ActionResponseResult} from "~/utils/response";
import tryCatch from "~/utils/tryCatch";

export async function action({request}: ActionFunctionArgs) {
    const session = await getSession(request);

    // Try to get valid token
    // Returns error if token is invalid or expired -> logout
    // Returns token if token is valid, and headers if token was refreshed
    const {
        data: tokenResponse,
        error: tokenError
    } = await tryCatch(apiClient.auth.getValidToken(session));

    if (tokenError) {
        return redirect("/logout");
    }
    const {token} = tokenResponse;


    const formData = await request.formData();
    const name = formData.get("name") as string;
    const description = formData.get("description") as string;

    const {data: res, error} = await tryCatch(
        apiClient.registerCompany({
            name,
            description,
        }, token));

    if (error instanceof AuthenticationError) {
        return redirect("/logout");
    }
    if (error) {
        return ActionResponse({
            success: false,
            error: error.message
        })
    }

    if (res.ok) {
        const {data, error} = await tryCatch(
            apiClient.getUserInfo(token));

        if (error) {
            if (error instanceof AuthenticationError) {
                return redirect("/logout");
            }
            return ActionResponse({
                success: false,
                error: error.message
            })
        }


        session.set("user", data);

        // Dont need to append headers returned from getValidToken because we already need to commit the session from setting the user data
        return redirect("/dashboard", {
            headers: {
                "Set-Cookie": await commitSession(session),
            },
        });
    }

    return ActionResponse({success: false, error: "Failed to register company"})
}

export default function SetupCompanyRoute() {
    const navigation = useNavigation();
    const isSubmitting = navigation.state === "submitting";
    const actionData = useActionData<typeof action>() as ActionResponseResult;

    return (
        <div className="app-shell min-h-screen px-6 py-10 sm:px-10 sm:py-14">
            <div className="mx-auto w-full max-w-5xl">
                <FormShell
                    description="Create the company workspace your team will use for projects, tickets, and collaboration. You can invite everyone else after this step."
                    error={actionData?.error}
                    eyebrow="Company Setup"
                    leading={<BackButton to="/setup"/>}
                    title="Register Your Company"
                >
                    <Form className="space-y-8" method="post">
                        <fieldset className="space-y-6" disabled={isSubmitting}>
                            <div>
                                <FormFieldHeader detail="100 max" label="Company Name" required/>
                                <input
                                    className={formInputClassName}
                                    maxLength={100}
                                    name="name"
                                    placeholder="Acme Inc."
                                    required
                                    type="text"
                                />
                            </div>

                            <div>
                                <FormFieldHeader detail="500 max" label="Description"/>
                                <textarea
                                    className={formTextareaClassName}
                                    maxLength={500}
                                    name="description"
                                    placeholder="Describe the team, company mission, or the type of work this workspace will support."
                                    rows={5}
                                />
                            </div>

                            <div
                                className="rounded-[1.5rem] bg-[var(--app-surface-container-lowest)] p-5 outline outline-1 outline-[var(--app-outline-variant)]/10">
                                <p className="app-shell-mono text-[10px] uppercase tracking-[0.24em] text-[var(--app-outline)]">What
                                    happens next</p>
                                <div
                                    className="mt-4 grid gap-3 text-sm text-[var(--app-on-surface-variant)] sm:grid-cols-3">
                                    <div className="rounded-2xl bg-[var(--app-surface-container-high)]/60 px-4 py-4">
                                        <p className="font-semibold text-[var(--app-on-surface)]">1. Workspace
                                            created</p>
                                        <p className="mt-1 leading-6">We attach your account to the new company
                                            instantly.</p>
                                    </div>
                                    <div className="rounded-2xl bg-[var(--app-surface-container-high)]/60 px-4 py-4">
                                        <p className="font-semibold text-[var(--app-on-surface)]">2. Dashboard
                                            unlocked</p>
                                        <p className="mt-1 leading-6">You land in the app with company access ready to
                                            go.</p>
                                    </div>
                                    <div className="rounded-2xl bg-[var(--app-surface-container-high)]/60 px-4 py-4">
                                        <p className="font-semibold text-[var(--app-on-surface)]">3. Invite your
                                            team</p>
                                        <p className="mt-1 leading-6">Start adding members and projects from inside the
                                            workspace.</p>
                                    </div>
                                </div>
                            </div>

                            <div className="flex justify-end border-t border-[var(--app-outline-variant)]/10 pt-5">
                                <button
                                    className="inline-flex min-w-40 items-center justify-center gap-2 rounded-xl bg-[linear-gradient(135deg,var(--app-primary)_0%,var(--app-primary-fixed)_100%)] px-5 py-3 text-sm font-bold text-[#1000a9] transition-all duration-200 hover:opacity-95 active:scale-95 disabled:cursor-not-allowed disabled:opacity-60"
                                    disabled={isSubmitting}
                                    type="submit"
                                >
                                    {isSubmitting ? (
                                        <>
                                            <span
                                                className="inline-flex h-4 w-4 animate-spin rounded-full border-2 border-current border-r-transparent"/>
                                            Registering...
                                        </>
                                    ) : (
                                        <>
                                            <span className="material-symbols-outlined text-lg">apartment</span>
                                            Register Company
                                        </>
                                    )}
                                </button>
                            </div>
                        </fieldset>
                    </Form>
                </FormShell>
            </div>
        </div>
    );
}
