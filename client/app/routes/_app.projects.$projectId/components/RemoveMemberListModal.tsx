import { FetcherWithComponents } from "@remix-run/react"
import { RefObject, useState } from "react"
import { CompanyMemberPerRole } from "~/services/api.server/types"
import MemberListSkeleton from "./MemberListSkeleton"

export type RemoveMemberListModalProps = {
  projectId?: string,
  members?: { id: string, name: string, avatarUrl: string, role: string }[] | null,
  error?: string | null,
  actionFetcher: FetcherWithComponents<unknown>
  modalRef: RefObject<HTMLDialogElement> | undefined
}

export default function RemoveMemberListModal({
  projectId,
  members,
  error,
  actionFetcher,
  modalRef }: RemoveMemberListModalProps) {
  const [selectedMember, setSelectedMember] = useState<{ id: string, name: string } | null>(null)

  const memberSelectItemClassName = (memberId: string) =>
    selectedMember?.id === memberId
      ? "bg-base-200"
      : ""

  function handleOnMemberSelect(member: { id: string, name: string }) {
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

  function handleOnActionSubmit() {
    if (!selectedMember) return

    const formData = new FormData()
    formData.append("memberId", selectedMember.id)
    actionFetcher.submit(formData, {
      method: "post",
      action: `/projects/${projectId}/remove-member`
    })
    setSelectedMember(null)
    modalRef?.current?.close()
  }


  return (
    <dialog id="member-modal" className="modal" ref={modalRef}>
      {error || !members && <p className="text-error text-sm">{error}</p>}
      <div className="modal-box ">
        <h3 className="font-bold text-lg mb-2">Select Members</h3>
        {selectedMember && (
          <li key={selectedMember.id}
            className="flex items-center gap-1 cursor-pointer"
            onClick={() => setSelectedMember(null)}>
            <span className="text-error">X</span>
            <p className="badge badge-neutral cursor-pointer hover:bg-neutral-900">
              {selectedMember.name}
            </p>
          </li>
        )}
        <div className="mt-4">
          <>
            {members?.length === 0
              ? <p>No more members to remove</p>
              : (<ul className="list rounded bg-base-300 max-h-[450px] overflow-y-auto">
                {members?.map(m => {
                  return (
                    <li key={`${m.id}-${m.name}`}
                      className={`list-row flex w-fullitems-center cursor-pointer hover:bg-base-200 rounded ${memberSelectItemClassName(m.id)}`}
                      onClick={() => handleOnMemberSelect(m)}>
                      <div className="flex gap-4 items-center">
                        <div className="avatar rounded-full w-10 h-10">
                          <img src={m.avatarUrl} className="w-full h-auto rounded-full" />
                        </div>
                      </div>
                      <div className="w-full flex justify-between items-center">
                        <p className="">{m.name}</p>
                        <p className="text-neutral-content/40">{m.role}</p>
                      </div>
                    </li>)
                })}
              </ul>
              )}
          </>
        </div>
        <div className="modal-action">
          <button
            disabled={selectedMember === null}
            onClick={() => handleOnActionSubmit()}
            type="submit"
            className={`btn  ${selectedMember === null ? "btn-soft" : "btn-error text-error-content"}`}>
            {actionFetcher.state === "submitting" ?
              <span className={"loading loading-spinner loading-sm"}></span> :
              <>Remove Member</>}
          </button>
          <button className="btn" onClick={() => handleOnModalClose()}>Close</button>
        </div>
      </div>
    </dialog >
  )
}
