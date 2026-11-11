---
paths:
  - "src/HouseBroker.Api/**"
---

# API rules (third-party consumable)

- Routes: `api/v1/<resource>` with plural nouns (`api/v1/listings`, `api/v1/listings/{id:guid}`).
- `[ApiController]` on every controller; `[ProducesResponseType]` for each status code returned.
- Status codes: 200 read, 201 + `CreatedAtAction` on create, 204 on update/delete, 400 validation, 401/403 auth, 404 missing.
- Errors are RFC 7807 ProblemDetails (handled centrally — don't build error bodies in controllers).
- Auth: `[Authorize(Roles = Roles.Broker)]` for create/update/delete listings; search and details are `[AllowAnonymous]` or any authenticated user.
- Ownership: only the broker who owns a listing may update/delete it — enforce in the Application service, not the controller.
- Search uses query parameters + paging (`page`, `pageSize` max 50) and returns `PagedResult<T>`.
- Controllers take `CancellationToken ct` and pass it down.
