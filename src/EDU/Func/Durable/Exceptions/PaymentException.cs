namespace EDU.Func.Durable.Exceptions;

/// <summary>
/// Custom exception for payment processing failures.
/// </summary>
public sealed class PaymentException(string message, Exception? inner = null) : Exception(message, inner);
