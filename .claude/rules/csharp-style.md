---
paths:
  - "**/*.cs"
---

# C# style

- File-scoped namespaces; one public type per file (small related enums/records may share).
- Nullable reference types on; no `!` null-forgiving unless justified in a comment.
- Primary constructors for DI (`public class ListingService(IListingRepository repo) : IListingService`).
- `async`/`await` all the way; every async method ends with `Async` and accepts `CancellationToken ct`.
- Money is `decimal`, never `double`. Round commission with `Math.Round(x, 2, MidpointRounding.AwayFromZero)`.
- Use `record` for DTOs and requests; classes for entities.
- Collection expressions (`[]`), pattern matching and `switch` expressions where they read clearly.
- No magic strings for roles — use `Roles.Broker` / `Roles.Seeker`.
- XML `<summary>` on public services, interfaces and controllers.
- Don't swallow exceptions; throw domain-meaningful ones (`KeyNotFoundException`, `UnauthorizedAccessException`, `ValidationException`) and let `GlobalExceptionHandler` map them.
