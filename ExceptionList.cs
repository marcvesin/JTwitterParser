using System;
using System.Collections.Generic;
using System.Net;

namespace TwitterParser
{


    public class TwitterException : Exception
    { }
    public class TwitterWebException : TwitterException
    { 
        public TwitterWebException(WebException error)
        {
            if (error.Response != null)
            {
                int rep = (int)((HttpWebResponse)error.Response).StatusCode;
                if (rep == 406)
                    HelpLink= "Error 406 : One (or several) value of one (or several) parameters are wrong.";
                else
                    HelpLink = "Error "+ rep +" : For more informations : https://dev.twitter.com/docs/error-codes-responses ";

            }
            else HelpLink = "No connection found for the Twitter Request.";
        }
    
    }
    /// <summary>
    /// 
    /// </summary>
    public class ParametersException :TwitterException
    {
        /// <summary>
        /// 
        /// </summary>
        public List<string> _parameters;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request_type"></param>
        public ParametersException(string request_type)
        {
            _parameters = new List<string>();
            foreach (string key in TwitterRequest.ReadParameters(request_type).Keys)
                _parameters.Add(key);

            
           
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class KeysNumberException : ParametersException
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        public KeysNumberException(string method)
            : base(method)
        {
            HelpLink = "The number of input parameters is too high : ";
            HelpLink += "\nCorrect parameters for the request are :";
            foreach (string parameter in _parameters)
                HelpLink += String.Format("\n\t" + parameter);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class FalseKeyException : ParametersException
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestType"></param>
        public FalseKeyException(string requestType)
            : base(requestType)
        {
            HelpLink = "One of the parameters entered is not correct.";
            HelpLink += "\nCorrect parameters for the request are :";
            foreach (string parameter in _parameters)
                HelpLink += String.Format("\n\t" + parameter);
        }
    
    }
    /// <summary>
    /// 
    /// </summary>
    public class LimitationException : TwitterException
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="WaitTime"></param>
        public LimitationException(string type, string WaitTime)
        {
            HelpLink = "You have reached the limitation for "+type+" request. Please wait : " + WaitTime;
        }
    
    }

}
