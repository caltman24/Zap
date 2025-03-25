import { LoaderFunctionArgs, redirect } from "@remix-run/node";
import { Link, useLoaderData } from "@remix-run/react";
import apiClient from "~/services/api.server/apiClient";
import { AuthenticationError } from "~/services/api.server/errors";
import { CompanyInfoResponse } from "~/services/api.server/types";
import { getSession } from "~/services/sessions.server";
import tryCatch from "~/utils/tryCatch";

export const handle = {
    breadcrumb: () => <Link to="/company">Company</Link>,
};

// TODO: Handle when user gets kicked from company
// TODO: Handle role permissons on this page (edit company info, manage invites)

export async function loader({ request }: LoaderFunctionArgs) {
    const session = await getSession(request);
    const { data: tokenResponse, error: tokenError } = await tryCatch(apiClient.auth.getValidToken(session));
    if (tokenError) {
        return redirect("/logout");
    }

    const { data: res, error } = await tryCatch(apiClient.getCompanyInfo(tokenResponse.token));

    if (error instanceof AuthenticationError) {
        return redirect("/logout");
    }

    if (error) {
        return Response.json({ data: null, error: "Server Error: Please try again later." });
    }


    return Response.json({
        data: res,
        error: null
    });

}

export default function CompanyRoute() {
    const { data, error } = useLoaderData<typeof loader>();

    const companyInfo = data as CompanyInfoResponse | null;
    const memberList = Object.entries(companyInfo?.members ?? {}).map(([role, members]) => {
        return (
            <div key={role} className="my-4">
                <h3 className="text-lg font-medium">{role}</h3>
                <ul className="flex flex-wrap gap-2 mt-4">
                    {members.map((member, index) => (
                        <li key={index}>
                            <div className="flex gap-2 items-center">
                                <div className="avatar">
                                    <div className="w-11 rounded-full">
                                        <img src={member.avatarUrl} />
                                    </div>
                                </div>
                                <p className="text-lg">{member.name}</p>
                            </div>
                        </li>
                    ))}
                </ul>
            </div>
        );
    })

    return (
        <div className="w-full bg-base-300 h-full p-6">
            {error && <p className="text-error mt-4">{error}</p>}
            {
                companyInfo && (
                    <div>
                        <div className="bg-base-100 rounded shadow p-8">
                            <div className="flex gap-4">
                                <div className="avatar">
                                    <div className="w-24 rounded-md">
                                        <img src="https://upload.wikimedia.org/wikipedia/commons/7/7c/Profile_avatar_placeholder_large.png" />
                                    </div>
                                </div>
                                <div>
                                    <p className="text-2xl font-bold">{companyInfo.name}</p>
                                    <p className="text-sm lg:text-base mt-2 text-base-content/80">{companyInfo.description}</p>
                                </div>
                            </div>

                            <div className="mt-8">
                                <h2 className="text-xl font-bold mb-4">Members</h2>
                                {memberList && memberList}
                            </div>
                        </div>
                        <div className="bg-base-100 rounded shadow p-8 mt-4">
                            <h2 className="text-xl font-bold mb-4">Invites</h2>
                        </div>
                    </div>
                )
            }
        </div>
    );
}
