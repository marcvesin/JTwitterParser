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
    public class SearchRequest : TwitterRequest
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestParameters"></param>
        /// <param name="limitation"></param>
        /// <param name="streamEncoding"></param>
        public SearchRequest(string requestParameters,LimitationRequest limitation,string streamEncoding = "utf-8")
            : base(requestParameters,limitation, streamEncoding)
        {
            Limitation.SearchLimitation--;
            if (Limitation.SearchLimitation == 0)   throw new LimitationException("SEARCH", Limitation.SearchWaitTime);
            Informations = new XDocument(new XElement("SEARCH_REQUEST"));
            GetStream(GetHttpWebResponse("SEARCH"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestType"></param>
        /// <returns></returns>
         private HttpWebResponse GetHttpWebResponse(string requestType)
         {
             string requestUrl = "https://api.twitter.com/1.1/search/tweets.json?";
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
             List<XElement> tweetList = TweetToXML(tweetData);
             if (tweetList != null)
             {
                 foreach (XElement tweetXML in tweetList)
                     Informations.Root.Add(tweetXML);

                 LastID = Informations.Root.Elements("tweet").Last().Element("id").Value.ToString();
             }
             return LastID;
         }
         private List<XElement> TweetToXML(string tweetData)
         {
             List<JObject> list = new List<JObject>();
             List<XElement> listXML = new List<XElement>();
             Dictionary<string, string> tweetParameters = ReadParameters("TWEET");
             Dictionary<string, string> userParameters = ReadParameters("USER");


             JObject tmp = (JObject)JsonConvert.DeserializeObject(tweetData);
             JArray array = (JArray)tmp["statuses"];
             if (array.Count == 0) return null;
             foreach (JToken token in array)
                 list.Add((JObject)token);

            foreach(JObject o in list)
             if (o["created_at"] != null)
             {
                 Dictionary<string, string> tweetDic = new Dictionary<string, string>();
                 Dictionary<string, string> userDic = new Dictionary<string, string>();

                 foreach (string key in userParameters.Keys)
                 {
                     if (userDic.ContainsKey(key)) userDic[key] = o["user"][key].ToString();
                     else if (o["user"][key].ToString().Length == 0) userDic.Add(key, "N/A");
                     else userDic.Add(key, o["user"][key].ToString());
                 }

                 foreach (string key in tweetParameters.Keys)
                 {
                     if (tweetDic.ContainsKey(key)) tweetDic[key] = o[key].ToString();
                     else if (o[key].ToString().Length == 0) tweetDic.Add(key, "N/A");
                     else tweetDic.Add(key, o[key].ToString());
                 }

                 Tweet tweet = new Tweet(tweetDic, new User(userDic));
                 Tweets.Add(tweet);
                 listXML.Add(tweet.Get_XmlElement());
                 //userDic.Clear();
                 //tweetDic.Clear();
             }
             if (listXML.Count == 0) return null;

             return listXML;
         }

    }
}
