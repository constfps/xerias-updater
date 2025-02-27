using System;
using System.Net;
using System.Text.Json;
using System.Text;

namespace asdf
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<VersionType, int> latestVersion = new Dictionary<VersionType, int>();

            GetLatestVersion();
            Console.WriteLine(latestVersion[VersionType.Micro]);

            void GetLatestVersion()
            {
                using (var client = new HttpClient())
                {
                    //set endpoint and payload
                    var endpoint = new Uri("http://xerias.pw:879");
                    var payload = new Dictionary<string, string>
                    {
                        {"query", "version"}
                    };

                    //construct request and send that bih
                    var content = new FormUrlEncodedContent(payload);
                    var response = client.PostAsync(endpoint, content).Result;

                    //store response as key value pair
                    var rawString = response.Content.ReadAsStringAsync().Result;
                    var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(rawString);

                    //convert and set latestVersion
                    latestVersion.Add(VersionType.Main, Int32.Parse(jsonResponse["main"]));
                    latestVersion.Add(VersionType.Sub, Int32.Parse(jsonResponse["sub"]));
                    latestVersion.Add(VersionType.Micro, Int32.Parse(jsonResponse["micro"]));
                }
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
