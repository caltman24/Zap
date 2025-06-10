import { DEV_URL, handleResponse } from "~/utils/api";
import tryCatch from "~/utils/tryCatch";

export interface TicketHistoryEntry {
  id: string;
  type: number;
  oldValue?: string;
  newValue?: string;
  relatedEntityName?: string;
  relatedEntityId?: string;
  creator: {
    id: string;
    name: string;
    avatarUrl?: string;
    role: string;
  };
  createdAt: string;
  formattedMessage: string;
}

async function getTicketHistory(
  ticketId: string,
  accessToken: string
): Promise<TicketHistoryEntry[]> {
  const method = "GET";
  const { data: response, error } = await tryCatch(
    fetch(`${DEV_URL}/tickets/${ticketId}/history`, {
      method,
      headers: {
        Authorization: `Bearer ${accessToken}`,
        "Content-Type": "application/json",
      },
    })
  );

  const result = await handleResponse(response, error, method);
  
  if (result.ok) {
    return await result.json();
  }
  
  throw new Error("Failed to fetch ticket history");
}

export default getTicketHistory;
