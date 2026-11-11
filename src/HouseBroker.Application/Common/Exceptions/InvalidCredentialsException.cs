namespace HouseBroker.Application.Common.Exceptions;

/// <summary>Login failed. Mapped to HTTP 401. The message never says which part was wrong.</summary>
public sealed class InvalidCredentialsException() : Exception("Invalid email or password.");
