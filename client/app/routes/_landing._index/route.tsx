import { MetaFunction, Outlet } from "@remix-run/react";
import MainNavbar from "~/routes/_landing/MainNavbar";


export const meta: MetaFunction = () => {
    return [
        { title: "Zap" },
        { name: "description", content: "A bug tracker app" },
    ];
};

export async function loader() {
    return null;
}

export default function Index() {
    return (
        <div className="">
            <div className="max-w-7xl mx-auto p-4">
                <header>
                    <h1 className="text-7xl text-base-content">Welcome to Zap!</h1>
                </header>
            </div>
        </div>
    );
}
