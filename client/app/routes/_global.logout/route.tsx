import { ActionFunctionArgs, LoaderFunctionArgs, redirect } from "@remix-run/node";
import { destroySession, getSession } from "~/services/sessions.server";

export async function action({ request }: ActionFunctionArgs) {
    const session = await getSession(request);

    return redirect("/", {
        headers: {
            "Set-Cookie": await destroySession(session),
        },
    });
}

export async function loader({ request }: LoaderFunctionArgs) {
    const session = await getSession(request);

    if (!session.get("isAuthenticated")) {
        return redirect("/login");
    } else {
        return redirect("/", {
            headers: {
                "Set-Cookie": await destroySession(session),
            },
        });
    }
}

export default function Logout() {
    return null;
}