using ImageMagick;
using System.Text.Json.Serialization;

namespace covergen;

public class Config
{
    public int? Noise { get; init; }
    public Crop? Crop { get; init; }
}

public class Crop
{
    public int Top { get; init; }
    public int Bottom { get; init; }
    public int Left { get; init; }
    public int Right { get; init; }

    public MagickGeometry GetMagickGeometry(int width, int height) => new(Left, Top, width - Left - Right, height - Top - Bottom);

}