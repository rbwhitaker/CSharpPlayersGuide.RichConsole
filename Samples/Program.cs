using CSharpPlayersGuide.RichConsole;

// Basic text with color.
RichConsole.WriteLine("Hello, World!", Colors.IndianRed);

// You can also create a color with specific values.
RichConsole.WriteLine("Hello, World!", new Color(255, 127, 0));

// You can set a background color as well.
RichConsole.WriteLine("Hello, World!", Colors.IndianRed, Colors.BlanchedAlmond);

// The rich console also supports a number of text effects like italics, underline, and all caps.
RichConsole.WriteLine("Hello, World!", Colors.IndianRed, Colors.BlanchedAlmond, TextEffects.Italics);
RichConsole.WriteLine("Hello, World!", Colors.IndianRed, Colors.BlanchedAlmond, TextEffects.Underline);
RichConsole.WriteLine("Hello, World!", Colors.IndianRed, Colors.BlanchedAlmond, TextEffects.AllCaps);

// TextEffects is a "Flags" enumeration, so you can use "bitwise or" to combine several:
RichConsole.WriteLine("Hello, World!", Colors.IndianRed, Colors.BlanchedAlmond, TextEffects.Italics | TextEffects.Underline | TextEffects.AllCaps);

// You can leave things as the default like this...
RichConsole.WriteLine("Hello, World!", null, null, TextEffects.None);

// ... but there are a lot of overloads and optional parameters that allow you to generally specify only the parts you care about:
RichConsole.WriteLine("Hello, World!", Colors.IndianRed, TextEffects.Blink);
RichConsole.WriteLine("Hello, World!", TextEffects.Strikethrough);

// You can write text fragments without wrapping to the next line with Write:
RichConsole.Write("Hello, ", Colors.IndianRed);
RichConsole.Write("World!", Colors.BlanchedAlmond);
RichConsole.WriteLine();

// Emojis work automatically.
RichConsole.WriteLine("👿 The Uncoded One", Colors.White, Colors.Gray, TextEffects.AllCaps);

// You can also multiply colors with double values to get variations:
RichConsole.WriteLine("👿 The Uncoded One", Colors.White, Colors.White * 0.25, TextEffects.AllCaps);

// For more advanced situations, you can use styling, which gives you a different syntax for embedding colors and text effects directly in the string:
RichConsole.WriteLineStyled("This is [red]colored[/] text, and this is [italics palegreen]italic[/].");

// It starts with a block containing modifiers in square brackets, which ends when a [/] is encountered. This is reminiscent of HTML tags.

// As long as a word is either one of the TextEffects values or one of the named colors in the Colors class, it will work.
// Separate multiple with spaces.

// You can specify the background color by prefixing a color with "b:".
RichConsole.WriteLineStyled("[white b:gray allcaps]👿 The Uncoded One[/] has arrived.");

// You can also specify the foreground (text) color with an "f:" prefix, though this is the default and isn't ever strictly required.
RichConsole.WriteLineStyled("[f:white b:gray allcaps]👿 The Uncoded One[/] has arrived.");

// You can also use RGB colors in the format (r,g,b) (with no spaces):
RichConsole.WriteLineStyled("[(255,255,255) b:(127,127,127) allcaps]👿 The Uncoded One[/] has arrived.");

// The Color class has overridden its ToString to use this (r,g,b) format, which makes this possible:
Color color = Colors.IndianRed;
RichConsole.WriteLineStyled($"[{color} b:{color * 0.25} allcaps]👿 The Uncoded One[/] has arrived.");

// You can nest tags inside of others, and their styles "stack":
RichConsole.WriteLineStyled($"[indianred]Hello, [italics allcaps]world![/] More text[/] over here.");

// Here's a longer example, showing what this system is capable of:
RichConsole.WriteLineStyled($"[white b:(25,25,25) allcaps]👿 The Uncoded One[/] used [{Colors.Magenta} b:{Colors.Magenta * 0.25} allcaps]unraveling[/] on [{Colors.Cerulean} b:{Colors.Cerulean * 0.25} allcaps]🗡️ Tog[/] and dealt [red b:darkred]❤️×5[/] damage!");

// To write a literal "[", escape it with \:
RichConsole.WriteLineStyled(@$"HP: \[22/25]");
// To write a literal slash, escape it with \:
RichConsole.WriteLineStyled($@"HP: 22\\25");
// Note that the @ symbol means this is a verbatim literal, so _nothing_ is escaped. You can alternatively do it like this:
RichConsole.WriteLineStyled($"HP: \\[22/25]\t\t\n\n\tMore Text.");
// Which gets a little weird, because it is encodings upon encodings...
RichConsole.WriteLineStyled("HP: 22\\\\25");
// Mostly, I try to avoid places that need backslashes to keep it cleaner.

// Try it out live, by running this code. (You won't be able to do string interpolation, but you can experiment with other effects.)
while (true)
{
    RichConsole.Write("Enter styled text and I'll render it for you. ");
    string text = RichConsole.ReadLine();
    RichConsole.WriteLineStyled(text);
}

// Ultimately, not every console window will support every option, and combining features with other features may not work.
// For example, emojis don't blink, and emojis tend to mess up the center and right justification because they aren't one
// character wide. This library was initially made over about two or three hours, and may still have some rough edges.
// If there's something that you think should "obviously" work that doesn't seem to be, you can add an issue in GitHub, or
// you can put up a PR.