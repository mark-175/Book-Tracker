import type { Book, ReadingStatus } from "../types/data";
import BookCover from "../components/BookCover";
import StarRating from "../components/StarRating";

interface Props {
  status: ReadingStatus;
  books: Book[];
  onBack: () => void;
  onNavigateToBook: (bookId: string) => void;
}

const STATUS_COLORS: Record<ReadingStatus, string> = {
  "to-read": "#934dee",
  reading: "#934dee",
  read: "#934dee",
};

const STATUS_LABELS: Record<ReadingStatus, string> = {
  "to-read": "To Read",
  reading: "Reading",
  read: "Read",
};

const STATUS_ICONS: Record<ReadingStatus, React.ReactNode> = {
  "to-read": (
    <svg
      className="w-6 h-6"
      fill="none"
      viewBox="0 0 24 24"
      stroke="#b46bdc"
      strokeWidth={1.5}
    >
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        d="M12 6.042A8.967 8.967 0 006 3.75c-1.052 0-2.062.18-3 .512v14.25A8.987 8.987 0 016 18c2.305 0 4.408.867 6 2.292m0-14.25a8.966 8.966 0 016-2.292c1.052 0 2.062.18 3 .512v14.25A8.987 8.987 0 0118 18a8.967 8.967 0 00-6 2.292m0-14.25v14.25"
      />
    </svg>
  ),
  reading: (
    <svg
      className="w-6 h-6"
      fill="none"
      viewBox="0 0 24 24"
      stroke="#b46bdc"
      strokeWidth={1.5}
    >
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        d="M2.036 12.322a1.012 1.012 0 010-.639C3.423 7.51 7.36 4.5 12 4.5c4.638 0 8.573 3.007 9.963 7.178.07.207.07.431 0 .639C20.577 16.49 16.64 19.5 12 19.5c-4.638 0-8.573-3.007-9.963-7.178z"
      />
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"
      />
    </svg>
  ),
  read: (
    <svg
      className="w-6 h-6"
      fill="none"
      viewBox="0 0 24 24"
      stroke="#b46bdc"
      strokeWidth={1.5}
    >
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        d="M9 12.75L11.25 15 15 9.75M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
      />
    </svg>
  ),
};

import type React from "react";

export default function ReadingStatusListPage({
  status,
  books,
  onBack,
  onNavigateToBook,
}: Props) {
  const filtered = books.filter((b) => b.status === status);
  const color = STATUS_COLORS[status];
  const label = STATUS_LABELS[status];

  return (
    <div className="min-h-screen bg-cream pb-8">
      {/* Header */}
      <div className="px-5 pt-14 pb-4">
        <div className="flex items-center gap-3 mb-4">
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

          <div className="flex items-center gap-3 flex-1">
            <div
              className="w-10 h-10 rounded-xl flex items-center justify-center"
              style={{ background: `${color}18` }}
            >
              {STATUS_ICONS[status]}
            </div>
            <div>
              <h1 className="font-serif text-xl font-semibold text-bark">
                {label}
              </h1>
              <p className="text-xs text-muted">
                {filtered.length} {filtered.length === 1 ? "book" : "books"}
              </p>
            </div>
          </div>
        </div>
      </div>

      <div className="px-5 flex flex-col gap-3">
        {filtered.length === 0 && (
          <div
            className="rounded-2xl p-10 flex flex-col items-center text-center"
            style={{ background: "#FDFBF6", border: "1px dashed #DDD4BF" }}
          >
            <div
              className="w-16 h-16 rounded-2xl flex items-center justify-center mb-4"
              style={{ background: `${color}18` }}
            >
              {STATUS_ICONS[status]}
            </div>
            <p className="font-serif text-bark text-lg font-semibold">
              No books here yet
            </p>
            <p className="text-muted text-sm mt-1.5 max-w-[200px]">
              Use the + button on the home page to add books to your library
            </p>
          </div>
        )}

        {filtered.map((book) => (
          <button
            key={book.id}
            onClick={() => onNavigateToBook(book.id)}
            className="w-full rounded-2xl p-4 flex gap-4 items-start text-left transition-all active:scale-[0.99]"
            style={{
              background: "#FDFBF6",
              border: "1px solid #DDD4BF",
              boxShadow: "0 1px 4px rgba(44,26,14,0.05)",
            }}
          >
            <BookCover book={book} size="md" />

            <div className="flex-1 min-w-0 pt-0.5">
              <p className="font-serif font-semibold text-bark text-[15px] leading-snug">
                {book.title}
              </p>
              <p className="text-muted text-sm mt-0.5">{book.author}</p>

              {status === "reading" && (
                <div className="mt-3.5">
                  <div className="flex justify-between items-center mb-1.5">
                    <span className="text-[11px] text-muted">Progress</span>
                    <span className="text-[11px] font-semibold text-bark">
                      {book.pagesRead}/{book.pageCount}
                    </span>
                  </div>
                  <div
                    className="h-1.5 rounded-full overflow-hidden"
                    style={{ background: "#EAE2D0" }}
                  >
                    <div
                      className="h-full rounded-full"
                      style={{
                        width: `${(book.pagesRead / book.pageCount) * 100}%`,
                        background:
                          "linear-gradient(90deg,   #b46bdc, #934dee)",
                      }}
                    />
                  </div>
                  <p className="text-[10px] text-muted-light mt-1">
                    {Math.round((book.pagesRead / book.pageCount) * 100)}%
                    complete
                  </p>
                </div>
              )}

              {status === "read" && (
                <div className="mt-2.5">
                  {book.rating !== null ? (
                    <StarRating rating={book.rating} readonly size="sm" />
                  ) : (
                    <p className="text-[11px] text-muted-light">Not rated</p>
                  )}
                  <p className="text-[10px] text-muted-light mt-1">
                    {book.pageCount} pages
                  </p>
                </div>
              )}

              {status === "to-read" && (
                <p className="text-xs text-muted-light mt-2">
                  {book.pageCount} pages
                </p>
              )}
            </div>

            <svg
              className="w-4 h-4 text-muted-light flex-shrink-0 mt-1"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              strokeWidth={2}
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M8.25 4.5l7.5 7.5-7.5 7.5"
              />
            </svg>
          </button>
        ))}
      </div>
    </div>
  );
}
