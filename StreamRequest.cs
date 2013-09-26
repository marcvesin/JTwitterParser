using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TwitterParser;

namespace TwitterParser
{
    /// <summary>
    /// 
    /// </summary>
    public class StreamRequest : TwitterRequest
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestType"></param>
        /// <param name="requestParameters"></param>
        /// <param name="limitation"></param>
        /// <param name="streamEncoding"></param>
        public StreamRequest(string requestType, string requestParameters, LimitationRequest limitation = null, string streamEncoding = "utf-8")
            : base(requestParameters,limitation, streamEncoding)
        {
            Informations = new XDocument(new XElement("STREAM_REQUEST"));
            GetStream(GetHttpWebResponse(requestType));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestType"></param>
        /// <returns></returns>
        private HttpWebResponse GetHttpWebResponse(string requestType)
        {
            string baseUrl = String.Empty, requestUrl = String.Empty, method = String.Empty;

            switch (requestType)
            {
                case "STREAM_GET":
                    baseUrl = @"https://stream.twitter.com/1.1/statuses/sample.json";
                    method = "GET";
                    break;
                case "STREAM_POST":
                    baseUrl = @"https://stream.twitter.com/1.1/statuses/filter.json";
                    method = "POST";
                    break;
                default:
                    break;
            }
            if (_parameters.Length != 0) requestUrl = baseUrl + "?";
            Dictionary<string, List<string>> parameters = SampleParameters(ref requestUrl, requestType, _parameters);
            if (parameters == null) requestUrl = baseUrl;

            return new OAuthUser(requestType, requestUrl, parameters).GetResponse();

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="myHttpWebResponse"></param>
        /// <returns></returns>
        protected override string GetStream(HttpWebResponse myHttpWebResponse)
        {
            StreamReader readStream = StreamReaderInitializer(myHttpWebResponse, _streamEncoding);
            Char[] read = new Char[256];
            //Reads 256 characters at a time.    
            int count = readStream.Read(read, 0, 256);
            string tweet1 = String.Empty, tweet2 = String.Empty;
            Console.WriteLine(String.Format("[{0}] : Connected to the stream.", DateTime.Now.ToLocalTime()));

            while (count > 0)
            {
                //Dumps the 256 characters on a string and displays the string to the console.
                String str = new String(read, 0, count);
                tweet1 += str;

                if (StreamFormat(ref tweet1, ref tweet2))
                {
                    List<XElement> listElement = TweetToXML(tweet1);
                    if (tweet2.Length != 0) { tweet1 = tweet2; tweet2 = String.Empty; }
                    else tweet1 = String.Empty;

                    if (listElement != null)
                        foreach (XElement tweet in listElement)
                        {
                            Console.WriteLine(tweet);
                            Informations.Root.Add(tweet);
                        }

                    
                }

                count = readStream.Read(read, 0, 256);
            }
            Console.WriteLine("=====DISCONNECTION=====");

            return Informations.Root.Value.ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tweetData"></param>
        /// <returns></returns>
        private List<XElement> TweetToXML(string tweetData)
        {
            List<XElement> listXML = new List<XElement>();
            Dictionary<string, string> tweetParameters = ReadParameters("TWEET");
            Dictionary<string, string> userParameters = ReadParameters("USER");
            Dictionary<string, string> tweetDic = new Dictionary<string, string>();
            Dictionary<string, string> userDic = new Dictionary<string, string>();

            JObject o = (JObject)JsonConvert.DeserializeObject(tweetData);

            if (o["created_at"] != null)
            {
                foreach (string key in userParameters.Keys)
                {
                    if (o["user"][key].ToString().Length == 0) userDic.Add(key, "N/A");
                    else userDic.Add(key, o["user"][key].ToString());
                }

                foreach (string key in tweetParameters.Keys)
                {
                    if (o[key].ToString().Length == 0) tweetDic.Add(key, "N/A");
                    else tweetDic.Add(key, o[key].ToString());
                }

                Tweet tweet = new Tweet(tweetDic, new User(userDic));
                listXML.Add(tweet.Get_XmlElement());
                userDic.Clear();
                tweetDic.Clear();
            }
            if (listXML.Count == 0) return null;

            return listXML;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tweet1"></param>
        /// <param name="tweet2"></param>
        /// <returns></returns>
        private bool StreamFormat(ref string tweet1, ref string tweet2)
        {
            int open = 0, close = 0;

            for (int i = 0; i < tweet1.Length; i++)
            {
                if (tweet1[i] == '{') open++;
                else if (tweet1[i] == '}') close++;

                if (close != 0 && close == open)
                {
                    if (i != tweet1.Length - 1)
                    {
                        tweet2 = tweet1.Substring(i + 1, tweet1.Length - 1 - i);
                        tweet1 = tweet1.Substring(0, i);
                    }
                    return true;
                }
                else if (close != 0 && i == tweet1.Length)
                {
                    if (i != tweet1.Length - 1)
                    {
                        tweet2 = tweet1.Substring(i + 1, tweet1.Length - 1);
                        tweet1 = tweet1.Substring(0, i);
                    }
                    return false;
                }


            }
            return false;
        }

    }
}
