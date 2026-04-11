import {FetcherWithComponents} from "@remix-run/react"
import {RefObject, useState} from "react"
import {BasicUserInfo} from "~/services/api.server/types"

export type RemoveMemberListModalProps = {
    projectId?: string,
    members?: BasicUserInfo[] | null,
    error?: string | null,
    actionFetcher: FetcherWithComponents<unknown>
    actionFetcherSubmit: (formData: FormData) => void;
    modalRef: RefObject<HTMLDialogElement> | undefined
}

const secondaryButtonClassName =
    "inline-flex items-center justify-center rounded-xl px-4 py-2.5 text-sm font-medium text-[var(--app-on-surface-variant)] transition-colors hover:bg-[var(--app-hover-overlay)] hover:text-[var(--app-on-surface)]";

const destructiveButtonClassName =
    "inline-flex min-w-32 items-center justify-center gap-2 rounded-xl bg-[var(--app-error-container)]/85 px-4 py-2.5 text-sm font-semibold text-[var(--app-error)] outline outline-1 outline-[var(--app-error)]/15 transition-colors hover:bg-[var(--app-error-container)] disabled:cursor-not-allowed disabled:opacity-60";

export default function RemoveMemberListModal({
                                                  members,
                                                  error,
                                                  actionFetcher,
                                                  actionFetcherSubmit,
                                                  modalRef,
                                              }: RemoveMemberListModalProps) {
    const [selectedMember, setSelectedMember] = useState<{ id: string, name: string } | null>(null)

    function resetSelection() {
        setSelectedMember(null)
    }

    function handleOnMemberSelect(member: { id: string, name: string }) {
        if (selectedMember?.id === member.id) {
            setSelectedMember(null)
            return;
        }

        setSelectedMember(member)
    }

    function handleOnModalClose() {
        modalRef?.current?.close();
    }

    function handleOnActionSubmit() {
        if (!selectedMember) return

        const formData = new FormData()
        formData.append("memberId", selectedMember.id)
        actionFetcherSubmit(formData)
        modalRef?.current?.close()
    }

    return (
        <dialog
            className="m-auto w-full max-w-2xl overflow-visible border-0 bg-transparent px-4 py-0 text-left text-[var(--app-on-surface)] shadow-none backdrop:bg-black/70 backdrop:backdrop-blur-sm sm:px-6 lg:px-0"
            id="member-modal"
            onClick={(event) => {
                if (event.target === event.currentTarget) {
                    handleOnModalClose();
                }
            }}
            onClose={resetSelection}
            ref={modalRef}
        >
            <div
                className="w-full rounded-[2rem] bg-[var(--app-surface-container-low)] p-0 outline outline-1 outline-[var(--app-outline-variant-soft)]">
                <div className="border-b border-[var(--app-outline-variant)]/10 px-6 py-5 sm:px-8">
                    <div>
                        <h3 className="text-xl font-bold tracking-tight text-[var(--app-on-surface)]">Remove Member</h3>
                        <p className="mt-1 text-sm text-[var(--app-on-surface-variant)]">
                            Select a member to remove from this project roster.
                        </p>
                    </div>
                </div>

                <div className="space-y-5 px-6 py-6 sm:px-8">
                    {error && !members ? (
                        <p className="rounded-2xl bg-[var(--app-error-container)]/20 px-4 py-3 text-sm text-[var(--app-error)] outline outline-1 outline-[var(--app-error)]/10">
                            {error}
                        </p>
                    ) : null}

                    {selectedMember ? (
                        <button
                            className="inline-flex items-center gap-2 rounded-full bg-[var(--app-error-container)]/20 px-3 py-1.5 text-sm font-medium text-[var(--app-error)] outline outline-1 outline-[var(--app-error)]/10 transition-colors hover:bg-[var(--app-error-container)]/30"
                            onClick={() => setSelectedMember(null)}
                            type="button"
                        >
                            <span className="material-symbols-outlined text-sm">close</span>
                            {selectedMember.name}
                        </button>
                    ) : null}

                    {members?.length === 0 ? (
                        <div
                            className="rounded-2xl border border-dashed border-[var(--app-outline-variant-soft)] px-6 py-10 text-center text-[var(--app-on-surface-variant)]">
                            <span
                                className="material-symbols-outlined mb-3 text-5xl text-[var(--app-outline)]">group_off</span>
                            <p className="text-base font-medium text-[var(--app-on-surface)]">No members to remove</p>
                            <p className="mt-1 text-sm">This project does not have removable members right now.</p>
                        </div>
                    ) : (
                        <div className="app-shell-scroll max-h-[26rem] space-y-3 overflow-y-auto pr-1">
                            {members?.map((member) => {
                                const isSelected = member.id === selectedMember?.id;

                                return (
                                    <button
                                        className={`flex w-full items-center gap-4 rounded-2xl px-4 py-4 text-left transition-all duration-150 outline outline-1 ${
                                            isSelected
                                                ? "bg-[var(--app-error-container)]/20 text-[var(--app-on-surface)] outline-[var(--app-error)]/15"
                                                : "bg-[var(--app-surface-container-lowest)]/70 text-[var(--app-on-surface)] outline-[var(--app-outline-variant)]/10 hover:bg-[var(--app-surface-container-high)]/20"
                                        }`}
                                        key={`${member.id}-${member.name}`}
                                        onClick={() => handleOnMemberSelect(member)}
                                        type="button"
                                    >
                                        <img
                                            alt={member.name}
                                            className="h-11 w-11 rounded-full border border-[var(--app-outline-variant)]/20 object-cover"
                                            src={member.avatarUrl}
                                        />
                                        <div className="min-w-0 flex-1">
                                            <div className="flex items-center justify-between gap-4">
                                                <p className="truncate text-sm font-semibold text-[var(--app-on-surface)]">{member.name}</p>
                                                <span
                                                    className="text-xs text-[var(--app-on-surface-variant)]">{member.role}</span>
                                            </div>
                                        </div>
                                        {isSelected ? <span
                                            className="material-symbols-outlined text-[var(--app-error)]">remove_circle</span> : null}
                                    </button>
                                );
                            })}
                        </div>
                    )}
                </div>

                <div
                    className="flex justify-end gap-3 border-t border-[var(--app-outline-variant)]/10 px-6 py-5 sm:px-8">
                    <button className={secondaryButtonClassName} onClick={handleOnModalClose} type="button">
                        Close
                    </button>
                    <button
                        className={destructiveButtonClassName}
                        disabled={selectedMember === null || actionFetcher.state === "submitting"}
                        onClick={handleOnActionSubmit}
                        type="button"
                    >
                        {actionFetcher.state === "submitting" ? (
                            <>
                                <span
                                    className="inline-flex h-4 w-4 animate-spin rounded-full border-2 border-current border-r-transparent"/>
                                Removing...
                            </>
                        ) : (
                            "Remove Member"
                        )}
                    </button>
                </div>
            </div>
        </dialog>
    )
}
