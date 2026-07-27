# Frontend API Integration + Login Page Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the frontend's hardcoded mock dataset with real calls to the backend API across every existing page, and add a login/register page gating the app.

**Architecture:** Two small backend additions (update-UserBook endpoint, session-check endpoint) close the gaps blocking full integration. On the frontend, a mapping layer (`api/mappers.ts`) converts backend DTOs into the existing UI-facing `Book`/`CatalogBook` types so no page component's props need to change shape. Auth and book state are each owned by one hook (`useAuth`, `useBooks`) called once at the top of `App.tsx` and threaded down via props — matching the app's existing no-router, no-context, prop-drilling style.

**Tech Stack:** ASP.NET Core (C#) + EF Core + SQLite backend; React 19 + TypeScript + Vite + axios frontend, cookie-based auth.

**Design spec:** `docs/superpowers/specs/2026-07-27-frontend-api-integration-design.md`

## Global Constraints

- Neither project has a test runner configured (`Backend/CLAUDE.md`, `Frontend/CLAUDE.md`), and per the approved spec, standing one up is out of scope for this work. Tasks below use `dotnet build` / `pnpm exec tsc --noEmit` for compile-correctness and manual HTTP calls (PowerShell `Invoke-RestMethod`) or browser interaction for behavior-correctness, in place of automated tests. Where the plan template says "Test:", read it as "Verify:".
- **Enums serialize as numbers, not strings.** `Program.cs` never registers a `JsonStringEnumConverter`, so ASP.NET Core's default `System.Text.Json` behavior applies: `BookStatus` and `AddBookStatus` cross the wire as their ordinal (`ToRead=0, Reading=1, Read=2`; `Success=0, BookNotFound=1, UserNotFound=2, UnexpectedError=3`). All JSON property names are camelCase (ASP.NET Core's MVC default). Frontend wire types must reflect this exactly.
- `Frontend/tsconfig.app.json` has `"verbatimModuleSyntax": true` — every type-only import must use `import type { X }`, not `import { X }`.
- No code comments except to explain a non-obvious constraint — matches the convention already followed by every existing file in both projects.
- `PATCH /api/Books/{id:int}` and `GET /api/Books/{id:int}` both use `Book.Id` (not `UserBook.Id`) as the route parameter — this is already the existing convention in `BooksController`.
- Frontend `Book.id` / `CatalogBook.id` stay `string` (stringified backend `int` ids) so the existing `Route` union and every page's props (`bookId: string`) need zero changes.
- `Book.rating: number | null` — backend `Rating` is a non-nullable `double` defaulting to `0`; `0` maps to UI `null` ("not rated").
- Out of scope (confirmed in spec): removing a book from the library, distinguishing "already in your library" from a generic add error, standing up test runners.
- Path alias `@/` resolves to `Frontend/src` (`vite.config.ts`, `tsconfig.app.json`) — use it in all new frontend files.

---

## Task 1: Backend — update a UserBook (`PATCH /api/Books/{id}`)

**Files:**
- Create: `Backend/DTOs/UpdateUserBookDTO.cs`
- Modify: `Backend/Services/Db/IDbBookService.cs`
- Modify: `Backend/Services/Db/DbBookService.cs`
- Modify: `Backend/Services/IBookService.cs`
- Modify: `Backend/Services/BookService.cs`
- Modify: `Backend/Controllers/BooksController.cs`
- Verify: manual HTTP calls via PowerShell against the running dev server (no test project exists)

**Interfaces:**
- Consumes: existing `BookMapper.ToUserBookDTO(UserBook)` (`Backend/DTOs/BookMapper.cs`), `AppDbContext.UserBooks` (`Backend/Data/AppDbContext.cs`), `ClaimsPrincipalExtensions.GetUserId()` (`Backend/Auth/ClaimPrincipalExtensions.cs`), `BookStatus` enum (`Backend/Enums/BookStatus.cs`: `ToRead=0, Reading=1, Read=2`).
- Produces: `UpdateUserBookDTO { BookStatus? Status; double? Rating; int? PagesRead; string? Notes; }`; `IBookService.UpdateUserBook(Guid userId, int bookId, UpdateUserBookDTO dto) -> Task<UserBookDTO?>`; HTTP `PATCH /api/Books/{id:int}` returning `200 UserBookDTO` or `404`.

- [ ] **Step 1: Create the DTO**

Create `Backend/DTOs/UpdateUserBookDTO.cs`:

```csharp
using BookTracker.Api.Enums;

namespace BookTracker.Api.DTOs;

public class UpdateUserBookDTO
{
    public BookStatus? Status { get; set; }
    public double? Rating { get; set; }
    public int? PagesRead { get; set; }
    public string? Notes { get; set; }
}
```

- [ ] **Step 2: Add the method to `IDbBookService`**

In `Backend/Services/Db/IDbBookService.cs`, add this line inside the interface, after `GetUserBook`:

```csharp
    public Task<UserBookDTO?> UpdateUserBook(Guid userId, int bookId, UpdateUserBookDTO dto);
```

- [ ] **Step 3: Implement it in `DbBookService`**

In `Backend/Services/Db/DbBookService.cs`, add `using BookTracker.Api.Enums;` to the top of the file (alongside the existing `using` lines), then add this method after `GetUserBook`:

```csharp
    public async Task<UserBookDTO?> UpdateUserBook(Guid userId, int bookId, UpdateUserBookDTO dto)
    {
        var userBook = await _dbContext.UserBooks
            .Include(ub => ub.Book)
            .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BookId == bookId);

        if (userBook is null) return null;

        if (dto.Status is not null && dto.Status != userBook.Status)
        {
            userBook.Status = dto.Status.Value;

            if (dto.Status == BookStatus.Reading && userBook.StartedAt is null)
                userBook.StartedAt = DateTime.UtcNow;
            else if (dto.Status == BookStatus.Read && userBook.FinishedAt is null)
                userBook.FinishedAt = DateTime.UtcNow;
            else if (dto.Status == BookStatus.ToRead)
            {
                userBook.StartedAt = null;
                userBook.FinishedAt = null;
            }
        }

        if (dto.Rating is not null) userBook.Rating = dto.Rating.Value;
        if (dto.PagesRead is not null) userBook.PagesRead = dto.PagesRead.Value;
        if (dto.Notes is not null) userBook.Notes = dto.Notes;

        userBook.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return BookMapper.ToUserBookDTO(userBook);
    }
```

- [ ] **Step 4: Add the method to `IBookService`**

In `Backend/Services/IBookService.cs`, add after `GetUserBook`:

```csharp
    public Task<UserBookDTO?> UpdateUserBook(Guid userId, int bookId, UpdateUserBookDTO dto);
```

- [ ] **Step 5: Implement it in `BookService`**

In `Backend/Services/BookService.cs`, add after `GetUserBook`:

```csharp
    public async Task<UserBookDTO?> UpdateUserBook(Guid userId, int bookId, UpdateUserBookDTO dto)
    {
        return await _dbBookService.UpdateUserBook(userId, bookId, dto);
    }
```

- [ ] **Step 6: Add the controller action**

In `Backend/Controllers/BooksController.cs`, add after `GetBook`:

```csharp
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdateBook(int id, [FromBody] UpdateUserBookDTO dto)
    {
        var userId = User.GetUserId();
        var result = await _bookService.UpdateUserBook(userId, id, dto);

        if (result is null) return NotFound();

        return Ok(result);
    }
```

- [ ] **Step 7: Build**

Run (from `Backend/`): `dotnet build`
Expected: build succeeds with no errors.

- [ ] **Step 8: Manually verify against the running server**

From `Backend/`, run `dotnet run` (leave it running), then in a separate PowerShell terminal:

```powershell
$base = "http://localhost:5238/api"
$body = '{"username":"plantest","password":"Testpass1!"}'
Invoke-RestMethod -Uri "$base/Auth/register" -Method Post -ContentType "application/json" -Body $body
Invoke-RestMethod -Uri "$base/Auth/login" -Method Post -ContentType "application/json" -Body $body -SessionVariable sess

$results = Invoke-RestMethod -Uri "$base/Books/search?query=dune" -Method Get -WebSession $sess
$bookId = $results[0].id
Invoke-RestMethod -Uri "$base/Books/add?bookId=$bookId" -Method Post -WebSession $sess

$updateBody = '{"status":1,"pagesRead":50}'
Invoke-RestMethod -Uri "$base/Books/$bookId" -Method Patch -ContentType "application/json" -Body $updateBody -WebSession $sess
```

Expected: the final call returns `200` with a JSON body where `status: 1`, `pagesRead: 50`, and `startedAt` is now a non-null timestamp (auto-set by the status transition to `Reading`). Also verify a bad id 404s: `Invoke-RestMethod -Uri "$base/Books/999999" -Method Patch -ContentType "application/json" -Body '{"notes":"x"}' -WebSession $sess` should throw with a 404 status (check via `try { ... } catch { $_.Exception.Response.StatusCode }`).

- [ ] **Step 9: Commit**

```bash
git add Backend/DTOs/UpdateUserBookDTO.cs Backend/Services/Db/IDbBookService.cs Backend/Services/Db/DbBookService.cs Backend/Services/IBookService.cs Backend/Services/BookService.cs Backend/Controllers/BooksController.cs
git commit -m "Add endpoint to update a UserBook's status, rating, progress, and notes"
```

---

## Task 2: Backend — session check (`GET /api/Auth/me`)

**Files:**
- Create: `Backend/DTOs/UserDTO.cs`
- Modify: `Backend/Services/IUserService.cs`
- Modify: `Backend/Services/UserService.cs`
- Modify: `Backend/Controllers/AuthController.cs`
- Verify: manual HTTP calls via PowerShell

**Interfaces:**
- Consumes: `ClaimsPrincipalExtensions.GetUserId()`, existing `IUserService`/`UserService` (`Backend/Services/IUserService.cs`, `Backend/Services/UserService.cs`), `User` entity (`Backend/Entities/User.cs`: `Id: Guid`, `Username: string`).
- Produces: `UserDTO { Guid Id; string Username; }`; `IUserService.GetUser(Guid userId) -> Task<User?>`; HTTP `GET /api/Auth/me` (`[Authorize]`) returning `200 UserDTO` or `401` (unauthenticated requests already get a clean 401 via the `OnRedirectToLogin` override in `Backend/Program.cs` — no new middleware needed).

- [ ] **Step 1: Create the DTO**

Create `Backend/DTOs/UserDTO.cs`:

```csharp
namespace BookTracker.Api.DTOs;

public class UserDTO
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
}
```

- [ ] **Step 2: Add `GetUser` to `IUserService`**

Replace the full contents of `Backend/Services/IUserService.cs` with:

```csharp
using BookTracker.Api.Entities;

namespace BookTracker.Api.Services;

public interface IUserService
{
    public Task<List<string>> GetPreferredLanguages(Guid userId);
    public Task<User?> GetUser(Guid userId);
}
```

- [ ] **Step 3: Implement it in `UserService`**

In `Backend/Services/UserService.cs`, add `using BookTracker.Api.Entities;` to the top of the file, then add this method after `GetPreferredLanguages`:

```csharp
    public async Task<User?> GetUser(Guid userId)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }
```

- [ ] **Step 4: Add the controller action**

Replace the full contents of `Backend/Controllers/AuthController.cs` with:

```csharp
using BookTracker.Api.Auth;
using BookTracker.Api.DTOs;
using BookTracker.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public AuthController(IAuthService authService, IUserService userService)
    {
        _authService = authService;
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] Login dto)
    {
        var result = await _authService.RegisterAsync(dto.Username, dto.Password);
        return result.Success ? Ok() : BadRequest(result.Error);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Login dto)
    {
        var result = await _authService.LoginAsync(dto.Username, dto.Password);
        return result.Success ? Ok() : Unauthorized(result.Error);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return Ok();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userId = User.GetUserId();
        var user = await _userService.GetUser(userId);

        if (user is null) return NotFound();

        return Ok(new UserDTO { Id = user.Id, Username = user.Username });
    }
}
```

`IUserService` is already registered as `Scoped` in `Backend/Program.cs` — no DI changes needed.

- [ ] **Step 5: Build**

Run (from `Backend/`): `dotnet build`
Expected: build succeeds with no errors.

- [ ] **Step 6: Manually verify against the running server**

With `dotnet run` still running from Task 1 (or restarted):

```powershell
$base = "http://localhost:5238/api"
Invoke-RestMethod -Uri "$base/Auth/login" -Method Post -ContentType "application/json" -Body '{"username":"plantest","password":"Testpass1!"}' -SessionVariable sess
Invoke-RestMethod -Uri "$base/Auth/me" -Method Get -WebSession $sess

try {
    Invoke-RestMethod -Uri "$base/Auth/me" -Method Get
} catch {
    $_.Exception.Response.StatusCode
}
```

Expected: the first call returns `200` with `{ "id": "...", "username": "plantest" }`; the second (no session) throws with status `401`.

- [ ] **Step 7: Commit**

```bash
git add Backend/DTOs/UserDTO.cs Backend/Services/IUserService.cs Backend/Services/UserService.cs Backend/Controllers/AuthController.cs
git commit -m "Add session-check endpoint (GET /api/Auth/me)"
```

---

## Task 3: Frontend — wire types (`types/api.ts`)

**Files:**
- Create: `Frontend/src/types/api.ts`
- Verify: `pnpm exec tsc --noEmit` (from `Frontend/`)

**Interfaces:**
- Consumes: the JSON shapes produced by Task 1 and Task 2's endpoints, and the existing (unmodified) `GET /api/Books`, `GET /api/Books/{id}`, `GET /api/Books/search`, `POST /api/Books/add` endpoints (`Backend/Controllers/BooksController.cs`, `Backend/DTOs/UserBookDTO.cs`, `Backend/DTOs/BookSearchResult.cs`, `Backend/DTOs/BookDTO.cs`, `Backend/DTOs/AddBookToUserResult.cs`). All properties are camelCase; `status`/`addBookStatus` are numeric ordinals (see Global Constraints).
- Produces: `BookStatusValue`, `BOOK_STATUS`, `AddBookStatusValue`, `ADD_BOOK_STATUS`, `UserBookDTO`, `BookSearchResultDTO`, `BookDTO`, `AddBookToUserResultDTO`, `UpdateUserBookRequest`, `UserDTO` — all exported from `@/types/api`, consumed by Tasks 4–8.

- [ ] **Step 1: Write the file**

Create `Frontend/src/types/api.ts`:

```typescript
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
```

- [ ] **Step 2: Verify it compiles**

Run (from `Frontend/`): `pnpm exec tsc --noEmit`
Expected: no errors referencing `types/api.ts` (errors from other not-yet-fixed files are expected at this point and will clear as later tasks land — see Step-by-step note below).

- [ ] **Step 3: Commit**

```bash
git add Frontend/src/types/api.ts
git commit -m "Add TypeScript types mirroring the backend API's wire shapes"
```

---

## Task 4: Frontend — mappers and `coverUrl`

**Files:**
- Modify: `Frontend/src/types/data.ts`
- Create: `Frontend/src/api/mappers.ts`
- Verify: `pnpm exec tsc --noEmit`

**Interfaces:**
- Consumes: `UserBookDTO`, `BookSearchResultDTO`, `UpdateUserBookRequest`, `BOOK_STATUS`, `BookStatusValue` from `@/types/api` (Task 3).
- Produces: `Book.coverUrl?: string` and `CatalogBook.coverUrl?: string` on the existing UI types; `toBook(dto: UserBookDTO): Book`, `toCatalogBook(dto: BookSearchResultDTO): CatalogBook`, `toUpdateRequest(updates: Partial<Book>): UpdateUserBookRequest` from `@/api/mappers` — consumed by Tasks 8, 9, 11.

- [ ] **Step 1: Add `coverUrl` to the UI types**

In `Frontend/src/types/data.ts`, add `coverUrl?: string` as the last field in both the `Book` and `CatalogBook` interfaces (leave `INITIAL_BOOKS`/`SEARCH_CATALOG` untouched for now — they're removed in Task 12 once nothing references them):

```typescript
export interface Book {
  id: string
  title: string
  subtitle?: string
  author: string
  pageCount: number
  description: string
  isbn10: string
  isbn13: string
  status: ReadingStatus
  pagesRead: number
  rating: number | null
  notes: string
  coverHue: number
  coverSat: number
  coverUrl?: string
}

export interface CatalogBook {
  id: string
  title: string
  subtitle?: string
  author: string
  pageCount: number
  description: string
  isbn10: string
  isbn13: string
  coverHue: number
  coverSat: number
  coverUrl?: string
}
```

- [ ] **Step 2: Write the mappers**

Create `Frontend/src/api/mappers.ts`:

```typescript
import { BOOK_STATUS } from "@/types/api";
import type {
  UserBookDTO,
  BookSearchResultDTO,
  UpdateUserBookRequest,
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
  if (updates.status !== undefined) request.status = STATUS_TO_DTO[updates.status];
  if (updates.rating !== undefined) request.rating = updates.rating ?? 0;
  if (updates.pagesRead !== undefined) request.pagesRead = updates.pagesRead;
  if (updates.notes !== undefined) request.notes = updates.notes;
  return request;
}
```

- [ ] **Step 3: Verify it compiles**

Run (from `Frontend/`): `pnpm exec tsc --noEmit`
Expected: no errors referencing `types/data.ts` or `api/mappers.ts`.

- [ ] **Step 4: Commit**

```bash
git add Frontend/src/types/data.ts Frontend/src/api/mappers.ts
git commit -m "Add DTO-to-view-model mappers and coverUrl field"
```

---

## Task 5: Frontend — rewrite `api/books.ts`

**Files:**
- Modify: `Frontend/src/api/books.ts` (replace entirely)
- Delete: `Frontend/src/api/addBook.ts`
- Verify: `pnpm exec tsc --noEmit`

**Interfaces:**
- Consumes: `apiClient` from `@/api/client` (existing, unchanged in this task), `UserBookDTO`, `BookSearchResultDTO`, `AddBookToUserResultDTO`, `UpdateUserBookRequest` from `@/types/api` (Task 3).
- Produces: `getUserBooks()`, `getUserBook(bookId: number)`, `searchBooks(query: string)`, `addBookToLibrary(bookId: number)`, `updateUserBook(bookId: number, updates: UpdateUserBookRequest)` from `@/api/books` — consumed by Task 8 (and Task 11 for `searchBooks`).

- [ ] **Step 1: Delete the dead file**

Delete `Frontend/src/api/addBook.ts` (its only content is an unused empty interface and a duplicate `getBooks` export — both replaced below).

- [ ] **Step 2: Replace `api/books.ts`**

Replace the full contents of `Frontend/src/api/books.ts` with:

```typescript
import { isAxiosError } from "axios";
import { apiClient } from "./client";
import type {
  UserBookDTO,
  BookSearchResultDTO,
  AddBookToUserResultDTO,
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
```

`GET /api/Books/search` returns `404` with a plain-text body when nothing matches (`BooksController.FindBook`) — `searchBooks` treats that as an empty result rather than an error, since "no matches" isn't a failure state for the UI. `addBookToLibrary` deliberately does *not* catch its `404` (book/user not found) — that's a real failure and should propagate to the caller, consistent with the rest of the app's error handling.

- [ ] **Step 3: Verify it compiles**

Run (from `Frontend/`): `pnpm exec tsc --noEmit`
Expected: no errors referencing `api/books.ts`; no errors about `api/addBook.ts` (deleted).

- [ ] **Step 4: Commit**

```bash
git add Frontend/src/api/books.ts
git rm Frontend/src/api/addBook.ts
git commit -m "Rewrite api/books.ts against the real backend, remove dead addBook.ts"
```

---

## Task 6: Frontend — `api/auth.ts`

**Files:**
- Create: `Frontend/src/api/auth.ts`
- Verify: `pnpm exec tsc --noEmit`

**Interfaces:**
- Consumes: `apiClient` from `@/api/client`, `UserDTO` from `@/types/api` (Task 3).
- Produces: `login(username, password): Promise<void>`, `register(username, password): Promise<void>`, `logout(): Promise<void>`, `getMe(): Promise<UserDTO | null>` from `@/api/auth` — consumed by Task 7.

- [ ] **Step 1: Write the file**

Create `Frontend/src/api/auth.ts`:

```typescript
import { isAxiosError } from "axios";
import { apiClient } from "./client";
import type { UserDTO } from "@/types/api";

export async function login(username: string, password: string): Promise<void> {
  try {
    await apiClient.post("/auth/login", { username, password });
  } catch (err) {
    if (isAxiosError(err) && err.response?.status === 401) {
      const message =
        typeof err.response.data === "string"
          ? err.response.data
          : "Invalid credentials.";
      throw new Error(message);
    }
    throw err;
  }
}

export async function register(
  username: string,
  password: string,
): Promise<void> {
  try {
    await apiClient.post("/auth/register", { username, password });
  } catch (err) {
    if (isAxiosError(err) && err.response?.status === 400) {
      const message =
        typeof err.response.data === "string"
          ? err.response.data
          : "Registration failed.";
      throw new Error(message);
    }
    throw err;
  }
}

export async function logout(): Promise<void> {
  await apiClient.post("/auth/logout");
}

export async function getMe(): Promise<UserDTO | null> {
  try {
    const response = await apiClient.get<UserDTO>("/auth/me");
    return response.data;
  } catch (err) {
    if (isAxiosError(err) && err.response?.status === 401) return null;
    throw err;
  }
}
```

`AuthController.Register`/`Login` return the failure message as a plain string body (`BadRequest(result.Error)` / `Unauthorized(result.Error)`), which is what the `typeof err.response.data === "string"` checks unwrap into a normal `Error` the UI can display.

- [ ] **Step 2: Verify it compiles**

Run (from `Frontend/`): `pnpm exec tsc --noEmit`
Expected: no errors referencing `api/auth.ts`.

- [ ] **Step 3: Commit**

```bash
git add Frontend/src/api/auth.ts
git commit -m "Add auth API wrapper (login/register/logout/getMe)"
```

---

## Task 7: Frontend — 401 handling and `useAuth`

**Files:**
- Modify: `Frontend/src/api/client.ts`
- Create: `Frontend/src/hooks/useAuth.ts`
- Verify: `pnpm exec tsc --noEmit`

**Interfaces:**
- Consumes: `login`, `register`, `logout`, `getMe` from `@/api/auth` (Task 6), `UserDTO` from `@/types/api` (Task 3).
- Produces: a global `window` event named `auth:unauthorized`, dispatched on any `401` response; `useAuth()` hook from `@/hooks/useAuth` returning `{ status: 'loading' | 'authenticated' | 'unauthenticated', user: UserDTO | null, login, register, logout }` — consumed by Task 9.

- [ ] **Step 1: Add the 401 interceptor**

Replace the full contents of `Frontend/src/api/client.ts` with:

```typescript
import axios from "axios";

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  withCredentials: true,
  headers: {
    "Content-Type": "application/json",
  },
});

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (axios.isAxiosError(error) && error.response?.status === 401) {
      window.dispatchEvent(new Event("auth:unauthorized"));
    }
    return Promise.reject(error);
  },
);
```

The backend's auth cookie has a hard 15-minute absolute expiry with no sliding renewal (`Backend/CLAUDE.md`), so any authenticated call can 401 mid-session. This interceptor is the single place that detects it; `useAuth` (below) is the single place that reacts to it. It also fires for `getMe()`'s initial "not logged in yet" 401 and for a failed `login()` (bad credentials, also a 401) — both harmless, since in both cases `status` is already `unauthenticated`.

- [ ] **Step 2: Write the hook**

Create `Frontend/src/hooks/useAuth.ts`:

```typescript
import { useCallback, useEffect, useState } from "react";
import * as authApi from "@/api/auth";
import type { UserDTO } from "@/types/api";

type AuthStatus = "loading" | "authenticated" | "unauthenticated";

export function useAuth() {
  const [status, setStatus] = useState<AuthStatus>("loading");
  const [user, setUser] = useState<UserDTO | null>(null);

  const checkSession = useCallback(async () => {
    const me = await authApi.getMe();
    setUser(me);
    setStatus(me ? "authenticated" : "unauthenticated");
  }, []);

  useEffect(() => {
    checkSession();
  }, [checkSession]);

  useEffect(() => {
    const handleUnauthorized = () => {
      setUser(null);
      setStatus("unauthenticated");
    };
    window.addEventListener("auth:unauthorized", handleUnauthorized);
    return () =>
      window.removeEventListener("auth:unauthorized", handleUnauthorized);
  }, []);

  const login = useCallback(
    async (username: string, password: string) => {
      await authApi.login(username, password);
      await checkSession();
    },
    [checkSession],
  );

  const register = useCallback(async (username: string, password: string) => {
    await authApi.register(username, password);
  }, []);

  const logout = useCallback(async () => {
    await authApi.logout();
    setUser(null);
    setStatus("unauthenticated");
  }, []);

  return { status, user, login, register, logout };
}
```

`register` deliberately does not call `checkSession()` — `POST /api/Auth/register` only creates the account, it does not sign the user in (only `Login` calls `SignInAsync` in `Backend/Auth/AuthService.cs`). Task 9's `LoginPage` calls `register` followed by `login` to get an authenticated session after account creation.

- [ ] **Step 3: Verify it compiles**

Run (from `Frontend/`): `pnpm exec tsc --noEmit`
Expected: no errors referencing `api/client.ts` or `hooks/useAuth.ts`.

- [ ] **Step 4: Commit**

```bash
git add Frontend/src/api/client.ts Frontend/src/hooks/useAuth.ts
git commit -m "Add 401 interceptor and useAuth hook"
```

---

## Task 8: Frontend — rewrite `useBooks`

**Files:**
- Modify: `Frontend/src/hooks/useBooks.ts` (replace entirely)
- Verify: `pnpm exec tsc --noEmit`

**Interfaces:**
- Consumes: `getUserBooks`, `getUserBook`, `addBookToLibrary`, `updateUserBook` from `@/api/books` (Task 5); `toBook`, `toUpdateRequest` from `@/api/mappers` (Task 4); `ADD_BOOK_STATUS` from `@/types/api` (Task 3); `Book` from `@/types/data`.
- Produces: `useBooks()` returning `{ books: Book[], loading: boolean, error: string | null, reload: () => Promise<void>, addBook: (bookId: string) => Promise<string | null>, updateBook: (id: string, updates: Partial<Book>) => Promise<void> }` — consumed by Task 9.

- [ ] **Step 1: Replace the hook**

Replace the full contents of `Frontend/src/hooks/useBooks.ts` with:

```typescript
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

  return {
    books,
    loading,
    error,
    reload: loadBooks,
    addBook,
    updateBook,
  };
}
```

Both mutations catch their own errors (log + set `error`) rather than throwing, because their callers — `BookDetailsPage`/`ReadingProgressPage` (unchanged in this plan, see Task 9) — invoke `onUpdateBook` without awaiting or catching. Swallowing at the hook boundary with a visible `error` string is what keeps a failed save from failing silently, per the project's error-handling convention.

- [ ] **Step 2: Verify it compiles**

Run (from `Frontend/`): `pnpm exec tsc --noEmit`
Expected: no errors referencing `hooks/useBooks.ts`. Errors in `App.tsx` referencing `updateBook`/`addBook` not existing are expected until Task 9.

- [ ] **Step 3: Commit**

```bash
git add Frontend/src/hooks/useBooks.ts
git commit -m "Rewrite useBooks against the real backend with add/update mutations"
```

---

## Task 9: Frontend — login page, auth gating, real wiring

**Files:**
- Create: `Frontend/src/pages/LoginPage.tsx`
- Modify: `Frontend/src/App.tsx` (replace entirely)
- Modify: `Frontend/src/pages/HomePage.tsx`
- Verify: `pnpm exec tsc --noEmit`, then manual browser QA

**Interfaces:**
- Consumes: `useAuth()` (Task 7), `useBooks()` (Task 8), existing page components' prop contracts — `BookDetailsPage` and `ReadingProgressPage` both take `onUpdateBook: (id: string, updates: Partial<Book>) => void` (`Frontend/src/pages/BookDetailsPage.tsx`, `Frontend/src/pages/ReadingProgressPage.tsx`, unchanged), `SearchModal` currently takes `onAddBook: (catalogBook: CatalogBook) => void` (`Frontend/src/components/SearchModal.tsx`, still on the old mock-driven signature until Task 11 — passing an async function here is fine, see note in Step 2 below).
- Produces: `LoginPage` component (props: `onLogin: (username: string, password: string) => Promise<void>`, `onRegister: (username: string, password: string) => Promise<void>`); rewritten `App.tsx`; `HomePage` gains an `onLogout: () => Promise<void>` prop and a logout button.

- [ ] **Step 1: Write `LoginPage`**

Create `Frontend/src/pages/LoginPage.tsx`:

```tsx
import { useState } from "react";
import type { FormEvent } from "react";

const PASSWORD_RULE = /^(?=.*\d)(?=.*[!@#$%^&*]).{8,20}$/;
const PASSWORD_HINT =
  "8-20 characters, at least one digit and one of !@#$%^&*";

interface Props {
  onLogin: (username: string, password: string) => Promise<void>;
  onRegister: (username: string, password: string) => Promise<void>;
}

type Mode = "login" | "register";

export default function LoginPage({ onLogin, onRegister }: Props) {
  const [mode, setMode] = useState<Mode>("login");
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);

    if (mode === "register" && !PASSWORD_RULE.test(password)) {
      setError(PASSWORD_HINT);
      return;
    }

    setSubmitting(true);
    try {
      if (mode === "register") {
        await onRegister(username, password);
      }
      await onLogin(username, password);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Something went wrong.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen bg-cream flex items-center justify-center px-5">
      <div
        className="w-full max-w-sm rounded-2xl p-6"
        style={{
          background: "#FDFBF6",
          border: "1px solid #DDD4BF",
          boxShadow: "0 2px 10px rgba(44,26,14,0.06)",
        }}
      >
        <h1 className="font-serif text-2xl font-semibold text-bark text-center mb-1">
          Book Tracker
        </h1>
        <p className="text-muted text-sm text-center mb-6">
          {mode === "login" ? "Sign in to your library" : "Create an account"}
        </p>

        <form onSubmit={handleSubmit} className="flex flex-col gap-3">
          <input
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            placeholder="Username"
            autoComplete="username"
            required
            className="w-full rounded-xl px-4 py-3 text-sm outline-none"
            style={{ background: "#EAE2D0", color: "#5C3D28" }}
          />
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="Password"
            autoComplete={
              mode === "login" ? "current-password" : "new-password"
            }
            required
            className="w-full rounded-xl px-4 py-3 text-sm outline-none"
            style={{ background: "#EAE2D0", color: "#5C3D28" }}
          />

          {mode === "register" && (
            <p className="text-[11px] text-muted-light">{PASSWORD_HINT}</p>
          )}

          {error && (
            <p className="text-xs" style={{ color: "#A85830" }}>
              {error}
            </p>
          )}

          <button
            type="submit"
            disabled={submitting}
            className="w-full py-3 rounded-xl text-sm font-semibold text-white transition-all active:scale-[0.99] disabled:opacity-60"
            style={{ background: "linear-gradient(135deg, #b46bdc, #934dee)" }}
          >
            {submitting
              ? "Please wait…"
              : mode === "login"
                ? "Sign In"
                : "Create Account"}
          </button>
        </form>

        <button
          onClick={() => {
            setMode((m) => (m === "login" ? "register" : "login"));
            setError(null);
          }}
          className="w-full text-center text-xs text-muted hover:text-bark transition-colors mt-4"
        >
          {mode === "login"
            ? "Need an account? Register"
            : "Already have an account? Sign in"}
        </button>
      </div>
    </div>
  );
}
```

- [ ] **Step 2: Rewrite `App.tsx`**

Replace the full contents of `Frontend/src/App.tsx` with:

```tsx
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
  const { books, error, loading, addBook, updateBook } = useBooks();
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
```

`SearchModal`'s `onAddBook` prop is still typed `(catalogBook: CatalogBook) => void` until Task 11 — passing an `async` function here compiles cleanly regardless, since TypeScript allows a function returning any value (including `Promise<void>`) to satisfy a callback typed as returning `void`. `SearchModal` itself still searches the old mock catalog until Task 11 lands, so exercising "add via search" isn't part of this task's QA (see Step 4).

- [ ] **Step 3: Add the logout button to `HomePage`**

In `Frontend/src/pages/HomePage.tsx`, add `onLogout: () => Promise<void>;` to the `Props` interface:

```tsx
interface Props {
  books: Book[];
  onNavigateToBook: (bookId: string) => void;
  onNavigateToList: (status: ReadingStatus) => void;
  onOpenSearch: () => void;
  onLogout: () => Promise<void>;
}
```

Add `onLogout` to the destructured props in the function signature:

```tsx
export default function HomePage({
  books,
  onNavigateToBook,
  onNavigateToList,
  onOpenSearch,
  onLogout,
}: Props) {
```

Replace the header block (currently just the date + "My Library" heading) with:

```tsx
      <div className="px-5 pt-14 pb-5 flex items-start justify-between gap-3">
        <div>
          <p className="text-muted text-xs tracking-wide">{today}</p>
          <h1 className="font-serif text-[32px] font-semibold text-bark mt-1 leading-tight">
            My Library
          </h1>
        </div>
        <button
          onClick={onLogout}
          aria-label="Log out"
          className="w-9 h-9 rounded-full flex items-center justify-center transition-colors flex-shrink-0 mt-1"
          style={{
            background: "var(--bg-card)",
            border: "1px solid var(--border-card)",
          }}
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
              d="M15.75 9V5.25A2.25 2.25 0 0013.5 3h-6a2.25 2.25 0 00-2.25 2.25v13.5A2.25 2.25 0 007.5 21h6a2.25 2.25 0 002.25-2.25V15m3-3l3 3m0 0l-3 3m3-3H9"
            />
          </svg>
        </button>
      </div>
```

- [ ] **Step 4: Verify it compiles, then manual QA**

Run (from `Frontend/`): `pnpm exec tsc --noEmit`
Expected: no errors.

If `Frontend/.env` doesn't exist yet, create it from `.env.example` with `VITE_API_URL=https://localhost:7233/api` (it's gitignored — local only).

Start the backend (`dotnet run` from `Backend/`) and the frontend (`pnpm dev` from `Frontend/`), open `http://localhost:5173`:
1. Register a brand-new username (different from `plantest`) → should land inside the app with an empty library (not stuck on `LoginPage`).
2. Reload the page → should stay logged in (session cookie), not bounce back to `LoginPage`.
3. Click the logout button on the Home page → should return to `LoginPage`.
4. Log in as `plantest` / `Testpass1!` (the account Task 1's verification added the "Dune" book to) → should land inside the app with that book showing in the library — a "Couldn't load books" error banner here is a real bug, not expected.
5. Skip testing "add via search" — that's Task 11.

- [ ] **Step 5: Commit**

```bash
git add Frontend/src/pages/LoginPage.tsx Frontend/src/App.tsx Frontend/src/pages/HomePage.tsx
git commit -m "Add login/register page, gate the app on auth, wire real book data"
```

---

## Task 10: Frontend — real cover images in `BookCover`

**Files:**
- Modify: `Frontend/src/components/BookCover.tsx` (replace entirely)
- Verify: `pnpm exec tsc --noEmit`, then manual browser QA

**Interfaces:**
- Consumes: `Book.coverUrl` / `CatalogBook.coverUrl` (Task 4) — both already flow into every `<BookCover book={...} />` call site unchanged, since those call sites just pass the whole `Book`/`CatalogBook` object.
- Produces: no new exports; `BookCover` now renders a real `<img>` when `coverUrl` is present.

- [ ] **Step 1: Replace the component**

Replace the full contents of `Frontend/src/components/BookCover.tsx` with:

```tsx
interface CoverProps {
  title: string;
  author: string;
  coverHue: number;
  coverSat: number;
  coverUrl?: string;
}

interface Props {
  book: CoverProps;
  size?: "xs" | "sm" | "md" | "lg" | "xl";
}

const SIZES = {
  xs: { outer: "w-9 h-[52px]", title: "7px", author: "5px" },
  sm: { outer: "w-12 h-[70px]", title: "8px", author: "6px" },
  md: { outer: "w-20 h-28", title: "10px", author: "7px" },
  lg: { outer: "w-28 h-40", title: "11px", author: "8px" },
  xl: { outer: "w-36 h-52", title: "12px", author: "9px" },
};

export default function BookCover({ book, size = "md" }: Props) {
  const s = SIZES[size];

  if (book.coverUrl) {
    return (
      <div
        className={`${s.outer} rounded flex-shrink-0 relative overflow-hidden`}
        style={{ boxShadow: "var(--shadow-cover)" }}
      >
        <img
          src={book.coverUrl}
          alt={book.title}
          loading="lazy"
          className="w-full h-full object-cover"
        />
      </div>
    );
  }

  const h = book.coverHue;
  const sat = book.coverSat;

  return (
    <div
      className={`${s.outer} rounded flex-shrink-0 relative overflow-hidden`}
      style={{
        background: `linear-gradient(155deg, hsl(${h},${sat}%,20%) 0%, hsl(${h + 28},${sat - 4}%,33%) 55%, hsl(${h + 10},${sat}%,26%) 100%)`,
        boxShadow: "var(--shadow-cover)",
      }}
    >
      <div
        className="absolute left-0 top-0 bottom-0 w-[3px]"
        style={{ background: `hsl(${h},${sat}%,12%)` }}
      />
      <div
        className="absolute top-0 left-0 right-0 h-px"
        style={{ background: "rgba(164, 15, 15, 0.18)" }}
      />
      <div
        className="absolute inset-0"
        style={{
          background:
            "linear-gradient(135deg, rgba(97, 36, 133, 0.08) 0%, transparent 50%)",
        }}
      />

      <div className="absolute inset-0 flex flex-col justify-end p-1.5 pl-2.5">
        <p
          className="font-serif text-white leading-tight font-medium"
          style={{
            fontSize: s.title,
            display: "-webkit-box",
            WebkitBoxOrient: "vertical",
            WebkitLineClamp: 4,
            overflow: "hidden",
            opacity: 0.92,
          }}
        >
          {book.title}
        </p>
        <p
          className="text-white mt-0.5 leading-tight"
          style={{
            fontSize: s.author,
            opacity: 0.5,
            overflow: "hidden",
            textOverflow: "ellipsis",
            whiteSpace: "nowrap",
          }}
        >
          {book.author.split(" ").slice(-1)[0]}
        </p>
      </div>
    </div>
  );
}
```

- [ ] **Step 2: Verify it compiles, then manual QA**

Run (from `Frontend/`): `pnpm exec tsc --noEmit`
Expected: no errors.

With both dev servers running and logged in (from Task 9), open the Home page: the "Dune" test book added via PowerShell in Task 1 should now show its real Google Books cover image instead of the gradient placeholder. If you don't have a book with a cover in your library yet, use Swagger (`https://localhost:7233/swagger`) or the PowerShell commands from Task 1 Step 8 to search and add one.

- [ ] **Step 3: Commit**

```bash
git add Frontend/src/components/BookCover.tsx
git commit -m "Render real Google Books cover art with gradient fallback"
```

---

## Task 11: Frontend — live search in `SearchModal`

**Files:**
- Modify: `Frontend/src/components/SearchModal.tsx` (replace entirely)
- Verify: `pnpm exec tsc --noEmit`, then manual browser QA

**Interfaces:**
- Consumes: `searchBooks` from `@/api/books` (Task 5), `toCatalogBook` from `@/api/mappers` (Task 4), `addBook` via `App.tsx`'s already-wired `onAddBook` handler (Task 9).
- Produces: `SearchModal`'s `onAddBook` prop is now `(catalogBook: CatalogBook) => Promise<void>` — matches what Task 9's `App.tsx` already passes.

- [ ] **Step 1: Replace the component**

Replace the full contents of `Frontend/src/components/SearchModal.tsx` with:

```tsx
import { useEffect, useState } from "react";
import type { Book, CatalogBook } from "../types/data";
import { searchBooks } from "@/api/books";
import { toCatalogBook } from "@/api/mappers";
import BookCover from "./BookCover";

interface Props {
  books: Book[];
  onClose: () => void;
  onSelectBook: (bookId: string) => void;
  onAddBook: (catalogBook: CatalogBook) => Promise<void>;
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
  const [catalogResults, setCatalogResults] = useState<CatalogBook[]>([]);
  const [searching, setSearching] = useState(false);
  const [addingId, setAddingId] = useState<string | null>(null);
  const q = query.toLowerCase().trim();

  const libraryResults = books.filter(
    (b) =>
      b.title.toLowerCase().includes(q) || b.author.toLowerCase().includes(q),
  );

  useEffect(() => {
    if (q.length <= 1) {
      setCatalogResults([]);
      setSearching(false);
      return;
    }

    let cancelled = false;
    setSearching(true);

    const timer = setTimeout(() => {
      searchBooks(query.trim())
        .then((results) => {
          if (cancelled) return;
          const inLibrary = new Set(books.map((b) => b.id));
          setCatalogResults(
            results.map(toCatalogBook).filter((b) => !inLibrary.has(b.id)),
          );
        })
        .catch((err) => {
          console.error(err);
          if (!cancelled) setCatalogResults([]);
        })
        .finally(() => {
          if (!cancelled) setSearching(false);
        });
    }, 300);

    return () => {
      cancelled = true;
      clearTimeout(timer);
    };
  }, [q, query, books]);

  const hasResults = libraryResults.length > 0 || catalogResults.length > 0;

  const handleAdd = async (catalogBook: CatalogBook) => {
    setAddingId(catalogBook.id);
    await onAddBook(catalogBook);
    setAddingId(null);
  };

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

          {q && searching && !hasResults && (
            <p className="text-muted text-sm text-center py-10">Searching…</p>
          )}

          {q && !searching && !hasResults && (
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
                  disabled={addingId === book.id}
                  onClick={() => handleAdd(book)}
                  className="w-full flex items-center gap-4 px-5 py-3 hover:bg-surface transition-colors text-left disabled:opacity-60"
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
                    {addingId === book.id ? (
                      "Adding…"
                    ) : (
                      <>
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
                      </>
                    )}
                  </span>
                </button>
              ))}
            </>
          )}

          <div className="h-2" />
        </div>

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
```

- [ ] **Step 2: Verify it compiles, then manual QA**

Run (from `Frontend/`): `pnpm exec tsc --noEmit`
Expected: no errors.

With both dev servers running and logged in: open the search modal (FAB button on Home), type a real title (e.g. "project hail mary") and wait ~300ms — real results from Google Books should appear under "Add to library" with real cover thumbnails. Tap one → button shows "Adding…" → modal closes → you land on that book's detail page, and it now also appears in "To Read" on the Home page. Search again for the same title → it should now appear under "In your library" instead of "Add to library" (the `inLibrary` filter).

- [ ] **Step 3: Commit**

```bash
git add Frontend/src/components/SearchModal.tsx
git commit -m "Wire SearchModal to live Google Books search and real add-to-library"
```

---

## Task 12: Frontend — remove mock data, full golden-path QA

**Files:**
- Modify: `Frontend/src/types/data.ts`
- Verify: `pnpm exec tsc --noEmit`, `dotnet build` (from `Backend/`), then full manual browser QA

**Interfaces:**
- Consumes: nothing new — confirms nothing still references `INITIAL_BOOKS`/`SEARCH_CATALOG` (Task 9 stopped importing `INITIAL_BOOKS`, Task 11 stopped importing `SEARCH_CATALOG`).
- Produces: final, real-data-only `Frontend/src/types/data.ts`.

- [ ] **Step 1: Remove the mock arrays**

In `Frontend/src/types/data.ts`, delete the `INITIAL_BOOKS` and `SEARCH_CATALOG` exported constants (everything from `export const INITIAL_BOOKS: Book[] = [` through the end of the file), leaving only the `ReadingStatus` type and the `Book`/`CatalogBook` interfaces (as they stand after Task 4).

- [ ] **Step 2: Verify it compiles**

Run (from `Frontend/`): `pnpm exec tsc --noEmit`
Expected: no errors — confirms no file still imports the deleted constants.

- [ ] **Step 3: Full golden-path QA**

With `Backend/` running (`dotnet run`) and `Frontend/` running (`pnpm dev`), walk through the whole app in the browser:

1. Register a new account → lands inside the app with an empty library.
2. Open search, find and add two or three real books.
3. On a book's details page: change status to "Reading" → go back → Home's "Currently Reading" section shows it with a progress bar.
4. Open "Reading Progress" for that book, move the slider and/or type a page number, tap "Save" → back on details, the progress bar reflects the new value.
5. Set a star rating on the progress page.
6. On details, edit the notes field and save.
7. **Reload the browser tab (full refresh)** → status, pages read, rating, and notes must all still be there — this proves they're persisted server-side, not just local state.
8. Visit each of the three status list pages (To Read / Reading / Read) from Home and confirm books appear under the right one.
9. Log out via the Home page button → back at `LoginPage`. Log back in → library intact.
10. Optional session-expiry check: in browser devtools, delete the `access_token` cookie for `localhost`, then trigger any action (e.g. click into a book) → the app should detect the resulting `401` and return you to `LoginPage`, not hang or show a raw error.

- [ ] **Step 4: Final build check on both sides**

Run (from `Backend/`): `dotnet build`
Run (from `Frontend/`): `pnpm exec tsc --noEmit`
Expected: both succeed with no errors or warnings about unused imports (`noUnusedLocals`/`noUnusedParameters` are enabled in `tsconfig.app.json`).

- [ ] **Step 5: Commit**

```bash
git add Frontend/src/types/data.ts
git commit -m "Remove mock book data now that every page uses the real API"
```
