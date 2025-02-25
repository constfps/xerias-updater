using System;
using System.Net;

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
                    //fetch version as single string
                    var endpoint = new Uri("http://174.4.104.176:879/version.txt");
                    var rawVersionString = client.GetStringAsync(endpoint).Result;

                    //turn string into string array separated by ,
                    string[] rawVersionStringArray = rawVersionString.Split(".");

                    //create int array to store typecasted strings to int
                    int[] versionIntArray = new int[rawVersionStringArray.Length];
                    for (int i = 0; i < rawVersionStringArray.Length; i++)
                    {
                        versionIntArray[i] = int.Parse(rawVersionStringArray[i]);
                    }

                    //store everything into hashmap
                    for (int i = 0; i < versionIntArray.Length; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                latestVersion.Add(VersionType.Main, versionIntArray[i]);
                                break;
                            case 1:
                                latestVersion.Add(VersionType.Sub, versionIntArray[i]);
                                break;
                            case 2:
                                latestVersion.Add(VersionType.Micro, versionIntArray[i]);
                                break;
                        }
                    }
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
