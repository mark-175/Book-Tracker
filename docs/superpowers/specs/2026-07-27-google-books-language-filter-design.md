# Google Books Language Filtering — Design

Date: 2026-07-27

## Context

`GoogleBookService.FindBookInGoogle` currently joins the caller's preferred languages into Google's `langRestrict` query parameter, but Google doesn't honor it as expected — results come back in all languages regardless of the value passed. Separately, `UserService.GetPreferredLanguages` has been updated to return `[""]` (a list containing a single empty string) when a user has no preferred languages set, rather than `["en"]` or `[]`.

This spec replaces query-param-based language restriction with client-side filtering: fetch unrestricted from Google, then filter the returned volumes down to the caller's preferred languages in `GoogleBookService` itself. It also adds `printType=books` to the Google request to exclude magazines and other non-book volumes at the source, since the app only stores books.

## Changes

### 1. URL building

Drop `langRestrict` entirely (it doesn't work, and filtering now happens client-side). Add `printType=books`.

New URL: `{BaseUrl}?q={query}&printType=books&key={ApiKey}`

`preferredLanguages` remains a parameter of `FindBookInGoogle`, but is no longer used to build the URL — only for post-fetch filtering (see below).

### 2. Filtering logic

After deserializing the response, decide whether filtering is active:

- **No preference** (skip filtering, return everything): `preferredLanguages` is empty, or every entry is null/empty/whitespace. This covers both `[]` and today's `[""]` sentinel from `UserService.GetPreferredLanguages`, so the filter doesn't silently break if that method's exact return shape changes later.
- **Preference active**: at least one non-blank entry exists. Keep only volumes where `VolumeInfo.Language` is non-null and case-insensitively matches (`StringComparer.OrdinalIgnoreCase`) one of the non-blank preferred entries. Volumes with a null/missing `Language` are dropped when filtering is active (can't confirm a match, so exclude rather than risk showing the wrong language).

### 3. Response shape

`GoogleBooksSearchResponse.Items` is replaced with the filtered list. `TotalItems` is updated to `Items.Count` after filtering, so it stays consistent with what's actually returned even though nothing currently reads it.

### 4. Error handling / logging

Unchanged. The existing try/catch around the HTTP call and the `Warning`-level log on `HttpRequestException` stay as-is — filtering only runs on the successful-response path, after the existing null check.

### 5. Out of scope

`DbBookService.FindBookInDb`'s language filter (`preferredLanguages.Contains(b.Language)`, case-sensitive) is a separate code path not touched by this change. It has a pre-existing quirk where the `[""]` no-preference sentinel won't match any cached book's language, forcing a Google lookup even when a matching book exists locally — noted here for visibility, not addressed in this spec.

## Testing

No backend test project exists yet (`Backend/CLAUDE.md`). Verify manually via the search endpoint: once with a user that has a preferred language set (confirm only matching-language results come back), once with a user that has none (confirm all languages come back, unfiltered), and confirm returned items are books (no magazines) in both cases.
