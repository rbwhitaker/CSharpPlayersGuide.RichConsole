using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace CSharpPlayersGuide.RichConsole;

/// <summary>
/// A more advanced console window API that allows you to specify foreground colors, background colors, and text effects.
/// </summary>
public static class RichConsole
{
    /// <summary>
    /// Ensures we're using UTF8 encoding to support emoji characters.
    /// </summary>
    static RichConsole()
    {
        Console.OutputEncoding = Encoding.UTF8;
    }

    /// <summary>
    /// Writes text to the console window with a specific foreground color, background color, and set of text effects.
    /// </summary>
    /// <param name="text">The text to display in the console window.</param>
    /// <param name="foreground">The foreground (text) color to use.</param>
    /// <param name="background">The background color to use.</param>
    /// <param name="textEffects">A set of bit flags for text effects to apply to the rendering of the text.</param>
    public static void Write(string? text = null, Color? foreground = null, Color? background = null, TextEffects textEffects = TextEffects.None)
    {
        // This method is the "meat" of the basic text writing functionality. All other WriteLine and Write overloads call this method
        // directly or indirectly. This is where the rubber meets the road.

        // We want to collect the set of ANSI commands to be used when displaying text, turning the colors and text effects into ANSI
        // command codes. You can read more about these here: https://en.wikipedia.org/wiki/ANSI_escape_code.
        string commandCodes = "";

        // These are all either on or off, and we apply the "on" command or "off" command as needed.
        commandCodes += textEffects.HasFlag(TextEffects.Italics) ? "\e[3m" : "\e[23m";
        commandCodes += textEffects.HasFlag(TextEffects.Strikethrough) ? "\e[9m" : "\e[29m";
        commandCodes += textEffects.HasFlag(TextEffects.Overline) ? "\e[53m" : "\e[55m";
        commandCodes += textEffects.HasFlag(TextEffects.Blink) ? "\e[6m" : "\e[25m";

        // These have multiple competing options, and we want to apply them in a particular order. (For example, if they say "underline
        // and double underline," we want double underline to take precedence.
        if (textEffects.HasFlag(TextEffects.DoubleUnderline)) commandCodes += "\e[21m";
        else if (textEffects.HasFlag(TextEffects.Underline)) commandCodes += "\e[4m";
        else commandCodes += "\e[24m";

        // Here is where we account for foreground and background colors. If they pass in null, we'll reset to the default.
        commandCodes += background?.AnsiBackgroundCode ?? "\e[49m";
        commandCodes += foreground?.AnsiForegroundCode ?? "\e[39m";

        // These are not provided by an ANSI command code, but by manual text manipulation.
        text ??= "";

        // ALL CAPS
        if (textEffects.HasFlag(TextEffects.AllCaps)) text = text.ToUpper();

        // Left, right, and center alignment.
        int length = DetermineDisplayCharacterLength(text);
        int spareSpace = Console.WindowWidth - length;
        if (textEffects.HasFlag(TextEffects.Right)) Console.Write(new string(' ', spareSpace));
        else if (textEffects.HasFlag(TextEffects.Center)) Console.Write(new string(' ', spareSpace / 2));

        // Applies the command codes and writes the text.
        Console.Write($"{commandCodes}{text}");
    }

    /// <summary>
    /// Writes a line of text (including a new line) with a specific foreground and background, and text effects applied.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="foreground">The color to use for the foreground (text) color. A value of null will use the default foreground color.</param>
    /// <param name="background">The color to use for the background. A value of null will use the default background color.</param>
    /// <param name="textEffects">The text effects (italic, underline, all caps, etc.) to apply to the text.</param>
    public static void WriteLine(string? text, Color? foreground = null, Color? background = null, TextEffects textEffects = TextEffects.None) => Write(text + Environment.NewLine, foreground, background, textEffects);

    /// <summary>
    /// Writes an object to the console window with a specific foreground color, background color, and text effects. The object is converted to text with its ToString method.
    /// This makes it possible to write things like numbers, Boolean values, or other complex object types to the console window.
    /// </summary>
    /// <param name="element">The element to display in the console window.</param>
    /// <param name="foreground">The color to use for the foreground (text) color. A valu eof null will use the default foreground color.</param>
    /// <param name="background">The color to use for the background. A valu eof null will use the default background color.</param>
    /// <param name="textEffects">The text effects (italic, underline, all caps, etc.) to apply to the text.</param>
    public static void Write(object? element, Color? foreground = null, Color? background = null, TextEffects textEffects = TextEffects.None) => Write(element?.ToString(), foreground, background, textEffects);

    /// <summary>
    /// Writes an object to the console window with a specific foreground color, background color, and text effects. The object is converted to text with its ToString method.
    /// This makes it possible to write things like numbers, Boolean values, or other complex object types to the console window. The console is moved to the next line afterward.
    /// </summary>
    /// <param name="element">The element to display in the console window.</param>
    /// <param name="foreground">The color to use for the foreground (text) color. A valu eof null will use the default foreground color.</param>
    /// <param name="background">The color to use for the background. A valu eof null will use the default background color.</param>
    /// <param name="textEffects">The text effects (italic, underline, all caps, etc.) to apply to the text.</param>
    public static void WriteLine(object? element, Color? foreground = null, Color? background = null, TextEffects textEffects = TextEffects.None) => Write(element?.ToString() + Environment.NewLine, foreground, background, textEffects);

    /// <summary>
    /// Writes text to the console window with a specific foreground color and set of text effects. The background is the default color.
    /// </summary>
    /// <param name="text">The text to display in the console window.</param>
    /// <param name="foreground">The foreground (text) color to use.</param>
    /// <param name="textEffects">A set of bit flags for text effects to apply to the rendering of the text.</param>
    public static void Write(string? text, Color? foreground, TextEffects textEffects) => Write(text, foreground, null, textEffects);

    /// <summary>
    /// Writes a line of text (including a new line) with a specific foreground and text effects applied. The background color will be the default.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="foreground">The color to use for the foreground (text) color. A value of null will use the default foreground color.</param>
    /// <param name="textEffects">The text effects (italic, underline, all caps, etc.) to apply to the text.</param>
    public static void WriteLine(string? text, Color? foreground, TextEffects textEffects) => Write(text + Environment.NewLine, foreground, null, textEffects);

    /// <summary>
    /// Writes an object to the console window with a specific foreground color and text effects. The background color will be the default.
    /// The object is converted to text with its ToString method. This makes it possible to write things like numbers, Boolean values, or
    /// other complex object types to the console window.
    /// </summary>
    /// <param name="element">The element to display in the console window.</param>
    /// <param name="foreground">The color to use for the foreground (text) color. A value of null will use the default foreground color.</param>
    /// <param name="textEffects">The text effects (italic, underline, all caps, etc.) to apply to the text.</param>
    public static void Write(object? element, Color? foreground, TextEffects textEffects) => Write(element?.ToString(), foreground, null, textEffects);

    /// <summary>
    /// Writes an object to the console window with a specific foreground color and text effects. The background color will be the default background color.
    /// The object is converted to text with its ToString method. This makes it possible to write things like numbers, Boolean values, or other complex object
    /// types to the console window. The console is moved to the next line afterward.
    /// </summary>
    /// <param name="element">The element to display in the console window.</param>
    /// <param name="foreground">The color to use for the foreground (text) color. A valu eof null will use the default foreground color.</param>
    /// <param name="textEffects">The text effects (italic, underline, all caps, etc.) to apply to the text.</param>
    public static void WriteLine(object? element, Color? foreground, TextEffects textEffects) => Write(element?.ToString() + Environment.NewLine, foreground, null, textEffects);

    /// <summary>
    /// Writes text to the console window with a specific set of text effects. The foreground and background are the default colors.
    /// </summary>
    /// <param name="text">The text to display in the console window.</param>
    /// <param name="textEffects">A set of bit flags for text effects to apply to the rendering of the text.</param>
    public static void Write(string? text, TextEffects textEffects) => Write(text, null, null, textEffects);

    /// <summary>
    /// Writes a line of text (including a new line) with a specific text effects applied. The foreground and background colors are the default colors.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="textEffects">The text effects (italic, underline, all caps, etc.) to apply to the text.</param>
    public static void WriteLine(string? text, TextEffects textEffects) => Write(text + Environment.NewLine, null, null, textEffects);

    /// <summary>
    /// Writes an object to the console window with text effects. The foreground and background colors will be the default.
    /// The object is converted to text with its ToString method. This makes it possible to write things like numbers, Boolean values, or
    /// other complex object types to the console window.
    /// </summary>
    /// <param name="element">The element to display in the console window.</param>
    /// <param name="textEffects">The text effects (italic, underline, all caps, etc.) to apply to the text.</param>
    public static void Write(object? element, TextEffects textEffects) => Write(element?.ToString(), null, null, textEffects);

    /// <summary>
    /// Writes an object to the console window with a specific text effects. The foreground and background colors will be the default colors.
    /// The object is converted to text with its ToString method. This makes it possible to write things like numbers, Boolean values, or other complex object
    /// types to the console window. The console is moved to the next line afterward.
    /// </summary>
    /// <param name="element">The element to display in the console window.</param>
    /// <param name="textEffects">The text effects (italic, underline, all caps, etc.) to apply to the text.</param>
    public static void WriteLine(object? element, TextEffects textEffects) => Write(element?.ToString() + Environment.NewLine, null, textEffects);

    /// <summary>
    /// Writes text to the console window with default colors and text effects.
    /// </summary>
    /// <param name="text">The text to display in the console window.</param>
    public static void Write(string? text) => Write(text, null, null, TextEffects.None);

    /// <summary>
    /// Writes a line of text (including a new line) with default colors and text effects.
    /// </summary>
    /// <param name="text"></param>
    public static void WriteLine(string? text) => Write(text + Environment.NewLine, null, null, TextEffects.None);

    /// <summary>
    /// Writes an object to the console window with default colors and text effects.
    /// </summary>
    /// <param name="element">The element to display in the console window.</param>
    public static void Write(object? element) => Write(element?.ToString(), null, null, TextEffects.None);

    /// <summary>
    /// Writes an object to the console window with default colors and text effects.
    /// </summary>
    /// <param name="element">The element to display in the console window.</param>
    public static void WriteLine(object? element) => Write(element?.ToString() + Environment.NewLine, null, TextEffects.None);

    public static void WriteLine() => Console.WriteLine();

    /// <summary>
    /// A convenience method that is (nearly) equivalent to <c cref="Console.ReadLine"/>, so that all of your code can use the <c cref="RichConsole"/> class instead
    /// of having some lines use <c cref="Console"/> while others use <c cref="RichConsole"/>. However, this version promises to return a non-null string, turning
    /// any null value provided by the underlying call to <c cref="Console.ReadLine"/> into an empty string.
    /// </summary>
    public static string ReadLine() => Console.ReadLine() ?? string.Empty;

    /// <summary>
    /// A convenience method that is equivalent to <c cref="Console.Clear"/>, so that all of your code can use the <c cref="RichConsole"/> class instead
    /// of having some lines use <c cref="Console"/> while others use <c cref="RichConsole"/>.
    /// </summary>
    public static void Clear() => Console.Clear();

    /// <summary>
    /// A convenience method that is equivalent to <c cref="Console.ReadKey"/>, so that all of your code can use the <c cref="RichConsole"/> class instead
    /// of having some lines use <c cref="Console"/> while others use <c cref="RichConsole"/>. See the documentation for <c cref="Console.ReadKey"/> for details
    /// about the returned object.
    /// </summary>
    /// <param name="intercept">Indicates whether the letter should be intercepted and not displayed on screen (true) or printed as normal when typed (false, and
    /// the default).</param>
    public static ConsoleKeyInfo ReadKey(bool intercept = false) => Console.ReadKey(intercept);

    /// <summary>
    /// Writes a line of styled text, such as "This text has [red]red[/], [green underline]green[/], and [italic blue]blue[/] text in it.
    /// </summary>
    public static void WriteLineStyled(string? styledText) => WriteStyled(styledText + Environment.NewLine);

    /// <summary>
    /// Writes styled text (without moving to the next line), such as "This text has [red]red[/], [green underline]green[/], and [italic blue]blue[/] text in it.
    /// </summary>
    public static void WriteStyled(string? styledText) => Styling.WriteStyled(styledText);

    // String lengths are complicated. We can't just trust text.Length, because that counts UTF-16 characters, and some UTF-16
    // characters don't form a full character. We sometimes need two or more. This gives us a smarter idea of how long the rendered
    // text is, but note that the console window doesn't necessarily render every "full" letter as a single character. Emojis, for
    // example, usually use more than one, but, in my experience, they aren't even exactly two or exactly three, but somewhere in
    // between, which makes it hard to account for them in alignment.
    private static int DetermineDisplayCharacterLength(string text)
    {
        text = Regex.Replace(text, @"\e\[.*m", m => ""); // ANSI escape codes are not rendered, so we want to strip them.
        int count = 0;
        text ??= "";
        TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(text, 0);
        while (enumerator.MoveNext())
            count++;
        return count;
    }
}
