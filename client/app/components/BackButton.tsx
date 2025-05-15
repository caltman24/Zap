import { useNavigate } from "@remix-run/react";

export default function BackButton() {
    const navigate = useNavigate();
    return (
        <button onClick={() => navigate(-1)} className="btn btn-outline btn-sm">
            <span className="material-symbols-outlined">arrow_back</span>
            Back
        </button>
    )
}
