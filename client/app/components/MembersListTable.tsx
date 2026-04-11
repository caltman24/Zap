export type MembersListTableProps = {
    members: { id: string; name: string; avatarUrl: string; role: string }[];
    showRole?: boolean;
};

export default function MembersListTable({members, showRole = true}: MembersListTableProps) {
    return (
        <div className="overflow-x-auto overflow-y-auto max-h-[360px]">
            <table className="w-full border-collapse text-left">
                {showRole ? (
                    <thead className="border-b border-[color:var(--app-outline-variant)]/10">
                    <tr>
                        <th className="px-0 py-3 text-[10px] font-medium uppercase tracking-[0.24em] text-[var(--app-outline)]">Member</th>
                        <th className="px-0 py-3 text-[10px] font-medium uppercase tracking-[0.24em] text-[var(--app-outline)]">Role</th>
                    </tr>
                    </thead>
                ) : null}
                <tbody className="divide-y divide-[color:var(--app-outline-variant)]/6">
                {members.map((member) => (
                    <tr className="transition-colors hover:bg-[var(--app-hover-overlay)]/50" key={member.id}>
                        <td className="px-0 py-4">
                            <div className="flex items-center gap-3">
                  <span
                      className="inline-flex h-10 w-10 items-center justify-center overflow-hidden rounded-full bg-[var(--app-surface-container-high)]">
                    <img alt={member.name} className="h-full w-full object-cover" src={member.avatarUrl}/>
                  </span>
                                <span className="text-sm font-medium text-[var(--app-on-surface)]">{member.name}</span>
                            </div>
                        </td>
                        {showRole ?
                            <td className="px-0 py-4 text-sm text-[var(--app-on-surface-variant)]">{member.role}</td> : null}
                    </tr>
                ))}
                </tbody>
            </table>
        </div>
    );
}
