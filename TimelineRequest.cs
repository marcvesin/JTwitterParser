using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TwitterParser;

namespace TwitterParser
{
    /// <summary>
    /// 
    /// </summary>
    public class AllTimelineRequest : TwitterRequest
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestParameters"></param>
        /// <param name="limitation"></param>
        /// <param name="streamEncoding"></param>
        public AllTimelineRequest(string requestParameters, LimitationRequest limitation, string streamEncoding = "utf-8")
            : base(requestParameters, limitation, streamEncoding)
        {
            Informations = new XDocument(new XElement("ALLTIMELINE_REQUEST"));
            GetAllTimeLine(requestParameters, streamEncoding);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestParameters"></param>
        /// <param name="streamEncoding"></param>
        private void GetAllTimeLine(string requestParameters, string streamEncoding)
        {
            long tmpID = 0;
            Console.WriteLine("Connected to AllTimeLine ... Remaining : " + Limitation.TimelineLimitation + " WaitTime : " + Limitation.LimiteRateWaitTime);
            for (int i = 0; i < Limitation.TimelineLimitation && i < 17; i++)
            {

                TimelineRequest request = new TimelineRequest(requestParameters, Limitation, streamEncoding);
                if (requestParameters.Contains("max_id")) requestParameters = requestParameters.Substring(0, requestParameters.LastIndexOf('&'));

                long lastID = Convert.ToInt64(request.LastID);
                if (lastID == tmpID) break;
                else lastID--;

                requestParameters += ("&max_id=" + lastID.ToString());
                tmpID = Convert.ToInt64(request.LastID);

                foreach (XElement tweetXML in request.TweetList)
                    Informations.Root.Add(tweetXML);
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class TimelineRequest : TwitterRequest
    {
        /// <summary>
        /// 
        /// </summary>
        public List<XElement> TweetList;
        /// <summary>
        /// 
        /// </summary>
        public XElement UserInfoXML;
        public User UserInfo;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestParameters"></param>
        /// <param name="limitation"></param>
        /// <param name="streamEncoding"></param>
        public TimelineRequest(string requestParameters, LimitationRequest limitation, string streamEncoding = "utf-8")
            : base(requestParameters, limitation, streamEncoding)
        {
            Limitation.TimelineLimitation--;
            if (Limitation.TimelineLimitation <= 1) throw new LimitationException("TIMELINE", Limitation.TimelineWaitTime);
            Informations = new XDocument(new XElement("TIMELINE_REQUEST"));
            GetStream(GetHttpWebResponse("TIMELINE"));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestType"></param>
        /// <returns></returns>
        private HttpWebResponse GetHttpWebResponse(string requestType)
        {
            string requestUrl = "https://api.twitter.com/1.1/statuses/user_timeline.json?";
            SampleParameters(ref requestUrl, requestType, _parameters);
            return new OAuthApp(requestType, requestUrl).GetResponse();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="myHttpWebResponse"></param>
        /// <returns></returns>
        protected override string GetStream(HttpWebResponse myHttpWebResponse)
        {
            string tweetData = base.GetStream(myHttpWebResponse);
            Console.WriteLine(String.Format("[{0}] : Getting the Timeline", DateTime.Now.ToLocalTime()));
            TweetList = TweetToXML(tweetData);
            if (TweetList != null)
            {
                foreach (XElement tweetXML in TweetList)
                    Informations.Root.Add(tweetXML);

                LastID = Informations.Root.Elements("tweet").Last().Element("id").Value.ToString();
            }

            return LastID;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tweetData"></param>
        /// <returns></returns>
        private List<XElement> TweetToXML(string tweetData)
        {
            List<JObject> list = new List<JObject>();
            List<XElement> listXML = new List<XElement>();
            Dictionary<string, string> tweetParameters = ReadParameters("TWEET");
            Dictionary<string, string> userParameters = ReadParameters("USER");
            
            Dictionary<string, string> userDic = new Dictionary<string, string>();

            JArray array = (JArray)JsonConvert.DeserializeObject(tweetData);
            foreach (JToken ob in array.Root)
                list.Add((JObject)ob);

            foreach (JObject o in list)
                if (o["created_at"] != null)
                {
                    foreach (string key in userParameters.Keys)
                    {
                        if (userDic.ContainsKey(key)) userDic[key] = o["user"][key].ToString();
                        else if (o["user"][key].ToString().Length == 0) userDic.Add(key, "N/A");
                        else userDic.Add(key, o["user"][key].ToString());
                    }

                    User user = new User(userDic);
                    UserInfo = user;
                    UserInfoXML = user.Get_XmlElement();

                    Informations.Root.Add(UserInfo);
                    break;
                }

            foreach (JObject o in list)
                if (o["created_at"] != null)
                {
                    Dictionary<string, string> tweetDic = new Dictionary<string, string>();

                    foreach (string key in tweetParameters.Keys)
                    {
                        
                        if (tweetDic.ContainsKey(key)) tweetDic[key] = o[key].ToString();
                        else if (o[key].ToString().Length == 0) tweetDic.Add(key, "N/A");
                        else tweetDic.Add(key, o[key].ToString());
                    }

                    Tweet tweet = new Tweet(tweetDic);
                    Tweets.Add(tweet);
                    listXML.Add(tweet.Get_XmlElement(false));
                    //tweetDic.Clear();
                }

            if (listXML.Count == 0) return null;

            return listXML;
        }
    }
}
