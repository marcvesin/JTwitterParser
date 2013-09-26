using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using TwitterParser;



namespace TwitterParser
{
    class Program
    {

        static void Main(string[] args)
        {
            string location = "locations=2.392244,48.868328,2.414045,48.888988";
            string track = "track=psg";
            string screen_name = "screen_name=afpfr&count=2";
            string q = "q=Sudria&count=1";

            //StreamRequest r = new StreamRequest("STREAM_POST", location);
           // //StreamRequest r = new StreamRequest("STREAM_GET", "");
           // //SearchRequest search = new SearchRequest(q);
           // //LimitationRequest req = new LimitationRequest();
           // ////AllTimelineRequest request = new AllTimelineRequest(screen_name,req);
           // //SearchRequest sear = null;
           // //for (int i = 0; i < 10; i++)
           // //    sear = new SearchRequest(q, req);
          LimitationRequest limitation = new LimitationRequest();
          //TimelineRequest s = new TimelineRequest(screen_name,limitation);
          //foreach(Tweet a in s.Tweets)
          //    Console.WriteLine(a.Get_XmlElement());
          SearchRequest s = new SearchRequest(q, limitation);
          Console.WriteLine(s.Informations);
           // using (StreamWriter str = new StreamWriter("lolilol.txt"))
           // { str.WriteLine(s.Informations); }
           // //TimelineRequest s = new TimelineRequest("screen_name=afpfr&count=5", limitation);


           //// Console.WriteLine(s.Informations);
           // //for (int i = 0; i < s.Tweets.Count; i++)
           // //    Console.WriteLine(s.Tweets[i].TweetParameters["text"] +"/n");

           // //Console.WriteLine("\n==>Limitation<==");
           // //Console.WriteLine(String.Format("LimiteRate remaining : {0}  | Waittime : {1}",req.LimiteRateLimitation,req.LimiteRateWaitTime));
           // //Console.WriteLine(String.Format("TimeLine remaining : {0}  | Waittime : {1}", req.TimelineLimitation, req.TimelineWaitTime));
           // //Console.WriteLine(String.Format("Search remaining : {0}  | Waittime : {1}", req.SearchLimitation, req.SearchWaitTime));

        }


    }
}





