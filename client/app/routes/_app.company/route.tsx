import { LoaderFunctionArgs, ActionFunctionArgs, redirect, HeadersFunction } from "@remix-run/node";
import RouteLayout from "~/layouts/RouteLayout";
import { Form, Link, useLoaderData, useOutletContext, useNavigation, useActionData } from "@remix-run/react";
import { useState, useRef, useEffect } from "react";
import { EditModeForm } from "~/components/EditModeForm";
import apiClient from "~/services/api.server/apiClient";
import { AuthenticationError } from "~/services/api.server/errors";
import { CompanyInfoResponse, UserInfoResponse } from "~/services/api.server/types";
import { getSession } from "~/services/sessions.server";
import { useEditMode } from "~/utils/editMode";
import { ActionResponse, ActionResponseResult, ForbiddenResponse, JsonResponse } from "~/utils/response";
import tryCatch from "~/utils/tryCatch";
import { validateRole } from "~/utils/validate";
import permissions from "~/data/permissions";
import roleNames from "~/data/roles";
import MembersListTable from "~/components/MembersListTable";

export const handle = {
    breadcrumb: () => <Link to="/company">Company</Link>,
};

// TODO: Handle when user gets kicked from company
// TODO: Handle role permissons on this page (edit company info, manage invites)

export async function loader({ request }: LoaderFunctionArgs) {
    const session = await getSession(request);

    const {
        data: tokenResponse,
        error: tokenError
    } = await tryCatch(apiClient.auth.getValidToken(session));

    if (tokenError) {
        return redirect("/logout");
    }

    const {
        data: res,
        error
    } = await tryCatch(apiClient.getCompanyInfo(tokenResponse.token));

    if (error instanceof AuthenticationError) {
        return redirect("/logout");
    }
    if (error) {
        return JsonResponse({
            data: null,
            error: error.message,
            headers: tokenResponse.headers
        });
    }

    return JsonResponse({
        data: res,
        error: null,
        headers: tokenResponse.headers
    });

}

export async function action({ request }: ActionFunctionArgs) {
    const session = await getSession(request);
    const userRole = session.get("user").role;

    if (!validateRole(userRole, permissions.company.edit)) {
        return ForbiddenResponse()
    }

    const { data: tokenResponse, error: tokenError } = await tryCatch(apiClient.auth.getValidToken(session));
    if (tokenError) {
        return redirect("/logout");
    }

    const formData = await request.formData();
    const name = formData.get("name") as string;
    const description = formData.get("description") as string;
    const websiteUrl = formData.get("websiteUrl") as string || null;
    const removeLogo = formData.get("removeLogo") === "true";

    // Create a FormData object for the multipart request
    const requestFormData = new FormData();
    requestFormData.append("name", name);
    requestFormData.append("description", description);
    requestFormData.append("websiteUrl", websiteUrl ?? "");
    requestFormData.append("removeLogo", removeLogo.toString());

    // Handle file upload if provided
    const logoFile = formData.get("logo") as File;

    const maxMB = 2;
    if (logoFile.size > maxMB * 1024 * 1024) {
        return ActionResponse({ success: false, error: `Logo file is too large. Maximum size is ${maxMB}MB.`, headers: tokenResponse.headers })
    }
    if (logoFile && logoFile.size > 0) {
        requestFormData.append("file", logoFile);
    }

    const { error } = await tryCatch(apiClient.updateCompanyInfo(requestFormData, tokenResponse.token));

    if (error instanceof AuthenticationError) {
        return redirect("/logout");
    }

    if (error) {
        return ActionResponse({ success: false, error: error.message, headers: tokenResponse.headers })
    }

    return ActionResponse({ success: true, error: null, headers: tokenResponse.headers })
}

export default function CompanyRoute() {
    const { data, error } = useLoaderData<typeof loader>();
    const actionData = useActionData<typeof action>() as ActionResponseResult;
    const userData = useOutletContext<UserInfoResponse>();
    const navigation = useNavigation();
    const isSubmitting = navigation.state === "submitting";
    const canEdit = validateRole(userData.role, permissions.company.edit);

    // State for edit mode
    const {
        isEditing,
        formError,
        toggleEditMode,
    } = useEditMode({ actionData });



    const [removeLogo, setRemoveLogo] = useState(false);
    const [previewImage, setPreviewImage] = useState<string | null>(null);
    const fileInputRef = useRef<HTMLInputElement>(null);

    const companyInfo = data as CompanyInfoResponse | null;
    console.log(companyInfo?.members)
    const memberList = Object.entries(companyInfo?.members ?? {}).map(([role, members]) => {
        return (
            <div key={role} className="my-4 bg-base-300 p-4 rounded-sm">
                <h3 className="text-lg font-medium">{role}</h3>
                <MembersListTable members={members as any} />
            </div>
        );
    })


    // Handle file selection for preview
    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (file) {
            const reader = new FileReader();
            reader.onloadend = () => {
                setPreviewImage(reader.result as string);
                setRemoveLogo(false);
            };
            reader.readAsDataURL(file);
        } else {
            setPreviewImage(null);
        }
    };

    // Handle remove logo checkbox
    const handleRemoveLogoChange = () => {
        setRemoveLogo(!removeLogo);
        if (!removeLogo) {
            setPreviewImage(null);
            if (fileInputRef.current) {
                fileInputRef.current.value = '';
            }
        }
    };

    // Reset preview when exiting edit mode
    useEffect(() => {
        if (!isEditing) {
            setPreviewImage(null);
        }
    }, [isEditing]);

    return (
        <RouteLayout className="w-full p-6">
            {error && <p className="text-error mt-4">{error}</p>}
            {
                companyInfo && (
                    <div>
                        <div className="bg-base-100 rounded shadow p-8 relative">
                            {canEdit && !isEditing && (
                                <div className="flex gap-4 mb-4 justify-end absolute right-6 top-6 z-10">
                                    <button onClick={() => { toggleEditMode(); }} className="btn btn-soft btn-sm">
                                        <span className="material-symbols-outlined text-primary">edit</span> Edit Company
                                    </button>
                                    <Link to="/company/invites" className="btn btn-soft btn-sm">
                                        <span className="material-symbols-outlined text-accent">settings</span>Manage Roles
                                    </Link>
                                </div>
                            )}

                            {isEditing ? (
                                <EditModeForm
                                    error={formError}
                                    isSubmitting={isSubmitting}
                                    encType="multipart/form-data"
                                    onCancel={() => { toggleEditMode(); }}>

                                    <div className="flex gap-4 mb-6">
                                        <div className="avatar">
                                            <div className="w-24 h-24 rounded-md bg-base-200 grid place-items-center text-center overflow-hidden">
                                                {!removeLogo && (
                                                    <img
                                                        src={previewImage ||
                                                            companyInfo.logoUrl ||
                                                            "https://upload.wikimedia.org/wikipedia/commons/7/7c/Profile_avatar_placeholder_large.png"}
                                                        alt="Company logo"
                                                        className="object-cover w-full h-full"
                                                    />
                                                )}
                                                {removeLogo && (

                                                    <span className="material-symbols-outlined text-base-content/50" style={{ fontSize: "4rem" }}>image_not_supported</span>
                                                )}
                                            </div>
                                        </div>
                                        <div className="flex flex-col gap-2">
                                            <input
                                                type="file"
                                                name="logo"
                                                ref={fileInputRef}
                                                className="file-input file-input-bordered w-full max-w-xs"
                                                accept="image/jpg, image/jpeg, image/png, image/svg+xml, image/webp"
                                                onChange={handleFileChange}
                                                disabled={removeLogo}
                                            />
                                            <div className="form-control">
                                                <label className="label cursor-pointer">
                                                    <span className="label-text">Remove logo</span>
                                                    <input
                                                        type="checkbox"
                                                        className="checkbox"
                                                        checked={removeLogo}
                                                        onChange={handleRemoveLogoChange}
                                                    />
                                                </label>
                                                <input type="hidden" name="removeLogo" value={removeLogo.toString()} />
                                            </div>
                                        </div>
                                    </div>

                                    <div className="form-control mb-4">
                                        <label className="label">
                                            <span className="label-text">Company Name</span>
                                        </label>
                                        <input
                                            type="text"
                                            name="name"
                                            className="input input-bordered w-full"
                                            defaultValue={companyInfo.name}
                                            required
                                            maxLength={75}
                                        />
                                    </div>

                                    <div className="form-control mb-6">
                                        <label className="label">
                                            <span className="label-text">Description</span>
                                        </label>
                                        <textarea
                                            name="description"
                                            className="textarea textarea-bordered w-full"
                                            defaultValue={companyInfo.description}
                                            rows={4}
                                            required
                                            maxLength={1000}
                                        ></textarea>
                                    </div>
                                </EditModeForm>
                            ) : (<>
                                <div className="flex gap-4">
                                    <div className="avatar">
                                        <div className="w-24 rounded-md">
                                            <img src={companyInfo.logoUrl ?? "https://upload.wikimedia.org/wikipedia/commons/7/7c/Profile_avatar_placeholder_large.png"} />
                                        </div>
                                    </div>
                                    <div>
                                        <p className="text-2xl font-bold">{companyInfo.name}</p>
                                        <p className="text-sm lg:text-base mt-2 text-base-content/80">{companyInfo.description}</p>

                                    </div>
                                </div>

                                <div className="mt-8">
                                    <div className="grid grid-cols-2 gap-2">
                                        {memberList && memberList}
                                    </div>
                                </div>
                            </>
                            )}
                        </div>
                        <div className="bg-base-100 rounded shadow p-8 mt-4">
                            <h2 className="text-xl font-bold mb-4">Invites</h2>
                        </div>
                    </div>
                )
            }
        </RouteLayout>
    );
}
