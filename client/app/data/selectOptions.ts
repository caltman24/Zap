export type SelectOption = {
    value: string;
    label: string;
};

export const projectPriorityOptions: SelectOption[] = [
    {value: "Low", label: "Low"},
    {value: "Medium", label: "Medium"},
    {value: "High", label: "High"},
    {value: "Urgent", label: "Urgent"},
];

export const ticketPriorityOptions: SelectOption[] = [...projectPriorityOptions];

export const ticketStatusOptions: SelectOption[] = [
    {value: "New", label: "New"},
    {value: "In Development", label: "In Development"},
    {value: "Testing", label: "Testing"},
    {value: "Resolved", label: "Resolved"},
];

export const ticketTypeOptions: SelectOption[] = [
    {value: "Defect", label: "Defect"},
    {value: "Feature", label: "Feature"},
    {value: "General Task", label: "General Task"},
    {value: "Work Task", label: "Work Task"},
    {value: "Change Request", label: "Change Request"},
    {value: "Enhancement", label: "Enhancement"},
];
