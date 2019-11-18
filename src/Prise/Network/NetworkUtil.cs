using System.IO;
using System.Net.Http;

namespace Prise
{
    public static class NetworkUtil
    {
        public static byte[] Download(HttpClient httpClient, string pathToFile)
        {
            var response = httpClient.GetAsync(pathToFile).Result;
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            return response.Content.ReadAsByteArrayAsync().Result;
        }

        public static Stream DownloadAsStream(HttpClient httpClient, string pathToFile)
        {
            var response = httpClient.GetAsync(pathToFile).Result;
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            return response.Content.ReadAsStreamAsync().Result;
        }

        public static string SaveToTempFolder(string tempFileLocation, byte[] data)
        {
            if (!File.Exists(tempFileLocation))
                System.IO.File.WriteAllBytes(tempFileLocation, data);
            return tempFileLocation;
        }
    }
}
