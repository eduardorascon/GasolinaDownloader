using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;

namespace Downloader
{
    public class FirebaseClient
    {
        public static void Update(List<string> files)
        {
            string base_firebase_url = ConfigurationManager.AppSettings["firebase_url"];

            string endpoint = string.Empty;
            foreach (string file in files)
            {
                endpoint = base_firebase_url + Path.GetFileName(file).Replace(".json", "").Substring(8) + ".json";
                string json = File.ReadAllText(file);
                HttpWebRequest request = WebRequest.CreateHttp(endpoint);
                request.Method = "PUT";
                request.ContentType = "application/json";
                byte[] data = Encoding.UTF8.GetBytes(json);
                request.ContentLength = data.Length;
                request.GetRequestStream().Write(data, 0, data.Length);
                WebResponse response = request.GetResponse();
                json = new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
        }
    }
}