export const BOOK_STATUS = {
  ToRead: 0,
  Reading: 1,
  Read: 2,
} as const;

export type BookStatusValue = (typeof BOOK_STATUS)[keyof typeof BOOK_STATUS];

export const ADD_BOOK_STATUS = {
  Success: 0,
  BookNotFound: 1,
  UserNotFound: 2,
  UnexpectedError: 3,
  AlreadyInLibrary: 4,
} as const;

export type AddBookStatusValue =
  (typeof ADD_BOOK_STATUS)[keyof typeof ADD_BOOK_STATUS];

export interface UserBookDTO {
  bookId: number;
  title: string;
  subtitle: string;
  authors: string;
  language: string;
  description: string | null;
  coverUrl: string | null;
  isbn10: string | null;
  isbn13: string | null;
  pageCount: number | null;
  status: BookStatusValue;
  rating: number;
  pagesRead: number;
  notes: string;
  startedAt: string | null;
  finishedAt: string | null;
  addedAt: string;
  updatedAt: string;
}

export interface BookSearchResultDTO {
  id: number;
  title: string;
  subtitle: string;
  authors: string;
  language: string;
  description: string | null;
  coverUrl: string | null;
  isbn10: string | null;
  isbn13: string | null;
  pageCount: number | null;
}

export interface BookDTO {
  title: string;
  subtitle: string;
  authors: string;
  language: string;
  description: string | null;
  coverUrl: string | null;
  isbn10: string | null;
  isbn13: string | null;
  pageCount: number | null;
}

export interface AddBookToUserResultDTO {
  addBookStatus: AddBookStatusValue;
  bookId: number;
  book: BookDTO | null;
  userId: string;
  message: string;
}

export interface AddManualBookRequest {
  title: string;
  authors: string;
  subtitle?: string;
  isbn10?: string;
  isbn13?: string;
  pageCount?: number;
  description?: string;
}

export interface RemoveBookFromUserResultDTO {
  success: boolean;
  message: string;
}

export interface UpdateUserBookRequest {
  status?: BookStatusValue;
  rating?: number;
  pagesRead?: number;
  notes?: string;
}

export interface UserDTO {
  id: string;
  username: string;
}
