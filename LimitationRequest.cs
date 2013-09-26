using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TwitterParser;

namespace TwitterParser
{
    /// <summary>
    /// 
    /// </summary>
    public class LimitationRequest
    {
        /// <summary>
        /// 
        /// </summary>
        public int SearchLimitation;
        /// <summary>
        /// 
        /// </summary>
        public string SearchWaitTime;
        /// <summary>
        /// 
        /// </summary>
        public int TimelineLimitation;
        /// <summary>
        /// 
        /// </summary>
        public string TimelineWaitTime;
        /// <summary>
        /// 
        /// </summary>
        public int LimiteRateLimitation;
        /// <summary>
        /// 
        /// </summary>
        public string LimiteRateWaitTime;
        /// <summary>
        /// 
        /// </summary>
        private string _response;
        /// <summary>
        /// 
        /// </summary>
        public LimitationRequest()
        {
            
            
            string requestUrl = "https://api.twitter.com/1.1/application/rate_limit_status.json?resources=help,account,application,users,search,statuses";

            OAuthApp oauth = new OAuthApp("LIMITE_RATE", requestUrl);
            _response = GetStream(oauth.GetResponse());


            SearchLimitation = GetSearchLimitation(_response,ref SearchWaitTime);
            TimelineLimitation = GetTimelineLimitation(_response,ref TimelineWaitTime);
            LimiteRateLimitation = GetLimitRateLimitation(_response, ref LimiteRateWaitTime);

            if (LimiteRateLimitation <= 2) throw new LimitationException("LIMITATION", LimiteRateWaitTime);
        }
       /// <summary>
       /// 
       /// </summary>
       /// <param name="myHttpWebResponse"></param>
       /// <returns></returns>
        private StreamReader StreamReaderInitializer(HttpWebResponse myHttpWebResponse)
        {
            // Gets the stream associated with the response.
            Stream receiveStream = myHttpWebResponse.GetResponseStream();
            // Pipes the stream to a higher level stream reader with the required encoding format. 
            StreamReader readStream = new StreamReader(receiveStream);
            return readStream;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="myHttpWebResponse"></param>
        /// <returns></returns>
        public string GetStream(HttpWebResponse myHttpWebResponse)
        {
            StreamReader readStream = StreamReaderInitializer(myHttpWebResponse);
            Char[] read = new Char[256];
            //Reads 256 characters at a time.    
            int count = readStream.Read(read, 0, 256);
            string data = String.Empty;
            while (count > 0)
            {
                String str = new String(read, 0, count);
                data += str;
                count = readStream.Read(read, 0, 256);
            }

            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="waitTime"></param>
        /// <returns></returns>
        private int GetSearchLimitation(string data,ref string waitTime)
        {
            string type = "/search/tweets";
            
            JObject o = (JObject)JsonConvert.DeserializeObject(data);

            int limitation = Convert.ToInt32(o["resources"]["search"][type]["remaining"].ToString());
            waitTime = DateFromUnixTime(o["resources"]["search"][type]["reset"].ToString());
            return limitation;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="waitTime"></param>
        /// <returns></returns>
        private int GetTimelineLimitation(string data, ref string waitTime)
        {
            string type = "/statuses/user_timeline";

            JObject o = (JObject)JsonConvert.DeserializeObject(data);
            int limitation = Convert.ToInt32(o["resources"]["statuses"][type]["remaining"].ToString());
            waitTime = DateFromUnixTime(o["resources"]["statuses"][type]["reset"].ToString());
            return limitation;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="waitTime"></param>
        /// <returns></returns>
        private int GetLimitRateLimitation(string data, ref string waitTime)
        {
            string type = "/application/rate_limit_status";
            JObject o = (JObject)JsonConvert.DeserializeObject(data);
            int limitation = Convert.ToInt32(o["resources"]["application"][type]["remaining"].ToString());
            waitTime = DateFromUnixTime(o["resources"]["application"][type]["reset"].ToString());
            return limitation;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="waitTime"></param>
        /// <returns></returns>
        private string DateFromUnixTime(string Time)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long unixTime = Convert.ToInt64(Time);
            epoch = epoch.AddSeconds(unixTime).ToLocalTime();
            return epoch.ToString();
        }
   
    
    
    }
}
