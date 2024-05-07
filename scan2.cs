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
            int PORT = 3031;
        
            using (HttpListener listener = new HttpListener())
            {
                listener.Prefixes.Add($"http://localhost:{PORT}/");
                listener.Start();
                Console.WriteLine($"Running on Port: {PORT}");
        
                while (true)
                {
                    HttpListenerContext context = listener.GetContext();
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;
        
                    SetResponseHeaders(response);
        
                    string ui = request.QueryString["ui"];
                    string document = request.QueryString["document"];
                    string image = request.QueryString["image"];
                    string scan = request.QueryString["scan"];
        
                    if (ui == "false" && scan == "true" && document == "true" && !isProcessRunning)
                    {
                        HandleScanRequest(response);
                    }
                    else if (ui == "true" && !isSettingsUIOpen)
                    {
                        HandleUIRequest(response);
                    }
                    else
                    {
                        HandleInvalidRequest(response);
                    }
        
                    CloseOutputStream(response);
                }
            }
        }
        private static void SetResponseHeaders(HttpListenerResponse response)
        {
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS, PUT, PATCH, DELETE");
            response.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept, Authorization, x-access-token");
            response.Headers.Add("Access-Control-Allow-Credentials", "true");
        }

        private static void CloseOutputStream(HttpListenerResponse response)
        {
            response.OutputStream.Close();
        }

        private static void HandleScanRequest(HttpListenerResponse response)
        {
            Console.WriteLine($"Running Scanner - {DateTime.Now.ToShortTimeString()} : {DateTime.Now.ToShortDateString()}");

            if (isProcessRunning)
            {
                Console.WriteLine("A process is already running.");
                return;
            }

            isProcessRunning = true;
            PrepareOutputDirectory();

            try
            {
                bool isProcessSuccessful = RunProcess();
            
                if (isProcessSuccessful)
                {
                    Console.WriteLine("Command ran successfully.");
                    Task.Delay(TimeSpan.FromSeconds(10)).Wait();
                     string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Output", "output.pdf");
                    if (File.Exists(filePath))
                    {
                        Console.WriteLine($"Reading PDF- {DateTime.Now.ToShortTimeString()} : {DateTime.Now.ToShortDateString()}");
                        SendPdf(response);
                    }
                    else
                    {
                        Console.WriteLine("File not found.");
                    }

                
                    //SendPdf(response);
                }
                else
                {
                    Console.WriteLine("Command failed. Not sending PDF.");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                response.StatusCode = 202;
                response.StatusDescription = "Process was interrupted, please try again";
                response.OutputStream.Close();
            }
            finally
            {
                isProcessRunning = false;
            }
        }

        private static void PrepareOutputDirectory()
        {
            string outputDir = "Output";
            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, true);
            }
            Directory.CreateDirectory(outputDir);
        }

        private static bool RunProcess()
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c cd commands && start.bat",
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                }
            };
        
            process.Start();
            process.WaitForExit();
        
            string errorMessage = process.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(errorMessage))
            {
                Console.WriteLine($"Error: {errorMessage}");
                return false;
            }
        
            if (process.ExitCode != 0)
            {
                Console.WriteLine("Process exited with non-zero exit code.");
                return false;
            }
        
            return true;
        }
        private static void SendPdf(HttpListenerResponse response)
        {
            Console.WriteLine($"Sending PDF- {DateTime.Now.ToShortTimeString()} : {DateTime.Now.ToShortDateString()}");

             string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Output", "output.pdf");
            // if (!File.Exists(filePath))
            // {
            //     Console.WriteLine($"File not found: {filePath}");
            //     return ;
            // }

            using (MemoryStream memoryStream = new MemoryStream(File.ReadAllBytes(filePath)))
            {
                response.ContentType = "application/pdf";
                response.StatusCode = 200;
                memoryStream.WriteTo(response.OutputStream);
                response.OutputStream.Close();

                Console.WriteLine($"PDF sent successfully- {DateTime.Now.ToShortTimeString()} : {DateTime.Now.ToShortDateString()}");
            }
        }
        private static void HandleUIRequest(HttpListenerResponse response)
        {
            if (!isSettingsUIOpen)
            {
                isSettingsUIOpen = true;

                Console.WriteLine($"Scanner UI (Options)- {DateTime.Now.ToShortTimeString()} : {DateTime.Now.ToShortDateString()}");
                // Start NAPS2.exe directly
                Process.Start(new ProcessStartInfo
                {
                    FileName = "NAPS2.exe",
                    WindowStyle = ProcessWindowStyle.Maximized
                });
                response.StatusCode = 204;
                isSettingsUIOpen = false;
            }
            else
            {
                Console.WriteLine("Settings UI is already open.");
                response.StatusCode = 409; // Conflict
            }
            response.OutputStream.Close();
        }

        private static void HandleInvalidRequest(HttpListenerResponse response)
        {
            response.StatusCode = 404;
            Console.WriteLine($"Invalid Request -  {DateTime.Now.ToShortTimeString()} : {DateTime.Now.ToShortDateString()}");
        }
    }
}