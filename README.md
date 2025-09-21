# Rich Console

This is a small library, mostly meant as a sample NuGet package for my book The C# Player's Guide. However, it is a nice library that provides a clean, simple way to write text with colors (all colors, not just the 16 pre-defined colors in the ConsoleColor enumeration), as well as text effects like underline, italics, and all caps.

The best way to see this in action is to look at the code in the Samples project, especially the one in Program.cs, which is a bit of a tutorial/walkthrough of the major features.

I put this project together very quickly. It should generally work as designed when you do things correctly, but doesn't fail as gracefully as I'd like if you do something stupid with it. If you're interested in making this better--more samples, adding unit tests (especially around the styling parts), making it more robust--I'm interested in the help. To maximize the chances of your PR getting approved, make an issue to discuss what you're thinking about doing so the community can see it and react to it, before you spend a lot of time on it.
