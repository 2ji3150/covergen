using System.Diagnostics;

namespace covergen;

static class Waifu2xUpScaler
{
    const string fileName = @"D:\FIONE\bin\waifu2x-caffe\waifu2x-caffe-cui.exe";

    internal static void UpscaleAndDenoise(int noiseLevel = 0, int ratio = 2)
    {
        string arguments = $@"-i cover.png -o cover_upscaled.png -s {ratio} -n {noiseLevel}";

        ProcessStartInfo psi = new(fileName, arguments)
        {
            WorkingDirectory = "tmp",
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using Process? p = Process.Start(psi);
        if (p == null) throw new NullReferenceException();

        p.WaitForExit();
        if (p.ExitCode != 0) throw new Exception($"ExitCode NEQ 0");
    }
}