export default function MemberListSkeleton({ count = 2 }: { count?: number }) {
  return (
    <ul className="list rounded bg-base-300 max-h-[450px] overflow-y-auto ">
      {[...Array(count)].map(_ => (
        <li key={crypto.randomUUID()} className="list-row flex flex-col">
          <div className="skeleton w-12 h-6"></div>
          <ul className="list">
            {[...Array(4)].map(_ => (
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
}
