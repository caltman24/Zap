import {FetcherWithComponents} from "@remix-run/react";
import {RefObject, useEffect, useState} from "react";
import {CompanyMemberPerRole} from "~/services/api.server/types";
import MemberListSkeleton from "./MemberListSkeleton";

export type MemberListModalProps = {
    modalRef: RefObject<HTMLDialogElement> | undefined,
    members?: CompanyMemberPerRole | null,
    loading: boolean,
    error?: string | null
    actionFetcher: FetcherWithComponents<unknown>
    actionFetcherSubmit: (formData: FormData) => void;
    projectId?: string
}

const secondaryButtonClassName =
    "inline-flex items-center justify-center rounded-xl px-4 py-2.5 text-sm font-medium text-[var(--app-on-surface-variant)] transition-colors hover:bg-[var(--app-hover-overlay)] hover:text-[var(--app-on-surface)]";

const primaryButtonClassName =
    "inline-flex min-w-32 items-center justify-center gap-2 rounded-xl bg-[linear-gradient(135deg,var(--app-primary)_0%,var(--app-primary-fixed)_100%)] px-4 py-2.5 text-sm font-bold text-[#1000a9] transition-all duration-200 hover:opacity-95 active:scale-95 disabled:cursor-not-allowed disabled:opacity-60";

export default function MemberListModal({
                                            modalRef,
                                            members,
                                            error,
                                            loading,
                                            actionFetcher,
                                            actionFetcherSubmit,
                                        }: MemberListModalProps) {
    const [selectedMembers, setSelectedMembers] = useState<{ id: string; name: string }[]>([]);

    function resetSelection() {
        setSelectedMembers([]);
    }

    function handleOnMemberSelect(member: { id: string; name: string }) {
        if (selectedMembers.find((selectedMember) => selectedMember.id === member.id)) {
            setSelectedMembers((previous) => previous.filter((selectedMember) => selectedMember.id !== member.id));
            return;
        }

        setSelectedMembers((previous) => [...previous, member]);
    }

    function handleOnModalClose() {
        modalRef?.current?.close();
    }

    function handleAddMembersToProject() {
        if (selectedMembers.length === 0) return;

        const formData = new FormData();
        for (const member of selectedMembers) {
            formData.append("memberId", member.id);
        }

        actionFetcherSubmit(formData);
    }

    useEffect(() => {
        if (actionFetcher.data && modalRef) {
            modalRef.current?.close();
        }
    }, [actionFetcher.data, modalRef]);

    return (
        <dialog
            className="m-auto w-full max-w-3xl overflow-visible border-0 bg-transparent px-4 py-0 text-left text-[var(--app-on-surface)] shadow-none backdrop:bg-black/70 backdrop:backdrop-blur-sm sm:px-6 lg:px-0"
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
                        <h3 className="text-xl font-bold tracking-tight text-[var(--app-on-surface)]">Select
                            Members</h3>
                        <p className="mt-1 text-sm text-[var(--app-on-surface-variant)]">
                            Pick one or more company members to add to this project.
                        </p>
                    </div>
                </div>

                <div className="space-y-5 px-6 py-6 sm:px-8">
                    {error && !members ? (
                        <p className="rounded-2xl bg-[var(--app-error-container)]/20 px-4 py-3 text-sm text-[var(--app-error)] outline outline-1 outline-[var(--app-error)]/10">
                            {error}
                        </p>
                    ) : null}

                    {selectedMembers.length > 0 ? (
                        <ul className="flex flex-wrap gap-2">
                            {selectedMembers.map((member) => (
                                <li key={member.id}>
                                    <button
                                        className="inline-flex items-center gap-2 rounded-full bg-[var(--app-primary-fixed)]/10 px-3 py-1.5 text-sm font-medium text-[var(--app-primary)] outline outline-1 outline-[var(--app-primary-fixed)]/15 transition-colors hover:bg-[var(--app-primary-fixed)]/15"
                                        onClick={() => setSelectedMembers((previous) => previous.filter((selectedMember) => selectedMember.id !== member.id))}
                                        type="button"
                                    >
                                        <span className="material-symbols-outlined text-sm">close</span>
                                        {member.name}
                                    </button>
                                </li>
                            ))}
                        </ul>
                    ) : null}

                    {loading ? (
                        <MemberListSkeleton/>
                    ) : Object.keys(members ?? {}).length === 0 ? (
                        <div
                            className="rounded-2xl border border-dashed border-[var(--app-outline-variant-soft)] px-6 py-10 text-center text-[var(--app-on-surface-variant)]">
                            <span
                                className="material-symbols-outlined mb-3 text-5xl text-[var(--app-outline)]">group_off</span>
                            <p className="text-base font-medium text-[var(--app-on-surface)]">No more members to add</p>
                            <p className="mt-1 text-sm">Everyone available for this project is already assigned.</p>
                        </div>
                    ) : (
                        <div className="app-shell-scroll max-h-[26rem] space-y-5 overflow-y-auto pr-1">
                            {Object.entries(members ?? {}).map(([role, roleMembers]) => (
                                <section key={role} className="space-y-3">
                                    <div className="flex items-center gap-3">
                                        <p className="app-shell-mono text-[10px] uppercase tracking-[0.22em] text-[var(--app-outline)]">{role}</p>
                                        <div className="h-px flex-1 bg-[var(--app-outline-variant)]/10"/>
                                    </div>

                                    <div className="space-y-3">
                                        {roleMembers.map((member) => {
                                            const isSelected = selectedMembers.some((selectedMember) => selectedMember.id === member.id);

                                            return (
                                                <button
                                                    className={`flex w-full items-center gap-4 rounded-2xl px-4 py-4 text-left transition-all duration-150 outline outline-1 ${
                                                        isSelected
                                                            ? "bg-[var(--app-primary-fixed)]/10 text-[var(--app-on-surface)] outline-[var(--app-primary-fixed)]/20"
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
                                                        <p className="truncate text-sm font-semibold text-[var(--app-on-surface)]">{member.name}</p>
                                                        <p className="mt-1 text-xs text-[var(--app-on-surface-variant)]">{role}</p>
                                                    </div>
                                                    {isSelected ? <span
                                                        className="material-symbols-outlined text-[var(--app-primary)]">check_circle</span> : null}
                                                </button>
                                            );
                                        })}
                                    </div>
                                </section>
                            ))}
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
                        disabled={selectedMembers.length === 0 || actionFetcher.state === "submitting"}
                        onClick={handleAddMembersToProject}
                        type="button"
                    >
                        {actionFetcher.state === "submitting" ? (
                            <>
                                <span
                                    className="inline-flex h-4 w-4 animate-spin rounded-full border-2 border-current border-r-transparent"/>
                                Adding...
                            </>
                        ) : (
                            "Add Members"
                        )}
                    </button>
                </div>
            </div>
        </dialog>
    );
}
