using ImageMagick;
using System.IO.Compression;
using System.Text.Json;

namespace covergen;

internal class ImageProcessor
{
    private MagickGeometry SquareMagickGeometry { get; } = new("1:1");
    private MagickGeometry ResizeMagickGeometry { get; }

    private readonly int _coverWidth;

    internal ImageProcessor(int coverWidth)
    {
        _coverWidth = coverWidth;
        ResizeMagickGeometry = new MagickGeometry(coverWidth);
    }

    private bool Process(DirectoryInfo di)
    {
        #region Local Method
        string GetOutputCoverPath() => Path.Combine(di.
            FullName, "cover.webp");

        FileInfo GetImageSourceZipFileInfo()
        {
            var zipFileInfosInWvDir = di.GetFiles("*.zip");

            FileInfo? fileInfo = zipFileInfosInWvDir.FirstOrDefault(zfi => zfi.Name == "base.zip") ??
                             zipFileInfosInWvDir.FirstOrDefault(zfi => zfi.Name == "orig.zip");

            if (fileInfo == null) throw new FileNotFoundException($"Could not be found base.zip or orig.zip in {di.FullName}");
            return fileInfo;
        }

        ZipArchiveEntry GetImageEntry(ZipArchive archive)
        {
            ZipArchiveEntry? imageEntry = archive.Entries.FirstOrDefault(entry => entry.FullName.EndsWith(".webp") || entry.FullName.EndsWith(".jpg"));

            if (imageEntry == null) throw new FileNotFoundException($"Could not be found Image file in imageSourceZip");

            return imageEntry;
        }

        Config? GetConfig(ZipArchive archive)
        {
            ZipArchiveEntry? configEntry = archive.GetEntry("config.json");
            if (configEntry == null) return null;

            using Stream configStream = configEntry.Open();
            using StreamReader streamReader = new(configStream);
            string jsonString = streamReader.ReadToEnd();
            return JsonSerializer.Deserialize<Config>(jsonString);
        }

        void Crop(MagickImage image, Crop cropSetting)
        {
            MagickGeometry cropMagickGeometry = cropSetting.GetMagickGeometry(image.Width, image.Height);
            image.Crop(cropMagickGeometry);
            image.RePage();

            Console.WriteLine($"cropped: {image.Width}x{image.Height}");
        }

        void CenterSquareCropAndSaveToFile(MagickImage image)
        {
            image.Crop(SquareMagickGeometry, Gravity.Center);
            image.RePage();

            // Save Image.bmp to Tmp Dir
            image.Write(@"tmp\cover.png");

            Console.WriteLine($"center cropped: {image.Width}x{image.Height}");
        }

        void UpScale(int ratio, int noiseLevel)
        {
            Waifu2xUpScaler.UpscaleAndDenoise( noiseLevel, ratio);
        }

        void DownScaleAndSaveToFile()
        {
            using MagickImage image_cover = new(@"tmp\cover_upscaled.png");

            image_cover.Resize(ResizeMagickGeometry);

            //convert to webp and write to File
            image_cover.Write(GetOutputCoverPath());
        }


        #endregion

        // Get ImageSource zip file
        FileInfo imageSourceZipFileInfo = GetImageSourceZipFileInfo();

        // OpenRead Zip Archive
        using ZipArchive archive = ZipFile.OpenRead(imageSourceZipFileInfo.FullName);

        // Get ImageEntry
        ZipArchiveEntry imageEntry = GetImageEntry(archive);

        // Open Image Stream
        using Stream imageStream = imageEntry.Open();

        using MagickImage image = new(imageStream);

        Config? config = GetConfig(archive);
        Crop? cropSetting = config?.Crop;

        // Apply crop setting in config file
        if (cropSetting != null) Crop(image, cropSetting);

        // Center Crop into AspectRatio 1:1
        CenterSquareCropAndSaveToFile(image);

        // Caculate UpScale ratio
        int ratio = ScaleRatioCaculator.CalculateRatio(image.Width, _coverWidth);

        int noiseLevel = config?.Noise ?? 0;

        UpScale(ratio, noiseLevel);

        DownScaleAndSaveToFile();

        return true;
    }

    internal void ProcessAll(DirectoryInfo[] dis)
    {
        static void ShowProgress(ref int counter)
        {
            Console.WriteLine($"{++counter} covers generate");
        }

        int counter = 0;
        Directory.CreateDirectory("tmp");

        foreach (var di in dis)
        {
            if (Process(di)) ShowProgress(ref counter);
        }
    }
}