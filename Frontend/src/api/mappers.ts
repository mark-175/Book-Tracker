import { BOOK_STATUS } from "@/types/api";
import type {
  UserBookDTO,
  BookSearchResultDTO,
  UpdateUserBookRequest,
  BookStatusValue,
} from "@/types/api";
import type { Book, CatalogBook, ReadingStatus } from "@/types/data";

const STATUS_FROM_DTO: Record<number, ReadingStatus> = {
  [BOOK_STATUS.ToRead]: "to-read",
  [BOOK_STATUS.Reading]: "reading",
  [BOOK_STATUS.Read]: "read",
};

const STATUS_TO_DTO: Record<ReadingStatus, number> = {
  "to-read": BOOK_STATUS.ToRead,
  reading: BOOK_STATUS.Reading,
  read: BOOK_STATUS.Read,
};

function hashCover(seed: string): { coverHue: number; coverSat: number } {
  let hash = 0;
  for (let i = 0; i < seed.length; i++) {
    hash = (hash << 5) - hash + seed.charCodeAt(i);
    hash |= 0;
  }
  return {
    coverHue: Math.abs(hash) % 360,
    coverSat: 40 + (Math.abs(hash) % 25),
  };
}

export function toBook(dto: UserBookDTO): Book {
  return {
    id: String(dto.bookId),
    title: dto.title,
    subtitle: dto.subtitle || undefined,
    author: dto.authors,
    pageCount: dto.pageCount ?? 0,
    description: dto.description ?? "",
    isbn10: dto.isbn10 ?? "",
    isbn13: dto.isbn13 ?? "",
    status: STATUS_FROM_DTO[dto.status],
    pagesRead: dto.pagesRead,
    rating: dto.rating === 0 ? null : dto.rating,
    notes: dto.notes,
    coverUrl: dto.coverUrl ?? undefined,
    ...hashCover(dto.title + dto.isbn13),
  };
}

export function toCatalogBook(dto: BookSearchResultDTO): CatalogBook {
  return {
    id: String(dto.id),
    title: dto.title,
    subtitle: dto.subtitle || undefined,
    author: dto.authors,
    pageCount: dto.pageCount ?? 0,
    description: dto.description ?? "",
    isbn10: dto.isbn10 ?? "",
    isbn13: dto.isbn13 ?? "",
    coverUrl: dto.coverUrl ?? undefined,
    ...hashCover(dto.title + dto.isbn13),
  };
}

export function toUpdateRequest(updates: Partial<Book>): UpdateUserBookRequest {
  const request: UpdateUserBookRequest = {};
  if (updates.status !== undefined) request.status = STATUS_TO_DTO[updates.status] as BookStatusValue;
  if (updates.rating !== undefined) request.rating = updates.rating ?? 0;
  if (updates.pagesRead !== undefined) request.pagesRead = updates.pagesRead;
  if (updates.notes !== undefined) request.notes = updates.notes;
  return request;
}
