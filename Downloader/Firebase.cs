using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;

namespace Downloader
{
    public class FirebaseClient
    {
        public FirebaseClient()
        {
            string firebase_url = ConfigurationManager.AppSettings["firebase_url"];
            HttpWebRequest request = WebRequest.CreateHttp(firebase_url);
            request.Method = "POST";
            request.ContentType = "application/json";

            DateTime now = DateTime.Now;

            //TODO
            //To be replaced with string json = File.ReadAll(filename);
            //Filename should be a .json file
            string json = JsonConvert.SerializeObject(new
            {
                Name = now.ToString("yyyyMMdd"),
                Value = now.ToString("HHmmss")
            });

            byte[] data = Encoding.UTF8.GetBytes(json);
            request.ContentLength = data.Length;
            request.GetRequestStream().Write(data, 0, data.Length);
            WebResponse response = request.GetResponse();
            json = new StreamReader(response.GetResponseStream()).ReadToEnd();
        }
    }
}