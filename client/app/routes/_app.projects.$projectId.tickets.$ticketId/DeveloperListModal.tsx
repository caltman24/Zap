import type { FetcherWithComponents } from "@remix-run/react";
import { useEffect, useState, type RefObject } from "react";
import type { BasicUserInfo } from "~/services/api.server/types";

export type RemoveMemberListModalProps = {
  members?: BasicUserInfo[] | null;
  currentMember?: BasicUserInfo;
  error?: string | null;
  actionFetcher: FetcherWithComponents<unknown>;
  actionFetcherSubmit: (formData: FormData) => void;
  modalRef: RefObject<HTMLDialogElement> | undefined;
};

export default function RemoveMemberListModal({
  members,
  currentMember,
  error,
  actionFetcher,
  actionFetcherSubmit,
  modalRef,
}: RemoveMemberListModalProps) {
  const [selectedMember, setSelectedMember] = useState<{ id: string; name: string } | null>(null);

  function handleOnMemberSelect(member: { id: string; name: string }) {
    if (currentMember?.id === member.id) {
      return;
    }

    if (selectedMember?.id === member.id) {
      setSelectedMember(null);
      return;
    }

    setSelectedMember(member);
  }

  function handleOnModalClose() {
    modalRef?.current?.close();
    setSelectedMember(null);
  }

  function handleOnActionSubmit() {
    if (!selectedMember) return;

    const formData = new FormData();
    formData.append("memberId", selectedMember.id);
    actionFetcherSubmit(formData);
  }

  useEffect(() => {
    if (actionFetcher.data && modalRef) {
      setSelectedMember(null);
      modalRef.current?.close();
    }
  }, [actionFetcher.data, modalRef]);

  const buttonDisabled = selectedMember === null || selectedMember.id === currentMember?.id;

  return (
    <dialog
      className="m-auto w-full max-w-2xl overflow-visible border-0 bg-transparent p-0 text-left text-[var(--app-on-surface)] shadow-none backdrop:bg-black/70 backdrop:backdrop-blur-sm"
      id="member-modal"
      onClick={(event) => {
        if (event.target === event.currentTarget) {
          handleOnModalClose();
        }
      }}
      onClose={() => setSelectedMember(null)}
      ref={modalRef}
    >
      <div className="w-full rounded-[2rem] bg-[var(--app-surface-container-low)] p-0 outline outline-1 outline-[var(--app-outline-variant-soft)]">
        <div className="border-b border-[var(--app-outline-variant)]/10 px-6 py-5 sm:px-8">
          <div>
            <h3 className="text-xl font-bold tracking-tight text-[var(--app-on-surface)]">Assign Developer</h3>
            <p className="mt-1 text-sm text-[var(--app-on-surface-variant)]">
              Pick a developer to own the implementation of this ticket.
            </p>
          </div>
        </div>

        <div className="px-6 py-6 sm:px-8">
          {error && !members ? <p className="text-sm text-[var(--app-error)]">{error}</p> : null}

          {members?.length === 0 ? (
            <div className="rounded-2xl border border-dashed border-[var(--app-outline-variant-soft)] px-6 py-10 text-center text-[var(--app-on-surface-variant)]">
              <span className="material-symbols-outlined mb-3 text-5xl text-[var(--app-outline)]">group_off</span>
              <p className="text-base font-medium text-[var(--app-on-surface)]">No developers available</p>
              <p className="mt-1 text-sm">There are no additional developers available to assign right now.</p>
            </div>
          ) : (
            <ul className="max-h-[26rem] space-y-3 overflow-y-auto pr-1">
              {members?.map((member) => {
                const isCurrent = member.id === currentMember?.id;
                const isSelected = selectedMember?.id === member.id;

                return (
                  <li key={member.id}>
                    <button
                      className={`flex w-full items-center gap-4 rounded-2xl px-4 py-4 text-left transition-all duration-150 outline outline-1 ${
                        isSelected
                          ? "bg-[var(--app-primary-fixed)]/10 text-[var(--app-on-surface)] outline-[var(--app-primary-fixed)]/20"
                          : "bg-[var(--app-surface-container-lowest)]/70 text-[var(--app-on-surface)] outline-[var(--app-outline-variant)]/10 hover:bg-[var(--app-surface-container-high)]/20"
                      } ${isCurrent ? "opacity-70" : ""}`}
                      disabled={isCurrent}
                      onClick={() => handleOnMemberSelect(member)}
                      type="button"
                    >
                      <img alt={member.name} className="h-11 w-11 rounded-full border border-[var(--app-outline-variant)]/20 object-cover" src={member.avatarUrl} />
                      <div className="min-w-0 flex-1">
                        <div className="flex flex-wrap items-center gap-2">
                          <p className="truncate text-sm font-semibold text-[var(--app-on-surface)]">{member.name}</p>
                          {isCurrent ? (
                            <span className="app-shell-mono text-[10px] uppercase tracking-[0.22em] text-[var(--app-outline)]">Current</span>
                          ) : null}
                        </div>
                        <p className="mt-1 text-xs text-[var(--app-on-surface-variant)]">{member.role}</p>
                      </div>
                      {isSelected ? <span className="material-symbols-outlined text-[var(--app-primary)]">check_circle</span> : null}
                    </button>
                  </li>
                );
              })}
            </ul>
          )}
        </div>

        <div className="flex justify-end gap-3 border-t border-[var(--app-outline-variant)]/10 px-6 py-5 sm:px-8">
          <button
            className="inline-flex items-center justify-center rounded-xl px-4 py-2.5 text-sm font-medium text-[var(--app-on-surface-variant)] transition-colors hover:bg-[var(--app-hover-overlay)] hover:text-[var(--app-on-surface)]"
            onClick={handleOnModalClose}
            type="button"
          >
            Close
          </button>
          <button
            className="inline-flex min-w-28 items-center justify-center gap-2 rounded-xl bg-[linear-gradient(135deg,var(--app-primary)_0%,var(--app-primary-fixed)_100%)] px-4 py-2.5 text-sm font-bold text-[#1000a9] transition-all duration-200 hover:opacity-95 active:scale-95 disabled:cursor-not-allowed disabled:opacity-60"
            disabled={buttonDisabled}
            onClick={handleOnActionSubmit}
            type="button"
          >
            {actionFetcher.state === "submitting" ? (
              <>
                <span className="inline-flex h-4 w-4 animate-spin rounded-full border-2 border-current border-r-transparent" />
                Assigning...
              </>
            ) : (
              "Assign"
            )}
          </button>
        </div>
      </div>
    </dialog>
  );
}
