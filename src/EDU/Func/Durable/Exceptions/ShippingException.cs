namespace EDU.Func.Durable.Exceptions;

/// <summary>
/// Custom exception for shipping failures.
/// </summary>
public sealed class ShippingException(string message, Exception? inner = null) : Exception(message, inner);
