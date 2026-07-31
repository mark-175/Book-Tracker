interface Props {
  title: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
  danger?: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

export default function ConfirmDialog({
  title,
  message,
  confirmLabel = "Confirm",
  cancelLabel = "Cancel",
  danger = false,
  onConfirm,
  onCancel,
}: Props) {
  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center px-5"
      style={{ background: "var(--overlay-bg)", backdropFilter: "blur(6px)" }}
      onClick={(e) => e.target === e.currentTarget && onCancel()}
    >
      <div
        className="w-full rounded-2xl p-5"
        style={{
          maxWidth: 360,
          background: "var(--bg-card)",
          boxShadow: "var(--shadow-modal)",
        }}
      >
        <h2 className="font-serif text-lg font-semibold text-bark">
          {title}
        </h2>
        <p className="text-sm text-muted mt-2 leading-relaxed">{message}</p>
        <div className="flex gap-3 mt-5">
          <button
            onClick={onCancel}
            className="flex-1 py-2.5 rounded-xl text-sm font-semibold text-bark transition-colors"
            style={{ background: "var(--border-card)" }}
          >
            {cancelLabel}
          </button>
          <button
            onClick={onConfirm}
            className="flex-1 py-2.5 rounded-xl text-sm font-semibold text-white transition-colors"
            style={{
              background: danger ? "var(--color-rust)" : "var(--gradient-accent)",
            }}
          >
            {confirmLabel}
          </button>
        </div>
      </div>
    </div>
  );
}
