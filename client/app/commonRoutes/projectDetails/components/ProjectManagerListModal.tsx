import {FetcherWithComponents} from "@remix-run/react";
import {RefObject, useEffect, useState} from "react";
import {ProjectManagerInfo} from "~/services/api.server/types";
import MemberListSkeleton from "./MemberListSkeleton";

export type MemberListModalProps = {
    modalRef: RefObject<HTMLDialogElement> | undefined,
    members?: ProjectManagerInfo[] | null,
    loading: boolean,
    error?: string | null
    currentPM: {
        id: string;
        name: string;
        avatarUrl: string;
        role: string;
    } | null
    actionFetcherSubmit: (formData: FormData) => void
    actionFetcher: FetcherWithComponents<unknown>
    modalTitle: string
    buttonText: string
}

const secondaryButtonClassName =
    "inline-flex items-center justify-center rounded-xl px-4 py-2.5 text-sm font-medium text-[var(--app-on-surface-variant)] transition-colors hover:bg-[var(--app-hover-overlay)] hover:text-[var(--app-on-surface)]";

const primaryButtonClassName =
    "inline-flex min-w-28 items-center justify-center gap-2 rounded-xl bg-[linear-gradient(135deg,var(--app-primary)_0%,var(--app-primary-fixed)_100%)] px-4 py-2.5 text-sm font-bold text-[#1000a9] transition-all duration-200 hover:opacity-95 active:scale-95 disabled:cursor-not-allowed disabled:opacity-60";

export default function ProjectManagerListModal({
                                                    modalRef,
                                                    members,
                                                    error,
                                                    loading,
                                                    currentPM,
                                                    actionFetcherSubmit,
                                                    actionFetcher,
                                                    modalTitle,
                                                    buttonText,
                                                }: MemberListModalProps) {
    const [selectedMember, setSelectedMember] = useState<{ id: string; name: string } | null>(null);

    function resetSelection() {
        setSelectedMember(null);
    }

    function handleOnMemberSelect(member: { id: string; name: string }) {
        if (currentPM?.id === member.id) return;

        if (selectedMember?.id === member.id) {
            setSelectedMember(null);
            return;
        }

        setSelectedMember(member);
    }

    function handleOnModalClose() {
        modalRef?.current?.close();
    }

    function handleAction() {
        if (!selectedMember) return;

        const formData = new FormData();
        formData.append("memberId", selectedMember.id);

        actionFetcherSubmit(formData);
    }

    useEffect(() => {
        if (actionFetcher.data && modalRef) {
            modalRef.current?.close();
        }
    }, [actionFetcher.data, modalRef]);

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
                        <h3 className="text-xl font-bold tracking-tight text-[var(--app-on-surface)]">{modalTitle}</h3>
                        <p className="mt-1 text-sm text-[var(--app-on-surface-variant)]">
                            Choose the member who should lead this project from here.
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
                            className="inline-flex items-center gap-2 rounded-full bg-[var(--app-primary-fixed)]/10 px-3 py-1.5 text-sm font-medium text-[var(--app-primary)] outline outline-1 outline-[var(--app-primary-fixed)]/15 transition-colors hover:bg-[var(--app-primary-fixed)]/15"
                            onClick={() => setSelectedMember(null)}
                            type="button"
                        >
                            <span className="material-symbols-outlined text-sm">close</span>
                            {selectedMember.name}
                        </button>
                    ) : null}

                    {loading ? (
                        <MemberListSkeleton count={1}/>
                    ) : members?.length === 0 ? (
                        <div
                            className="rounded-2xl border border-dashed border-[var(--app-outline-variant-soft)] px-6 py-10 text-center text-[var(--app-on-surface-variant)]">
                            <span
                                className="material-symbols-outlined mb-3 text-5xl text-[var(--app-outline)]">group_off</span>
                            <p className="text-base font-medium text-[var(--app-on-surface)]">No more members to
                                select</p>
                            <p className="mt-1 text-sm">There are no other eligible project managers right now.</p>
                        </div>
                    ) : (
                        <div className="app-shell-scroll max-h-[26rem] space-y-3 overflow-y-auto pr-1">
                            {members?.map((member) => {
                                const isCurrent = member.id === currentPM?.id;
                                const isSelected = member.id === selectedMember?.id;

                                return (
                                    <button
                                        className={`flex w-full items-center gap-4 rounded-2xl px-4 py-4 text-left transition-all duration-150 outline outline-1 ${
                                            isSelected
                                                ? "bg-[var(--app-primary-fixed)]/10 text-[var(--app-on-surface)] outline-[var(--app-primary-fixed)]/20"
                                                : "bg-[var(--app-surface-container-lowest)]/70 text-[var(--app-on-surface)] outline-[var(--app-outline-variant)]/10 hover:bg-[var(--app-surface-container-high)]/20"
                                        } ${isCurrent ? "opacity-70" : ""}`}
                                        disabled={isCurrent}
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
                                            <div className="flex flex-wrap items-center gap-2">
                                                <p className="truncate text-sm font-semibold text-[var(--app-on-surface)]">{member.name}</p>
                                                {isCurrent ? (
                                                    <span
                                                        className="app-shell-mono text-[10px] uppercase tracking-[0.22em] text-[var(--app-outline)]">Already assigned</span>
                                                ) : null}
                                            </div>
                                            <p className="mt-1 text-xs text-[var(--app-on-surface-variant)]">{member.role}</p>
                                        </div>
                                        {isSelected ? <span
                                            className="material-symbols-outlined text-[var(--app-primary)]">check_circle</span> : null}
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
                        className={primaryButtonClassName}
                        disabled={selectedMember === null || selectedMember.id === currentPM?.id || actionFetcher.state === "submitting"}
                        onClick={handleAction}
                        type="button"
                    >
                        {actionFetcher.state === "submitting" ? (
                            <>
                                <span
                                    className="inline-flex h-4 w-4 animate-spin rounded-full border-2 border-current border-r-transparent"/>
                                Saving...
                            </>
                        ) : (
                            buttonText
                        )}
                    </button>
                </div>
            </div>
        </dialog>
    );
}
