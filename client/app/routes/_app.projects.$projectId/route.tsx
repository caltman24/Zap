import { LoaderFunctionArgs, redirect, ActionFunctionArgs } from "@remix-run/node";
import { Link, useLoaderData, useOutletContext, useParams, useActionData, useNavigation, useFetcher, Form, Outlet } from "@remix-run/react";
import { useEffect, useRef, useState, } from "react";
import { EditModeForm, PrioritySelect } from "~/components/EditModeForm";
import apiClient from "~/services/api.server/apiClient";
import { AuthenticationError } from "~/services/api.server/errors";
import { BasicUserInfo, CompanyMemberPerRole, ProjectResponse, UserInfoResponse } from "~/services/api.server/types";
import { getSession } from "~/services/sessions.server";
import { useEditMode, getPriorityClass, getStatusClass } from "~/utils/editMode";
import { ActionResponse, ActionResponseParams, ForbiddenResponse, JsonResponse, JsonResponseResult } from "~/utils/response";
import tryCatch from "~/utils/tryCatch";
import RouteLayout from "~/layouts/RouteLayout";
import MemberListModal from "./components/MemberListModal";
import roleNames from "~/data/roles";
import { validateRole } from "~/utils/validate";
import permissions from "~/data/permissions";
import RemoveMemberListModal from "./components/RemoveMemberListModal";
import MembersListTable from "~/components/MembersListTable";

export const handle = {
  breadcrumb: (match: any) => {
    const projectId = match.params.projectId;
    const projectName = match.data?.name || "Project Details";
    return <Link to={`/projects/${projectId}`}>{projectName}</Link>;
  },
};

export async function loader({ request, params }: LoaderFunctionArgs) {
  const projectId = params.projectId!;

  const session = await getSession(request);
  const {
    data: tokenResponse,
    error: tokenError
  } = await tryCatch(apiClient.auth.getValidToken(session));

  if (tokenError) {
    return redirect("/logout");
  }

  const { data: project, error } = await tryCatch(
    apiClient.getProjectById(
      projectId,
      tokenResponse.token));

  if (error) {
    return JsonResponse({
      data: null,
      error: error.message,
      headers: tokenResponse.headers
    });
  }

  return JsonResponse({
    data: project,
    error: null,
    headers: tokenResponse.headers
  });
}


export default function ProjectDetailsRoute() {
  const { data: project, error } = useLoaderData<JsonResponseResult<ProjectResponse>>();
  const userInfo = useOutletContext<UserInfoResponse>();
  const userRole = userInfo?.role?.toLowerCase()
  const actionData = useActionData<typeof action>() as ActionResponseParams;
  const navigation = useNavigation();
  const isSubmitting = navigation.state === "submitting";
  const { isEditing, formError, toggleEditMode } = useEditMode({ actionData });
  const archiveFetcher = useFetcher({ key: "archive-project" });
  const getProjectManagersFetcher = useFetcher({ key: "get-project-managers" })
  const addMembersFetcher = useFetcher({ key: "add-members" })
  const removeMemberFetcher = useFetcher({ key: "remove-member" })

  const { projectId } = useParams()

  // State for form fields
  const [priority, setPriority] = useState<string>(project?.priority || "");

  const isAssignedPM = project?.projectManager?.id === userInfo.id
  const canEdit =
    userRole === roleNames.admin ||
    (userRole === roleNames.projectManager && isAssignedPM)


  const modalRef = useRef<HTMLDialogElement>(null)
  const removeMemberModalRef = useRef<HTMLDialogElement>(null)

  // Reset priority when toggling edit mode
  const handleEditToggle = () => {
    if (project) {
      setPriority(project.priority);
    }
    toggleEditMode();
  };

  const handleOnGetPMs = () => {
    if (modalRef && projectId) {
      modalRef.current?.showModal()
      getProjectManagersFetcher.load(`/projects/${projectId}/assignable-pms`)
    }
  }

  const handleOnRemoveMembersList = () => {
    if (removeMemberModalRef && projectId) {
      removeMemberModalRef.current?.showModal()
    }
  }

  return (
    <RouteLayout >
      {error || !project ?
        <p className="text-error mt-4">{error}</p> :
        <>
          <div className="bg-base-100 rounded-lg shadow-lg p-6 mb-6">
            {isEditing ? (
              <EditModeForm
                error={formError}
                isSubmitting={isSubmitting}
                onCancel={handleEditToggle}
              >
                <div className="form-control mb-4">
                  <label className="label">
                    <span className="label-text">Project Name</span>
                  </label>
                  <input
                    type="text"
                    name="name"
                    className="input input-bordered w-full"
                    defaultValue={project?.name}
                    required
                    maxLength={50}
                  />
                </div>

                <div className="form-control mb-4">
                  <label className="label">
                    <span className="label-text">Description</span>
                  </label>
                  <textarea
                    name="description"
                    className="textarea textarea-bordered w-full"
                    defaultValue={project?.description}
                    rows={4}
                    required
                    maxLength={1000}
                  ></textarea>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
                  <PrioritySelect
                    value={priority}
                    onChange={setPriority}
                  />

                  <div className="form-control">
                    <label className="label">
                      <span className="label-text">Due Date</span>
                    </label>
                    <input
                      type="date"
                      name="dueDate"
                      className="input input-bordered w-full"
                      defaultValue={new Date(project!.dueDate).toISOString().split('T')[0]}
                      required
                    />
                  </div>
                </div>
              </EditModeForm>
            ) : (
              // Display project details
              <>
                <div className="flex justify-between items-start">
                  <div>
                    <h1 className="text-3xl font-bold">{project?.name}</h1>
                    <p className="text-base-content/70 mt-2">{project?.description}</p>
                  </div>
                  <div className="flex gap-2 items-center">
                    {canEdit && (
                      <>
                        <div className="dropdown dropdown-center">
                          <div tabIndex={0} role="button" className="btn btn-info flex gap-2 p-3 items-center justify-between">
                            <p>Edit</p>
                            <svg className="w-4 h-7" viewBox="0 0 25 25" fill="none" xmlns="http://www.w3.org/2000/svg">
                              <path d="M11.1808 15.8297L6.54199 9.20285C5.89247 8.27496 6.55629 7 7.68892 7L16.3111 7C17.4437 7 18.1075 8.27496 17.458 9.20285L12.8192 15.8297C12.4211 16.3984 11.5789 16.3984 11.1808 15.8297Z" fill="#33363F" />
                            </svg>
                          </div>
                          <ul tabIndex={0} className="menu dropdown-content bg-base-300 rounded-box z-1 w-52 p-2 shadow-sm mt-1">
                            <li>
                              <a onClick={handleEditToggle} className="flex items-center gap-2">
                                <span className="material-symbols-outlined">edit</span>
                                Project Details
                              </a>
                            </li>
                            <li>
                              <archiveFetcher.Form method="post" action={`/projects/${project.id}/archive`} className="block">
                                <button type="submit" className="flex items-center text-left gap-2 cursor-pointer w-full">
                                  <span className={`material-symbols-outlined ${project?.isArchived ? "text-warning" : ""}`}>folder</span>
                                  <p className="w-full">
                                    {project?.isArchived ? "Unarchive" : "Archive"}
                                  </p>
                                </button>
                              </archiveFetcher.Form>
                            </li>
                            <li>
                              <a onClick={() => handleOnGetPMs()}>
                                <span className="material-symbols-outlined">person_add</span>
                                Assign PM
                              </a>
                            </li>
                          </ul>
                          <MemberListModal
                            modalRef={modalRef}
                            loading={getProjectManagersFetcher.state === "loading"}
                            members={(getProjectManagersFetcher.data as JsonResponseResult<BasicUserInfo[]>)?.data}
                            actionFetcher={addMembersFetcher}
                            projectId={projectId}
                            modalTitle="Select Project Manager"
                            buttonText="Assign"
                          />
                        </div>
                        {/* Archive/Unarchive Project button */}
                      </>
                    )}
                    <Link to="/projects" className="btn btn-outline">
                      <span className="material-symbols-outlined">arrow_back</span>
                      Back
                    </Link>
                  </div>
                </div>

                {/* Project Details */}
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mt-6">
                  <div className="stat bg-base-200 rounded-lg">
                    <div className="stat-title">Priority</div>
                    <div className={`stat-value text-lg ${getPriorityClass(project.priority)}`}>
                      {project.priority}
                    </div>
                  </div>
                  <div className="stat bg-base-200 rounded-lg">
                    <div className="stat-title">Due Date</div>
                    <div className="stat-value text-lg">
                      {new Date(project.dueDate).toLocaleDateString()}
                    </div>
                  </div>
                  <div className="stat bg-base-200 rounded-lg">
                    <div className="stat-title">Project Manager</div>
                    <div className="stat-value text-lg">
                      {project.projectManager
                        ? (<div>
                          <img
                            src={project.projectManager.avatarUrl}
                            alt="Project Manager Avatar"
                            className="w-10 h-10 rounded-full"
                          />
                          <div>{project.projectManager.name}</div>
                        </div>)
                        : ("N/A")}
                    </div>
                  </div>
                </div>
              </>
            )}
          </div>

          {/* Team Members Section */}
          {/* <div className="bg-base-100 rounded-lg shadow-lg p-6 mb-6"> */}
          {/*   <div className="flex justify-between items-center mb-4"> */}
          {/*     <h2 className="text-2xl font-bold">Assigned Members</h2> */}
          {/*     {canEdit && ( */}
          {/*       <div className="flex gap-2"> */}
          {/*         <button className="btn btn-info success btn-sm" onClick={() => handleOnGetMembersList()}> */}
          {/*           <span className="material-symbols-outlined">person_add</span> */}
          {/*           Add */}
          {/*         </button> */}
          {/*         <button className="btn btn-error btn-sm" onClick={() => handleOnRemoveMembersList()}> */}
          {/*           <span className="material-symbols-outlined">person_remove</span> */}
          {/*           Remove */}
          {/*         </button> */}
          {/*         <MemberListModal */}
          {/*           modalRef={modalRef} */}
          {/*           loading={getMembersFetcher.state === "loading"} */}
          {/*           members={(getMembersFetcher.data as JsonResponseResult<CompanyMemberPerRole>)?.data} */}
          {/*           actionFetcher={addMembersFetcher} */}
          {/*           projectId={projectId} */}
          {/*         /> */}
          {/*         <RemoveMemberListModal */}
          {/*           modalRef={removeMemberModalRef} */}
          {/*           projectId={projectId} */}
          {/*           members={project.members.filter(m => m.id !== userInfo.id)} */}
          {/*           actionFetcher={removeMemberFetcher} */}
          {/*         /> */}
          {/*       </div> */}
          {/*     )} */}
          {/*   </div> */}
          {/**/}
          {/*   <MembersListTable members={project.members} /> */}
          {/* </div> */}

          {/* Tickets Section */}
          <div className="bg-base-100 rounded-lg shadow-lg p-6" >
            <div className="flex justify-between items-center mb-4">
              <h2 className="text-2xl font-bold">Tickets</h2>
              <div className="flex gap-2">
                <Link to={`/tickets/new?projectId=${project.id}`} className="btn btn-soft btn-sm">
                  <span className="material-symbols-outlined">add_circle</span>
                  New Ticket
                </Link>
                <Link to={`/projects/${project.id}/tickets`} className="btn btn-outline btn-sm">
                  <span className="material-symbols-outlined">visibility</span>
                  View All
                </Link>
              </div>
            </div>

            {/* Tickets Table */}
            <div className="overflow-x-auto">
              <table className="table table-zebra w-full">
                <thead>
                  <tr>
                    <th>Title</th>
                    <th>Status</th>
                    <th>Priority</th>
                    <th>Assignee</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {/* {tickets.map((ticket) => ( */}
                  {/*   <tr key={ticket.id}> */}
                  {/*     <td>{ticket.title}</td> */}
                  {/*     <td> */}
                  {/*       <div className={`badge ${getStatusClass(ticket.status)}`}> */}
                  {/*         {ticket.status} */}
                  {/*       </div> */}
                  {/*     </td> */}
                  {/*     <td> */}
                  {/*       <div className={`badge ${getPriorityClass(ticket.priority)}`}> */}
                  {/*         {ticket.priority} */}
                  {/*       </div> */}
                  {/*     </td> */}
                  {/*     <td>{ticket.assignee}</td> */}
                  {/*     <td> */}
                  {/*       <Link to={`/tickets/${ticket.id}`} className="btn btn-xs btn-ghost"> */}
                  {/*         <span className="material-symbols-outlined">visibility</span> */}
                  {/*       </Link> */}
                  {/*     </td> */}
                  {/*   </tr> */}
                  {/* ))} */}
                </tbody>
              </table>
            </div>
          </div>
        </>
      }
    </RouteLayout >
  );
}

export async function action({ request, params }: ActionFunctionArgs) {
  const session = await getSession(request);
  const userRole = session.get('user').role

  if (!validateRole(userRole, permissions.project.edit)) {
    return ForbiddenResponse()
  }
  const { data: tokenResponse, error: tokenError } = await tryCatch(
    apiClient.auth.getValidToken(session));

  if (tokenError) {
    return redirect("/logout");
  }

  const projectId = params.projectId!;
  const formData = await request.formData();
  const name = formData.get("name") as string;
  const description = formData.get("description") as string;
  const priority = formData.get("priority") as string;
  const dueDateValue = formData.get("dueDate") as string;
  const dueDate = new Date(dueDateValue).toISOString();

  const projectData = {
    name,
    description,
    priority,
    dueDate
  };

  const { error } = await tryCatch(
    apiClient.updateProject(
      projectId,
      projectData,
      tokenResponse.token));

  if (error instanceof AuthenticationError) {
    return redirect("/logout");
  }

  if (error) {
    return ActionResponse({
      success: false,
      error: error.message
    })
  }

  return ActionResponse({
    success: true,
    error: null
  });
}
