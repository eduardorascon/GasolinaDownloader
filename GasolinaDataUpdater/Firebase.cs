using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;

namespace DownloaderLibrary
{
    public class FirebaseClient
    {
        public static void UpdateEstados(string file)
        {
            string base_firebase_url = ConfigurationManager.AppSettings["firebase_url"];

            string endpoint = base_firebase_url + "estados.json?auth=";
            string json = File.ReadAllText(file);
            HttpWebRequest request = WebRequest.CreateHttp(endpoint);
            request.Method = "PATCH";
            request.ContentType = "application/json";
            byte[] data = Encoding.UTF8.GetBytes(json);
            request.ContentLength = data.Length;
            request.GetRequestStream().Write(data, 0, data.Length);
            WebResponse response = request.GetResponse();
            json = new StreamReader(response.GetResponseStream()).ReadToEnd();
        }

        public static void UpdatePrecios(string file)
        {
            string base_firebase_url = ConfigurationManager.AppSettings["firebase_url"];

            string endpoint = base_firebase_url + "precios.json?auth=";
            string json = File.ReadAllText(file);
            HttpWebRequest request = WebRequest.CreateHttp(endpoint);
            request.Method = "PATCH";
            request.ContentType = "application/json";
            byte[] data = Encoding.UTF8.GetBytes(json);
            request.ContentLength = data.Length;
            request.GetRequestStream().Write(data, 0, data.Length);
            WebResponse response = request.GetResponse();
            json = new StreamReader(response.GetResponseStream()).ReadToEnd();
        }
    }
}