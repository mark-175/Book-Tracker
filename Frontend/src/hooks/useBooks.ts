import { useCallback, useEffect, useState } from "react";
import type { Book } from "@/types/data";
import { ADD_BOOK_STATUS } from "@/types/api";
import * as booksApi from "@/api/books";
import { toBook, toUpdateRequest } from "@/api/mappers";

export function useBooks() {
  const [books, setBooks] = useState<Book[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadBooks = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const dtos = await booksApi.getUserBooks();
      setBooks(dtos.map(toBook));
    } catch (err) {
      setError("Couldn't load books.");
      console.error(err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadBooks();
  }, [loadBooks]);

  const addBook = useCallback(async (bookId: string): Promise<string | null> => {
    try {
      setError(null);
      const result = await booksApi.addBookToLibrary(Number(bookId));
      if (result.addBookStatus !== ADD_BOOK_STATUS.Success) {
        throw new Error(result.message);
      }
      const dto = await booksApi.getUserBook(Number(bookId));
      const book = toBook(dto);
      setBooks((bs) => [...bs, book]);
      return book.id;
    } catch (err) {
      setError("Couldn't add that book.");
      console.error(err);
      return null;
    }
  }, []);

  const updateBook = useCallback(
    async (id: string, updates: Partial<Book>): Promise<void> => {
      try {
        setError(null);
        const request = toUpdateRequest(updates);
        const dto = await booksApi.updateUserBook(Number(id), request);
        const updated = toBook(dto);
        setBooks((bs) => bs.map((b) => (b.id === id ? updated : b)));
      } catch (err) {
        setError("Couldn't save your changes.");
        console.error(err);
      }
    },
    [],
  );

  const deleteBook = useCallback(async (id: string): Promise<boolean> => {
    try {
      setError(null);
      const result = await booksApi.deleteBook(Number(id));
      if (!result.success) {
        throw new Error(result.message);
      }
      setBooks((bs) => bs.filter((b) => b.id !== id));
      return true;
    } catch (err) {
      setError("Couldn't remove that book.");
      console.error(err);
      return false;
    }
  }, []);

  return {
    books,
    loading,
    error,
    reload: loadBooks,
    addBook,
    updateBook,
    deleteBook,
  };
}
