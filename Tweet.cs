using System;
using System.Collections.Generic;
using System.Xml.Linq;


namespace TwitterParser
{
    /// <summary>
    /// Class initializing an user.
    /// </summary>
    public class Tweet
    {
        public Dictionary<string, string> TweetParameters;
        public User User;

       public Tweet(Dictionary<string,string> parameters,User user)
       {
           TweetParameters = parameters;
           User = user;
       }
       public Tweet(Dictionary<string, string> parameters)
       {

           TweetParameters = parameters;

       }

        /// <summary>
        /// Method creating a XMLElement from the tweet's data.
        /// </summary>
        /// <returns>Return a XMLElement</returns>
       public XElement Get_XmlElement(bool includeUser = true)
       {
           XElement tweet = new XElement("tweet");
           foreach (string key in TweetParameters.Keys)
               tweet.Add(new XElement(key, TweetParameters[key]));

           if(includeUser == true && User != null)  tweet.Add(User.Get_XmlElement());

           return tweet;
       }
    }
    ///// <summary>
    ///// Class associated to Coordinates with longitude and latitude informations
    ///// </summary>
    //class Coordinates
    //{
    //    string _longitude;
    //    string _latitude;
    //    string _coordinates;

    //    public Coordinates(string coordinates)
    //    {
    //        _coordinates = coordinates;
    //    }
    //    /// <summary>
    //    /// Class constructor
    //    /// </summary>
    //    /// <param name="longitude">Longitude stocked in _longitude</param>
    //    /// <param name="latitude">Latitude stocked in _latitude</param>
    //    public Coordinates(string longitude, string latitude)
    //    {
    //        _latitude = latitude;
    //        _longitude = longitude;
    //    }
    //    /// <summary>
    //    /// Method creating a XMLElement from the coordinates data.
    //    /// </summary>
    //    /// <returns>Return a XMLElement</returns>
    //    public XElement Create_XmlElement()
    //    { XElement a = new XElement("user",new XAttribute("id","id"));
    //    return a;
    //    }
    //}
    ///// <summary>
    ///// Class associated to a User's informations in a tweet
    ///// </summary>
    public class User
    {
        public Dictionary<string, string> UserParameters;

        public User(Dictionary<string,string> parameters)
        {
            UserParameters = parameters;
        }
        /// <summary>
        /// Method creating a XMLElement from the User data.
        /// </summary>
        /// <returns>Return a XMLElement</returns>
        public XElement Get_XmlElement()
        {
            XElement user = new XElement("user");
            foreach(string key in UserParameters.Keys)
                user.Add(new XElement(key,UserParameters[key]));

            return user;

        }
    }
}
