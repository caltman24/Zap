import { ActionFunctionArgs } from "@remix-run/node";
import { Form, redirect, useActionData, useNavigation } from "@remix-run/react";
import { useEffect, useRef, useTransition } from "react";
import apiService from "~/services/ApiService";
import { commitSession, getSession } from "~/services/sessions.server";
import tryCatch from "~/utils/tryCatch";


export async function action({ request }: ActionFunctionArgs) {
    const formData = await request.formData();
    const intent = formData.get("intent");

    if (intent === "pwd") {
        const email = formData.get("email") as string;
        const password = formData.get("password") as string;

        const { data: res, error } = await tryCatch(
            apiService.SignInUser(email, password));

        if (error) {
            return Response.json(
                { message: "Server Error: Please try again later." });
        }

        if (res.status === 400) {
            return Response.json({ message: await res.json() });
        }

        if (res.ok) {
            const tokenResonse: {
                tokenType: string;
                accessToken: string;
                expiresIn: number;
                refreshToken: string;
            } = await res.json();


            const session = await getSession(request);
            session.set("tokens", tokenResonse.accessToken);
            session.set("isAuthenticated", true);

            return redirect("/dashboard", {
                headers: {
                    "Set-Cookie": await commitSession(session),
                },
            });
        }


    }
    if (intent === "demo:admin") {
        return Response.json({ message: "Admin login successful" });
    }
    if (intent === "demo:pm") {
        return Response.json({ message: "Project Manager login successful" });
    }
    if (intent === "demo:dev") {
        return Response.json({ message: "Developer login successful" });
    }
    if (intent === "demo:submitter") {
        return Response.json({ message: "Submitter login successful" });
    }

    return Response.json({ message: "Failed to login" });
}


export default function Login() {
    const actionData = useActionData<typeof action>();
    const navigation = useNavigation();
    const isSubmitting = navigation.state === "submitting";

    useEffect(() => {
        formRef.current?.reset();
    }, [actionData])

    let formRef = useRef<HTMLFormElement | null>(null);

    return (
        <div className="grid place-items-center">
            <div className="mt-10">
                <Form method="post" ref={formRef}>
                    <fieldset className="fieldset w-md bg-base-200 border border-base-300 p-4 rounded-box" disabled={isSubmitting}>
                        <legend className="fieldset-legend text-2xl">Sign Into Zap</legend>

                        <p className="text-error">{actionData ? actionData.message : ""}</p>

                        <label className="fieldset-label">Email</label>
                        <label className="input validator w-full">
                            <svg className="h-[1em] opacity-50" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><g strokeLinejoin="round" strokeLinecap="round" strokeWidth="2.5" fill="none" stroke="currentColor"><rect width="20" height="16" x="2" y="4" rx="2"></rect><path d="m22 7-8.97 5.7a1.94 1.94 0 0 1-2.06 0L2 7"></path></g></svg>
                            <input type="email" name="email" placeholder="mail@site.com" required />
                        </label>
                        <div className="validator-hint hidden">Enter valid email address</div>


                        <label className="fieldset-label">Password</label>
                        <label className="input validator w-full">
                            <svg className="h-[1em] opacity-50" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><g strokeLinejoin="round" strokeLinecap="round" strokeWidth="2.5" fill="none" stroke="currentColor"><path d="M2.586 17.414A2 2 0 0 0 2 18.828V21a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-1a1 1 0 0 1 1-1h1a1 1 0 0 0 1-1v-1a1 1 0 0 1 1-1h.172a2 2 0 0 0 1.414-.586l.814-.814a6.5 6.5 0 1 0-4-4z"></path><circle cx="16.5" cy="7.5" r=".5" fill="currentColor"></circle></g></svg>
                            <input type="password" name="password" required minLength={6} placeholder="Password" />
                        </label>

                        <button className="btn btn-primary mt-4" type="submit" name="intent" value="pwd">
                            {isSubmitting ?
                                <span className="loading loading-spinner loading-sm"></span>
                                : "Login"}
                        </button>
                    </fieldset>
                </Form>

                <div className="h-[1px] w-full bg-base-content/10 mt-8 mb-10 text-center"><p className="text-base-content/50 text-lg">OR</p></div>
                <Form className="flex flex-col gap-4" method="post">
                    <button className="btn btn-outline btn-primary" type="submit" name="intent" value="demo:admin">Demo as Admin</button>
                    <button className="btn btn-outline btn-secondary" type="submit" name="intent" value="demo:pm">Demo as Project Manager</button>
                    <button className="btn btn-outline btn-accent" type="submit" name="intent" value="demo:dev">Demo as Developer</button>
                    <button className="btn btn-outline btn-warning" type="submit" name="intent" value="demo:submitter">Demo as Submitter</button>
                </Form>
            </div>
        </div>
    );
}