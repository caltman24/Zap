import { ActionFunctionArgs, LoaderFunctionArgs, redirect } from "@remix-run/node";
import { useLoaderData, useNavigate } from "@remix-run/react";
import { useEffect, useRef } from "react";
import apiClient from "~/services/api.server/apiClient";
import { CompanyMemberPerRole } from "~/services/api.server/types";
import { getSession } from "~/services/sessions.server";
import { JsonResponse, JsonResponseResult } from "~/utils/response";
import tryCatch from "~/utils/tryCatch";


export async function loader({ request, params }: LoaderFunctionArgs) {
    const projectId = params.projectId!

    const session = await getSession(request);
    const {
        data: tokenResponse,
        error: tokenError
    } = await tryCatch(apiClient.auth.getValidToken(session));

    if (tokenError) {
        return redirect("/logout");
    }

    const { data: res, error } = await tryCatch(
        apiClient.getUnassignedProjectMembers(
            projectId,
            tokenResponse.token));

    console.log(res)

    if (error) {
        return JsonResponse({
            data: null,
            error: error.message,
            headers: tokenResponse.headers
        })
    }

    return JsonResponse({
        data: res,
        error: null,
        headers: tokenResponse.headers
    })
}

export default function UnassignedProjectMembers() {
    const { data: members, error } = useLoaderData<JsonResponseResult<CompanyMemberPerRole>>();

    const modalRef = useRef<HTMLDialogElement>(null);
    const navigate = useNavigate()

    useEffect(() => {
        if (modalRef) {
            modalRef.current?.showModal();
        }
    }, [])

    //TODO: Add search for member list

    return (
        <dialog id="member-modal" className="modal" ref={modalRef}>
            {error || !members && <p className="text-error text-sm">{error}</p>}
            <div className="modal-box">
                <h3 className="font-bold text-lg mb-8">Select Members</h3>
                <div>
                    {Object.keys(members as object).length === 0
                        ? <p>No more members to add</p>
                        : (<ul className="list rounded bg-base-300 max-h-[350px] overflow-y-auto">
                            {Object.entries(members ?? {})
                                .map(([role, m]) => {
                                    return (
                                        <li className="list-row flex flex-col gap-2">
                                            <p className="font-bold">{role}</p>
                                            <ul className="list">
                                                {m.map(x =>
                                                (<li className="list-row flex items-center cursor-pointer hover:bg-base-200 rounded">
                                                    {x.name}
                                                </li>))}
                                            </ul>
                                        </li>
                                    )
                                })}
                        </ul>
                        )}
                </div>
                <div className="modal-action">
                    <form method="dialog">
                        <button className="btn" onClick={() => navigate(-1)}>Close</button>
                    </form>
                </div>
            </div>
        </dialog>
    )
}
