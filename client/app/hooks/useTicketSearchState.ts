import {useMemo, useState} from "react";

export const DEFAULT_TICKET_SEARCH_QUERY = "";

export default function useTicketSearchState(initialValue = DEFAULT_TICKET_SEARCH_QUERY) {
    const [searchQuery, setSearchQuery] = useState(initialValue);

    const normalizedSearchQuery = useMemo(() => searchQuery.trim().toLowerCase(), [searchQuery]);

    return {
        searchQuery,
        setSearchQuery,
        normalizedSearchQuery,
    };
}
