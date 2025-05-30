import { FetcherWithComponents } from "@remix-run/react"
import { RefObject, useEffect, useState } from "react"
import { BasicUserInfo, CompanyMemberPerRole } from "~/services/api.server/types"

export type RemoveMemberListModalProps = {
    members?: BasicUserInfo[] | null,
    currentMember?: BasicUserInfo
    error?: string | null,
    actionFetcher: FetcherWithComponents<unknown>
    actionFetcherSubmit: (formData: FormData) => void;
    modalRef: RefObject<HTMLDialogElement> | undefined
}

export default function RemoveMemberListModal({
    members,
    currentMember,
    error,
    actionFetcher,
    actionFetcherSubmit,
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
        if (currentMember?.id === member.id) {
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
        actionFetcherSubmit(formData)
        setSelectedMember(null)
        modalRef?.current?.close()
    }

    useEffect(() => {
        if (actionFetcher.data && modalRef) {
            setSelectedMember(null)
            modalRef.current?.close()
        }
    }, [actionFetcher.data])

    const buttonDisabled = () => selectedMember === null || selectedMember.id === currentMember?.id;

    return (
        <dialog id="member-modal" className="modal" ref={modalRef}>
            {error || !members && <p className="text-error text-sm">{error}</p>}
            <div className="modal-box ">
                <h3 className="font-bold text-lg mb-2">Assign Developer</h3>
                {selectedMember && (
                    <p className="badge badge-neutral">
                        {selectedMember.name}
                    </p>
                )}
                <div className="mt-4">
                    <>
                        {members?.length === 0
                            ? <p>No more members to remove</p>
                            : (<ul className="list rounded bg-base-300 max-h-[450px] overflow-y-auto">
                                {members?.map(m => {
                                    return (
                                        <li key={`${m.id}-${m.name}`}
                                            className={`list-row w-full flex flex-col gap-2 cursor-pointer hover:bg-base-200 rounded ${memberSelectItemClassName(m.id)}`}
                                            onClick={() => handleOnMemberSelect(m)}>
                                            {m.id === currentMember?.id && <p className="text-info/40 text-xs">Already Assigned</p>}
                                            <div className="flex items-center gap-2">
                                                <div className="flex gap-4 items-center">
                                                    <div className="avatar rounded-full w-10 h-10">
                                                        <img src={m.avatarUrl} className="w-full h-auto rounded-full" />
                                                    </div>
                                                </div>
                                                <div className="w-full flex justify-between items-center">
                                                    <p className="">{m.name}</p>
                                                    <p className="text-neutral-content/40">{m.role}</p>
                                                </div>
                                            </div>
                                        </li>)
                                })}
                            </ul>
                            )}
                    </>
                </div>
                <div className="modal-action">
                    <button
                        disabled={buttonDisabled() || selectedMember === null}
                        onClick={() => handleOnActionSubmit()}
                        type="submit"
                        className={`btn  ${buttonDisabled() ? "btn-soft" : "btn-primary"}`}>
                        {actionFetcher.state === "submitting" ?
                            <span className={"loading loading-spinner loading-sm"}></span> :
                            <>Assign</>}
                    </button>
                    <button className="btn" onClick={() => handleOnModalClose()}>Close</button>
                </div>
            </div>
        </dialog >
    )
}
