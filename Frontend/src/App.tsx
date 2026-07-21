import { useState } from "react";
import {
  INITIAL_BOOKS,
  type Book,
  type ReadingStatus,
  type CatalogBook,
} from "./types/data";
import HomePage from "./pages/HomePage";
import BookDetailsPage from "./pages/BookDetailsPage";
import ReadingProgressPage from "./pages/ReadingProgressPage";
import ReadingStatusListPage from "./pages/ReadingStatusListPage";
import SearchModal from "./components/SearchModal";

type Route =
  | { name: "home" }
  | { name: "book-details"; bookId: string }
  | { name: "reading-progress"; bookId: string }
  | { name: "reading-status-list"; status: ReadingStatus };

export default function App() {
  const [books, setBooks] = useState<Book[]>(INITIAL_BOOKS);
  const [route, setRoute] = useState<Route>({ name: "home" });
  const [history, setHistory] = useState<Route[]>([]);
  const [searchOpen, setSearchOpen] = useState(false);

  const navigate = (to: Route) => {
    setHistory((h) => [...h, route]);
    setRoute(to);
    window.scrollTo({ top: 0, behavior: "instant" });
  };

  const goBack = () => {
    if (history.length > 0) {
      const prev = history[history.length - 1];
      setHistory((h) => h.slice(0, -1));
      setRoute(prev);
      window.scrollTo({ top: 0, behavior: "instant" });
    } else {
      setRoute({ name: "home" });
      setHistory([]);
    }
  };

  const updateBook = (id: string, updates: Partial<Book>) => {
    setBooks((bs) => bs.map((b) => (b.id === id ? { ...b, ...updates } : b)));
  };

  const addBook = (catalogBook: CatalogBook) => {
    const newBook: Book = {
      ...catalogBook,
      status: "to-read",
      pagesRead: 0,
      rating: null,
      notes: "",
    };
    setBooks((bs) => [...bs, newBook]);
    navigate({ name: "book-details", bookId: newBook.id });
  };

  const renderPage = () => {
    switch (route.name) {
      case "home":
        return (
          <HomePage
            books={books}
            onNavigateToBook={(id) =>
              navigate({ name: "book-details", bookId: id })
            }
            onNavigateToList={(status) =>
              navigate({ name: "reading-status-list", status })
            }
            onOpenSearch={() => setSearchOpen(true)}
          />
        );
      case "book-details": {
        const book = books.find((b) => b.id === route.bookId);
        if (!book) return null;
        return (
          <BookDetailsPage
            book={book}
            onBack={goBack}
            onNavigateToProgress={(id) =>
              navigate({ name: "reading-progress", bookId: id })
            }
            onUpdateBook={updateBook}
          />
        );
      }
      case "reading-progress": {
        const book = books.find((b) => b.id === route.bookId);
        if (!book) return null;
        return (
          <ReadingProgressPage
            book={book}
            onBack={goBack}
            onUpdateBook={updateBook}
          />
        );
      }
      case "reading-status-list":
        return (
          <ReadingStatusListPage
            status={route.status}
            books={books}
            onBack={goBack}
            onNavigateToBook={(id) =>
              navigate({ name: "book-details", bookId: id })
            }
          />
        );
    }
  };

  return (
    <div className="min-h-screen relative" style={{ background: "#E5DDD0" }}>
      {/* Phone-width app container */}
      <div
        className="relative mx-auto min-h-screen"
        style={{
          maxWidth: 430,
          background: "#F4EFE4",
          boxShadow: "0 0 40px rgba(44,26,14,0.15)",
        }}
      >
        {renderPage()}
      </div>

      {/* Search modal renders full-screen above everything */}
      {searchOpen && (
        <SearchModal
          books={books}
          onClose={() => setSearchOpen(false)}
          onSelectBook={(id) => {
            setSearchOpen(false);
            navigate({ name: "book-details", bookId: id });
          }}
          onAddBook={(catalogBook) => {
            setSearchOpen(false);
            addBook(catalogBook);
          }}
        />
      )}
    </div>
  );
}
