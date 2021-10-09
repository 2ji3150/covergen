namespace covergen;

/// <summary>
/// Console UI Interactionss
/// </summary>
internal class ConsoleUI
{
    /// <summary>
    /// Get the processing directory.
    /// </summary>
    /// <returns>the DirectoryInfo of processing dir</returns>
    internal static DirectoryInfo GetProcessingDir()
    {
        Console.Write("Enter the music root path:");

        // Read Input
        string? path = Console.ReadLine();

        if (path == null)
        {
            throw new ArgumentException();
        }

        DirectoryInfo di = new(path);

        // If the input dir is not exsist, throw exception.
        if (!di.Exists)
        {
            throw new DirectoryNotFoundException();
        }

        return di;
    }

    /// <summary>
    /// Get the desired output width of covers.
    /// </summary>
    /// <returns>width of covers</returns>
    internal static int GetOutputCoverWidth()
    {
        Console.WriteLine("Enter the desired output width of covers.");

        string? input = Console.ReadLine();
        // Parse the UserInput into Int
        if (int.TryParse(input, out int cover_width))
        {
            return cover_width;
        }

        return 0;
    }

    /// <summary>
    ///  Confirm for user is ready for processing
    /// </summary>
    internal static void ConfirmForProcessing()
    {
        Console.WriteLine("Press enter to start processing");
        Console.ReadLine();
    }

    /// <summary>
    /// Display Exception Error Stack on Console output.
    /// </summary>
    /// <param name="ex"></param>
    internal static void ShowException(Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(ex.ToString());
        Console.ResetColor();
    }

    /// <summary>
    ///  Confirm for user is ready for exit the application.
    /// </summary>
    internal static void ConfirmForTerminate()
    {
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadLine();
    }
}