import type {SelectHTMLAttributes} from "react";
import SelectControl from "./SelectControl";

type TicketSelectControlProps = SelectHTMLAttributes<HTMLSelectElement> & {
    className?: string;
};

export default function TicketSelectControl({
                                                children,
                                                className = "",
                                                ...props
                                            }: TicketSelectControlProps) {
    return <SelectControl className={className} controlSize="sm" {...props}>{children}</SelectControl>;
}
