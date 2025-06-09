import { useRef, useEffect } from "react";

interface ArchiveWarningModalProps {
  isOpen: boolean;
  onClose: () => void;
  title: string;
  message: string;
}

export default function ArchiveWarningModal({ 
  isOpen, 
  onClose, 
  title, 
  message 
}: ArchiveWarningModalProps) {
  const modalRef = useRef<HTMLDialogElement>(null);

  useEffect(() => {
    if (isOpen) {
      modalRef.current?.showModal();
    } else {
      modalRef.current?.close();
    }
  }, [isOpen]);

  const handleClose = () => {
    onClose();
  };

  return (
    <dialog ref={modalRef} className="modal" onClose={handleClose}>
      <div className="modal-box">
        <h3 className="font-bold text-lg text-warning">{title}</h3>
        <p className="py-4">{message}</p>
        <div className="modal-action">
          <button className="btn btn-primary" onClick={handleClose}>
            OK
          </button>
        </div>
      </div>
      <form method="dialog" className="modal-backdrop">
        <button onClick={handleClose}>close</button>
      </form>
    </dialog>
  );
}
