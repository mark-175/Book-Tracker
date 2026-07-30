import { useState } from "react";
import type { FormEvent } from "react";
import { addManualBook } from "@/api/books";
import { toCatalogBook } from "@/api/mappers";
import type { CatalogBook } from "@/types/data";

interface Props {
  onCancel: () => void;
  onSubmit: (catalogBook: CatalogBook) => Promise<void>;
}

const inputStyle = {
  background: "var(--color-cream-200)",
  color: "var(--color-bark-700)",
};

const labelClass =
  "text-[10px] font-semibold text-muted-light uppercase tracking-widest";
const inputClass = "w-full mt-1 rounded-xl px-4 py-2.5 text-sm outline-none";

export default function ManualAddForm({ onCancel, onSubmit }: Props) {
  const [title, setTitle] = useState("");
  const [authors, setAuthors] = useState("");
  const [subtitle, setSubtitle] = useState("");
  const [isbn10, setIsbn10] = useState("");
  const [isbn13, setIsbn13] = useState("");
  const [pageCount, setPageCount] = useState("");
  const [description, setDescription] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const canSubmit = title.trim().length > 0 && authors.trim().length > 0;

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (!canSubmit || submitting) return;

    setSubmitting(true);
    setError(null);
    try {
      const dto = await addManualBook({
        title: title.trim(),
        authors: authors.trim(),
        subtitle: subtitle.trim() || undefined,
        isbn10: isbn10.trim() || undefined,
        isbn13: isbn13.trim() || undefined,
        pageCount: pageCount ? Number(pageCount) : undefined,
        description: description.trim() || undefined,
      });
      await onSubmit(toCatalogBook(dto));
    } catch (err) {
      console.error(err);
      setError("Couldn't add that book. Please try again.");
      setSubmitting(false);
    }
  };

  return (
    <>
      <div className="flex items-center gap-3 px-5 py-4 border-b border-border">
        <button
          type="button"
          onClick={onCancel}
          className="w-5 h-5 rounded-full flex items-center justify-center flex-shrink-0"
          style={{ background: "var(--bg-surface)" }}
          aria-label="Back to search"
        >
          <svg
            className="w-3 h-3 text-muted"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            strokeWidth={2.5}
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              d="M6 18L18 6M6 6l12 12"
            />
          </svg>
        </button>
        <p className="font-serif text-bark text-base font-semibold">
          Add a book manually
        </p>
      </div>

      <form
        onSubmit={handleSubmit}
        className="flex flex-col flex-1 overflow-hidden"
      >
        <div className="overflow-y-auto flex-1 px-5 py-4 flex flex-col gap-3">
          <div>
            <label className={labelClass}>Title *</label>
            <input
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              placeholder="Book title"
              required
              className={inputClass}
              style={inputStyle}
            />
          </div>

          <div>
            <label className={labelClass}>Author(s) *</label>
            <input
              value={authors}
              onChange={(e) => setAuthors(e.target.value)}
              placeholder="Comma separated if more than one"
              required
              className={inputClass}
              style={inputStyle}
            />
          </div>

          <div>
            <label className={labelClass}>Subtitle</label>
            <input
              value={subtitle}
              onChange={(e) => setSubtitle(e.target.value)}
              className={inputClass}
              style={inputStyle}
            />
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className={labelClass}>ISBN-10</label>
              <input
                value={isbn10}
                onChange={(e) => setIsbn10(e.target.value)}
                className={inputClass}
                style={inputStyle}
              />
            </div>
            <div>
              <label className={labelClass}>ISBN-13</label>
              <input
                value={isbn13}
                onChange={(e) => setIsbn13(e.target.value)}
                className={inputClass}
                style={inputStyle}
              />
            </div>
          </div>

          <div>
            <label className={labelClass}>Page Count</label>
            <input
              type="number"
              min={1}
              value={pageCount}
              onChange={(e) => setPageCount(e.target.value)}
              className={inputClass}
              style={inputStyle}
            />
          </div>

          <div>
            <label className={labelClass}>Description</label>
            <textarea
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              rows={3}
              className={`${inputClass} resize-none`}
              style={inputStyle}
            />
          </div>

          {error && (
            <p className="text-xs" style={{ color: "var(--color-rust)" }}>
              {error}
            </p>
          )}
        </div>

        <div className="px-5 py-3 border-t border-border flex gap-2">
          <button
            type="button"
            onClick={onCancel}
            className="flex-1 py-2.5 rounded-2xl text-sm font-medium text-muted hover:text-bark hover:bg-surface transition-colors"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={!canSubmit || submitting}
            className="flex-1 py-2.5 rounded-2xl text-sm font-semibold text-white transition-all active:scale-[0.99] disabled:opacity-60"
            style={{ background: "var(--gradient-accent)" }}
          >
            {submitting ? "Adding…" : "Add to library"}
          </button>
        </div>
      </form>
    </>
  );
}
