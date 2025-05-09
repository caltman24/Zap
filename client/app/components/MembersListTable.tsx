export type MembersListTableProps = {
    members: { id: string, name: string, avatarUrl: string, role: string }[]
}
export default function MembersListTable({ members }: MembersListTableProps) {
    return (
        <div className="overflow-x-auto overflow-y-auto max-h-[350px]">
            <table className="table">
                {/* head */}
                <thead>
                    <tr>
                        <th>Name</th>
                        <th>Role</th>
                    </tr>
                </thead>
                <tbody>
                    {members.map(member => {
                        return (
                            <tr key={crypto.randomUUID()}>
                                <td>
                                    <div className="flex items-center gap-3">
                                        <div className="avatar">
                                            <div className="mask mask-squircle h-12 w-12">
                                                <img
                                                    src={member.avatarUrl}
                                                    alt="Avatar Tailwind CSS Component" />
                                            </div>
                                        </div>
                                        <div>
                                            <div className="font-bold">{member.name}</div>
                                        </div>
                                    </div>
                                </td>
                                <td>{member.role}</td>
                            </tr>
                        )
                    })}
                </tbody>
            </table>
        </div>

    )
}
