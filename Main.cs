using System;
using System.Net;
using System.Text.Json;
using System.Text;
using System.IO;

namespace asdf
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<VersionType, int> latestVersion = new Dictionary<VersionType, int>();
            Dictionary<VersionType, int> localVersion = new Dictionary<VersionType, int>();

            //testing field
            DownloadLatest();

            void DownloadLatest()
            {
                using (var client = new HttpClient())
                {
                    //set endpoint and payload
                    var endpoint = new Uri("http://xerias.pw:879");
                    var payload = new Dictionary<string, string>
                    {
                        {"query", "latest_build"}
                    };

                    //construct request and send
                    var content = new FormUrlEncodedContent(payload);
                    var response = client.PostAsync(endpoint, content).Result;
                    var rawString = response.Content.ReadAsStringAsync().Result;

                    File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "text.zip"), rawString);
                    Console.WriteLine(rawString);
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

            void GetLocalVersion()
            {
                string versionFileLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "version.json") ;
                if (File.Exists(versionFileLocation))
                {
                    string rawJson = File.ReadAllText(versionFileLocation);
                    localVersion = stringVersionParse(rawJson);
                }
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
                    var response = client.PostAsync(endpoint, content).Result;

                    //parse raw string to proper dict
                    var rawString = response.Content.ReadAsStringAsync().Result;
                    latestVersion = stringVersionParse(rawString);
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
