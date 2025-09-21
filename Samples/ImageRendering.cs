using SixLabors.ImageSharp;
using CSharpPlayersGuide.RichConsole;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

// I'm not going to try to explain this, because it is very complicated, but I think people might find it interesting, if they want to unpack it themselves.
// This draws pictures using the rich console. In fact, it draws animated GIFs! I thought about making image drawing a first-class citizen in the rich console,
// but I wasn't finding a lot of images that were well-designed for such low-resolution displays. So I'll limit it to just a sample for now. But if somebody
// wants to create a large set of super-low-resolution images that make sense with the book, I'd be interested in either directly adding image rendering
// to the rich console or creating a second library dedicated to image rendering.
// Note that this sample makes heavy use of the SixLabors.ImageSharp library.

public class ImageRendering
{
    public static void Run()
    {
        // These two lines are the most interesting to tweak. The first decides the image and the second determines the size.
        Image<Rgba32> image = Image.Load<Rgba32>(@"C:\Users\RB\Downloads\demon-tiger-kpop-demon-hunters.gif");
        int size = 48;

        image.Mutate(x => x.Resize(0, size));
        int frame = 0;
        Console.CursorVisible = false;

        while (frame < image.Frames.Count)
        {
            Console.CursorLeft = 0;
            Console.CursorTop = 0;
            DateTime start = DateTime.Now;

            for (int row = 0; row < image.Height / 2; row++)
            {
                for (int column = 0; column < image.Width; column++)
                {

                    Rgba32 top = image.Frames[frame][column, row * 2 + 0];
                    Rgba32 bottom = image.Frames[frame][column, row * 2 + 1];
                    RichConsole.Write("▄", new CSharpPlayersGuide.RichConsole.Color(bottom.R, bottom.G, bottom.B) * (bottom.A / 255.0), new CSharpPlayersGuide.RichConsole.Color(top.R, top.G, top.B) * (bottom.A / 255.0));
                }
                RichConsole.WriteLine();
            }

            int renderTime = (int)(DateTime.Now - start).TotalMilliseconds;
            Console.WriteLine(renderTime);
            int delay = image.Frames[frame].Metadata.GetGifMetadata().FrameDelay * 10 - renderTime;
            if (delay > 0) Thread.Sleep(delay);
            frame++;
            if (frame == image.Frames.Count) frame = 0;
        }
    }
}