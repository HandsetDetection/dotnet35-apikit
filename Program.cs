using System;
using System.Linq;
using System.Configuration;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace HD3
{
    ///<Summary>
    /// Program class to test our HD3 class.
    ///</Summary>
    public class Program
    {
        ///<Summary>
        /// Main method
        ///</Summary>
        public static void Main(string[] args)
        {           
            IDictionary<string, string> settings = HD3Config.Initialize();
            Console.WriteLine(settings["username"]);    // print our username      
            Console.ReadKey();
        }
    }

    ///<Summary>
    /// static class HD3Config.
    ///</Summary>
    public static class HD3Config 
    {
        private static IDictionary<string, string> keyvaluepairs =
            new Dictionary<string, string>();

        ///<Summary>
        /// Initialize method
        ///</Summary>
        public static IDictionary<string, string> Initialize()
        {
            // Grab the Environments listed in the App.config and add them to our list.
            var appSettings = ConfigurationManager.GetSection("appSettings") as NameValueCollection;
            if (appSettings != null)
            {
                foreach (var key in appSettings.AllKeys)
                {
                    string value = appSettings.GetValues(key).FirstOrDefault();
                    keyvaluepairs.Add(key, value);                   
                }
            }
            return keyvaluepairs;
        }
    }
}
