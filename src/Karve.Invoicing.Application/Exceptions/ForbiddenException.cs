namespace Karve.Invoicing.Application.Exceptions;

/// <summary>
/// Represents an authorization failure for a resource the user is not allowed to access.
/// </summary>
public sealed class ForbiddenException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
    /// </summary>
    public ForbiddenException()
        : base("Access to the requested resource is forbidden.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
    /// </summary>
    /// <param name="message">Error message describing the authorization failure.</param>
    public ForbiddenException(string message)
        : base(message)
    {
    }
}
