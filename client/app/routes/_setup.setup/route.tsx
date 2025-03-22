import { LoaderFunctionArgs, redirect } from "@remix-run/node";
import { Link, useOutletContext } from "@remix-run/react";
import { getSession } from "~/services/sessions.server";


export default function SetupRoute() {
    const contextData = useOutletContext() as { firstName: string };

    return (
        <div className="grid text-center place-items-center w-full bg-base-300 h-screen p-6">
            <div>
                <h1 className="text-3xl font-semibold mb-4">Welcome <span className="text-primary">{contextData.firstName}</span>!</h1>
                <h2 className="text-3xl font-bold">Join or Register a Company</h2>
                <div className="flex gap-4 mt-16">
                    <div className="card w-96 bg-base-100 card-sm shadow-sm hover:scale-105 hover:transition cursor-pointer transition hover:bg-primary/40">
                        <div className="card-body">
                            <h2 className="card-title text-2xl">Join Via Invite Link</h2>
                        </div>
                    </div>

                    <div className="card w-96 h-44 bg-base-100 card-sm shadow-sm hover:scale-105 hover:transition cursor-pointer transition hover:bg-primary/40">
                        <div className="card-body">
                            <h2 className="card-title text-2xl">Register a New Company</h2>
                        </div>
                    </div>
                </div>
                <Link to={"/logout"} className="btn btn-link text-lg mt-16">Logout</Link>
            </div>
        </div >
    );
}