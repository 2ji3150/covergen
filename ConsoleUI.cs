using System;
using System.IO;

namespace covergen;

/// <summary>
/// Console UI interaction
/// </summary>
internal class ConsoleUI
{

    /// <summary>
    /// Get the processing directory.
    /// </summary>
    /// <returns>the DirectoryInfo of processing dir</returns>
    public DirectoryInfo GetProcessingDir()
    {
        Console.Write("Enter the music root path:");

        // Read Input
        string path = Console.ReadLine();

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
    public int GetOutputCoverWidth()
    {
        Console.WriteLine("Enter the desired output width of covers.");

        // Parse the UserInput into Int
        int cover_width = int.Parse(Console.ReadLine());

        return cover_width;
    }

    /// <summary>
    ///  Confirm for user is ready for processing
    /// </summary>
    public void ConfirmForProcessing()
    {
        Console.WriteLine("Press enter to start processing");
        Console.ReadLine();
    }

    /// <summary>
    /// Display Exception Error Stack on Console output.
    /// </summary>
    /// <param name="ex"></param>
    public void ShowException(Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(ex.ToString());
        Console.ResetColor();
    }

    /// <summary>
    ///  Confirm for user is ready for exit the application.
    /// </summary>
    public void ConfirmForTerminate()
    {
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadLine();
    }
}

