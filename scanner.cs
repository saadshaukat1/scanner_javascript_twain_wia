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
        static void Main(string[] args)
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

                // HTTP request is asking for a image
                if (request.ContentType == "image/jpeg")
                {
                    // Get a list of the file names in "/Output" folder
                    string[] fileNames = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Output"), "*.jpg");
                    if (fileNames.Length > 0)
                    {
                        try
                        {
                            // Send the first jpg file in "/Output" folder
                            byte[] fileBytes = File.ReadAllBytes(fileNames[0]);
                            response.ContentType = "image/jpeg";
                            response.StatusCode = 200;
                            response.OutputStream.Write(fileBytes, 0, fileBytes.Length);
                            response.OutputStream.Close();
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine(ex.Message);
                            response.StatusCode = 202;
                            response.StatusDescription = "Process was interrupted, please try again";
                            response.OutputStream.Close();
                        }
                    }
                    else
                    {
                        response.StatusCode = 404;
                        response.StatusDescription = "No image files found";
                        response.OutputStream.Close();
                    }
                }
                // HTTP request is asking for a pdf file
                else if (request.ContentType == "application/pdf" || document == "true")
                {
                    try
                    {
                        // Send the Output.pdf file from "/Output" folder
                        byte[] fileBytes = File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "Output", "output.pdf"));
                        response.ContentType = "application/pdf";
                        response.StatusCode = 200;
                        response.OutputStream.Write(fileBytes, 0, fileBytes.Length);
                        response.OutputStream.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex.Message);
                        response.StatusCode = 202;
                        response.StatusDescription = "Process was interrupted, please try again";
                        response.OutputStream.Close();
                    }
                }
                // HTTP request is asking to open the Scanner Settings UI
                else if (ui == "true")
                {
                    Console.WriteLine($"Scanner UI (Options)- {DateTime.Now.ToShortTimeString()} : {DateTime.Now.ToShortDateString()}");
                    // Run batch script to open it minimized
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c cd commands && settings.bat",
                        WindowStyle = ProcessWindowStyle.Minimized
                    });
                    response.StatusCode = 204;
                    response.OutputStream.Close();
                }
                // HTTP request is asking to run scanner
                else if (ui == "false" && scan == "true")
                {
                    Console.WriteLine($"Running Scanner - {DateTime.Now.ToShortTimeString()} : {DateTime.Now.ToShortDateString()}");
                    // Run batch script to open it minimized
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c cd commands && start.bat",
                        WindowStyle = ProcessWindowStyle.Minimized
                    });
                    response.StatusCode = 200;
                    response.OutputStream.Close();
                }
            }
        }
    }
}