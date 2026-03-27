import {
  type ActionFunctionArgs,
  type HeadersFunction,
  type LoaderFunctionArgs,
  redirect,
} from "@remix-run/node";
import { Form, Link, useActionData, useLoaderData, useNavigation, useOutletContext } from "@remix-run/react";
import { useEffect, useRef, useState } from "react";
import MembersListTable from "~/components/MembersListTable";
import { formInputClassName, formTextareaClassName } from "~/components/FormShell";
import RouteLayout from "~/layouts/RouteLayout";
import apiClient from "~/services/api.server/apiClient";
import { AuthenticationError } from "~/services/api.server/errors";
import type { CompanyInfoResponse, UserInfoResponse } from "~/services/api.server/types";
import { getSession } from "~/services/sessions.server";
import { useEditMode } from "~/utils/editMode";
import { ActionResponse, type ActionResponseResult, ForbiddenResponse, JsonResponse } from "~/utils/response";
import { hasPermission } from "~/utils/permissions";
import tryCatch from "~/utils/tryCatch";

const companyPlaceholderLogo =
  "https://upload.wikimedia.org/wikipedia/commons/7/7c/Profile_avatar_placeholder_large.png";

export const handle = {
  breadcrumb: () => <Link to="/company">Company</Link>,
  breadcrumbLabel: "Company",
};

export async function loader({ request }: LoaderFunctionArgs) {
  const session = await getSession(request);
  const user = session.get("user") as UserInfoResponse;

  if (!hasPermission(user.permissions, "company.edit")) {
    return redirect("/dashboard");
  }

  const { data: tokenResponse, error: tokenError } = await tryCatch(apiClient.auth.getValidToken(session));

  if (tokenError) {
    return redirect("/logout");
  }

  const { data: res, error } = await tryCatch(apiClient.getCompanyInfo(tokenResponse.token));

  if (error instanceof AuthenticationError) {
    return redirect("/logout");
  }

  if (error) {
    return JsonResponse({
      data: null,
      error: error.message,
      headers: tokenResponse.headers,
    });
  }

  return JsonResponse({
    data: res,
    error: null,
    headers: tokenResponse.headers,
  });
}

export async function action({ request }: ActionFunctionArgs) {
  const session = await getSession(request);
  const user = session.get("user") as UserInfoResponse;

  if (!hasPermission(user.permissions, "company.edit")) {
    return ForbiddenResponse();
  }

  const { data: tokenResponse, error: tokenError } = await tryCatch(apiClient.auth.getValidToken(session));
  if (tokenError) {
    return redirect("/logout");
  }

  const formData = await request.formData();
  const name = formData.get("name") as string;
  const description = formData.get("description") as string;
  const websiteUrl = (formData.get("websiteUrl") as string) || null;
  const removeLogo = formData.get("removeLogo") === "true";

  const requestFormData = new FormData();
  requestFormData.append("name", name);
  requestFormData.append("description", description);
  requestFormData.append("websiteUrl", websiteUrl ?? "");
  requestFormData.append("removeLogo", removeLogo.toString());

  const logoFile = formData.get("logo") as File;
  const maxMB = 2;
  if (logoFile && logoFile.size > maxMB * 1024 * 1024) {
    return ActionResponse({
      success: false,
      error: `Logo file is too large. Maximum size is ${maxMB}MB.`,
      headers: tokenResponse.headers,
    });
  }

  if (logoFile && logoFile.size > 0) {
    requestFormData.append("file", logoFile);
  }

  const { error } = await tryCatch(apiClient.updateCompanyInfo(requestFormData, tokenResponse.token));

  if (error instanceof AuthenticationError) {
    return redirect("/logout");
  }

  if (error) {
    return ActionResponse({ success: false, error: error.message, headers: tokenResponse.headers });
  }

  return ActionResponse({ success: true, error: null, headers: tokenResponse.headers });
}

function formatRoleLabel(role: string) {
  return role.replace(/([a-z])([A-Z])/g, "$1 $2");
}

export default function CompanyRoute() {
  const { data, error } = useLoaderData<typeof loader>();
  const actionData = useActionData<typeof action>() as ActionResponseResult;
  const userData = useOutletContext<UserInfoResponse>();
  const navigation = useNavigation();
  const isSubmitting = navigation.state === "submitting";
  const canEdit = hasPermission(userData.permissions, "company.edit");

  const { isEditing, formError, toggleEditMode } = useEditMode({ actionData });

  const [removeLogo, setRemoveLogo] = useState(false);
  const [previewImage, setPreviewImage] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const companyInfo = data as CompanyInfoResponse | null;

  const memberGroups = Object.entries(companyInfo?.members ?? {}).map(([role, members]) => ({
    role,
    label: formatRoleLabel(role),
    members: members as { id: string; name: string; avatarUrl: string; role: string }[],
  }));

  function handleFileChange(event: React.ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0];
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
  }

  function handleRemoveLogoChange() {
    setRemoveLogo(!removeLogo);
    if (!removeLogo) {
      setPreviewImage(null);
      if (fileInputRef.current) {
        fileInputRef.current.value = "";
      }
    }
  }

  useEffect(() => {
    if (!isEditing) {
      setPreviewImage(null);
    }
  }, [isEditing]);

  if (error) {
    return (
      <RouteLayout>
        <div className="rounded-[1.5rem] bg-[var(--app-surface-container-low)] p-6 text-[var(--app-error)] outline outline-1 outline-[var(--app-outline-variant-soft)]">
          {error}
        </div>
      </RouteLayout>
    );
  }

  if (!companyInfo) {
    return null;
  }

  return (
    <RouteLayout className="space-y-10">
      <section className="space-y-8 border-b border-[var(--app-outline-variant)]/10 pb-10">
        {isEditing ? (
          <div className="space-y-8">
            <div className="flex items-start justify-between gap-4">
              <div>
                <h1 className="text-3xl font-bold tracking-[-0.03em] text-[var(--app-on-surface)]">Edit Company</h1>
                <p className="mt-1 max-w-2xl text-sm text-[var(--app-on-surface-variant)] sm:text-base">
                  Update the core company profile and branding shown across the workspace.
                </p>
              </div>
            </div>

            <Form className="space-y-8" encType="multipart/form-data" method="post">
              {formError ? (
                <div className="rounded-2xl bg-[var(--app-error-container)]/20 px-4 py-3 text-sm text-[var(--app-error)] outline outline-1 outline-[var(--app-error)]/10">
                  {formError}
                </div>
              ) : null}

              <div className="flex flex-col gap-6 lg:flex-row lg:items-start">
                <div className="flex flex-col gap-4 lg:w-72">
                  <div className="overflow-hidden rounded-2xl bg-[var(--app-surface-container-low)] outline outline-1 outline-[var(--app-outline-variant-soft)]">
                    <div className="aspect-square w-full bg-[var(--app-surface-container-lowest)]">
                      {!removeLogo ? (
                        <img
                          alt="Company logo preview"
                          className="h-full w-full object-cover"
                          src={previewImage || companyInfo.logoUrl || companyPlaceholderLogo}
                        />
                      ) : (
                        <div className="grid h-full place-items-center text-[var(--app-outline)]">
                          <span className="material-symbols-outlined text-6xl">image_not_supported</span>
                        </div>
                      )}
                    </div>
                  </div>

                  <div className="space-y-3">
                    <input
                      accept="image/jpg, image/jpeg, image/png, image/svg+xml, image/webp"
                      className="block w-full cursor-pointer text-sm text-[var(--app-on-surface)] file:mr-4 file:cursor-pointer file:rounded-xl file:border-0 file:bg-[var(--app-surface-container-high)] file:px-4 file:py-2.5 file:text-sm file:font-medium file:text-[var(--app-on-surface)] hover:file:bg-[var(--app-surface-container-highest)]"
                      disabled={removeLogo}
                      name="logo"
                      onChange={handleFileChange}
                      ref={fileInputRef}
                      type="file"
                    />

                    <button
                      className={`inline-flex w-full items-center justify-center rounded-xl px-4 py-2.5 text-sm font-medium transition-colors ${
                        removeLogo
                          ? "bg-[var(--app-tertiary-container)]/20 text-[var(--app-tertiary)] outline outline-1 outline-[var(--app-tertiary)]/15"
                          : "bg-[var(--app-surface-container-high)] text-[var(--app-on-surface-variant)] hover:bg-[var(--app-surface-container-highest)] hover:text-[var(--app-on-surface)]"
                      }`}
                      onClick={handleRemoveLogoChange}
                      type="button"
                    >
                      {removeLogo ? "Logo removal enabled" : "Remove logo"}
                    </button>
                    <input name="removeLogo" type="hidden" value={removeLogo.toString()} />
                  </div>
                </div>

                <div className="min-w-0 flex-1 space-y-6">
                  <div>
                    <label className="mb-2 block text-sm font-medium text-[var(--app-on-surface)]">Company Name</label>
                    <input
                      className={formInputClassName}
                      defaultValue={companyInfo.name}
                      maxLength={75}
                      name="name"
                      required
                      type="text"
                    />
                  </div>

                  <div>
                    <label className="mb-2 block text-sm font-medium text-[var(--app-on-surface)]">Description</label>
                    <textarea
                      className={formTextareaClassName}
                      defaultValue={companyInfo.description}
                      maxLength={1000}
                      name="description"
                      required
                      rows={5}
                    />
                  </div>

                  <div className="flex justify-end gap-3 border-t border-[var(--app-outline-variant)]/10 pt-5">
                    <button
                      className="inline-flex min-w-28 items-center justify-center rounded-xl px-4 py-2.5 text-sm font-medium text-[var(--app-on-surface-variant)] transition-colors hover:bg-[var(--app-hover-overlay)] hover:text-[var(--app-on-surface)]"
                      disabled={isSubmitting}
                      onClick={toggleEditMode}
                      type="button"
                    >
                      Cancel
                    </button>
                    <button
                      className="inline-flex min-w-36 items-center justify-center gap-2 rounded-xl bg-[linear-gradient(135deg,var(--app-primary)_0%,var(--app-primary-fixed)_100%)] px-5 py-3 text-sm font-bold text-[#1000a9] transition-all duration-200 hover:opacity-95 active:scale-95 disabled:cursor-not-allowed disabled:opacity-60"
                      disabled={isSubmitting}
                      type="submit"
                    >
                      {isSubmitting ? "Saving..." : "Save Changes"}
                    </button>
                  </div>
                </div>
              </div>
            </Form>
          </div>
        ) : (
          <div className="flex flex-col gap-6 lg:flex-row lg:items-start lg:justify-between">
            <div className="flex min-w-0 flex-1 flex-col gap-5 sm:flex-row sm:items-start">
              <div className="overflow-hidden rounded-2xl bg-[var(--app-surface-container-low)] outline outline-1 outline-[var(--app-outline-variant-soft)]">
                <div className="h-24 w-24 bg-[var(--app-surface-container-lowest)] sm:h-28 sm:w-28">
                  <img alt={`${companyInfo.name} logo`} className="h-full w-full object-cover" src={companyInfo.logoUrl ?? companyPlaceholderLogo} />
                </div>
              </div>

              <div className="min-w-0 flex-1 space-y-3">
                <h1 className="text-3xl font-bold tracking-[-0.03em] text-[var(--app-on-surface)]">{companyInfo.name}</h1>
                <p className="max-w-3xl text-sm leading-6 text-[var(--app-on-surface-variant)] sm:text-base sm:leading-7">
                  {companyInfo.description}
                </p>
              </div>
            </div>

            {canEdit ? (
              <button
                className="inline-flex items-center gap-2 self-start rounded-xl px-4 py-2.5 text-sm font-medium text-[var(--app-on-surface-variant)] outline outline-1 outline-[var(--app-outline-variant-soft)] transition-colors hover:bg-[var(--app-hover-overlay)] hover:text-[var(--app-on-surface)]"
                onClick={toggleEditMode}
                type="button"
              >
                <span className="material-symbols-outlined text-lg">edit</span>
                Edit Company
              </button>
            ) : null}
          </div>
        )}
      </section>

      <section className="space-y-6">
        <div>
          <h2 className="text-xl font-bold tracking-tight text-[var(--app-on-surface)]">Members</h2>
          <p className="mt-1 text-sm text-[var(--app-on-surface-variant)] sm:text-base">
            Team members grouped by role across your company.
          </p>
        </div>

        <div className="grid gap-x-10 gap-y-8 lg:grid-cols-2">
          {memberGroups.map((group) => (
            <section className="space-y-3 border-t border-[var(--app-outline-variant)]/10 pt-4" key={group.role}>
              <div className="flex items-center justify-between gap-3">
                <h3 className="text-base font-semibold text-[var(--app-on-surface)]">{group.label}</h3>
                <span className="app-shell-mono text-[10px] uppercase tracking-[0.22em] text-[var(--app-outline)]">
                  {group.members.length} {group.members.length === 1 ? "member" : "members"}
                </span>
              </div>
              <MembersListTable members={group.members} showRole={false} />
            </section>
          ))}
        </div>
      </section>
    </RouteLayout>
  );
}

export const headers: HeadersFunction = ({ actionHeaders, loaderHeaders, parentHeaders }) => {
  const headers = new Headers(parentHeaders);

  const setCookie = actionHeaders.get("Set-Cookie") || loaderHeaders.get("Set-Cookie");
  if (setCookie) {
    headers.append("Set-Cookie", setCookie);
  }

  return headers;
};
