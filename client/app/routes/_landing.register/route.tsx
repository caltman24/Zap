import { ActionFunctionArgs, LoaderFunctionArgs } from "@remix-run/node";
import { Form, redirect, useActionData, useNavigation } from "@remix-run/react";
import { useEffect, useState } from "react";
import apiService, { TokenResponse, ValidateAccountResponse } from "~/services/ApiService";
import { commitSession, getSession } from "~/services/sessions.server";
import setSession from "../_landing.login/setSession";

export async function action({ request }: ActionFunctionArgs) {
    const formData = await request.formData();
    const firstName = formData.get("firstName");
    const lastName = formData.get("lastName");
    const email = formData.get("email");
    const password = formData.get("password");

    const res = await apiService.RegisterAccount({
        firstName: firstName as string,
        lastName: lastName as string,
        email: email as string,
        password: password as string,
    });

    if (res.ok) {
        const tokenResonse: TokenResponse = await res.json();

        return await setSession(request, tokenResonse, "/dashboard");
    }

    if (res.status === 400) {
        return Response.json({ message: await res.json() });
    }

    return Response.json({ message: "Failed to register" });
}

export async function loader({ request }: LoaderFunctionArgs) {
    const session = await getSession(request);

    if (session.get("isAuthenticated")) {
        return redirect("/dashboard");
    }

    return null;
}

export default function RegisterCompany() {
    const navigation = useNavigation();
    const isSubmitting = navigation.state === "submitting";
    const actionData = useActionData<typeof action>();
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (actionData?.message) {
            setError(actionData.message);
        }
    }, [actionData])


    return (
        <div className="grid place-items-center py-10">
            <Form method="post" >
                <fieldset className="fieldset w-xs bg-base-200 border border-base-300 p-4 rounded-box">
                    <legend className="fieldset-legend text-2xl">Register Account</legend>
                    {error && <p className="text-error">{error}</p>}

                    <label className="fieldset-label">First Name</label>
                    <label className="input validator w-full">
                        <input type="text" name="firstName" maxLength={50} placeholder="John" required />
                    </label>
                    <div className="validator-hint hidden">Required</div>
                    <label className="fieldset-label">Last Name</label>
                    <label className="input validator w-full">
                        <input type="text" name="lastName" maxLength={50} placeholder="Doe" required />
                    </label>
                    <div className="validator-hint hidden">Required</div>

                    <label className="fieldset-label">Email</label>
                    <label className="input validator w-full">
                        <svg className="h-[1em] opacity-50" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><g strokeLinejoin="round" strokeLinecap="round" strokeWidth="2.5" fill="none" stroke="currentColor"><rect width="20" height="16" x="2" y="4" rx="2"></rect><path d="m22 7-8.97 5.7a1.94 1.94 0 0 1-2.06 0L2 7"></path></g></svg>
                        <input type="email" name="email" placeholder="mail@site.com" required />
                    </label>
                    <div className="validator-hint hidden">Enter valid email address</div>


                    <label className="fieldset-label">Password</label>
                    <label className="input validator w-full">
                        <svg className="h-[1em] opacity-50" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><g strokeLinejoin="round" strokeLinecap="round" strokeWidth="2.5" fill="none" stroke="currentColor"><path d="M2.586 17.414A2 2 0 0 0 2 18.828V21a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-1a1 1 0 0 1 1-1h1a1 1 0 0 0 1-1v-1a1 1 0 0 1 1-1h.172a2 2 0 0 0 1.414-.586l.814-.814a6.5 6.5 0 1 0-4-4z"></path><circle cx="16.5" cy="7.5" r=".5" fill="currentColor"></circle></g></svg>
                        <input type="password" name="password" required minLength={6} placeholder="Password" pattern="(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z0-9]).{6,}" title="Must be more than 6 characters, including number, lowercase letter, uppercase letter, and non alphanumeric character" />
                    </label>
                    <p className="validator-hint hidden">
                        Must be more than 6 characters, including
                        <br />At least one number
                        <br />At least one lowercase letter
                        <br />At least one uppercase letter
                        <br />At least one non alphanumeric character
                    </p>

                    <button className="btn btn-primary mt-4" type="submit" >
                        {isSubmitting ?
                            <span className="loading loading-spinner loading-sm"></span>
                            : "Register"}
                    </button>
                </fieldset>
            </Form>
        </div>
    );
}
