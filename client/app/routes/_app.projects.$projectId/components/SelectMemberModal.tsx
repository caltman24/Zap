import { FetcherWithComponents } from "@remix-run/react"

export type SelectMemberModalProps = {
  memberList: any[],
  fetcher: FetcherWithComponents<unknown>,
  modalId: string
}
export default function SelectMemberModal({ memberList, fetcher, modalId }: SelectMemberModalProps) {
  return (
    <dialog id={modalId} className="modal">
      <div className="modal-box">
        <h3 className="font-bold text-lg mb-8">Select Members</h3>
        <div>
          <ul className="list rounded bg-base-300 max-h-[350px] overflow-y-auto">
            <li className="list-row flex items-center cursor-pointer hover:bg-base-200 rounded">
              <div className="avatar-placeholder w-10 h-10 bg-amber-300 rounded-full">
              </div>
              <div className="flex gap-1 text-md font-medium">
                <p>Corbyn</p> <p>Altman</p>
              </div>
            </li>
            <li className="list-row flex items-center cursor-pointer hover:bg-base-200 rounded">
              <div className="avatar-placeholder w-10 h-10 bg-amber-300 rounded-full">
              </div>
              <div className="flex gap-1 text-md font-medium">
                <p>Corbyn</p> <p>Altman</p>
              </div>
            </li>
            <li className="list-row flex items-center cursor-pointer hover:bg-base-200 rounded">
              <div className="avatar-placeholder w-10 h-10 bg-amber-300 rounded-full">
              </div>
              <div className="flex gap-1 text-md font-medium">
                <p>Corbyn</p> <p>Altman</p>
              </div>
            </li>
            <li className="list-row flex items-center cursor-pointer hover:bg-base-200 rounded">
              <div className="avatar-placeholder w-10 h-10 bg-amber-300 rounded-full">
              </div>
              <div className="flex gap-1 text-md font-medium">
                <p>Corbyn</p> <p>Altman</p>
              </div>
            </li>
          </ul>
          {/* Add Member(s) submit */}
          <fetcher.Form>
          </fetcher.Form>
        </div>
        <div className="modal-action">
          <form method="dialog">
            {/* if there is a button in form, it will close the modal */}
            <button className="btn">Close</button>
          </form>
        </div>
      </div>
    </dialog>
  )
}
