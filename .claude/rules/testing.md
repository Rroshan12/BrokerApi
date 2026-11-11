---
paths:
  - "tests/**"
---

# Testing rules

- xUnit + Moq + FluentAssertions. Name tests `Method_Scenario_ExpectedResult`.
- Arrange / Act / Assert with blank lines between sections.
- Required coverage (assignment):
  - **Commission**: each band plus boundaries — 49,99,999 · 50,00,000 · 1,00,00,000 · 1,00,00,001 — and inactive rules ignored.
  - **ListingService**: create computes commission; update by non-owner throws; commission hidden for non-owners; not-found throws.
  - **Controllers**: mocked service → correct status codes (200/201/204/400/404).
- Unit tests never touch a real database or network. Use mocks or EF InMemory only where a repository is under test.
- Use `[Theory]` + `[InlineData]` for tables of cases (commission bands).
- Delete template `UnitTest1.cs` once real tests exist.
