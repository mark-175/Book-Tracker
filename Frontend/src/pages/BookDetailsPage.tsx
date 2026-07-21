import { useState } from "react";
import type { Book, ReadingStatus } from "../types/data";
import BookCover from "../components/BookCover";
import StarRating from "../components/StarRating";

interface Props {
  book: Book;
  onBack: () => void;
  onNavigateToProgress: (bookId: string) => void;
  onUpdateBook: (id: string, updates: Partial<Book>) => void;
}

const STATUS_COLORS: Record<ReadingStatus, string> = {
  "to-read": "#9B8E7A",
  reading: "#A85830",
  read: "#527A52",
};

const STATUS_LABELS: Record<ReadingStatus, string> = {
  "to-read": "To Read",
  reading: "Reading",
  read: "Read",
};

export default function BookDetailsPage({
  book,
  onBack,
  onNavigateToProgress,
  onUpdateBook,
}: Props) {
  const [editingNotes, setEditingNotes] = useState(false);
  const [notes, setNotes] = useState(book.notes);

  const color = STATUS_COLORS[book.status];
  const label = STATUS_LABELS[book.status];
  const pct =
    book.pageCount > 0
      ? Math.round((book.pagesRead / book.pageCount) * 100)
      : 0;

  const saveNotes = () => {
    onUpdateBook(book.id, { notes });
    setEditingNotes(false);
  };

  const changeStatus = (s: ReadingStatus) => {
    const updates: Partial<Book> = { status: s };
    if (s === "read") updates.pagesRead = book.pageCount;
    if (s === "to-read") updates.pagesRead = 0;
    onUpdateBook(book.id, updates);
  };

  return (
    <div className="min-h-screen bg-cream pb-8">
      {/* Back header */}
      <div className="flex items-center gap-3 px-5 pt-14 pb-4">
        <button
          onClick={onBack}
          className="w-9 h-9 rounded-full flex items-center justify-center transition-colors"
          style={{ background: "#FDFBF6", border: "1px solid #DDD4BF" }}
        >
          <svg
            className="w-4 h-4 text-bark"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            strokeWidth={2}
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              d="M15.75 19.5L8.25 12l7.5-7.5"
            />
          </svg>
        </button>
        <h1 className="font-serif text-lg font-semibold text-bark">
          Book Details
        </h1>
      </div>

      {/* Cover + title hero */}
      <div className="px-5 mb-5">
        <div
          className="rounded-2xl p-5 flex gap-5"
          style={{
            background: "#FDFBF6",
            border: "1px solid #DDD4BF",
            boxShadow: "0 2px 10px rgba(44,26,14,0.06)",
          }}
        >
          <BookCover book={book} size="xl" />
          <div className="flex-1 min-w-0 flex flex-col justify-between">
            <div>
              <span
                className="text-[10px] font-semibold px-2.5 py-1 rounded-full inline-block mb-2.5"
                style={{ background: `${color}18`, color }}
              >
                {label}
              </span>
              <h2 className="font-serif text-xl font-semibold text-bark leading-snug">
                {book.title}
              </h2>
              {book.subtitle && (
                <p className="text-muted text-xs mt-1 leading-snug">
                  {book.subtitle}
                </p>
              )}
              <p className="text-bark-700 font-medium text-sm mt-2.5">
                {book.author}
              </p>
            </div>
            {book.status === "read" && book.rating !== null && (
              <div className="mt-3">
                <StarRating rating={book.rating} readonly size="sm" />
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Reading progress (only if currently reading) */}
      {book.status === "reading" && (
        <div className="px-5 mb-4">
          <button
            onClick={() => onNavigateToProgress(book.id)}
            className="w-full rounded-2xl p-4 text-left transition-all active:scale-[0.99]"
            style={{ background: "#FDFBF6", border: "1px solid #DDD4BF" }}
          >
            <div className="flex items-center justify-between mb-3">
              <p className="text-sm font-semibold text-bark">
                Reading Progress
              </p>
              <div
                className="flex items-center gap-1 text-xs font-semibold"
                style={{ color: "#A85830" }}
              >
                <span>Update</span>
                <svg
                  className="w-3.5 h-3.5"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                  strokeWidth={2.5}
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    d="M8.25 4.5l7.5 7.5-7.5 7.5"
                  />
                </svg>
              </div>
            </div>
            <div className="flex justify-between items-center mb-2">
              <span className="text-xs text-muted">
                {book.pagesRead} of {book.pageCount} pages
              </span>
              <span className="text-xs font-semibold text-bark">{pct}%</span>
            </div>
            <div
              className="h-2 rounded-full overflow-hidden"
              style={{ background: "#EAE2D0" }}
            >
              <div
                className="h-full rounded-full"
                style={{
                  width: `${pct}%`,
                  background: "linear-gradient(90deg, #A85830, #C8963E)",
                }}
              />
            </div>
          </button>
        </div>
      )}

      {/* Description */}
      <div className="px-5 mb-4">
        <div
          className="rounded-2xl p-4"
          style={{ background: "#FDFBF6", border: "1px solid #DDD4BF" }}
        >
          <p className="text-[10px] font-semibold text-muted-light uppercase tracking-widest mb-2.5">
            Description
          </p>
          <p className="text-bark text-sm leading-relaxed">
            {book.description}
          </p>
        </div>
      </div>

      {/* Metadata */}
      <div className="px-5 mb-4">
        <div
          className="rounded-2xl p-4"
          style={{ background: "#FDFBF6", border: "1px solid #DDD4BF" }}
        >
          <p className="text-[10px] font-semibold text-muted-light uppercase tracking-widest mb-3">
            Details
          </p>
          <div className="grid grid-cols-2 gap-y-4">
            <div>
              <p className="text-[10px] text-muted-light uppercase tracking-wide mb-1">
                Pages
              </p>
              <p className="font-serif text-2xl font-bold text-bark">
                {book.pageCount}
              </p>
            </div>
            <div>
              <p className="text-[10px] text-muted-light uppercase tracking-wide mb-1">
                ISBN-10
              </p>
              <p className="font-mono text-sm text-bark">{book.isbn10}</p>
            </div>
            <div className="col-span-2">
              <p className="text-[10px] text-muted-light uppercase tracking-wide mb-1">
                ISBN-13
              </p>
              <p className="font-mono text-sm text-bark">{book.isbn13}</p>
            </div>
          </div>
        </div>
      </div>

      {/* Notes */}
      <div className="px-5 mb-4">
        <div
          className="rounded-2xl p-4"
          style={{ background: "#FDFBF6", border: "1px solid #DDD4BF" }}
        >
          <div className="flex items-center justify-between mb-2.5">
            <p className="text-[10px] font-semibold text-muted-light uppercase tracking-widest">
              Notes
            </p>
            {editingNotes ? (
              <button
                onClick={saveNotes}
                className="text-xs font-semibold"
                style={{ color: "#A85830" }}
              >
                Save
              </button>
            ) : (
              <button
                onClick={() => setEditingNotes(true)}
                className="text-xs font-semibold text-muted hover:text-bark transition-colors"
              >
                Edit
              </button>
            )}
          </div>
          <textarea
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            onFocus={() => setEditingNotes(true)}
            placeholder="Add your thoughts, quotes, or reflections…"
            className="w-full bg-transparent text-bark text-sm leading-relaxed resize-none outline-none placeholder-muted-light"
            style={{ minHeight: 80 }}
          />
        </div>
      </div>

      {/* Change status */}
      <div className="px-5">
        <div
          className="rounded-2xl p-4"
          style={{ background: "#FDFBF6", border: "1px solid #DDD4BF" }}
        >
          <p className="text-[10px] font-semibold text-muted-light uppercase tracking-widest mb-3">
            Change Status
          </p>
          <div className="flex gap-2">
            {(["to-read", "reading", "read"] as ReadingStatus[]).map((s) => {
              const active = book.status === s;
              const c = STATUS_COLORS[s];
              return (
                <button
                  key={s}
                  onClick={() => changeStatus(s)}
                  className="flex-1 py-2.5 rounded-xl text-xs font-semibold transition-all"
                  style={
                    active
                      ? { background: c, color: "#fff" }
                      : { background: "#EAE2D0", color: "#9B8E7A" }
                  }
                >
                  {STATUS_LABELS[s]}
                </button>
              );
            })}
          </div>
        </div>
      </div>
    </div>
  );
}
