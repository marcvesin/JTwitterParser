using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// If compiling from the command line, compile with: /doc:YourFileName.xml 

namespace TwitterParser
{

    /// <summary>
    ///  This class performs an the operation to return the connection header for
    ///  the Twitter API connection request.
    ///     <remarks> 
    ///     The connection tokens are read from App.config 
    ///     </remarks> 
    /// </summary>
    public class OAuthUser
    {
        /// <summary>
        /// Store the elements for the base string ordonned by key.
        ///     <remarks>
        ///     The parameters are read in the Twitter/OAuth_config section of App.config
        ///     </remarks>
        /// </summary>
        SortedDictionary<string, string> _baseString;
        /// <summary>
        /// Store the method used for the conection ( GET or POST )
        /// </summary>
        string _method;
        string _requestType;
        /// <summary>
        /// Store the parameters for the research sorted in 3 sections : Begin , Middle, End
        /// This in fonction of their placement in the base string.
        /// </summary>
        Dictionary<string,List<string>> _parameters;
        /// <summary>
        /// Store the url used in order to connect the program to the API.
        ///     <remarks>
        ///     Generally : https://stream.twitter.com/1.1/statuses
        ///     </remarks>
        /// </summary>
        string _baseUrl;
        string _requestUrl;
        public string AuthToken;
/// <summary>
/// 
/// </summary>
/// <param name="requestType"></param>
/// <param name="requestUrl"></param>
/// <param name="parameters"></param>
        public OAuthUser(string requestType,string requestUrl, Dictionary<string,List<string>> parameters)
        {
            try{
                _requestUrl = requestUrl;
                _requestType = requestType;
                _parameters = parameters;
                SetMethodAndUrl();
                _baseString = new SortedDictionary<string, string>();

                Hashtable config = (Hashtable)ConfigurationManager.GetSection("Twitter/OAuth_config");
            
                foreach (string key in config.Keys)
                    _baseString.Add(key,(string)config[key]);

                AuthToken = Header();
            }
            catch (WebException error) { throw new TwitterWebException(error); }
        }
        /// <summary>
        /// Method returning the header.
        /// </summary>
        /// <returns>Return a the Header as a string for the connection</returns>
        private string Header()
        {
            _baseString.Add("oauth_timestamp", TimeStamp());
            _baseString.Add("oauth_nonce", Nonce());
            _baseString.Add("oauth_signature", Signature());
            

            string header = "OAuth oauth_consumer_key=\"" + Uri.EscapeDataString(_baseString["oauth_consumer_key"])+
                            "\", oauth_nonce=\"" + Uri.EscapeDataString(_baseString["oauth_nonce"]) +
                            "\", oauth_signature=\"" + Uri.EscapeDataString(_baseString["oauth_signature"]) +
                            "\", oauth_signature_method=\"" + Uri.EscapeDataString(_baseString["oauth_signature_method"]) +
                            "\", oauth_timestamp=\"" + Uri.EscapeDataString(_baseString["oauth_timestamp"]) +
                            "\", oauth_token=\"" + Uri.EscapeDataString(_baseString["oauth_token"]) +
                            "\", oauth_version=\"" + Uri.EscapeDataString(_baseString["oauth_version"]) + "\"";
            return header;
        }
        /// <summary>
        /// Method returning the base string according to the parameters
        /// </summary>
        /// <returns>Return  a string</returns>
        private string BaseString()
        {
            string base_Url = _baseUrl;

            string url_encoded = _method + "&" + Uri.EscapeDataString(base_Url) + "&";

            if (_parameters != null)
                foreach(var parameter in _parameters["begin"])
                    url_encoded += Uri.EscapeDataString(parameter+"&");
            
            foreach (var key in _baseString.Keys)
                url_encoded += Uri.EscapeDataString(key + "=" + _baseString[key] + "&");

                if (_parameters != null)
                {
                    foreach (var parameter_middle in _parameters["middle"])
                        url_encoded += Uri.EscapeDataString(parameter_middle+"&");
                    foreach (var parameter_end in _parameters["end"])
                        url_encoded += Uri.EscapeDataString(parameter_end+"&");
                }

                 url_encoded = url_encoded.Substring(0, url_encoded.Length - 3);
            return url_encoded;
        }
        /// <summary>
        /// Method returning a parameter that indicates when the request was created. 
        /// This value should be the number of seconds since the Unix epoch at the point the request is generated.
        /// </summary>
        /// <returns>Return a string</returns>
        private string TimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }
        /// <summary>
        /// Method producing a unique token your application should generate for each unique request. 
        /// Twitter will use this value to determine whether a request has been submitted multiple
        /// times. The value for this request was generated by base64 encoding 32 bytes of random data,
        /// and stripping out all non-word characters.
        /// </summary>
        /// <returns>Return a string</returns>
        private string Nonce()
        {
            return Convert.ToBase64String(new ASCIIEncoding().GetBytes( DateTime.Now.Ticks.ToString()));
        }
        /// <summary>
        /// Method providing a unique token that the application must generate for each unique request. 
        /// Twitter will use this value to determine whether a request has been submitted multiple times. 
        /// The value for this request was generated by base64 encoding 32 bytes of random data,
        /// and stripping out all non-word characters.
        /// </summary>
        /// <returns>Return a string</returns>
        private string Signature()
        {
            Hashtable infos = (Hashtable)ConfigurationManager.GetSection("Twitter/OAuth_secret");

            string cle = Uri.EscapeDataString((string)infos["oauth_consumer_secret"]) + "&" 
                        + Uri.EscapeDataString((string)infos["oauth_token_secret"]);

            HMACSHA1 hasher = new HMACSHA1(new ASCIIEncoding().GetBytes(cle));

            return Convert.ToBase64String(hasher.ComputeHash(new ASCIIEncoding().GetBytes(BaseString())));
        }

        private void SetMethodAndUrl()
        {
            switch (_requestType)
            {
                case "STREAM_GET":
                    _baseUrl = @"https://stream.twitter.com/1.1/statuses/sample.json";
                    _method = "GET";
                    break;
                case "STREAM_POST":
                    _baseUrl = @"https://stream.twitter.com/1.1/statuses/filter.json";
                    _method = "POST";
                    break;
                case "TIMELINE":
                   _baseUrl = @"https://api.twitter.com/1.1/statuses/user_timeline.json";
                    _method = "GET";
                    break;
                case "SEARCH":
                    _baseUrl = @"https://api.twitter.com/1.1/search/tweets.json";
                    _method = "GET";
                    break;
                case "LIMITE_RATE":
                    _baseUrl = @"https://api.twitter.com/1.1/application/rate_limit_status.json";
                    _method = "GET";
                    break;
                default:
                    Console.WriteLine("False key method");
                    break;
            }
        }
        public HttpWebResponse GetResponse()
        {
            HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create(_requestUrl);
            hwr.Proxy = null;
            if (_method == "POST")
            {
                hwr.Method = "POST";
                hwr.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
            }

            hwr.Headers.Add("Authorization", AuthToken);
            HttpWebResponse  myHttpWebResponse = (HttpWebResponse)hwr.GetResponse();
            return myHttpWebResponse;
           
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class OAuthApp
    {
        /// <summary>
        /// 
        /// </summary>
        string _method;
        /// <summary>
        /// 
        /// </summary>
        string _requestUrl;
        /// <summary>
        /// 
        /// </summary>
        public string AccessToken;

        object _locker = new object();


        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestType"></param>
        /// <param name="requestUrl"></param>
        public OAuthApp(string requestType,string requestUrl)
        {
            try
            {
                _requestUrl = requestUrl;
                SetMethod(requestType);
                lock (_locker)
                {
                    if (!SetAccessTokenFromFile())
                    {
                        Console.WriteLine("FromRequest");
                        SetAccessTokenFromRequest();
                    }
                }
            }
            catch (WebException error) { throw new TwitterWebException(error); }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestType"></param>
        private void SetMethod(string requestType)
        {
            switch (requestType)
            {
                case "TIMELINE":
                    _method = "GET";
                    break;
                case "SEARCH":
                    _method = "GET";
                    break;
                case "LIMITE_RATE":
                    _method = "GET";
                    break;
                default:
                    Console.WriteLine("False key method");
                    break;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void SetAccessTokenFromRequest()
        {
            HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create("https://api.twitter.com/oauth2/token");
            hwr.Proxy = null;
            hwr.Method = "POST";
            hwr.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";

            Hashtable config1 = (Hashtable)ConfigurationManager.GetSection("Twitter/OAuth_config");
            Hashtable config2 = (Hashtable)ConfigurationManager.GetSection("Twitter/OAuth_secret");

            string key = Uri.EscapeDataString((string)config1["oauth_consumer_key"]);
            string secret = Uri.EscapeDataString((string)config2["oauth_consumer_secret"]);

            string authorization = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(String.Format("{0}:{1}", key, secret)));
            hwr.Headers.Add("Authorization", authorization);

            byte[] byteArray = Encoding.UTF8.GetBytes("grant_type=client_credentials");
            hwr.ContentLength = byteArray.Length;

            using (Stream newStream = hwr.GetRequestStream()) { newStream.Write(byteArray, 0, byteArray.Length); }

            HttpWebResponse bearerResponse= null;
            try
            {
                bearerResponse = (HttpWebResponse)hwr.GetResponse();
            }
            catch (WebException ex)
            {
                var z = ex.Response.Headers;
                foreach (var a in ex.Response.Headers.AllKeys)
                    Console.WriteLine(z[a]);

            }
            string responseStr = String.Empty;
            using (Stream response = bearerResponse.GetResponseStream()){
                using (StreamReader reader = new StreamReader(response)) { responseStr = reader.ReadToEnd(); }
            }

            Dictionary<string,string> parsedResponse = BearerFormat(responseStr);
            //Vérfier la validité
            
            AccessToken = parsedResponse["access_token"];
            using(StreamWriter wr = new StreamWriter("access_token.txt"))
            {
                wr.WriteLine(AccessToken);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool SetAccessTokenFromFile()
        {
            using (Stream s = new FileStream("access_token.txt", FileMode.OpenOrCreate))
            {
                if (s.Length == 0) return false;
                else
                    using (StreamReader reader = new StreamReader(s))
                    { AccessToken = reader.ReadToEnd(); }
            }
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public HttpWebResponse GetResponse()
        { 
            HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create(_requestUrl);
            hwr.Proxy = null;
            if (_method == "POST")
            {
                hwr.Method = "POST";
                hwr.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
            }
            else        hwr.Headers.Add("Authorization", "Bearer "+ AccessToken);

            HttpWebResponse myHttpWebResponse = null;
            try
            {
                myHttpWebResponse = (HttpWebResponse)hwr.GetResponse();
            }
            catch (WebException ex) 
            {
                Console.WriteLine(ex.Message);

                SetAccessTokenFromRequest();
                hwr.Headers.Add("Authorization", "Bearer " + AccessToken);
                myHttpWebResponse = (HttpWebResponse)hwr.GetResponse(); 
            }

            return myHttpWebResponse;
        
        }

        private Dictionary<string, string> BearerFormat(string response)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            JObject o = (JObject)JsonConvert.DeserializeObject(response);
            result.Add("bearer", (string)o["bearer"]);
            result.Add("access_token", (string)o["access_token"]);
            return result;
        }
    }

}