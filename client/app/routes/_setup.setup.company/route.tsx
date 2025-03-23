import { ActionFunctionArgs, redirect } from "@remix-run/node";
import { Form, Link, useActionData, useNavigation } from "@remix-run/react";
import { useEffect, useState } from "react";
import apiService, { AuthenticationError } from "~/services/ApiService.server";
import { commitSession, getSession } from "~/services/sessions.server";
import tryCatch from "~/utils/tryCatch";

export async function action({ request }: ActionFunctionArgs) {
    const session = await getSession(request);
    const formData = await request.formData();

    const name = formData.get("name") as string;
    const description = formData.get("description") as string;

    const { data: res, error } = await tryCatch(apiService.RegisterCompany({
        name,
        description,
    }, session));

    if (error) {
        if (error instanceof AuthenticationError) {
            return redirect("/login");
        }
        return Response.json({ message: "Server Error: Please try again later." });
    }

    if (res.ok) {
        const { data, error } = await tryCatch(apiService.GetUserInfo(session));

        if (error) {
            if (error instanceof AuthenticationError) {
                return redirect("/login");
            }
            return Response.json({ message: "Server Error: Please try again later." });
        }


        session.set("user", data);

        return redirect("/dashboard", {
            headers: {
                "Set-Cookie": await commitSession(session),
            },
        });
    }

    if (res.status === 400) {
        return Response.json({ message: await res.json() });
    }

    return Response.json({ message: "Failed to register company" });
}

export default function SetupCompanyRoute() {
    const navigation = useNavigation();
    const isSubmitting = navigation.state === "submitting";
    const actionData = useActionData<typeof action>();
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (actionData?.message) {
            setError(actionData.message);
        }
    }, [actionData]);

    return (
        <div className="text-center w-full bg-base-300 h-screen p-6">
            <div className="max-w-md mx-auto">
                <Link to="/setup" className="btn btn-ghost mb-6">
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 19l-7-7m0 0l7-7m-7 7h18" />
                    </svg>
                    Back
                </Link>

                <h1 className="text-3xl font-bold mb-6">Register Your Company</h1>

                <Form method="post">
                    <fieldset className="fieldset bg-base-200 border border-base-300 p-6 rounded-box" disabled={isSubmitting}>
                        {error && <p className="text-error mb-4">{error}</p>}

                        <div className="form-control mb-4">
                            <label className="label">
                                <span className="label-text">Company Name</span>
                            </label>
                            <input
                                type="text"
                                name="name"
                                className="input input-bordered w-full"
                                placeholder="Acme Inc."
                                required
                                maxLength={100}
                            />
                        </div>

                        <div className="form-control mb-6">
                            <label className="label">
                                <span className="label-text">Description</span>
                            </label>
                            <textarea
                                name="description"
                                className="textarea textarea-bordered w-full"
                                placeholder="Brief description of your company"
                                rows={4}
                                maxLength={500}
                            ></textarea>
                        </div>

                        <div className="form-control">
                            <button
                                type="submit"
                                className="btn btn-primary w-full"
                                disabled={isSubmitting}
                            >
                                {isSubmitting ? "Registering..." : "Register Company"}
                            </button>
                        </div>
                    </fieldset>
                </Form>
            </div>
        </div>
    );
}
