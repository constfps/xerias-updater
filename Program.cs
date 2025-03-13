using System.Text.Json;
using System.IO.Compression;
using System.Diagnostics;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Serilog;

namespace xerias_updater
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Dictionary<VersionType, int> latestVersion = new Dictionary<VersionType, int>();
            Dictionary<VersionType, int> localVersion = new Dictionary<VersionType, int>();

            //initialize logger
            Serilog.ILogger serilogLogger = new LoggerConfiguration().WriteTo.Console(
                outputTemplate: "{Timestamp:HH:mm:ss} {Level:u4}: {Message:lj}{Exception}{NewLine}"    
            ).CreateLogger();
            Serilog.Log.Logger = serilogLogger;
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddSerilog(serilogLogger);
            });
            ILogger<Program> logger = loggerFactory.CreateLogger<Program>();

            /*
            if (pingServer("174.4.104.176", 879))
            {
                logger.LogInformation("Ping successful");

                if (GetLocalVersion())
                {
                     
                }
                else
                {
                    var zipPath = DownloadLatest();
                    var extractPath = UncompressFile(zipPath);
                    CleanUp(zipPath, extractPath);
                }
            }
            */

            void WriteVersionFile(Dictionary<VersionType, int> ver)
            {
                Dictionary<string, string> temp = new Dictionary<string, string>();
                temp.Add("main", ver[VersionType.Main].ToString());
                temp.Add("sub", ver[VersionType.Sub].ToString());
                temp.Add("micro", ver[VersionType.Micro].ToString());

                File.AppendAllText(@".\version.json", JsonSerializer.Serialize(temp));
            }

            bool pingServer(string uri, int portNum)
            {
                logger.LogInformation("Pinging Server");
                try
                {
                    using (var client = new TcpClient(uri, portNum)) return true;
                }
                catch
                {
                    logger.LogWarning("Ping failed");
                    return false;
                }
            }
            
            void CleanUp(string zipPath, string gamePath)
            {
                logger.LogInformation("Doing some clean up");

                logger.LogInformation($"Deleting {zipPath}");
                try
                {
                    File.Delete(zipPath);
                }
                catch
                {
                    logger.LogError("Something went wrong :(");
                    Thread.Sleep(5000);
                    Environment.Exit(0);
                }

                string gameExe = "";

                logger.LogInformation("Trying to find game executable");
                var exes = Directory.GetFiles(gamePath, "*.exe");
                foreach ( var exe in exes )
                {
                    if (!exe.Contains("Unity"))
                    {
                        logger.LogInformation($"Found {exe}");
                        gameExe = exe;
                    }
                }

                if (gameExe == "")
                {
                    logger.LogError("Couldn't find game executable. Exiting");
                    Thread.Sleep(5000);
                    Environment.Exit(0);
                }

                logger.LogInformation($"Starting {gameExe}");
                Process.Start(gameExe);
            }

            string UncompressFile(string zipPath)
            {
                string extractPath = @".\game";

                logger.LogInformation($" Extracting {zipPath} to {extractPath}");

                try
                {
                    ZipFile.ExtractToDirectory(zipPath, extractPath);
                }
                catch
                {
                    logger.LogError($"Something went wrong :(");
                    Thread.Sleep(5000);
                    Environment.Exit(0);
                }

                return extractPath;
            }

            string DownloadLatest()
            {
                using (var client = new HttpClient())
                {
                    //set endpoint and payload
                    var endpoint = new Uri("http://174.4.104.176:879");
                    var payload = new Dictionary<string, string>
                    {
                        {"query", "latest_build"}
                    };

                    //construct request and send
                    var content = new FormUrlEncodedContent(payload);
                    var response = client.PostAsync(endpoint, content).Result;
                    try
                    {
                        logger.LogInformation("Downloading files to temp.zip");
                        var rawBytes = response.Content.ReadAsByteArrayAsync().Result;
                        File.WriteAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp.zip"), rawBytes);
                    }
                    catch
                    {
                        logger.LogError("Something went wrong :(");
                        Thread.Sleep(5000);
                        Environment.Exit(0);
                    }

                    return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp.zip");
                }
            }

            bool CompareVersions(Dictionary<VersionType, int> version1, Dictionary<VersionType, int> version2)
            {
                if (version1[VersionType.Main] == version2[VersionType.Main] &&
                    version1[VersionType.Sub] == version2[VersionType.Sub] &&
                    version1[VersionType.Micro] == version2[VersionType.Micro])
                {
                    return true;
                }
                return false;
            }

            bool GetLocalVersion()
            {
                string versionFileLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "version.json") ;
                logger.LogInformation($"Trying to find {versionFileLocation}");
                if (File.Exists(versionFileLocation))
                {
                    logger.LogInformation($"Found {versionFileLocation}. Now reading it");
                    string rawJson = File.ReadAllText(versionFileLocation);
                    localVersion = stringVersionParse(rawJson);
                    return true;
                }

                logger.LogWarning($"Couldn't find {versionFileLocation}. Proceeding with first time install");
                return false;
            }

            void GetLatestVersion()
            {
                using (var client = new HttpClient())
                {
                    //set endpoint and payload
                    var endpoint = new Uri("http://174.4.104.176:879");
                    var payload = new Dictionary<string, string>
                    {
                        {"query", "version"}
                    };

                    //construct request and send
                    var content = new FormUrlEncodedContent(payload);

                    try
                    {
                        logger.LogInformation("Requesting latest version");
                        var response = client.PostAsync(endpoint, content).Result;
                        //parse raw string to proper dict
                        logger.LogInformation("Response receieved. Parsing");
                        var rawString = response.Content.ReadAsStringAsync().Result;
                        latestVersion = stringVersionParse(rawString);
                    }
                    catch
                    {
                        logger.LogError("Something went wrong with the request");
                        Thread.Sleep(5000);
                        Environment.Exit(0);
                    }

                }
            }

            Dictionary<VersionType, int> stringVersionParse(string json)
            {
                var jsonDict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                Dictionary<VersionType, int> temp = new Dictionary<VersionType, int>();

                temp.Add(VersionType.Main, Int32.Parse(jsonDict["main"]));
                temp.Add(VersionType.Sub, Int32.Parse(jsonDict["sub"]));
                temp.Add(VersionType.Micro, Int32.Parse(jsonDict["micro"]));

                return temp;
            }
        }
        private enum VersionType
        {
            Main,
            Sub,
            Micro
        }
    }
}
