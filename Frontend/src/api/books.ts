import { isAxiosError } from "axios";
import { apiClient } from "./client";
import type {
  UserBookDTO,
  BookSearchResultDTO,
  AddBookToUserResultDTO,
  AddManualBookRequest,
  UpdateUserBookRequest,
} from "@/types/api";

export async function getUserBooks(): Promise<UserBookDTO[]> {
  const response = await apiClient.get<UserBookDTO[]>("/books");
  return response.data;
}

export async function getUserBook(bookId: number): Promise<UserBookDTO> {
  const response = await apiClient.get<UserBookDTO>(`/books/${bookId}`);
  return response.data;
}

export async function searchBooks(
  query: string,
): Promise<BookSearchResultDTO[]> {
  try {
    const response = await apiClient.get<BookSearchResultDTO[]>(
      "/books/search",
      { params: { query } },
    );
    return response.data;
  } catch (err) {
    if (isAxiosError(err) && err.response?.status === 404) return [];
    throw err;
  }
}

export async function addManualBook(
  request: AddManualBookRequest,
): Promise<BookSearchResultDTO> {
  const response = await apiClient.post<BookSearchResultDTO>(
    "/books/manual",
    request,
  );
  return response.data;
}

export async function addBookToLibrary(
  bookId: number,
): Promise<AddBookToUserResultDTO> {
  const response = await apiClient.post<AddBookToUserResultDTO>(
    "/books/add",
    null,
    { params: { bookId } },
  );
  return response.data;
}

export async function updateUserBook(
  bookId: number,
  updates: UpdateUserBookRequest,
): Promise<UserBookDTO> {
  const response = await apiClient.patch<UserBookDTO>(
    `/books/${bookId}`,
    updates,
  );
  return response.data;
}
