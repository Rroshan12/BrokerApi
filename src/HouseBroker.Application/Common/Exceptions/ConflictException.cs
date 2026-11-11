namespace HouseBroker.Application.Common.Exceptions;

/// <summary>The request clashes with existing data (e.g. email already registered). Mapped to HTTP 409.</summary>
public sealed class ConflictException(string message) : Exception(message);
