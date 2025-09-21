using System.Reflection;

namespace CSharpPlayersGuide.RichConsole;

/// <summary>
/// Handles styled text such as "[italic]Hello![/] My favorite colors are [red]red[/], [green underline]green[/], and [cerulean blink]blue[/]!"
/// Warning: Here be dragons. This is complicated code that uses regular expressions and a parser for a domain-specific language. It's not for the
/// faint of heart, but there's also some interesting things to see here.
/// </summary>
internal static class Styling
{
    /// <summary>
    /// Renders styled text, parsing delimiters like [red] or [underline] (terminated with a corresponding [/] tag).
    /// </summary>
    /// <param name="styledText"></param>
    public static void WriteStyled(string? styledText)
    {
        if (styledText == null) return;

        // Use a ReadOnlySpan<char> for the entire input to avoid unnecessary allocations
        ReadOnlySpan<char> text = styledText;

        // Reuse a single stack for styles to reduce allocations
        Stack<Style> styleStack = new();
        styleStack.Push(new Style(TextEffects.None, null, null));

        int index = 0;
        while (index < text.Length)
        {
            ReadOnlySpan<char> remaining = text[index..];

            if (remaining.IsEmpty) break;

            if (remaining.StartsWith("[/]"))
            {
                // End style tag
                if (styleStack.Count > 1)
                    styleStack.Pop();
                index += 3;
            }
            else if (remaining.Length >= 2 && remaining[0] == '\\' && remaining[1] == '[')
            {
                // Escaped open bracket
                DisplayText("[", styleStack.Peek());
                index += 2;
            }
            else if (remaining.Length >= 2 && remaining[0] == '\\' && remaining[1] == '\\')
            {
                // Escaped backslash
                DisplayText("\\", styleStack.Peek());
                index += 2;
            }
            else if (remaining[0] == '[')
            {
                // Opening style tag
                int closingBracket = remaining.IndexOf(']');
                if (closingBracket > 0)
                {
                    // Process style directly from the span
                    ReadOnlySpan<char> styleContent = remaining[1..closingBracket];
                    Style newStyle = ParseStyle(styleContent, styleStack.Peek());
                    styleStack.Push(newStyle);
                    index += closingBracket + 1;
                }
                else
                {
                    // Unclosed bracket, treat as text
                    DisplayText("[", styleStack.Peek());
                    index++;
                }
            }
            else
            {
                // Regular text - find the next special character
                int nextOpenBracket = remaining.IndexOf('[');
                int nextBackslash = remaining.IndexOf('\\');

                int endIndex;
                if (nextOpenBracket < 0 && nextBackslash < 0)
                {
                    // No more special characters, process the rest of the text
                    endIndex = remaining.Length;
                }
                else if (nextOpenBracket < 0)
                {
                    endIndex = nextBackslash;
                }
                else if (nextBackslash < 0)
                {
                    endIndex = nextOpenBracket;
                }
                else
                {
                    endIndex = Math.Min(nextOpenBracket, nextBackslash);
                }

                // Display the text segment directly from the span
                if (endIndex > 0)
                {
                    DisplayTextSpan(remaining[..endIndex], styleStack.Peek());
                }
                index += Math.Max(1, endIndex);
            }
        }
    }

    // New method to display text directly from a span without allocating a string
    private static void DisplayTextSpan(ReadOnlySpan<char> text, Style style)
    {
        // Convert to string only when needed for the RichConsole API
        RichConsole.Write(text.ToString(), style.Foreground, style.Background, style.TextEffects);
    }

    // Keep original DisplayText for compatibility
    private static void DisplayText(string text, Style style)
    {
        RichConsole.Write(text, style.Foreground, style.Background, style.TextEffects);
    }

    // Parse a style directly from a span
    private static Style ParseStyle(ReadOnlySpan<char> styleAttributes, Style currentStyle)
    {
        TextEffects textEffects = TextEffects.None;
        Color? foreground = null;
        Color? background = null;

        // Split the attributes by space without allocating a string array
        int startIndex = 0;
        while (startIndex < styleAttributes.Length)
        {
            // Find the next space
            int endIndex = styleAttributes[startIndex..].IndexOf(' ');
            ReadOnlySpan<char> attribute;

            if (endIndex < 0)
            {
                attribute = styleAttributes[startIndex..];
                startIndex = styleAttributes.Length;
            }
            else
            {
                attribute = styleAttributes.Slice(startIndex, endIndex);
                startIndex += endIndex + 1;
            }

            if (attribute.IsEmpty) continue;

            // Process each attribute
            ref Color? activeBrush = ref foreground;

            // Handle prefix (f: or b:)
            if (attribute.Length > 2 && attribute[1] == ':')
            {
                if (attribute[0] is 'b' or 'B')
                    activeBrush = ref background;
                else if (attribute[0] is 'f' or 'F')
                    activeBrush = ref foreground;

                attribute = attribute[2..];
            }

            // Check for text effects
            string attributeString = attribute.ToString().ToLower();
            foreach (var option in Enum.GetValues<TextEffects>())
            {
                if (attributeString.Equals(option.ToString(), StringComparison.CurrentCultureIgnoreCase))
                {
                    textEffects |= option;
                    break;
                }
            }

            // Check for named colors
            foreach (PropertyInfo option in typeof(Colors).GetProperties())
            {
                if (option.Name.Equals(attributeString, StringComparison.CurrentCultureIgnoreCase))
                {
                    activeBrush = (Color?)option.GetValue(null, null);
                    break;
                }
            }

            // Check for RGB color format (r,g,b)
            if (attribute.Length > 2 && attribute[0] == '(' && attribute[^1] == ')')
            {
                attribute = attribute[1..^1];
                int firstComma = attribute.IndexOf(',');
                int secondComma = firstComma >= 0 ? attribute[(firstComma + 1)..].IndexOf(',') + firstComma + 1 : -1;

                if (firstComma > 0 && secondComma > firstComma)
                {
                    ReadOnlySpan<char> rSpan = attribute[..firstComma];
                    ReadOnlySpan<char> gSpan = attribute.Slice(firstComma + 1, secondComma - firstComma - 1);
                    ReadOnlySpan<char> bSpan = attribute[(secondComma + 1)..];

                    if (byte.TryParse(rSpan, out byte r) && byte.TryParse(gSpan, out byte g) && byte.TryParse(bSpan, out byte b))
                    {
                        activeBrush = new Color(r, g, b);
                    }
                }
            }
        }

        return new Style(textEffects, foreground, background) + currentStyle;
    }

    /// <summary>
    /// Represents a style, which is the set of colors and text effects to apply. Styles can be aggregated with the `+` operator.
    /// </summary>
    private record Style(TextEffects TextEffects, Color? Foreground, Color? Background)
    {
        // Non-null color values in the second style supersede values from the first style, and all set attributes in both the first
        // and second style remain set in the resulting style.
        public static Style operator +(Style a, Style b)
        {
            return new Style(a.TextEffects | b.TextEffects, b.Foreground ?? a.Foreground, b.Background ?? a.Background);
        }
    }
}