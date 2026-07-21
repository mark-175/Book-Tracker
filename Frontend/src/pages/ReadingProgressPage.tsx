import { useState } from "react";
import type { Book } from "../types/data";
import BookCover from "../components/BookCover";
import StarRating from "../components/StarRating";

interface Props {
  book: Book;
  onBack: () => void;
  onUpdateBook: (id: string, updates: Partial<Book>) => void;
}

export default function ReadingProgressPage({
  book,
  onBack,
  onUpdateBook,
}: Props) {
  const [localPages, setLocalPages] = useState(book.pagesRead);

  const pct =
    book.pageCount > 0 ? Math.round((localPages / book.pageCount) * 100) : 0;
  const remaining = book.pageCount - localPages;

  const saveAndBack = () => {
    onUpdateBook(book.id, {
      pagesRead: localPages,
      status: localPages >= book.pageCount ? "read" : "reading",
    });
    onBack();
  };

  const handleSlider = (val: number) => setLocalPages(val);

  const fillPct = book.pageCount > 0 ? (localPages / book.pageCount) * 100 : 0;

  // Chart placeholder bars (fake weekly reading data)
  const chartBars = [12, 35, 20, 48, 30, 55, 40];
  const maxBar = Math.max(...chartBars);

  return (
    <div className="min-h-screen bg-cream pb-8">
      {/* Header */}
      <div className="flex items-center gap-3 px-5 pt-14 pb-4">
        <button
          onClick={saveAndBack}
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
          Reading Progress
        </h1>
      </div>

      {/* Book info */}
      <div className="px-5 mb-6">
        <div
          className="rounded-2xl p-4 flex gap-4 items-center"
          style={{ background: "#FDFBF6", border: "1px solid #DDD4BF" }}
        >
          <BookCover book={book} size="md" />
          <div className="flex-1 min-w-0">
            <p className="font-serif font-semibold text-bark text-[15px] leading-snug">
              {book.title}
            </p>
            <p className="text-muted text-sm mt-0.5">{book.author}</p>
            <p className="text-[11px] text-muted-light mt-1">
              {book.pageCount} pages total
            </p>
          </div>
        </div>
      </div>

      {/* Progress display + slider */}
      <div className="px-5 mb-5">
        <div
          className="rounded-2xl p-5"
          style={{
            background: "#FDFBF6",
            border: "1px solid #DDD4BF",
            boxShadow: "0 2px 10px rgba(44,26,14,0.05)",
          }}
        >
          {/* Big % */}
          <div className="text-center mb-6">
            <p
              className="font-serif font-bold text-bark leading-none"
              style={{ fontSize: 72 }}
            >
              {pct}
              <span className="text-4xl">%</span>
            </p>
            <p className="text-muted text-sm mt-2">
              <span className="font-semibold text-bark">
                {localPages.toLocaleString()}
              </span>{" "}
              of{" "}
              <span className="font-semibold text-bark">
                {book.pageCount.toLocaleString()}
              </span>{" "}
              pages
            </p>
            {remaining > 0 && (
              <p className="text-xs text-muted-light mt-0.5">
                {remaining} pages remaining
              </p>
            )}
          </div>

          {/* Slider */}
          <input
            type="range"
            min={0}
            max={book.pageCount}
            value={localPages}
            onChange={(e) => handleSlider(Number(e.target.value))}
            style={{
              background: `linear-gradient(to right, #863aaf ${fillPct}%, #DDD4BF ${fillPct}%)`,
            }}
          />
          <div className="flex justify-between mt-1.5">
            <span className="text-[10px] text-muted-light">p. 0</span>
            <span className="text-[10px] text-muted-light">
              p. {book.pageCount}
            </span>
          </div>

          {/* Manual page input */}
          <div className="flex items-center gap-3 mt-5">
            <div
              className="flex-1 flex items-center gap-2 rounded-xl px-4 py-3"
              style={{ background: "#EAE2D0" }}
            >
              <span className="text-xs text-muted">Current page</span>
              <input
                type="number"
                min={0}
                max={book.pageCount}
                value={localPages}
                onChange={(e) =>
                  setLocalPages(
                    Math.min(
                      book.pageCount,
                      Math.max(0, Number(e.target.value)),
                    ),
                  )
                }
                className="flex-1 bg-transparent text-bark text-sm font-semibold outline-none text-right w-16"
              />
            </div>
            <button
              onClick={saveAndBack}
              className="px-5 py-3 rounded-xl text-sm font-semibold text-white transition-all active:scale-95"
              style={{
                background: "linear-gradient(135deg,  #b46bdc, #934dee)",
              }}
            >
              Save
            </button>
          </div>
        </div>
      </div>

      {/* Star rating */}
      <div className="px-5 mb-5">
        <div
          className="rounded-2xl p-5"
          style={{ background: "#FDFBF6", border: "1px solid #DDD4BF" }}
        >
          <p className="text-[10px] font-semibold text-muted-light uppercase tracking-widest mb-5 text-center">
            Your Rating
          </p>
          <StarRating
            rating={book.rating}
            onChange={(rating) => onUpdateBook(book.id, { rating })}
            size="lg"
          />
        </div>
      </div>

      {/* Reading history chart placeholder */}
      <div className="px-5">
        <div
          className="rounded-2xl p-5"
          style={{ background: "#FDFBF6", border: "1px solid #DDD4BF" }}
        >
          <div className="flex items-center justify-between mb-5">
            <p className="text-[10px] font-semibold text-muted-light uppercase tracking-widest">
              Reading History
            </p>
            <span className="text-[10px] text-muted-light">Last 7 days</span>
          </div>

          {/* Placeholder bar chart */}
          <div className="rounded-xl p-4" style={{ background: "#EAE2D0" }}>
            <div className="flex items-end gap-2 h-28 mb-2">
              {["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"].map(
                (day, i) => (
                  <div
                    key={day}
                    className="flex-1 flex flex-col items-center gap-1"
                  >
                    <div
                      className="w-full rounded-t-sm transition-all"
                      style={{
                        height: `${(chartBars[i] / maxBar) * 100}%`,
                        background:
                          i === 5
                            ? "linear-gradient(90deg,   #b46bdc, #934dee);"
                            : "#b46bdc",
                      }}
                    />
                  </div>
                ),
              )}
            </div>
            <div className="flex gap-2">
              {["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"].map((day) => (
                <p
                  key={day}
                  className="flex-1 text-center text-[9px] text-muted"
                >
                  {day}
                </p>
              ))}
            </div>
          </div>

          <p className="text-[11px] text-muted-light text-center mt-3">
            Track daily reading to build your history
          </p>
        </div>
      </div>
    </div>
  );
}
