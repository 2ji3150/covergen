using ImageMagick;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace covergen;

class Program
{
    private readonly static MagickGeometry squareMagickGeometry = new("1:1");

    static async Task Main()
    {
        ConsoleUI cui = new();

        DirectoryInfo music_di = cui.GetProcessingDir();

        int cover_width = cui.GetOutputCoverWidth();

        cui.ConfirmForProcessing();

        Stopwatch sw = new();

        sw.Start();
        try
        {
            // Get all dirs.
            var dirs = music_di.EnumerateDirectories("*", SearchOption.AllDirectories);

            // Get all dirs which contain WavePack Files .
            var wvdirs = dirs.Where(dir => dir.EnumerateFiles("*.wv").Any()).ToArray();

            static async Task ExcuteBatchAsync(string batName, string input)
            {
                var fileName = Path.Combine("batch", $"{batName}.bat");
                var arguments = $@"""{input}""";
                ProcessStartInfo psi = new(fileName, arguments)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                using Process p = Process.Start(psi);
                await p.WaitForExitAsync();
                if (p.ExitCode != 0) throw new Exception($"ExitCode NEQ 0");
            }

            // Loop through all wv dirs.
            for (int i = 0; i < wvdirs.Length; i++)
            {
                var wvdir = wvdirs[i];

                // Get output cover fullPath.
                string cover = Path.Combine(wvdir.FullName, "cover.webp");

                // Get zip for cover Image source.
                var zip = wvdir.EnumerateFiles("base.zip").FirstOrDefault() ?? wvdir.EnumerateFiles("orig.zip").FirstOrDefault();
                if (zip == null) continue;

                Console.WriteLine($"Processing {wvdir.Name} ... {i + 1} of {wvdirs.Length}");

                // Iterate through the collection of entries
                using var archive = ZipFile.OpenRead(zip.FullName);
              
                // Get imageFile in Zip
                ZipArchiveEntry imgEntry = archive.Entries.First(entry => entry.FullName.EndsWith(".webp") || entry.FullName.EndsWith(".jpg"));
                
                // Get json config File in Zip
                ZipArchiveEntry configJsonEntry = archive.GetEntry("config.json");

                using var stream = imgEntry.Open();
                using var image = new MagickImage(stream);
                string noiseSwitch = string.Empty;
                string scaleSwitch = string.Empty;
                int scale = 2;

                Console.WriteLine($"original: {image.Height}x{image.Width}");

                // image crop process accroiding json config
                if (configJsonEntry != null)
                {
                    using var stream_config = configJsonEntry.Open();
                    using var sr = new StreamReader(stream_config);
                    var jsonString = sr.ReadToEnd();
                    var config = JsonSerializer.Deserialize<Config>(jsonString);
                    if (config.Crop != null)
                    {
                        int right = config.Crop.Right;
                        int left = config.Crop.Left;
                        int top = config.Crop.Top;
                        int bottom = config.Crop.Bottom;
                        image.Crop(new MagickGeometry(left, top, image.Width - left - right, image.Height - top - bottom));
                        image.RePage();
                        Console.WriteLine($"cropped: {image.Width}x{image.Height}");
                    }
                    if (config.Noise != null) noiseSwitch = $"-n {config.Noise}";
                }

                // caculate scale ratio
                var breadth = Math.Min(image.Width, image.Height);
                while (scale * breadth < cover_width) scale *= 2;
                if (scale != 2) scaleSwitch = $"-s {scale}";
                Console.WriteLine($"scale:{scale}");
                image.Write(@"tmp\cover.bmp");

                // waifu2x processing
                await ExcuteBatchAsync("waifu2x", string.Join(" ", noiseSwitch, scaleSwitch));

                // Get waifu2x processed image
                using MagickImage image_cover = new(@"tmp\cover.png");

                Console.WriteLine($"upscaled: {image_cover.Width}x{image_cover.Height}");

                // center crop in last progress
                if (image_cover.Width != image_cover.Height)
                {
                    image_cover.Crop(squareMagickGeometry, Gravity.Center);
                    image_cover.RePage();
                }

                Console.WriteLine($"center_cropped: {image_cover.Width}x{image_cover.Height}");

                // Scale down to the desired cover width
                if (image_cover.Width > cover_width) image_cover.Resize(cover_width, cover_width);
                
                Console.WriteLine($"final: {image_cover.Width}x{image_cover.Height}");

                //convert to cover.webp
                image_cover.Write(cover);

                Console.WriteLine("done");
            }

            Console.WriteLine("Complete");
            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            Console.WriteLine($"...done\n\nRuntime {ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:00}");
        }
        catch (Exception ex)
        {
            cui.ShowException(ex);
        }
        finally
        {
            cui.ConfirmForTerminate();
        }
    }
}
