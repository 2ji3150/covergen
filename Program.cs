using ImageMagick;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace covergen {
    class Program {
        static void Main() {

            MagickGeometry square = new MagickGeometry("1:1");
            Console.Write("Enter the music root path:");
            string path = Console.ReadLine();
            if (!Directory.Exists(path)) return;
            Console.WriteLine("Enter the width of cover:");
            int.TryParse(Console.ReadLine(), out int cover_width);
            Console.WriteLine("Press enter to start processing");
            Console.ReadLine();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try {
                var wvs = new DirectoryInfo(path).EnumerateFiles("*.wv", SearchOption.AllDirectories);
                var wvdirs = wvs.GroupBy(wv => wv.Directory.FullName).Select(wvGrop => wvGrop.First().Directory).ToArray();

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

                string configPath = @"tmp\config.json";
                for (int i = 0; i < wvdirs.Length; i++) {
                    var wvdir = wvdirs[i];
                    string cover = Path.Combine(wvdir.FullName, "cover.webp");
                    var rar = wvdir.EnumerateFiles("base.rar").FirstOrDefault() ?? wvdir.EnumerateFiles("orig.rar").FirstOrDefault();
                    if (rar == null) continue;

                    Console.WriteLine($"Processing {wvdir.Name} ... {i + 1} of {wvdirs.Length}");

                    //extract to tmp dir
                    ExcuteBatch("unpack", rar.FullName);

                    var img_input = Directory.EnumerateFiles("tmp", "*.*")
                        .Where(f => {
                            var ext = Path.GetExtension(f);
                            return ext == ".webp" || ext == ".jpg";
                        }).First();

                    using MagickImage image = new MagickImage(img_input);
                    string noiseSwitch = string.Empty;
                    string scaleSwitch = string.Empty;
                    int scale = 2;

                    Console.WriteLine($"original: {image.Height}x{image.Width}");

                    //config.json
                    if (File.Exists(configPath)) {
                        var jsonString = File.ReadAllText(configPath);
                        var config = JsonSerializer.Deserialize<Config>(jsonString);
                        if (config.Crop != null) {
                            int right = config.Crop.Right;
                            int left = config.Crop.Left;
                            int top = config.Crop.Top;
                            int bottom = config.Crop.Bottom;
                            image.Crop(new MagickGeometry(left, top, image.Width - left - right, image.Height - top - bottom));
                            image.RePage();
                            Console.WriteLine($"cropped: {image.Height}x{image.Width}");
                        }

                        if (config.Noise != null) noiseSwitch = $"-n {config.Noise}";
                    }

                    while (scale * image.Width < cover_width) scale *= 2;
                    if (scale != 2) scaleSwitch = $"-s {scale}";
                    Console.WriteLine($"scale:{scale}");
                    image.Write(@"tmp\cover.bmp");

                    //waifu2x
                    ExcuteBatch("waifu2x", string.Join(" ", noiseSwitch, scaleSwitch));

                    //center crop
                    using MagickImage image_cover = new MagickImage(@"tmp\cover.png");
                    Console.WriteLine($"upscaled: {image_cover.Height}x{image_cover.Width}");
                    if (image_cover.Width != image_cover.Height) {
                        image_cover.Crop(square, Gravity.Center);
                        image_cover.RePage();
                    }
                    Console.WriteLine($"center_cropped: {image_cover.Height}x{image_cover.Width}");

                    //resize
                    if (image_cover.Width > cover_width) image_cover.Resize(cover_width, cover_width);
                    Console.WriteLine($"final: {image_cover.Height}x{image_cover.Width}");

                    //convert to cover.webp
                    image_cover.Write(cover);

                    Directory.Delete("tmp", true);
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