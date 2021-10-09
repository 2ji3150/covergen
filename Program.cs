using covergen;

try
{
    DirectoryInfo music_di = ConsoleUI.GetProcessingDir();
    int coverWidth = ConsoleUI.GetOutputCoverWidth();

    ConsoleUI.ConfirmForProcessing();

    // Get all dirs which contain WavePack Files .
    var wvDirs = music_di
        .EnumerateDirectories("*", SearchOption.AllDirectories)
        .Where(dir => dir.EnumerateFiles("*.wv").Any())
        .ToArray();

    ImageProcessor imgProcessor = new(coverWidth);
    imgProcessor.ProcessAll(wvDirs);

}
catch (Exception ex)
{
    ConsoleUI.ShowException(ex);
}
finally
{
    ConsoleUI.ConfirmForTerminate();
}