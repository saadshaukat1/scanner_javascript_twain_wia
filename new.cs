using System;
using System.IO;
using System.Diagnostics;

namespace ScannerApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Running Scanner - {DateTime.Now.ToShortTimeString()} : {DateTime.Now.ToShortDateString()}");

            string outputDir = "MyOutput";
            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, true);
            }
            Directory.CreateDirectory(outputDir);

            try
            {
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        // FileName = "cmd.exe",
                        // Arguments = $"/c NAPS2.Console.exe -o /{outputDir}/output.pdf --progress",
                         FileName = "cmd.exe",
                         Arguments = "/c cd commands && start.bat",
                         WindowStyle = ProcessWindowStyle.Minimized,

                        RedirectStandardOutput = true,
                        RedirectStandardError = true,   
                        UseShellExecute = false,
//                        CreateNoWindow = false,
                    }
                };

                process.Start();

                // Wait for the process to exit
                process.WaitForExit();

                // Read the error message
                string errorMessage = process.StandardError.ReadToEnd();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Console.WriteLine($"Error: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}