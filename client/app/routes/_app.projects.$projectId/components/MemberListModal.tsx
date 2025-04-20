import { Await, FetcherWithComponents, useNavigate } from "@remix-run/react";
import { LegacyRef, RefObject, Suspense, useEffect, useRef, useState } from "react";
import { CompanyMemberPerRole } from "~/services/api.server/types";

const MemberListSkeleton = () => (
  <ul className="list rounded bg-base-300 max-h-[450px] overflow-y-auto ">
    {[...Array(4)].map(_ => (
      <li key={crypto.randomUUID()} className="list-row flex flex-col">
        <div className="skeleton w-12 h-6"></div>
        <ul className="list">
          {[...Array(3)].map(_ => (
            <li key={crypto.randomUUID()} className={`list-row flex items-center cursor-pointer hover:bg-base-200 rounded`}>
              <div className="flex gap-4 items-center">
                <div className="avatar rounded-full w-10 h-10 skeleton">
                  <div className="w-full h-auto rounded-full"></div>
                </div>
                <p className="skeleton w-24 h-6"></p>
              </div>
            </li>
          ))}
        </ul>
      </li>
    ))}
  </ul>
)

export type MemberListModalProps = {
  modalRef: RefObject<HTMLDialogElement> | undefined,
  members?: CompanyMemberPerRole | null,
  loading: boolean,
  error?: string | null
  addMembersFetcher: FetcherWithComponents<unknown>
  projectId?: string
}
export default function MemberListModal({ modalRef, members, error, loading, addMembersFetcher, projectId }: MemberListModalProps) {
  const [selectedMembers, setSelectedMembers] = useState<{ id: string; name: string }[]>([]);

  const memberSelectItemClassName = (memberId: string) =>
    selectedMembers.find(x => x.id === memberId) !== undefined
      ? "bg-base-200"
      : ""

  function handleOnMemberSelect(member: any) {
    if (selectedMembers.find(x => x.id === member.id)) {
      setSelectedMembers(prev => prev.filter(x => x.id !== member.id))
      return;
    }
    setSelectedMembers(prev => [...prev, member])
  }

  return (
    <dialog id="member-modal" className="modal" ref={modalRef}>
      {error || !members && <p className="text-error text-sm">{error}</p>}
      <div className="modal-box ">
        <h3 className="font-bold text-lg mb-2">Select Members</h3>
        <ul className="flex flex-wrap gap-2">
          {selectedMembers.length > 0 && (
            selectedMembers.map(member => (
              <li key={member.id} className="flex items-center gap-1 cursor-pointer"
                onClick={() =>
                  setSelectedMembers(prev =>
                    prev.filter(x => x.id !== member.id)
                  )}>
                <span className="text-error">X</span>
                <p className="badge badge-neutral cursor-pointer hover:bg-neutral-900">
                  {member.name}
                </p>
              </li>
            )))}
        </ul>
        <div className="mt-4">
          {loading && <MemberListSkeleton />}
          {!loading && Object.keys(members ?? {}).length === 0
            ? <p>No more members to add</p>
            : (<ul className="list rounded bg-base-300 max-h-[450px] overflow-y-auto">
              {Object.entries(members ?? {})
                .map(([role, m]) => {
                  return (
                    <li key={role} className="list-row flex flex-col gap-2">
                      <p className="font-bold">{role}</p>
                      <ul className="list">
                        {m.map((x: any) =>
                        (<li key={`${x.id}-${x.name}`}
                          className={`list-row flex items-center cursor-pointer hover:bg-base-200 rounded ${memberSelectItemClassName(x.id)}`}
                          onClick={() => handleOnMemberSelect(x)}>
                          <div className="flex gap-4 items-center">
                            <div className="avatar rounded-full w-10 h-10">
                              <img src={x.avatarUrl} className="w-full h-auto rounded-full" />
                            </div>
                            <p className="">{x.name}</p>
                          </div>
                        </li>))}
                      </ul>
                    </li>
                  )
                })}
            </ul>
            )}
        </div>
        <div className="modal-action">
          <addMembersFetcher.Form method="post" action={`/projects/${projectId}/add-members`}>
            <button
              disabled={selectedMembers.length === 0}
              type="submit"
              className={`btn  ${selectedMembers.length === 0 ? "btn-soft" : "btn-primary"}`}>
              Add Members
            </button>
          </addMembersFetcher.Form>
          <form method="dialog">
            <button className="btn">Close</button>
          </form>
        </div>
      </div>
    </dialog >
  )
}
