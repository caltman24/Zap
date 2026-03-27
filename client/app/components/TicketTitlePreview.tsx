import type { BasicTicketInfo } from "~/services/api.server/types";
import { getTicketTypeChipClass, truncateTicketText } from "./ticketTableUtils";

type TicketTitlePreviewProps = {
  ticket: BasicTicketInfo;
  descriptionLength?: number;
  titleClassName?: string;
  descriptionClassName?: string;
};

export default function TicketTitlePreview({
  ticket,
  descriptionLength = 72,
  titleClassName = "text-[1rem] font-semibold text-[var(--app-on-surface)]",
  descriptionClassName = "text-xs text-[var(--app-on-surface-variant)]",
}: TicketTitlePreviewProps) {
  return (
    <div className="space-y-2">
      <div className="flex flex-wrap items-center gap-2">
        <span className={titleClassName}>{ticket.name}</span>
        <span className={`inline-flex rounded-md px-2 py-1 text-[10px] font-medium ${getTicketTypeChipClass(ticket.type)}`}>
          {ticket.type}
        </span>
      </div>
      <p className={descriptionClassName}>{truncateTicketText(ticket.description, descriptionLength)}</p>
    </div>
  );
}
