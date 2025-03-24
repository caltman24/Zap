import { ActionFunctionArgs, LoaderFunctionArgs } from "@remix-run/node";
import { Form, redirect, useActionData, useNavigation } from "@remix-run/react";
import { useEffect, useState } from "react";
import apiClient from "~/services/api.server/apiClient";
import { commitSession, getSession } from "~/services/sessions.server";
import setSession from "../_landing.login/setSession";
import tryCatch from "~/utils/tryCatch";
import { TokenResponse } from "~/services/api.server/types";

export async function action({ request }: ActionFunctionArgs) {
    const formData = await request.formData();
    const firstName = formData.get("firstName");
    const lastName = formData.get("lastName");
    const email = formData.get("email");
    const password = formData.get("password");
    const confirmPassword = formData.get("confirmPassword");

    // Check if passwords match
    if (password !== confirmPassword) {
        return Response.json({ message: "Passwords do not match" });
    }

    const { data: res, error } = await tryCatch(apiClient.auth.registerAccount({
        firstName: firstName as string,
        lastName: lastName as string,
        email: email as string,
        password: password as string,
    }));

    if (error) {
        return Response.json({ message: "Server Error: Please try again later." });
    }

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

    if (session.get("user")) {
        return redirect("/dashboard");
    }

    return null;
}

export default function RegisterCompany() {
    const navigation = useNavigation();
    const isSubmitting = navigation.state === "submitting";
    const actionData = useActionData<typeof action>();
    const [error, setError] = useState<string | null>(null);
    const [password, setPassword] = useState<string>("");
    const [confirmPassword, setConfirmPassword] = useState<string>("");
    const [showPassword, setShowPassword] = useState<boolean>(false);
    const [showConfirmPassword, setShowConfirmPassword] = useState<boolean>(false);

    useEffect(() => {
        if (actionData?.message) {
            setError(actionData.message);
        }
    }, [actionData])


    return (
        <div className="grid place-items-center py-10">
            <Form method="post" >
                <fieldset className="fieldset w-sm md:w-lg bg-base-200 border border-base-300 p-4 rounded-box">
                    <legend className="fieldset-legend text-2xl">Register Account</legend>
                    {error && <p className="text-error">{error}</p>}
                    <div className="flex gap-4">
                        <div className="w-full">
                            <label className="fieldset-label mb-1">First Name</label>
                            <label className="input validator w-full">
                                <input type="text" name="firstName" maxLength={50} placeholder="John" required className="w-full" />
                            </label>
                            <div className="validator-hint hidden">Required</div>
                        </div>

                        <div className="w-full">
                            <label className="fieldset-label mb-1">Last Name</label>
                            <label className="input validator w-full">
                                <input type="text" name="lastName" maxLength={50} placeholder="Doe" required className="w-full" />
                            </label>
                            <div className="validator-hint hidden">Required</div>
                        </div>
                    </div>

                    <label className="fieldset-label">Email</label>
                    <label className="input validator w-full">
                        <svg className="h-[1em] opacity-50" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><g strokeLinejoin="round" strokeLinecap="round" strokeWidth="2.5" fill="none" stroke="currentColor"><rect width="20" height="16" x="2" y="4" rx="2"></rect><path d="m22 7-8.97 5.7a1.94 1.94 0 0 1-2.06 0L2 7"></path></g></svg>
                        <input type="email" name="email" maxLength={75} placeholder="mail@site.com" required />
                    </label>
                    <div className="validator-hint hidden">Enter valid email address</div>


                    <label className="fieldset-label">Password</label>
                    <label className="input validator w-full">
                        <svg className="h-[1em] opacity-50" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><g strokeLinejoin="round" strokeLinecap="round" strokeWidth="2.5" fill="none" stroke="currentColor"><path d="M2.586 17.414A2 2 0 0 0 2 18.828V21a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-1a1 1 0 0 1 1-1h1a1 1 0 0 0 1-1v-1a1 1 0 0 1 1-1h.172a2 2 0 0 0 1.414-.586l.814-.814a6.5 6.5 0 1 0-4-4z"></path><circle cx="16.5" cy="7.5" r=".5" fill="currentColor"></circle></g></svg>
                        <input
                            type={showPassword ? "text" : "password"}
                            name="password"
                            required
                            minLength={6}
                            maxLength={50}
                            placeholder="Password"
                            pattern="(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z0-9]).{6,}"
                            title="Must be more than 6 characters, including number, lowercase letter, uppercase letter, and non alphanumeric character"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                        />
                        <button
                            type="button"
                            className="opacity-70 hover:opacity-100"
                            onClick={() => setShowPassword(!showPassword)}
                        >
                            {showPassword ?
                                <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                    <path d="M10 12a2 2 0 100-4 2 2 0 000 4z" />
                                    <path fillRule="evenodd" d="M.458 10C1.732 5.943 5.522 3 10 3s8.268 2.943 9.542 7c-1.274 4.057-5.064 7-9.542 7S1.732 14.057.458 10zM14 10a4 4 0 11-8 0 4 4 0 018 0z" clipRule="evenodd" />
                                </svg>
                                :
                                <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                    <path fillRule="evenodd" d="M3.707 2.293a1 1 0 00-1.414 1.414l14 14a1 1 0 001.414-1.414l-1.473-1.473A10.014 10.014 0 0019.542 10C18.268 5.943 14.478 3 10 3a9.958 9.958 0 00-4.512 1.074l-1.78-1.781zm4.261 4.26l1.514 1.515a2.003 2.003 0 012.45 2.45l1.514 1.514a4 4 0 00-5.478-5.478z" clipRule="evenodd" />
                                    <path d="M12.454 16.697L9.75 13.992a4 4 0 01-3.742-3.741L2.335 6.578A9.98 9.98 0 00.458 10c1.274 4.057 5.065 7 9.542 7 .847 0 1.669-.105 2.454-.303z" />
                                </svg>
                            }
                        </button>
                    </label>
                    <p className="validator-hint hidden">
                        Must be more than 6 characters, including
                        <br />At least one number
                        <br />At least one lowercase letter
                        <br />At least one uppercase letter
                        <br />At least one non alphanumeric character
                    </p>

                    <label className="fieldset-label">Confirm Password</label>
                    <label className={`input w-full ${(confirmPassword.length > 1 || password.length > 1) && (password === confirmPassword ? "input-success" : "input-error")}`}>
                        <svg className="h-[1em] opacity-50" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><g strokeLinejoin="round" strokeLinecap="round" strokeWidth="2.5" fill="none" stroke="currentColor"><path d="M2.586 17.414A2 2 0 0 0 2 18.828V21a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-1a1 1 0 0 1 1-1h1a1 1 0 0 0 1-1v-1a1 1 0 0 1 1-1h.172a2 2 0 0 0 1.414-.586l.814-.814a6.5 6.5 0 1 0-4-4z"></path><circle cx="16.5" cy="7.5" r=".5" fill="currentColor"></circle></g></svg>
                        <input
                            type={showConfirmPassword ? "text" : "password"}
                            name="confirmPassword"
                            required
                            minLength={6}
                            maxLength={50}
                            placeholder="Confirm Password"
                            value={confirmPassword}
                            onChange={(e) => setConfirmPassword(e.target.value)}
                        />
                        <button
                            type="button"
                            className="opacity-70 hover:opacity-100"
                            onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                        >
                            {showConfirmPassword ?
                                <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                    <path d="M10 12a2 2 0 100-4 2 2 0 000 4z" />
                                    <path fillRule="evenodd" d="M.458 10C1.732 5.943 5.522 3 10 3s8.268 2.943 9.542 7c-1.274 4.057-5.064 7-9.542 7S1.732 14.057.458 10zM14 10a4 4 0 11-8 0 4 4 0 018 0z" clipRule="evenodd" />
                                </svg>
                                :
                                <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                    <path fillRule="evenodd" d="M3.707 2.293a1 1 0 00-1.414 1.414l14 14a1 1 0 001.414-1.414l-1.473-1.473A10.014 10.014 0 0019.542 10C18.268 5.943 14.478 3 10 3a9.958 9.958 0 00-4.512 1.074l-1.78-1.781zm4.261 4.26l1.514 1.515a2.003 2.003 0 012.45 2.45l1.514 1.514a4 4 0 00-5.478-5.478z" clipRule="evenodd" />
                                    <path d="M12.454 16.697L9.75 13.992a4 4 0 01-3.742-3.741L2.335 6.578A9.98 9.98 0 00.458 10c1.274 4.057 5.065 7 9.542 7 .847 0 1.669-.105 2.454-.303z" />
                                </svg>
                            }
                        </button>
                    </label>
                    <div className={`text-error text-xs ${password === confirmPassword ? "hidden" : ""}`}>Passwords must match</div>

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
