namespace CSharpPlayersGuide.RichConsole;

/// <summary>
/// An enumeration of the possible text effects the <c>ColoredConsole</c> can render.
/// </summary>
/// <remarks>
/// You can use multiple effects by combining them with the bitwise operator <c>|</c>:
/// <code>
/// TextEffects effects = TextEffects.Italic | Underline;
/// </code>
/// Each of these effects requires the underlying window to support the effect. If it isn't supported, the intended effect will be ignored.
/// </remarks>
[Flags]
public enum TextEffects
{
    None = 0,
    Italics = 1 << 0,
    Underline = 1 << 1,
    DoubleUnderline = 1 << 2,
    Strikethrough = 1 << 3,
    Overline = 1 << 4,
    AllCaps = 1 << 5,
    Left = 1 << 6,
    Center = 1 << 7,
    Right = 1 << 8,
    Blink = 1 << 9
}
