using System;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ScannerApp
{
    class Program
    {
        private static bool isProcessRunning = false;
        private static bool isSettingsUIOpen = false;
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = Process.GetCurrentProcess().MainModule.FileName,
                    Arguments = "minimized",
                    WindowStyle = ProcessWindowStyle.Minimized

                };
                Process.Start(psi);
            }
            else if(args[0] == "minimized")
            {
                int PORT = 3031;
                HttpListener listener = new HttpListener();
                listener.Prefixes.Add($"http://localhost:{PORT}/");
                listener.Start();
                Console.WriteLine($"Running on Port: {PORT}");

                while (true)
                {
                    HttpListenerContext context = listener.GetContext();
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;

                    // Permission to access and fetch files from local machine
                    response.Headers.Add("Access-Control-Allow-Origin", "*");
                    response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS, PUT, PATCH, DELETE");
                    response.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept, Authorization, x-access-token");
                    response.Headers.Add("Access-Control-Allow-Credentials", "true");

                    // HTTP Queries ex. "localhost:3031/?scan=true"
                    string ui = request.QueryString["ui"];
                    string document = request.QueryString["document"];
                    string image = request.QueryString["image"];
                    string scan = request.QueryString["scan"];

                    if (ui == "false" && scan == "true" && document == "true")
                    {
                        Console.WriteLine($"Running Scanner - {DateTime.Now.ToShortTimeString()} : {DateTime.Now.ToShortDateString()}");

                        string outputDir = "../Output";
                        if (!Directory.Exists(outputDir))
                        {
                            Directory.Delete(outputDir, true);
                        }
                        Directory.CreateDirectory(outputDir);
                        // Run batch script to open it minimized
                        if (!isProcessRunning)
                        {
                            isProcessRunning = true;

                            Process process = new Process
                            {
                                StartInfo = new ProcessStartInfo
                                {
                                    FileName = "../NAPS2.Console.exe",
                                    Arguments = "-o ../Output/output.pdf --progress",
                                    RedirectStandardOutput = true,
                                    UseShellExecute = false,
                                    CreateNoWindow = true,
                                }
                            };

                            process.Start();

                            // Wait for the process to exit
                            process.WaitForExit();

                            // Check the exit code
                            if (process.ExitCode == 0)
                            {
                                Console.WriteLine("Command ran successfully.");
                                Task.Delay(TimeSpan.FromSeconds(10)).Wait();
                                try
                                {
                                    // Send the Output.pdf file from "/Output" folder
                                    byte[] fileBytes = File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "Output", "output.pdf"));
                                    response.ContentType = "application/pdf";
                                    response.StatusCode = 200;
                                    response.OutputStream.Write(fileBytes, 0, fileBytes.Length);
                                    response.OutputStream.Close();
                                    isProcessRunning = false;
                                }
                                catch (Exception ex)
                                {
                                    Console.Error.WriteLine(ex.Message);
                                    response.StatusCode = 202;
                                    response.StatusDescription = "Process was interrupted, please try again";
                                    response.OutputStream.Close();
                                    isProcessRunning = false;
                                }
                            }

                            else
                            {
                                Console.WriteLine("Batch file did not run successfully.");
                                // Handle the error
                            }

                        }


                        else
                        {
                            Console.WriteLine("A process is already running.");
                        }

                        // Wait for the scanner to finish before sending the PDF
                        // This is a simplification, in a real-world scenario you would need to implement a more robust mechanism


                        // HTTP request is asking to open the Scanner Settings UI
                    }
                    else if (ui == "true")
                    {
                        if (!isSettingsUIOpen)
                        {
                            isSettingsUIOpen = true;

                            Console.WriteLine($"Scanner UI (Options)- {DateTime.Now.ToShortTimeString()} : {DateTime.Now.ToShortDateString()}");
                            // Run batch script to open it minimized
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = "cmd.exe",
                                Arguments = "/c cd commands && settings.bat",
                                WindowStyle = ProcessWindowStyle.Minimized
                            });
                            response.StatusCode = 204;
                        }
                        else
                        {
                            Console.WriteLine("Settings UI is already open.");
                            response.StatusCode = 409; // Conflict
                        }
                        response.OutputStream.Close();
                    }

                    else
                    {
                        response.StatusCode = 404;
                        Console.WriteLine($"Invalid Request -  {DateTime.Now.ToShortTimeString()} : {DateTime.Now.ToShortDateString()}");
                        response.OutputStream.Close();
                    }
                
                }
            }
        }
    }
}