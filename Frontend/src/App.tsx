import { useState } from "react";
import type { ReadingStatus } from "./types/data";
import HomePage from "./pages/HomePage";
import BookDetailsPage from "./pages/BookDetailsPage";
import ReadingProgressPage from "./pages/ReadingProgressPage";
import ReadingStatusListPage from "./pages/ReadingStatusListPage";
import LoginPage from "./pages/LoginPage";
import SearchModal from "./components/SearchModal";
import { useAuth } from "./hooks/useAuth";
import { useBooks } from "./hooks/useBooks";

type Route =
  | { name: "home" }
  | { name: "book-details"; bookId: string }
  | { name: "reading-progress"; bookId: string }
  | { name: "reading-status-list"; status: ReadingStatus };

export default function App() {
  const { status, login, register, logout } = useAuth();

  if (status === "loading") {
    return (
      <div className="min-h-screen bg-cream flex items-center justify-center">
        <p className="text-muted text-sm">Loading…</p>
      </div>
    );
  }

  if (status === "unauthenticated") {
    return <LoginPage onLogin={login} onRegister={register} />;
  }

  return <Library onLogout={logout} />;
}

function Library({ onLogout }: { onLogout: () => Promise<void> }) {
  const { books, error, loading, addBook, updateBook, deleteBook } = useBooks();
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
            onLogout={onLogout}
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
            onDeleteBook={deleteBook}
          />
        );
    }
  };

  return (
    <div
      className="min-h-screen relative"
      style={{ background: "var(--bg-outer)" }}
    >
      <div
        className="relative mx-auto min-h-screen"
        style={{
          maxWidth: 430,
          background: "var(--bg-app)",
          boxShadow: "0 0 40px rgba(44,26,14,0.15)",
        }}
      >
        {loading && books.length === 0 && (
          <p className="text-muted text-sm text-center pt-14">
            Loading your library…
          </p>
        )}
        {error && (
          <p
            className="text-xs text-center pt-4"
            style={{ color: "#A85830" }}
          >
            {error}
          </p>
        )}
        {renderPage()}
      </div>

      {searchOpen && (
        <SearchModal
          books={books}
          onClose={() => setSearchOpen(false)}
          onSelectBook={(id) => {
            setSearchOpen(false);
            navigate({ name: "book-details", bookId: id });
          }}
          onAddBook={async (catalogBook) => {
            const newBookId = await addBook(catalogBook.id);
            setSearchOpen(false);
            if (newBookId) navigate({ name: "book-details", bookId: newBookId });
          }}
        />
      )}
    </div>
  );
}
