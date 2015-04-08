using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace api.example
{
    class Program
    {
        static void Main(string[] args)
        {
            string key = "YOUR KEY";                                // Please use your own api key
            string secret = "YOUR SECRET";                          // and secret.
            string route = "/merchantV1/GetCurrencyExchangeRates";  // The api-method path
            string method = "POST";                                 // The request method for your api-method.
            string content = "{\"currency\":\"USD\",\"amount\":2}"; // The content of your request. (a.k.a. the body-part for your requested api-method)
            
            string url = "https://api-demo.coinsnap.eu";            // Please use the base url for your enviroment (LIVE/DEMO)

            // Generate a new nonce for every request to minimize the possibility of brute force attacks.
            string nonce = (new Random(DateTime.Now.Millisecond)).Next(0,99999999).ToString();

            // Calculate the hashes for this request
            var hashRaw = SHA256.Create("sha256").ComputeHash(Encoding.UTF8.GetBytes(nonce + content));                             // Calculate the content hash
            var hash = BitConverter.ToString(hashRaw).Replace("-", string.Empty).ToLower();                                         // Format it accordingly
            var signRaw = (new HMACSHA512(Encoding.Default.GetBytes(secret))).ComputeHash(Encoding.UTF8.GetBytes(route + hash));    // Calculate the sign hash
            var sign = BitConverter.ToString(signRaw).Replace("-", string.Empty).ToLower();                                         // Format it accordingly

            // Create and configure the request
            var request = HttpWebRequest.Create(url + route);
            request.Method = method;
            request.Headers.Add("X-Key", key);
            request.Headers.Add("nonce", nonce);
            request.Headers.Add("X-Sign", sign);
            
            // Provide the request with your body data.
            byte[] buf = Encoding.UTF8.GetBytes(content);
            request.ContentLength = buf.Length;
            request.GetRequestStream().Write(buf, 0, buf.Length);

            // Send the request. Please note that any other status code then 20X indecates an error, to get more information about the error evaluate the error response. 
            try
            {
                Console.WriteLine(new StreamReader(request.GetResponse().GetResponseStream()).ReadToEnd());
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(new StreamReader(ex.Response.GetResponseStream()).ReadToEnd());
            }
            Console.ReadKey();
        }
    }
}
