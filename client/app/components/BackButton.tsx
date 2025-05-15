import { useNavigate } from "@remix-run/react";

export default function BackButton({ to }: { to?: string }) {
    const navigate = useNavigate();
    return (
        <button onClick={() => to ? navigate({ pathname: to }) : navigate(-1)} className="btn btn-ghost btn-sm text-base max-w-fit">
            <span className="material-symbols-outlined text-primary">arrow_back</span>
            Back
        </button>
    )
}
