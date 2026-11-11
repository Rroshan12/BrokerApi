# Resilience roadmap (Polly)

Status: **planned, not implemented.** The MVP runs as a single instance against one SQL Server and calls no
external services, so it has no retry or circuit-breaker policies yet. This document records what to add, and
where, once the API is scaled out or starts depending on other services.

## Why a circuit breaker

When a dependency (SQL Server, Redis, an external API) is slow or down, every request waits for its full
timeout and then fails. Under load, those waiting requests use up threads and connections, so one failing
dependency takes the whole API down with it.

A circuit breaker watches the failure rate. Once failures pass a threshold it **opens**, and calls fail
immediately for a short break instead of waiting. After the break it lets a few trial calls through
(**half-open**) and **closes** again if they succeed.

```
 Closed ──(failure ratio ≥ threshold)──▶ Open ──(break duration elapsed)──▶ Half-open
   ▲                                                                           │
   └──────────────────────(trial calls succeed)────────────────────────────────┘
                          (trial calls fail) ──▶ back to Open
```

## Packages

| Package | Use |
|---|---|
| `Microsoft.Extensions.Resilience` | Polly v8 pipelines registered in DI (`AddResiliencePipeline`) |
| `Microsoft.Extensions.Http.Resilience` | Ready-made pipeline for outgoing `HttpClient` calls (`AddStandardResilienceHandler`) |

Both are built on Polly v8 and publish metrics and logs through the standard .NET telemetry APIs.

## Where it would apply

### 1. SQL Server: retry inside, circuit breaker outside

- **Retry:** use EF Core's own execution strategy rather than a Polly retry, so the two don't retry twice:
  ```csharp
  services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connectionString,
      sql => sql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null)));
  ```
  **Required follow-up:** with retries on, a manual transaction must run inside the execution strategy.
  Otherwise EF throws. That affects `IdentityService.CreateUserAsync`, which opens a transaction:
  ```csharp
  var strategy = db.Database.CreateExecutionStrategy();
  await strategy.ExecuteAsync(async () =>
  {
      await using var tx = await db.Database.BeginTransactionAsync(ct);
      // create user + add role
      await tx.CommitAsync(ct);
  });
  ```
- **Circuit breaker:** wrap repository calls in a named pipeline, so that a database outage fails fast
  instead of piling up requests:
  ```csharp
  services.AddResiliencePipeline("sql", b => b
      .AddCircuitBreaker(new CircuitBreakerStrategyOptions
      {
          FailureRatio = 0.5,                          // open when half of the calls fail…
          MinimumThroughput = 20,                      // …across at least 20 calls…
          SamplingDuration = TimeSpan.FromSeconds(30), // …within 30 seconds
          BreakDuration = TimeSpan.FromSeconds(15),
          ShouldHandle = new PredicateBuilder().Handle<SqlException>().Handle<TimeoutException>()
      })
      .AddTimeout(TimeSpan.FromSeconds(10)));
  ```
  The repositories (Infrastructure) take a `ResiliencePipelineProvider<string>` and run their queries through
  `GetPipeline("sql")`. Application and Domain don't change, which keeps resilience an infrastructure
  concern.

### 2. Redis (when the output cache moves to Redis)

A cache must never take the API down. If Redis fails, requests should **skip the cache and read from the
database**. Put a circuit breaker with a short timeout around the cache store, so that while Redis is down
the app stops trying to reach it on every request and serves uncached responses. Eviction failures are
already logged and swallowed in `OutputCacheInvalidator`.

### 3. Future outgoing HTTP calls

Likely future integrations include image storage or CDN uploads, maps and geocoding for locations, and SMS
or email to notify brokers. Each typed `HttpClient` should get the standard pipeline: rate limiter, total
timeout, retry with jittered backoff, circuit breaker, and per-attempt timeout.

```csharp
services.AddHttpClient<IGeocodingClient, GeocodingClient>()
    .AddStandardResilienceHandler(o => o.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30));
```

Only retry idempotent calls (GET, or PUT with an idempotency key). Never blindly retry a POST that
sends an SMS or charges money.

## API behaviour when a circuit is open

Polly throws `BrokenCircuitException`. `GlobalExceptionHandler` should map it to
**`503 Service Unavailable`** with a `Retry-After` header set to the remaining break time, as ProblemDetails
like every other error. Third-party clients then know to back off rather than retry straight away.

```csharp
BrokenCircuitException => (StatusCodes.Status503ServiceUnavailable, "Service temporarily unavailable"),
```

## Testing plan

- Unit test: when a repository throws `BrokenCircuitException`, the handler returns `503` and `Retry-After`.
- Unit test: the pipeline opens after the configured failure ratio. Polly's `TimeProvider` support lets
  these tests use fake time.
- Manual: stop SQL Server while the API is under load. Responses should switch quickly to `503` instead of
  hanging, and recover on their own once the database is back.

## Related decisions

- **Rate limiting** is left to the gateway or WAF, not done in the app (see README, *Design notes*).
- **Account lockout** (Identity, stored in the database) already protects login on every instance.
