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

        // Start by parsing the text to get all of the individual start, end, and text markers.
        List<Token> tokens = new();
        int index = 0;
        while (index < styledText.Length)
        {
            ReadOnlySpan<char> active = ((ReadOnlySpan<char>)styledText).Slice(index); // Despite using ReadOnlySpan here, I suspect we could make all of this _much_ more efficient.

            // This tiny language is simple enough I did it in sort of a brute force way. I've studied parsers before, but I didn't consult with any
            // resources besides what was kicking around in my brain. I'd accept a PR that turns this into a better example of how to do this "by the book."
            if (active.Length == 0) tokens.Add(new EndToken(index, 0));
            else if (active.StartsWith("[/]")) tokens.Add(new EndStyleToken(index, 3));
            else if (active.StartsWith("\\[")) tokens.Add(new TextToken(index, 2, "["));
            else if (active.StartsWith("\\\\")) tokens.Add(new TextToken(index, 2, "\\"));
            else if (active.StartsWith("[")) tokens.Add(ParseStartStyleToken(index, active));
            else tokens.Add(ParseTextToken(index, active));
            index += tokens[^1].Length;
        }

        // This is the actual rendering code, which applies styles as needed and displays text.
        Stack<Style> style = new();
        style.Push(new Style(TextEffects.None, null, null));
        foreach (Token token in tokens)
        {
            if (token is EndToken) break;
            if (token is EndStyleToken) style.Pop();
            if (token is StartStyleToken t) style.Push(style.Peek() + ParseStartStyleToken(t));
            if (token is TextToken text) DisplayText(text.Text, style.Peek());
        }
    }

    // Where text is actually displayed with a given style.
    private static void DisplayText(string text, Style style)
    {
        RichConsole.Write(text, style.Foreground, style.Background, style.TextEffects);
    }

    private static Style ParseStartStyleToken(StartStyleToken t)
    {
        // This is one of the more complex methods here. It parses something like "[blink red underline b:(128,29,0)]" and turns that into a style.
        // I believe it works great for the happy path, but I haven't done a lot of testing to ensure it fails in smart ways. I'd accept a PR that
        // adds unit tests and/or addresses failure cases better.
        TextEffects textEffects = TextEffects.None;
        Color? foreground = null;
        Color? background = null;
        foreach (string attribute in t.Attributes)
        {
            ref Color? activeBrush = ref foreground;
            string a = attribute.ToLower();
            if (a.StartsWith("b:")) activeBrush = ref background;
            else if (a.StartsWith("f:")) activeBrush = ref foreground;

            if (a[1] == ':') a = a.Substring(2);

            foreach (var option in Enum.GetValues<TextEffects>())
                if (a == option.ToString().ToLower()) textEffects |= option;

            foreach (PropertyInfo option in typeof(Colors).GetProperties())
            {
                if (option.Name.ToLower() == a) activeBrush = (Color?)option.GetValue(null, null);
            }
            if (a.StartsWith("(") && a.EndsWith(")"))
            {
                a = a.Substring(1, a.Length - 2);
                string[] tokens = a.Split(',');
                if (byte.TryParse(tokens[0], out byte r) && byte.TryParse(tokens[1], out byte g) && byte.TryParse(tokens[2], out byte b))
                    activeBrush = new Color(r, g, b);
            }
        }
        return new Style(textEffects, foreground, background);
    }

    /// <summary>
    /// Represents a style, which is the set of colors and text effects to apply. Styles can be aggregated with the `+` operator.
    /// </summary>
    private record Style(TextEffects TextEffects, Color? Foreground, Color? Background)
    {
        /// Non-null color values in the second style supersede values from the first style, and all set attributes in both the first
        /// and second style remain set in the resulting style.
        public static Style operator +(Style a, Style b)
        {
            return new Style(a.TextEffects | b.TextEffects, b.Foreground ?? a.Foreground, b.Background ?? a.Background);
        }
    }

    private static TextToken ParseTextToken(int startIndex, ReadOnlySpan<char> upcoming)
    {
        int nextBracket = upcoming.IndexOf("[");
        int nextSlash = upcoming.IndexOf("\\");
        if (nextBracket < 0) nextBracket = upcoming.Length;
        if (nextSlash < 0) nextSlash = upcoming.Length;
        int endIndex = Math.Min(nextBracket, nextSlash);
        return new TextToken(startIndex, endIndex, upcoming.Slice(0, endIndex).ToString());
    }

    private static StartStyleToken ParseStartStyleToken(int startIndex, ReadOnlySpan<char> upcoming)
    {
        int end = upcoming.IndexOf("]");
        ReadOnlySpan<char> attributes = upcoming.Slice(1, end - 1);
        return new StartStyleToken(startIndex, end + 1, attributes.ToString().Split(" ").ToArray());
    }

    private record EndToken(int Start, int Length) : Token(Start, Length);

    private record Token(int Start, int Length);

    private record StartStyleToken(int Start, int Length, string[] Attributes) : Token(Start, Length);

    private record EndStyleToken(int Start, int Length) : Token(Start, Length);

    private record TextToken(int Start, int Length, string Text) : Token(Start, Length);
}