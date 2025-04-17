import { ActionFunctionArgs, LoaderFunctionArgs, redirect } from "@remix-run/node";
import { Form, useActionData, useNavigation } from "@remix-run/react";
import { useEffect, useRef } from "react";
import DemoUserLoginHandler from "./_handlers/DemoUserLoginHandler";
import PwdLoginHandler from "./_handlers/PwdLoginHandler";
import TestUserLoginHandler from "./_handlers/TestUserLoginHandler";
import { getSession } from "~/services/sessions.server";


export async function action({ request }: ActionFunctionArgs) {
    const formData = await request.formData();
    const intent = formData.get("intent");

    if (intent === "pwd") {
        return await PwdLoginHandler(request, formData);
    }
    if (intent === "test:user") {
        return await TestUserLoginHandler(request)
    }

    if (intent?.toString().startsWith("demo:")) {
        return await DemoUserLoginHandler()
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

    useEffect(() => {
        formRef.current?.reset();
    }, [actionData])

    let formRef = useRef<HTMLFormElement | null>(null);

    return (
        <div className="grid place-items-center">
            <div className="pt-10 pb-20">
                <Form method="post" ref={formRef}>
                    <fieldset className="fieldset w-md bg-base-200 border border-base-300 p-4 rounded-box" disabled={isSubmitting}>
                        <legend className="fieldset-legend text-2xl">Sign Into Zap</legend>

                        <p className="text-error">{actionData ? actionData.message : ""}</p>

                        <label className="fieldset-label">Email</label>
                        <label className="input validator w-full">
                            <svg className="h-[1em] opacity-50" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24">
                                <g strokeLinejoin="round" strokeLinecap="round" strokeWidth="2.5" fill="none" stroke="currentColor">
                                    <rect width="20" height="16" x="2" y="4" rx="2"></rect>
                                    <path d="m22 7-8.97 5.7a1.94 1.94 0 0 1-2.06 0L2 7"></path>
                                </g>
                            </svg>
                            <input type="email" name="email" placeholder="mail@site.com" required />
                        </label>
                        <div className="validator-hint hidden">Enter valid email address</div>


                        <label className="fieldset-label">Password</label>
                        <label className="input validator w-full">
                            <svg className="h-[1em] opacity-50" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24">
                                <g strokeLinejoin="round" strokeLinecap="round" strokeWidth="2.5" fill="none" stroke="currentColor">
                                    <path d="M2.586 17.414A2 2 0 0 0 2 18.828V21a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-1a1 1 0 0 1 1-1h1a1 1 0 0 0 1-1v-1a1 1 0 0 1 1-1h.172a2 2 0 0 0 1.414-.586l.814-.814a6.5 6.5 0 1 0-4-4z">
                                    </path>
                                    <circle cx="16.5" cy="7.5" r=".5" fill="currentColor"></circle></g></svg>
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
                    <button className="btn btn-neutral" type="submit" name="intent" value="test:user">Test User</button>
                </Form>
            </div>
        </div>
    );
}
