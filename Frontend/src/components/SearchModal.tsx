import { useState } from "react";
import type { Book, CatalogBook } from "../types/data";
import { SEARCH_CATALOG } from "../types/data";
import BookCover from "./BookCover";

interface Props {
  books: Book[];
  onClose: () => void;
  onSelectBook: (bookId: string) => void;
  onAddBook: (catalogBook: CatalogBook) => void;
}

const STATUS_COLORS: Record<string, string> = {
  "to-read": "var(--status-to-read)",
  reading: "var(--status-reading)",
  read: "var(--status-read)",
};

const STATUS_SUBTLE: Record<string, string> = {
  "to-read": "var(--status-to-read-subtle)",
  reading: "var(--status-reading-subtle)",
  read: "var(--status-read-subtle)",
};

const STATUS_LABELS: Record<string, string> = {
  "to-read": "To Read",
  reading: "Reading",
  read: "Read",
};

export default function SearchModal({
  books,
  onClose,
  onSelectBook,
  onAddBook,
}: Props) {
  const [query, setQuery] = useState("");
  const q = query.toLowerCase().trim();

  const libraryResults = books.filter(
    (b) =>
      b.title.toLowerCase().includes(q) || b.author.toLowerCase().includes(q),
  );

  const catalogResults =
    q.length > 1
      ? SEARCH_CATALOG.filter(
          (b) =>
            !books.find((lb) => lb.id === b.id) &&
            (b.title.toLowerCase().includes(q) ||
              b.author.toLowerCase().includes(q)),
        )
      : [];

  const hasResults = libraryResults.length > 0 || catalogResults.length > 0;

  return (
    <div
      className="fixed inset-0 z-50 flex flex-col"
      style={{ background: "var(--overlay-bg)", backdropFilter: "blur(6px)" }}
      onClick={(e) => e.target === e.currentTarget && onClose()}
    >
      <div
        className="mt-14 mx-4 rounded-3xl overflow-hidden flex flex-col"
        style={{
          background: "var(--bg-card)",
          maxHeight: "74vh",
          boxShadow: "var(--shadow-modal)",
        }}
      >
        {/* Search input */}
        <div className="flex items-center gap-3 px-5 py-4 border-b border-border">
          <svg
            className="w-4 h-4 flex-shrink-0"
            fill="none"
            viewBox="0 0 24 24"
            stroke="var(--color-muted)"
            strokeWidth={2}
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              d="m21 21-5.197-5.197m0 0A7.5 7.5 0 105.197 5.197a7.5 7.5 0 0010.606 10.606z"
            />
          </svg>
          <input
            autoFocus
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder="Search by title or author…"
            className="flex-1 bg-transparent text-bark text-[15px] outline-none placeholder-muted-light font-sans"
          />
          {query && (
            <button
              onClick={() => setQuery("")}
              className="w-5 h-5 rounded-full flex items-center justify-center flex-shrink-0"
              style={{ background: "var(--bg-surface)" }}
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
          )}
        </div>

        {/* Results */}
        <div className="overflow-y-auto flex-1">
          {!q && (
            <div className="flex flex-col items-center justify-center py-14 px-6 text-center">
              <div
                className="w-14 h-14 rounded-full flex items-center justify-center mb-4"
                style={{ background: "var(--bg-surface)" }}
              >
                <svg
                  className="w-6 h-6 text-muted"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                  strokeWidth={1.5}
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    d="M12 6.042A8.967 8.967 0 006 3.75c-1.052 0-2.062.18-3 .512v14.25A8.987 8.987 0 016 18c2.305 0 4.408.867 6 2.292m0-14.25a8.966 8.966 0 016-2.292c1.052 0 2.062.18 3 .512v14.25A8.987 8.987 0 0118 18a8.967 8.967 0 00-6 2.292m0-14.25v14.25"
                  />
                </svg>
              </div>
              <p className="font-serif text-bark text-lg font-semibold">
                Find a book
              </p>
              <p className="text-muted text-sm mt-1">
                Search your library or discover new books to add
              </p>
            </div>
          )}

          {q && !hasResults && (
            <div className="flex flex-col items-center justify-center py-14 px-6 text-center">
              <p className="font-serif text-bark text-lg font-semibold">
                No results
              </p>
              <p className="text-muted text-sm mt-1">
                Try a different title or author name
              </p>
            </div>
          )}

          {libraryResults.length > 0 && (
            <>
              <div className="px-5 pt-4 pb-1">
                <p className="text-[10px] font-semibold text-muted-light uppercase tracking-widest">
                  In your library
                </p>
              </div>
              {libraryResults.map((book) => (
                <button
                  key={book.id}
                  onClick={() => {
                    onSelectBook(book.id);
                    onClose();
                  }}
                  className="w-full flex items-center gap-4 px-5 py-3 hover:bg-surface transition-colors text-left"
                >
                  <BookCover book={book} size="xs" />
                  <div className="flex-1 min-w-0">
                    <p className="text-bark font-medium text-sm leading-tight truncate">
                      {book.title}
                    </p>
                    <p className="text-muted text-xs mt-0.5">{book.author}</p>
                  </div>
                  <span
                    className="text-[10px] font-semibold px-2.5 py-1 rounded-full flex-shrink-0"
                    style={{
                      background: STATUS_SUBTLE[book.status],
                      color: STATUS_COLORS[book.status],
                    }}
                  >
                    {STATUS_LABELS[book.status]}
                  </span>
                </button>
              ))}
            </>
          )}

          {catalogResults.length > 0 && (
            <>
              <div className="px-5 pt-4 pb-1">
                <p className="text-[10px] font-semibold text-muted-light uppercase tracking-widest">
                  Add to library
                </p>
              </div>
              {catalogResults.map((book) => (
                <button
                  key={book.id}
                  onClick={() => {
                    onAddBook(book);
                    onClose();
                  }}
                  className="w-full flex items-center gap-4 px-5 py-3 hover:bg-surface transition-colors text-left"
                >
                  <BookCover book={book} size="xs" />
                  <div className="flex-1 min-w-0">
                    <p className="text-bark font-medium text-sm leading-tight truncate">
                      {book.title}
                    </p>
                    <p className="text-muted text-xs mt-0.5">{book.author}</p>
                  </div>
                  <span
                    className="text-[10px] font-semibold px-2.5 py-1 rounded-full flex-shrink-0 flex items-center gap-1"
                    style={{
                      background: "var(--status-reading-subtle)",
                      color: "var(--status-reading)",
                    }}
                  >
                    <svg
                      className="w-2.5 h-2.5"
                      fill="none"
                      viewBox="0 0 24 24"
                      stroke="currentColor"
                      strokeWidth={2.5}
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        d="M12 4.5v15m7.5-7.5h-15"
                      />
                    </svg>
                    Add
                  </span>
                </button>
              ))}
            </>
          )}

          <div className="h-2" />
        </div>

        {/* Cancel */}
        <div className="px-5 py-3 border-t border-border">
          <button
            onClick={onClose}
            className="w-full py-2.5 rounded-2xl text-sm font-medium text-muted hover:text-bark hover:bg-surface transition-colors"
          >
            Cancel
          </button>
        </div>
      </div>
    </div>
  );
}
