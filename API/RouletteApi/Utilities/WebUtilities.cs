using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public static class WebUtilities
    {
        public enum Method
        {
            Get,
            Post,
            Put,
            Delete
        }

        public async static Task<HttpResponseMessage> ConectAsync(Method method, string baseAddress, string path, dynamic data, Dictionary<string, string> headers = null)
        {
            string httpContent = JsonConvert.SerializeObject(data);
            using var client = new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
            };
            if (headers != null)
                foreach (KeyValuePair<string, string> entry in headers)
                    client.DefaultRequestHeaders.Add(entry.Key, entry.Value);
            else
                client.DefaultRequestHeaders.Accept.Clear();
            StringContent stringContent = null;
            if (!string.IsNullOrEmpty(httpContent?.ToString()))
                stringContent = new StringContent(httpContent, Encoding.UTF8, "application/json");

            return method switch
            {
                Method.Get => await client.GetAsync(path),
                Method.Put => await client.PutAsync(path, stringContent),
                Method.Delete => await client.DeleteAsync(path),
                _ => await client.PostAsync(path, stringContent),
            };
        }
        public async static Task<HttpResponseMessage> ConectAsync(Method method, string baseAddress, string path, string httpContent, Dictionary<string, string> headers = null)
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
            };
            if (headers != null)
                foreach (KeyValuePair<string, string> entry in headers)
                    client.DefaultRequestHeaders.Add(entry.Key, entry.Value);
            else
                client.DefaultRequestHeaders.Accept.Clear();
            StringContent stringContent = null;
            if (!string.IsNullOrEmpty(httpContent))
                stringContent = new StringContent(httpContent, Encoding.UTF8, "application/json");

            return method switch
            {
                Method.Get => await client.GetAsync(path),
                Method.Put => await client.PutAsync(path, stringContent),
                Method.Delete => await client.DeleteAsync(path),
                _ => await client.PostAsync(path, stringContent),
            };
        }
        public static HttpResponseMessage Conect(Method method, string baseAddress, string path, dynamic data, Dictionary<string, string> headers = null)
            => ConectAsync(method, baseAddress, path, data, headers).Result;
        public static HttpResponseMessage Conect(Method method, string baseAddress, string path, string httpContent, Dictionary<string, string> headers = null)
            => ConectAsync(method, baseAddress, path, httpContent, headers).Result;
        public static string ValidateContent(this HttpResponseMessage httpResponse)
        {
            string resp = null;
            if (httpResponse.Content.Headers.ContentLength > 0)
            {
                Stream stream = httpResponse.Content.ReadAsStreamAsync().Result;
                StreamReader sr = new(stream);
                resp = sr.ReadToEnd();
            }

            return resp;
        }
        public static T MapResponse<T>(this HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return response.ValidateContent().ToEntitySimple<T>();
            else
                throw HttpCallError(response);
        }
        public static IEnumerable<T> MapListResponse<T>(this HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return response.ValidateContent().ToEntityListSimple<T>();
            else
                throw HttpCallError(response);
        }
        public static Exception HttpCallError(this HttpResponseMessage response) =>
            new($"StatusCode: {Convert.ToInt16(response.StatusCode)}, {Environment.NewLine} Messege: {response.ValidateContent()}");
    }
}