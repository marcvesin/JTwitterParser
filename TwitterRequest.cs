using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using TwitterParser;




namespace TwitterParser
{
    /// <summary>
    /// 
    /// </summary>
    public class TwitterRequest
    {
        /// <summary>
        /// 
        /// </summary>
        protected string _parameters;
        /// <summary>
        /// 
        /// </summary>
        protected string _streamEncoding;
        /// <summary>
        /// 
        /// </summary>
        public LimitationRequest Limitation;
        /// <summary>
        /// 
        /// </summary>
        public string LastID;
        /// <summary>
        /// 
        /// </summary>
        public XDocument Informations;
        public List<Tweet> Tweets;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestParameters"></param>
        /// <param name="checkLimitation"></param>
        /// <param name="streamEncoding"></param>
        public TwitterRequest(string requestParameters,LimitationRequest limitation,string streamEncoding = "utf-8")
        {
            Tweets = new List<Tweet>();
            _parameters = requestParameters;
            _streamEncoding = streamEncoding;
            Limitation = limitation;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="myHttpWebResponse"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        protected StreamReader StreamReaderInitializer(HttpWebResponse myHttpWebResponse, string encoding)
        {
            // Gets the stream associated with the response.
            Stream receiveStream = myHttpWebResponse.GetResponseStream();
            Encoding encode = Encoding.GetEncoding(encoding);
            // Pipes the stream to a higher level stream reader with the required encoding format. 
            StreamReader readStream = new StreamReader(receiveStream, encode);
            return readStream;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="myHttpWebResponse"></param>
        /// <returns></returns>
        protected virtual string GetStream(HttpWebResponse myHttpWebResponse)
        {
            StreamReader readStream = StreamReaderInitializer(myHttpWebResponse, _streamEncoding);
            Char[] read = new Char[256];
            //Reads 256 characters at a time.    
            int count = readStream.Read(read, 0, 256);
            string tweet = String.Empty;
            while (count > 0)
            {
                String str = new String(read, 0, count);
                tweet += str;
                count = readStream.Read(read, 0, 256);
            }
            
            return tweet;
        }

        /// <summary>
        /// Method retrieving the all values of parameters according to their
        /// position in the base string to compare them with the parameters entered in <c>_parameters</c>.
        /// <remarks>The position can be read in Parameters.xml</remarks>
        /// </summary>
        /// <param name="parameters">Parameter : <c>_parameters</c></param>
        /// <returns>Return a Dictionary ( parameter's name -> position) </returns>
        static public Dictionary<string, string> ReadParameters(string parameters)
        {

            string path = Directory.GetCurrentDirectory().ToString();

           XDocument doc = XDocument.Load(Path.Combine(path, "TwitterParameters.xml"));
            /*Format de type delimited   begin
                             track       end */
            var parameters_key = new Dictionary<string, string>();

            foreach (var parameter in doc.Descendants(parameters).Descendants("key"))
            {
                if (parameter.Attribute("position") == null)
                    parameters_key.Add(parameter.Attribute("name").Value, "");
                else
                    parameters_key.Add(parameter.Attribute("name").Value, parameter.Attribute("position").Value);
            }

            return parameters_key;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="requestType"></param>
        /// <param name="parametersInput"></param>
        /// <returns></returns>
        protected Dictionary<string, List<string>> SampleParameters(ref string requestUrl, string requestType, string parametersInput)
        {
            var parameters = new Dictionary<string, List<string>>();
            parameters.Add("begin", new List<string>());
            parameters.Add("middle", new List<string>());
            parameters.Add("end", new List<string>());

            Dictionary<string, string> parameters_method = ReadParameters(requestType);
            int size_parameter_str = parametersInput.Split('&').Count();
            int size_parameter = 0;

            if (parametersInput == String.Empty && requestType == "STREAM_GET") return null;
            else if (parametersInput == String.Empty && requestType != "STREAM_GET") throw new KeysNumberException(requestType);
            

            foreach (string parameter in parametersInput.Split('&'))
            {
                string[] section = parameter.Split('=');

                foreach (var key in parameters_method.Keys)
                {
                    //Verification qu'il n'y a pas + de parametres qu'autorisé.
                    if (parameters_method.Count == 0)
                        throw new KeysNumberException(requestType);

                    else if (key == section.First())
                    {
                        string parameter_value = key + "=" + Uri.EscapeDataString(section.Last());
                        switch (parameters_method[key])
                        {
                            case "begin":
                                parameters["begin"].Add(parameter_value);
                                break;
                            case "middle":
                                parameters["middle"].Add(parameter_value);
                                break;
                            case "end":
                                parameters["end"].Add(parameter_value);
                                break;
                            default:
                                throw new FalseKeyException(requestType);
                        }
                        requestUrl += parameter_value + "&";
                        parameters_method.Remove(key);
                        size_parameter++;
                        break;
                    }
                }
            }
            requestUrl = requestUrl.Substring(0, requestUrl.Length - 1);
            //Si jamais une clé entrée n'existe pas.
            if (size_parameter != size_parameter_str)
                throw new FalseKeyException(requestType);

            if (requestType != "STREAM_GET" && size_parameter == 0)
                throw new FalseKeyException(requestType);

            return parameters;
        }

    }
}