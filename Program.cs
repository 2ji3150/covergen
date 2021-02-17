using ImageMagick;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;

namespace covergen {
    class Program {
        static void Main() {
            var tmp_di = Directory.CreateDirectory("tmp");
            MagickGeometry square = new MagickGeometry("1:1");
            Console.Write("Enter the music root path:");
            string path = Console.ReadLine();
            if (!Directory.Exists(path)) return;
            Console.WriteLine("Enter the width of cover:");
            if (!int.TryParse(Console.ReadLine(), out int cover_width)) return;
            Console.WriteLine("Press enter to start processing");
            Console.ReadLine();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try {
                var dirs = new DirectoryInfo(path).EnumerateDirectories("*", SearchOption.AllDirectories);
                var wvdirs = dirs.Where(dir => dir.EnumerateFiles("*.wv").Any()).ToArray();

                static void ExcuteBatch(string batName, string input) {
                    var fileName = Path.Combine("batch", $"{batName}.bat");
                    var arguments = $@"""{input}""";
                    ProcessStartInfo psi = new ProcessStartInfo(fileName, arguments) {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };

                    using Process p = Process.Start(psi);
                    p.WaitForExit();
                    if (p.ExitCode != 0) throw new Exception($"ExitCode NEQ 0");
                }


                for (int i = 0; i < wvdirs.Length; i++) {
                    var wvdir = wvdirs[i];
                    string cover = Path.Combine(wvdir.FullName, "cover.webp");
                    var zip = wvdir.EnumerateFiles("base.zip").FirstOrDefault() ?? wvdir.EnumerateFiles("orig.zip").FirstOrDefault();
                    if (zip == null) continue;

                    Console.WriteLine($"Processing {wvdir.Name} ... {i + 1} of {wvdirs.Length}");

                    //iterate through the collection of entries
                    using var archive = ZipFile.OpenRead(zip.FullName);
                    var imgEntry = archive.Entries.First(entry => entry.FullName.EndsWith(".webp") || entry.FullName.EndsWith(".jpg"));
                    var configJsonEntry = archive.GetEntry("config.json");

                    using var stream = imgEntry.Open();
                    using var image = new MagickImage(stream);
                    string noiseSwitch = string.Empty;
                    string scaleSwitch = string.Empty;
                    int scale = 2;

                    Console.WriteLine($"original: {image.Height}x{image.Width}");

                    if (configJsonEntry != null) {
                        using var stream_config = configJsonEntry.Open();
                        using var sr = new StreamReader(stream_config);
                        var jsonString = sr.ReadToEnd();
                        var config = JsonSerializer.Deserialize<Config>(jsonString);
                        if (config.Crop != null) {
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
                    var breadth = Math.Min(image.Width, image.Height);
                    while (scale * breadth < cover_width) scale *= 2;
                    if (scale != 2) scaleSwitch = $"-s {scale}";
                    Console.WriteLine($"scale:{scale}");
                    image.Write(@"tmp\cover.bmp");
                    //waifu2x
                    ExcuteBatch("waifu2x", string.Join(" ", noiseSwitch, scaleSwitch));
                    //center crop
                    using MagickImage image_cover = new MagickImage(@"tmp\cover.png");
                    Console.WriteLine($"upscaled: {image_cover.Width}x{image_cover.Height}");
                    if (image_cover.Width != image_cover.Height) {
                        image_cover.Crop(square, Gravity.Center);
                        image_cover.RePage();
                    }
                    Console.WriteLine($"center_cropped: {image_cover.Width}x{image_cover.Height}");

                    //resize
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
            catch (Exception ex) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.ToString());
                Console.ResetColor();
            }
            finally {
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadLine();
            }
        }
    }
}