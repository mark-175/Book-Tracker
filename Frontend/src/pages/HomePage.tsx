import type { Book, ReadingStatus } from "../types/data";
import BookCover from "../components/BookCover";
import StarRating from "../components/StarRating";

interface Props {
  books: Book[];
  onNavigateToBook: (bookId: string) => void;
  onNavigateToList: (status: ReadingStatus) => void;
  onOpenSearch: () => void;
}

const STATUS_CONFIG = [
  {
    label: "To Read",
    status: "to-read" as ReadingStatus,
    color: "#9B8E7A",
    icon: (color: string) => (
      <svg
        className="w-5 h-5"
        fill="none"
        viewBox="0 0 24 24"
        stroke={color}
        strokeWidth={1.5}
      >
        <path
          strokeLinecap="round"
          strokeLinejoin="round"
          d="M12 6.042A8.967 8.967 0 006 3.75c-1.052 0-2.062.18-3 .512v14.25A8.987 8.987 0 016 18c2.305 0 4.408.867 6 2.292m0-14.25a8.966 8.966 0 016-2.292c1.052 0 2.062.18 3 .512v14.25A8.987 8.987 0 0118 18a8.967 8.967 0 00-6 2.292m0-14.25v14.25"
        />
      </svg>
    ),
  },
  {
    label: "Reading",
    status: "reading" as ReadingStatus,
    color: "#A85830",
    icon: (color: string) => (
      <svg
        className="w-5 h-5"
        fill="none"
        viewBox="0 0 24 24"
        stroke={color}
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
  },
  {
    label: "Read",
    status: "read" as ReadingStatus,
    color: "#527A52",
    icon: (color: string) => (
      <svg
        className="w-5 h-5"
        fill="none"
        viewBox="0 0 24 24"
        stroke={color}
        strokeWidth={1.5}
      >
        <path
          strokeLinecap="round"
          strokeLinejoin="round"
          d="M9 12.75L11.25 15 15 9.75M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
        />
      </svg>
    ),
  },
];

export default function HomePage({
  books,
  onNavigateToBook,
  onNavigateToList,
  onOpenSearch,
}: Props) {
  const currentBook = books.find((b) => b.status === "reading") ?? null;
  const toRead = books.filter((b) => b.status === "to-read");
  const reading = books.filter((b) => b.status === "reading");
  const read = books.filter((b) => b.status === "read");

  const pagesRead = books.reduce((acc, b) => acc + b.pagesRead, 0);
  const rated = books.filter((b) => b.rating !== null);
  const avgRating =
    rated.length > 0
      ? (
          rated.reduce((acc, b) => acc + (b.rating ?? 0), 0) / rated.length
        ).toFixed(1)
      : null;

  const today = new Date().toLocaleDateString("en-US", {
    weekday: "long",
    month: "long",
    day: "numeric",
  });

  const booksByStatus: Record<ReadingStatus, Book[]> = {
    "to-read": toRead,
    reading,
    read,
  };

  return (
    <div className="min-h-screen bg-cream pb-28">
      {/* Header */}
      <div className="px-5 pt-14 pb-5">
        <p className="text-muted text-xs font-sans tracking-wide">{today}</p>
        <h1 className="font-serif text-[32px] font-semibold text-bark mt-1 leading-tight">
          My Library
        </h1>
      </div>

      {/* Reading Stats */}
      <div className="px-5 mb-7">
        <div
          className="rounded-2xl p-5 overflow-hidden relative"
          style={{ background: "#2C1A0E" }}
        >
          {/* Decorative book lines */}
          <div className="absolute right-4 top-3 flex gap-1.5 opacity-10">
            {[40, 55, 35, 48, 60].map((h, i) => (
              <div
                key={i}
                className="w-1.5 rounded-sm"
                style={{ height: h, background: "#C8963E" }}
              />
            ))}
          </div>

          <p className="text-[10px] font-semibold text-amber/60 uppercase tracking-widest mb-4">
            Reading Statistics
          </p>

          <div className="grid grid-cols-3 gap-4">
            <div>
              <p className="font-serif text-[34px] font-bold leading-none text-amber">
                {read.length}
              </p>
              <p className="text-[11px] text-white/40 mt-1.5 font-sans leading-tight">
                Books
                <br />
                Finished
              </p>
            </div>
            <div>
              <p className="font-serif text-[34px] font-bold leading-none text-amber">
                {pagesRead.toLocaleString()}
              </p>
              <p className="text-[11px] text-white/40 mt-1.5 font-sans leading-tight">
                Pages
                <br />
                Read
              </p>
            </div>
            <div>
              <p className="font-serif text-[34px] font-bold leading-none text-amber">
                {avgRating ?? "—"}
              </p>
              <p className="text-[11px] text-white/40 mt-1.5 font-sans leading-tight">
                Average
                <br />
                Rating
              </p>
            </div>
          </div>

          {rated.length > 0 && (
            <div className="mt-4 flex items-center gap-1.5">
              {rated.slice(0, 5).map((b) => (
                <StarRating key={b.id} rating={b.rating} readonly size="sm" />
              ))}
            </div>
          )}
        </div>
      </div>

      {/* Current Book */}
      {currentBook && (
        <div className="px-5 mb-7">
          <div className="flex items-center justify-between mb-3">
            <h2 className="font-serif text-[18px] font-semibold text-bark">
              Currently Reading
            </h2>
            <span className="text-xs text-muted">
              {reading.length} in progress
            </span>
          </div>
          <button
            onClick={() => onNavigateToBook(currentBook.id)}
            className="w-full rounded-2xl p-4 flex gap-4 items-start text-left transition-all active:scale-[0.99]"
            style={{
              background: "#FDFBF6",
              border: "1px solid #DDD4BF",
              boxShadow: "0 2px 8px rgba(44,26,14,0.06)",
            }}
          >
            <BookCover book={currentBook} size="lg" />
            <div
              className="flex-1 min-w-0 pt-0.5 flex flex-col justify-between"
              style={{ minHeight: 160 }}
            >
              <div>
                <h3 className="font-serif text-[18px] font-semibold text-bark leading-snug">
                  {currentBook.title}
                </h3>
                {currentBook.subtitle && (
                  <p className="text-muted text-xs mt-1 leading-tight line-clamp-2">
                    {currentBook.subtitle}
                  </p>
                )}
                <p className="text-bark-700 text-sm mt-2 font-medium">
                  {currentBook.author}
                </p>
              </div>

              <div className="mt-3">
                <div className="flex justify-between items-center mb-1.5">
                  <span className="text-[11px] text-muted">
                    Reading progress
                  </span>
                  <span className="text-[11px] font-semibold text-bark">
                    {Math.round(
                      (currentBook.pagesRead / currentBook.pageCount) * 100,
                    )}
                    %
                  </span>
                </div>
                <div
                  className="h-1.5 rounded-full overflow-hidden"
                  style={{ background: "#DDD4BF" }}
                >
                  <div
                    className="h-full rounded-full transition-all"
                    style={{
                      width: `${(currentBook.pagesRead / currentBook.pageCount) * 100}%`,
                      background: "linear-gradient(90deg, #A85830, #C8963E)",
                    }}
                  />
                </div>
                <p className="text-[10px] text-muted mt-1.5">
                  {currentBook.pagesRead} of {currentBook.pageCount} pages
                </p>
              </div>
            </div>
          </button>
        </div>
      )}

      {/* Reading Status */}
      <div className="px-5">
        <div className="flex items-center justify-between mb-3">
          <h2 className="font-serif text-[18px] font-semibold text-bark">
            Reading Status
          </h2>
          <span className="text-xs text-muted">{books.length} total</span>
        </div>

        <div className="flex flex-col gap-3">
          {STATUS_CONFIG.map(({ label, status, color, icon }) => {
            const statusBooks = booksByStatus[status];
            return (
              <button
                key={status}
                onClick={() => onNavigateToList(status)}
                className="w-full rounded-2xl p-4 flex items-center gap-4 text-left transition-all active:scale-[0.99]"
                style={{
                  background: "#FDFBF6",
                  border: "1px solid #DDD4BF",
                  boxShadow: "0 1px 4px rgba(44,26,14,0.05)",
                }}
              >
                <div
                  className="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
                  style={{ background: `${color}18` }}
                >
                  {icon(color)}
                </div>

                <div className="flex-1 min-w-0">
                  <p className="text-bark font-semibold text-sm">{label}</p>
                  <div className="flex flex-wrap gap-x-2 mt-0.5">
                    {statusBooks.slice(0, 2).map((b) => (
                      <p
                        key={b.id}
                        className="text-[11px] text-muted truncate max-w-[140px]"
                      >
                        {b.title}
                      </p>
                    ))}
                    {statusBooks.length > 2 && (
                      <p className="text-[11px] text-muted-light">
                        +{statusBooks.length - 2} more
                      </p>
                    )}
                    {statusBooks.length === 0 && (
                      <p className="text-[11px] text-muted-light">
                        No books yet
                      </p>
                    )}
                  </div>
                </div>

                <div className="flex items-center gap-2 flex-shrink-0">
                  <span
                    className="font-serif text-base font-bold w-8 h-8 rounded-xl flex items-center justify-center"
                    style={{ background: `${color}18`, color }}
                  >
                    {statusBooks.length}
                  </span>
                  <svg
                    className="w-4 h-4 text-muted-light"
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
                </div>
              </button>
            );
          })}
        </div>
      </div>

      {/* FAB */}
      <button
        onClick={onOpenSearch}
        className="fixed bottom-8 right-5 w-14 h-14 rounded-full flex items-center justify-center transition-all active:scale-95 z-40"
        style={{
          background: "linear-gradient(135deg, #A85830, #C8963E)",
          boxShadow: "0 4px 16px rgba(168,88,48,0.45)",
        }}
      >
        <svg
          className="w-6 h-6"
          fill="none"
          viewBox="0 0 24 24"
          stroke="white"
          strokeWidth={2.5}
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            d="M12 4.5v15m7.5-7.5h-15"
          />
        </svg>
      </button>
    </div>
  );
}
