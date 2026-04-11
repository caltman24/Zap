export default function MemberListSkeleton({count = 2}: { count?: number }) {
    return (
        <div className="app-shell-scroll max-h-[26rem] space-y-5 overflow-y-auto pr-1">
            {Array.from({length: count}).map(() => (
                <section className="space-y-3" key={crypto.randomUUID()}>
                    <div className="flex items-center gap-3">
                        <div className="h-3 w-24 animate-pulse rounded-full bg-[var(--app-surface-container-high)]"/>
                        <div className="h-px flex-1 bg-[var(--app-outline-variant)]/10"/>
                    </div>

                    <div className="space-y-3">
                        {Array.from({length: 4}).map(() => (
                            <div
                                className="flex items-center gap-4 rounded-2xl bg-[var(--app-surface-container-lowest)]/70 px-4 py-4 outline outline-1 outline-[var(--app-outline-variant)]/10"
                                key={crypto.randomUUID()}
                            >
                                <div
                                    className="h-11 w-11 animate-pulse rounded-full bg-[var(--app-surface-container-high)]"/>
                                <div className="flex-1 space-y-2">
                                    <div
                                        className="h-4 w-32 animate-pulse rounded-full bg-[var(--app-surface-container-high)]"/>
                                    <div
                                        className="h-3 w-24 animate-pulse rounded-full bg-[var(--app-surface-container-highest)]"/>
                                </div>
                            </div>
                        ))}
                    </div>
                </section>
            ))}
        </div>
    )
}
