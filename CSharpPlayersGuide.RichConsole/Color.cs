namespace CSharpPlayersGuide.RichConsole;

/// <summary>
/// Represents a color used in the <c cref="RichConsole"/>.
/// </summary>
/// <param name="R">The value of the red channel.</param>
/// <param name="G">The value of the green channel.</param>
/// <param name="B">The value of the blue channel.</param>
public record Color(byte R, byte G, byte B)
{
    internal string AnsiForegroundCode => $"\e[38;2;{R};{G};{B}m";
    internal string AnsiBackgroundCode => $"\e[48;2;{R};{G};{B}m";
    
    /// <summary>
    /// Returns a string in the format "(R,G,B)".
    /// </summary>
    public override string ToString() => $"({R},{G},{B})";

    /// <summary>
    /// Produces a new color with the color channels each multiplied by the given multiplier. This can be used to create brighter or darker color variations on the original color.
    /// </summary>
    /// <param name="color">The original color</param>
    /// <param name="multiplier">The amount to multiply each channel by.</param>
    /// <returns>A new color whose channel values are computed from the original color. The original color is unchanged.</returns>
    public static Color operator *(Color color, double multiplier) => new Color((byte)(color.R * multiplier), (byte)(color.G * multiplier), (byte)(color.B * multiplier));

    /// <summary>
    /// Produces a new color with the color channels each divided by the given dividend. This can be used to create brighter or darker color variations on the original color.
    /// </summary>
    /// <param name="color">The original color</param>
    /// <param name="multiplier">The amount to divide each channel by.</param>
    /// <returns>A new color whose channel values are computed from the original color. The original color is unchanged.</returns>
    public static Color operator /(Color color, double dividend) => new Color((byte)(color.R / dividend), (byte)(color.G / dividend), (byte)(color.B / dividend));
}
