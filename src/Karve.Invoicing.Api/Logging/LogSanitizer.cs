using System.Text;
using System.Text.Encodings.Web;

namespace Karve.Invoicing.Api.Logging;

internal static class LogSanitizer
{
    private const int DefaultMaxLength = 256;

    public static string SanitizeForLog(string? value, int maxLength = DefaultMaxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Normalize(NormalizationForm.FormKC);
        var withoutControlChars = RemoveControlCharacters(normalized);
        var compactWhitespace = CollapseWhitespace(withoutControlChars);
        var neutralizedSqlMeta = NeutralizeSqlMetaCharacters(compactWhitespace);
        var truncated = Truncate(neutralizedSqlMeta, maxLength);

        // HTML-encode to keep log sinks and viewers safe from rendered markup.
        return HtmlEncoder.Default.Encode(truncated);
    }

    public static string SanitizeForUrlLog(string? value, int maxLength = DefaultMaxLength)
    {
        var sanitized = SanitizeForLog(value, maxLength);
        return Uri.EscapeDataString(sanitized);
    }

    private static string RemoveControlCharacters(string input)
    {
        var builder = new StringBuilder(input.Length);

        foreach (var ch in input)
        {
            if (ch == '\r' || ch == '\n' || ch == '\t')
            {
                builder.Append(' ');
                continue;
            }

            if (!char.IsControl(ch))
            {
                builder.Append(ch);
            }
        }

        return builder.ToString();
    }

    private static string CollapseWhitespace(string input)
    {
        var builder = new StringBuilder(input.Length);
        var previousWasWhitespace = false;

        foreach (var ch in input)
        {
            if (char.IsWhiteSpace(ch))
            {
                if (!previousWasWhitespace)
                {
                    builder.Append(' ');
                    previousWasWhitespace = true;
                }

                continue;
            }

            builder.Append(ch);
            previousWasWhitespace = false;
        }

        return builder.ToString().Trim();
    }

    private static string NeutralizeSqlMetaCharacters(string input)
    {
        return input
            .Replace("--", "- -", StringComparison.Ordinal)
            .Replace("/*", "/ *", StringComparison.Ordinal)
            .Replace("*/", "* /", StringComparison.Ordinal)
            .Replace(";", "\\;", StringComparison.Ordinal)
            .Replace("'", "''", StringComparison.Ordinal);
    }

    private static string Truncate(string input, int maxLength)
    {
        if (maxLength <= 0)
        {
            return string.Empty;
        }

        return input.Length <= maxLength
            ? input
            : string.Concat(input.AsSpan(0, maxLength), "...");
    }
}
