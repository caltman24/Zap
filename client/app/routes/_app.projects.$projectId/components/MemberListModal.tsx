import { Await, FetcherWithComponents, useNavigate } from "@remix-run/react";
import { LegacyRef, RefObject, Suspense, useEffect, useRef, useState } from "react";
import { BasicUserInfo, CompanyMemberPerRole } from "~/services/api.server/types";
import MemberListSkeleton from "./MemberListSkeleton";

export type MemberListModalProps = {
  modalRef: RefObject<HTMLDialogElement> | undefined,
  members?: BasicUserInfo[] | null,
  loading: boolean,
  error?: string | null
  actionFetcher: FetcherWithComponents<unknown>
  projectId?: string
  modalTitle: string
  buttonText: string
}
export default function MemberListModal({
  modalRef,
  members,
  error,
  loading,
  actionFetcher,
  projectId,
  modalTitle,
  buttonText }: MemberListModalProps) {
  const [selectedMember, setSelectedMember] = useState<{ id: string; name: string } | null>(null);

  const memberSelectItemClassName = (memberId: string) =>
    selectedMember?.id === memberId
      ? "bg-base-200"
      : ""

  function handleOnMemberSelect(member: any) {
    if (selectedMember?.id === member.id) {
      setSelectedMember(null)
      return;
    }
    setSelectedMember(member)
  }

  function handleOnModalClose() {
    modalRef?.current?.close();
    setSelectedMember(null)
  }

  function handleAddMembersToProject() {
    if (!selectedMember) return;

    const formData = new FormData();
    formData.append("memberId", selectedMember.id)

    actionFetcher.submit(formData, {
      method: "post",
      action: `/projects/${projectId}/assign-pm`
    })

    setSelectedMember(null)
  }

  useEffect(() => {
    if (actionFetcher.data && modalRef) {
      modalRef.current?.close()
    }
  }, [actionFetcher.data])

  return (
    <dialog id="member-modal" className="modal" ref={modalRef}>
      {error || !members && <p className="text-error text-sm">{error}</p>}
      <div className="modal-box ">
        <h3 className="font-bold text-lg mb-2">{modalTitle}</h3>
        {selectedMember && (
          <div key={selectedMember.id} className="flex items-center gap-1 cursor-pointer"
            onClick={() => setSelectedMember(null)}>
            <span className="text-error">X</span>
            <p className="badge badge-neutral cursor-pointer hover:bg-neutral-900">
              {selectedMember.name}
            </p>
          </div>
        )}
        <div className="mt-4">
          {loading ? <MemberListSkeleton /> : (
            <>
              {members?.length === 0
                ? <p>No more members to select</p>
                : (<ul className="list rounded bg-base-300 max-h-[450px] overflow-y-auto">
                  {members?.map(m => {
                    return (
                      <li key={`${m.id}-${m.name}`}
                        className={`list-row flex items-center cursor-pointer hover:bg-base-200 rounded ${memberSelectItemClassName(m.id)}`}
                        onClick={() => handleOnMemberSelect(m)}>
                        <div className="flex gap-4 items-center">
                          <div className="avatar rounded-full w-10 h-10">
                            <img src={m.avatarUrl} className="w-full h-auto rounded-full" />
                          </div>
                          <p className="">{m.name}</p>
                        </div>
                      </li>
                    )
                  })}
                </ul>
                )}
            </>
          )}
        </div>
        <div className="modal-action">
          <button
            disabled={selectedMember === null}
            onClick={() => handleAddMembersToProject()}
            type="submit"
            className={`btn  ${selectedMember === null ? "btn-soft" : "btn-primary"}`}>
            {actionFetcher.state === "submitting" ?
              <span className={"loading loading-spinner loading-sm"}></span> :
              <>{buttonText}</>}
          </button>
          <button className="btn" onClick={() => handleOnModalClose()}>Close</button>
        </div>
      </div>
    </dialog >
  )
}
