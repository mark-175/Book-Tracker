# Frontend API Integration + Login Page — Design

Date: 2026-07-27

## Context

The frontend UI was pasted in from a Figma export and is still wired to a hardcoded mock dataset (`src/types/data.ts`) whose shape doesn't match the backend's real models. `src/api/` and `src/hooks/` are an in-progress, broken integration layer (duplicate `getBooks()` exports, an empty unused `AddBookRequest` interface, `App.tsx` referencing `addBook`/`updateBook` handlers that are commented out). There is no login page. This spec covers finishing the data layer, wiring every existing page to the real backend, and adding authentication.

## Backend additions

The existing API (`GET/POST /api/Books*`, `POST /api/Auth/{register,login,logout}`) is missing two things the frontend needs. Both follow the codebase's existing patterns (DTOs, result objects, service → data-access layering, `[Authorize]` on `BooksController`-style endpoints).

### 1. Update a UserBook

`PATCH /api/Books/{id:int}` — `id` is `Book.Id`, same semantics as the existing `GET /api/Books/{id}`.

New `UpdateUserBookDTO`:
```csharp
public class UpdateUserBookDTO
{
    public BookStatus? Status { get; set; }
    public double? Rating { get; set; }
    public int? PagesRead { get; set; }
    public string? Notes { get; set; }
}
```
Only non-null fields are applied — one endpoint serves "just save notes" (`BookDetailsPage`) and "just update pages/rating" (`ReadingProgressPage`) without needing separate routes.

Side effects applied server-side on status transitions (the frontend has no way to set timestamps itself):
- Transitioning into `Reading`: set `StartedAt = DateTime.UtcNow` if it's currently null.
- Transitioning into `Read`: set `FinishedAt = DateTime.UtcNow` if it's currently null.
- Transitioning back to `ToRead`: clear both `StartedAt` and `FinishedAt` to null.
- `UpdatedAt` is always bumped to `DateTime.UtcNow` on any successful update.

Returns `200` with the updated `UserBookDTO` (so the frontend can patch local state from the response directly, no refetch needed), or `404` if the user has no active `UserBook` for that book id — matching `GetUserBook`'s existing 404 behavior.

Service layer: `IBookService.UpdateUserBook(Guid userId, int bookId, UpdateUserBookDTO dto) -> Task<UserBookDTO?>`, delegating to a new `IDbBookService.UpdateUserBook(...)` following the same `Include(ub => ub.Book)` + in-memory mapping pattern already used by `GetUserBooks`/`GetUserBook` (per the existing gotcha about `.Select()` with custom mapper methods not being SQL-translatable).

### 2. Session check

`GET /api/Auth/me`, `[Authorize]` on the action. Returns a small inline DTO: `{ Id: Guid, Username: string }` (never expose `PasswordHash`). Unauthenticated requests already get a clean `401` via the existing `OnRedirectToLogin` override in `Program.cs` — no new middleware needed.

## Frontend: reconciling data shapes

Keep `Book` / `CatalogBook` / `ReadingStatus` in `src/types/data.ts` as the UI-facing view-model — every page component already consumes this shape, so keeping it stable avoids touching `HomePage`, `BookDetailsPage`, `ReadingProgressPage`, `ReadingStatusListPage`, `BookCover`, and `StarRating`. A new `src/api/mappers.ts` converts backend DTOs into these view models:

- `toBook(dto: UserBookDTO): Book`
- `toCatalogBook(dto: BookSearchResult): CatalogBook`

Mapping decisions:
- `id`: kept as `string` in the UI type via `String(dto.BookId)` / `String(dto.Id)`. The existing `Route` union and every page prop already type `bookId: string` — this keeps that surface unchanged.
- `author`: backend `Authors` is already a comma-joined string; passed through as-is.
- `rating`: backend `Rating` is a non-nullable `double` defaulting to `0`. Maps to UI `null` when `0` (not yet rated), otherwise passes through.
- `coverUrl?: string`: new field on `Book`/`CatalogBook`, populated from `CoverUrl` when present.
- `coverHue` / `coverSat`: the backend never sends these — they're computed client-side via a deterministic hash of `title + isbn13` (stable across reloads, no dependency on id monotonicity).

`BookCover.tsx` is updated to render an `<img src={coverUrl}>` when `coverUrl` is present, falling back to the existing gradient placeholder (using the hashed hue/sat) when it's absent — Google Books doesn't always have a thumbnail.

`INITIAL_BOOKS` and `SEARCH_CATALOG` mock exports are deleted from `types/data.ts` once real data flows through every page.

## Frontend: auth & login page

- `src/hooks/useAuth.ts` — new hook. On mount, calls `GET /Auth/me`. Exposes:
  ```ts
  {
    status: 'loading' | 'authenticated' | 'unauthenticated',
    user: { id: string; username: string } | null,
    login(username, password): Promise<void>,
    register(username, password): Promise<void>,
    logout(): Promise<void>,
  }
  ```
  `login`/`register` throw on failure (with the backend's error message) so `LoginPage` can display it inline; success re-runs the `/Auth/me` check to populate `user` and flip `status`.

- `src/pages/LoginPage.tsx` — new page. Single page with a Login/Register toggle (not two separate routes, consistent with the app's manual-stack navigation style). Register mode client-side validates the password against the same rule as the backend (`^(?=.*\d)(?=.*[!@#$%^&*]).{8,20}$`) for immediate feedback; the backend remains the source of truth. Server error messages (e.g. "Invalid credentials.") are surfaced inline on submit failure. Submit buttons show a loading state.

- `App.tsx` gates rendering on `useAuth().status`:
  - `loading` → minimal splash/spinner (session check in flight).
  - `unauthenticated` → renders `LoginPage`.
  - `authenticated` → renders the existing routed app. `useBooks()` is only invoked in this branch, so no book requests fire before login.

- **Session expiry handling**: the auth cookie has a hard 15-minute absolute expiry with no sliding renewal (per `Backend/CLAUDE.md`), so any API call can 401 mid-session. `src/api/client.ts` gets a response interceptor: on `401`, it dispatches `window.dispatchEvent(new Event('auth:unauthorized'))`. `useAuth` listens for this event and resets to `unauthenticated`, bouncing the user back to `LoginPage` instead of the failure passing silently.

- A small logout button is added to `HomePage`'s header (calls `useAuth().logout()`) — nothing like it exists in the current Figma export, so this is a minimal, style-matched addition (icon button, same treatment as the existing back-button style used elsewhere).

## Frontend: wiring the pages

- `src/api/books.ts` is rewritten to replace the current broken/duplicate exports:
  - `getUserBooks(): Promise<UserBookDTO[]>`
  - `getUserBook(bookId: number): Promise<UserBookDTO>`
  - `searchBooks(query: string): Promise<BookSearchResult[]>`
  - `addBookToLibrary(bookId: number): Promise<AddBookToUserResult>`
  - `updateUserBook(bookId: number, updates: UpdateUserBookRequest): Promise<UserBookDTO>`
  - `src/api/addBook.ts` is deleted (its content merges into `books.ts`).
- `src/api/auth.ts` — `login`, `register`, `logout`, `getMe`, thin wrappers over `apiClient`.
- `src/types/api.ts` — TypeScript mirrors of the backend DTOs (wire types), kept separate from the UI view-model types in `types/data.ts`.
- `src/hooks/useBooks.ts` gains `addBook(bookId: string)` and `updateBook(bookId: string, updates: Partial<Book>)`. Both call the corresponding API function, map the returned DTO back through `mappers.ts`, and patch the single matching entry in local `books` state — no full-list refetch on every mutation.
- `App.tsx`'s commented-out `addBook`/`updateBook` handlers are replaced with calls into the hook's new mutation functions.
- `SearchModal.tsx` is rewired: the static `SEARCH_CATALOG` filter is replaced with a debounced (~300ms) live call to `searchBooks(query)`, keeping the existing `q.length > 1` guard. `onAddBook` calls `addBookToLibrary`, then closes the modal and navigates to the new book's detail page.
- `BookDetailsPage`, `ReadingProgressPage`, `ReadingStatusListPage`, `HomePage` need no structural changes — they already consume `Book[]` / `Book` via props, which will now carry real data.

## Explicitly out of scope

Flag if any of these should be pulled in:
- Removing a book from the library — no UI for it exists in the Figma export, and no backend endpoint exists (soft-delete infrastructure exists on `UserBook` but nothing wires to it yet).
- Distinguishing "already in your library" from a generic error when adding a book — currently both collapse to `AddBookStatus.UnexpectedError` per the existing gotcha noted in `Backend/CLAUDE.md`. The frontend will show a generic "Couldn't add book" message for now.
- Standing up test runners (Vitest / xUnit) — neither project has one configured yet; out of scope for this integration work.

## Execution plan

1. **Backend**: `UpdateUserBookDTO`, `PATCH /api/Books/{id}`, `GET /api/Auth/me`.
2. **Frontend data layer**: `types/api.ts`, `api/auth.ts`, `api/books.ts` (replacing the broken files), `api/mappers.ts`, `hooks/useAuth.ts`, extended `hooks/useBooks.ts`.
3. **Auth**: `LoginPage`, `App.tsx` gating, logout button, 401 interceptor.
4. **Page wiring**: `App.tsx` handlers, `SearchModal` live search, `BookCover` real-image rendering.
5. **Cleanup & QA**: delete mock data exports, run the app end-to-end in the browser (register → login → search → add → update status/progress/notes/rating → logout → session-expiry behavior).

Steps 1 and 2 have no dependency on each other and can be worked in parallel. Steps 3–5 depend on step 2 being complete.
