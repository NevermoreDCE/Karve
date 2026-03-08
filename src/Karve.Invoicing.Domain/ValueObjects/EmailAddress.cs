namespace Karve.Invoicing.Domain.ValueObjects;

public class EmailAddress
{
    public string Value { get; private set; }

    public EmailAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email address cannot be empty", nameof(value));

        // Basic email validation
        if (!value.Contains("@") || !value.Contains("."))
            throw new ArgumentException("Invalid email address format", nameof(value));

        Value = value;
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj)
    {
        if (obj is not EmailAddress other) return false;
        return Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}