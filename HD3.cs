/*
** Copyright (c) 2009-2012
** Richard Uren <richard@teleport.com.au>
** All Rights Reserved
**
** --
**
** LICENSE: Redistribution and use in source and binary forms, with or
** without modification, are permitted provided that the following
** conditions are met: Redistributions of source code must retain the
** above copyright notice, this list of conditions and the following
** disclaimer. Redistributions in binary form must reproduce the above
** copyright notice, this list of conditions and the following disclaimer
** in the documentation and/or other materials provided with the
** distribution.
**
** THIS SOFTWARE IS PROVIDED ``AS IS'' AND ANY EXPRESS OR IMPLIED
** WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
** MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
** NO EVENT SHALL CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
** INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
** BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS
** OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
** ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR
** TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE
** USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
** DAMAGE.
**
** --
**
** This is a reference implementation for interfacing with www.handsetdetection.com apiv3
**
*/

//#define HD3_DEBUG

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Web;
using System.Security.Cryptography;
using System.Web.Caching;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Xml;
using System.Linq;
using Newtonsoft.Json;

namespace HD3 {

    ///<Summary>
    /// The HD3Cache class
    ///</Summary>
    public class HD3Cache {        
        ///<Summary>
        /// The CacheEntry class
        ///</Summary>
        class CacheEntry
        {
            public string Key;
            public DateTime Validity;
            public string Content;
        }

        private int maxJsonLength = 40000000;
        string prefix = "hd32-";
        Cache myCache;

        /// <summary>
        /// The HD3Cache class constructor
        /// </summary>
        /// <param name="cache"></param>
        public HD3Cache()
        {
            this.myCache = System.Web.HttpRuntime.Cache;
        } 

        /*public HD3Cache(Cache cache) {
            this.myCache = System.Web.HttpRuntime.Cache;
            //this.myCache = System.Web.HttpContext.Current.Cache;            
        }

        /// <summary>
        /// The HD3Cache class default constructor
        /// </summary>
        public HD3Cache()
            :this(HttpRuntime.Cache)
        {}
        */
        /// <summary>
        /// Write new object to dictionary
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void write(string key, Dictionary<string, object> value) {
            var s = this.prefix + key;
            if (value != null && key != "" && this.myCache[s] == null)
            {
                var jss = new JavaScriptSerializer();
                jss.MaxJsonLength = this.maxJsonLength;
                string storethis = jss.Serialize(value);
                this.myCache[s] = storethis;
            }
        }

        /// <summary>
        /// Read object from dictionary
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Dictionary<string, object> read(string key) {
            try {
                var s = this.prefix + key;
                if (this.myCache[s] == null) {
                    return null;
                }
                string fromCache = this.myCache[s].ToString();
                var jss = new JavaScriptSerializer();
                jss.MaxJsonLength = this.maxJsonLength;
                return jss.Deserialize<Dictionary<string, object>>(fromCache);
            } catch (Exception ex) {
                // Not in cache
                return null;
            }
        }
     }

    /// <summary>
    /// Main class for all handset detection API calls
    /// </summary>
    public class HD3 {
        int maxJsonLength = 40000000;        
        public int ReadTimeout { get; set; }       
        public int ConnectTimeout { get; set; }
        public string Username { get; set; }
        public string Secret { get; set; }
        public string SiteId { get; set; }
        public bool UseLocal { get; set; }        
        public bool UseProxy { get; set; }
        public string ProxyServer { get; set; }        
        public int ProxyPort { get; set; }
        public string ProxyPass { get; set; }
        public string ProxyUser { get; set; }        
        public string MatchFilter { get; set; }        
        public string NonMobile { get; set; }        
        public string ApiServer { get; set; }        
        public string LogServer { get; set; }
        public string getRawReply() { return this.rawreply;  }
        public object getReply() { return this.reply; }
        private BinaryReader reader { get; set; }
        private string projectDir = String.Empty;
        private Stream responseStream = null;
        public string pathString = "hd4cache";
        public bool isDownloadableFiles = false;
        public string FileDirectory
        {
            get
            {
                return System.IO.Path.Combine(projectDir, this.pathString);
            }
            set { value = System.IO.Path.Combine(projectDir, this.pathString); }
        }
        public string getError() { return this.error; }
        private void setError(string msg) { 
            this.error = msg; 
#if HD3_DEBUG
            this._log("ERROR : "+msg);
#endif
        }

        /// <summary>
        /// Return replay
        /// </summary>
        private void setRawReply() {
            var jss = new JavaScriptSerializer();
            jss.MaxJsonLength = this.maxJsonLength;
            this.rawreply = jss.Serialize(this.reply);
        }

        private HD3Cache myCache = new HD3Cache();
        //Parameters to send for detection request
        public Dictionary<string, string> m_detectRequest = new Dictionary<string, string>();
        private string rawreply;
        private Dictionary<string, object> reply = new Dictionary<string, object>();
        private Dictionary<string, object> tree = new Dictionary<string, object>();
        private Dictionary<string, object> specs = new Dictionary<string, object>();
        private string error = "";
        private HttpRequest Request;
        public string log = "";
        
        #region "Init constructors"
        /// <summary>
        /// Initializes the necessary information for a lookup from request object
        /// Accepts Object inializers 
        /// </summary>
        /// <param name="request">HttpRequest object from page</param>
        public HD3(HttpRequest request) {
            this.Request = request;            
            NameValueCollection appSettings = System.Configuration.ConfigurationManager.AppSettings;
            setupProperties();
            if (appSettings["username"] != null)
                Username = appSettings["username"];
            if (appSettings["secret"] != null)
                Secret = appSettings["secret"];
            if (appSettings["site_id"] != null)
                SiteId = appSettings["site_id"];
            if (appSettings["use_local"] != null)
                UseLocal = Convert.ToBoolean(appSettings["use_local"]);
            if (appSettings["use_proxy"] != null)
                UseProxy = Convert.ToBoolean(appSettings["use_proxy"]);
            if (appSettings["match_filter"] != null)
                MatchFilter = appSettings["match_filter"];
            if (appSettings["api_server"] != null)
                ApiServer = appSettings["api_server"];
            if (appSettings["log_server"] != null)
                LogServer = appSettings["log_server"];
            
            Regex reg = new Regex("^x|^http", RegexOptions.IgnoreCase);
            foreach (string header in request.Headers) {
                if (reg.IsMatch(header)) {
                    AddKey(header.ToLower(), request[header]);
                }
            }
            AddKey("user-agent", request.UserAgent);
            AddKey("ipaddress", request.UserHostAddress);
            AddKey("request_uri", request.Url.ToString());            
        }

        /// <summary>
        /// Constructor arguments assign your credentials.
        /// </summary>
        /// <param name="username">Your api username</param>
        /// <param name="secret">Your api secret</param>
        /// <param name="siteId">Your api siteId</param>
        /// <param name="isLocal">false</param>
        public HD3(string username, string secret, string siteId, bool isLocal) {
            setupProperties();
            Username = username;
            Secret = secret;
            SiteId = siteId;
            UseLocal = isLocal;
            setupProperties();
        }

        /// <summary>
        /// set other fields
        /// </summary>
        private void setupProperties() {
            ReadTimeout = 10;
            ConnectTimeout = 10;
            UseProxy = false;
            UseLocal = false;            
            ProxyPort = 80;
            MatchFilter = " _\\#-,./:\"'";
            NonMobile = "^Feedfetcher|^FAST|^gsa_crawler|^Crawler|^goroam|^GameTracker|^http://|^Lynx|^Link|^LegalX|libwww|^LWP::Simple|FunWebProducts|^Nambu|^WordPress|^yacybot|^YahooFeedSeeker|^Yandex|^MovableType|^Baiduspider|SpamBlockerUtility|AOLBuild|Link Checker|Media Center|Creative ZENcast|GoogleToolbar|MEGAUPLOAD|Alexa Toolbar|^User-Agent|SIMBAR|Wazzup|PeoplePal|GTB5|Dealio Toolbar|Zango|MathPlayer|Hotbar|Comcast Install|WebMoney Advisor|OfficeLiveConnector|IEMB3|GTB6|Avant Browser|America Online Browser|SearchSystem|WinTSI|FBSMTWB|NET_lghpset";
            LogServer = "log.handsetdetection.com";
            ApiServer = "api.handsetdetection.com";
            if (!String.IsNullOrEmpty(HttpRuntime.AppDomainAppVirtualPath))
            {
                projectDir = Request.PhysicalApplicationPath;
            }
            else
            {
                projectDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            }
        }

        /// <summary>Sets additional http headers for detection request, will override default headers.</summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public void setDetectVar(string key, string val) { AddKey(key, val); }

        public void AddKey(string key, string value) {
            key = key.ToLower();
            if (this.m_detectRequest.ContainsKey(key)) {
                this.m_detectRequest.Remove(key);
            }
#if HD3_DEBUG
            this._log("Added httpheader " + key + " " + value);
#endif
            this.m_detectRequest.Add(key, value);
        }

        /// <summary>
        /// Reset logger
        /// </summary>
        public void resetLog() { this.log = ""; }

        /// <summary>
        /// Create a log
        /// </summary>
        /// <param name="msg"></param>
        private void _log(string msg) { 
            this.log += DateTime.Now.ToString("HH:mm:ss.ffffff") + " " + msg + "<br/>\n"; 
        }

        /// <summary>
        /// Return the log
        /// </summary>
        /// <returns></returns>
        public string getLog() { return this.log; }

        /// <summary>
        /// Clean object
        /// </summary>
        public void cleanUp() { this.rawreply = ""; this.reply = new Dictionary<string, object>(); }
        #endregion

        #region "API functions"

        /// <summary>
        /// Pre processes the request and try different servers on error/timeout
        /// </summary>
        /// <param name="data"></param>
        /// <param name="service">Service strings vary depending on the information needed</param>
        /// <returns>JsonData</returns>
        private bool Remote(string service, Dictionary<string, string> data) {
            bool status = false;
            string request;
            this.reply = null;
            var jss = new JavaScriptSerializer();
            jss.MaxJsonLength = this.maxJsonLength;
            Uri url = new Uri("http://" + ApiServer + "/apiv3" + service);            
#if HD3_DEBUG
            this._log("Preparing to send to " + "http://" + this.api_server + "/apiv3" + service);
#endif
            if (data == null || data.Count == 0)
                request = "";
            else
                request = jss.Serialize(data);            
            try {
                status = post(ApiServer, url, request);                
                if (status) {
                    status = true;
                    this.reply = jss.Deserialize<Dictionary<string, object>>(this.rawreply);
                }                
            } catch (Exception ex) {
                this.setError("Exception : " + ex.Message + " " + ex.StackTrace);                
            }
            return status;
        }

        /// <summary>
        /// Post 
        /// </summary>
        /// <param name="host"></param>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool post(string host, Uri url, string data) {
            try {
                IPAddress[] ipv4Addresses = Array.FindAll(Dns.GetHostEntry(ApiServer).AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);
                // ToDo : Randomize the order of entries in ipList
                foreach (IPAddress ip in ipv4Addresses) {
#if HD3_DEBUG
                    this._log("Sending to server " + ip.ToString());
#endif
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                    req.ServicePoint.BindIPEndPointDelegate = delegate(ServicePoint servicePoint, IPEndPoint remoteEndPoint, int retryCount) {
                        return new IPEndPoint(IPAddress.Any, 0);
                    };

                    if (UseProxy) {
                        WebProxy proxy = new WebProxy(ProxyServer, ProxyPort);                        
                        proxy.Credentials = new NetworkCredential(ProxyUser, ProxyPass);
                        req.Proxy = proxy;
                    }
                    req.Timeout = ReadTimeout * 1000;
                    //req.PreAuthenticate = true;
                    req.Method = "POST";
                    req.ContentType = "application/json";
                    // AuthDigest Components - 
                    // Precomputing the digest saves on the server having to issue a challenge so its much quicker (network wise)
                    // http://en.wikipedia.org/wiki/Digest_access_authentication
                    string realm = "APIv3";
                    string nc = "00000001";
                    string snonce = "APIv3";
                    string cnonce = _helperMD5Hash(DateTime.Now.ToString() + Secret);
                    string qop = "auth";
                    string ha1 = _helperMD5Hash(Username + ":" + realm + ":" + Secret);
                    string ha2 = _helperMD5Hash("POST:" + url.PathAndQuery);
                    string response = _helperMD5Hash(ha1 + ":" + snonce + ":" + nc + ":" + cnonce + ":" + qop + ":" + ha2);
                    string digest = "Digest username=\"" + Username + "\", realm=\"" + realm + "\", nonce=\"" + snonce + "\", uri=\"" + url.PathAndQuery + "\", qop=" + qop + ", nc=" + nc + ", cnonce=\"" + cnonce + "\", response=\"" + response + "\", opaque=\"" + realm + "\"";
                    byte[] payload = System.Text.Encoding.ASCII.GetBytes(data);
                    req.ContentLength = payload.Length;
                    req.Headers.Add("Authorization", digest);
#if HD3_DEBUG
                    this._log("ha1 : " + ha1);
                    this._log("ha2 : " + ha2);
                    this._log("response : " + response);
                    this._log("digest : " + digest);
                    this._log("Send Headers: " + req.ToString());
                    this._log("Send Data: " + data);
#endif
                    Stream dataStream = req.GetRequestStream();
                    dataStream.Write(payload, 0, payload.Length);
                    dataStream.Close();

                    var httpResponse = (HttpWebResponse) req.GetResponse();
                    responseStream = new MemoryStream();
                    responseStream = httpResponse.GetResponseStream();
                    this.reader = new BinaryReader(responseStream);                    
#if HD3_DEBUG
                    this._log("Received : " + this.rawreply);
#endif
                    if (httpResponse.StatusCode == HttpStatusCode.OK)
                    {
                        if(this.isDownloadableFiles)
                        {
                            this.rawreply = "{\"status\":0}"; 
                        }
                        else
                        {
                            this.rawreply = new StreamReader(responseStream).ReadToEnd();
                        }
                        return true;
                    }
                }
            } catch (Exception ex) {                
                this.setError("Exception : " + ex.Message + " " + ex.StackTrace);
            }
            return false;
        }

        /// <summary>Fetches all supported Vendors available at handsetdetection.com</summary>
        /// <returns>true if successful, false otherwise</returns>
        public bool deviceVendors() {
            resetLog();            
            try {
                if (this.UseLocal)
                    return _localDeviceVendors();
                else
                    return Remote("/device/vendors.json", null);
            } catch (Exception ex) {
                this.setError("Exception : " + ex.Message + " " + ex.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool _localDeviceVendors() {
            Dictionary<string, object> data = _localGetSpecs();
            if (data == null)
                return false;
            // If _localGetSpecs bails return false here
            var temp = new HashSet<string>();
            foreach (Dictionary<string, object> item in (IEnumerable)data["devices"]) {
                temp.Add(((IDictionary)((IDictionary)item["Device"])["hd_specs"])["general_vendor"].ToString());
            }
            this.reply = new Dictionary<string, object>();
            this.reply["vendor"] = temp;
            this.reply["status"] = 0;
            this.reply["message"] = "OK";
            this.setRawReply();
            return true;
        }

        /// <summary>
        /// Fetches all available phone models in handsetdetection.com database. If a vendor is specified then
        /// only models for that vendor are returned. Call getModel() to get access to the returned list.
        /// </summary>
        /// <param name="vendor">all or a valid vendor name</param>
        /// <returns>true if successful, false otherwise</returns>
        public bool deviceModels(string vendor) {
            resetLog();
            try {
                if (this.UseLocal) {
                    return _localDeviceModels(vendor);
                } else {
                    return Remote("/device/models/" + vendor + ".json", null);
                }
            } catch (Exception ex) {
                this.setError("Exception : " + ex.Message + " " + ex.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vendor"></param>
        /// <returns></returns>
        private bool _localDeviceModels(string vendor) {
            Dictionary<string, object> data = _localGetSpecs();
            if (data == null)
                return false;
            HashSet<string> temp = new HashSet<string>();
            foreach (Dictionary<string, object> item in (IEnumerable) data["devices"]) {
                if (vendor == (((IDictionary)((IDictionary)item["Device"])["hd_specs"])["general_vendor"].ToString())) {
                    temp.Add(((IDictionary)((IDictionary)item["Device"])["hd_specs"])["general_model"].ToString());
                }
                string key = vendor + " ";
                if (((IDictionary)((IDictionary)item["Device"])["hd_specs"])["general_aliases"].ToString() != "") {
                    foreach (string alias_item in (IEnumerable)((IDictionary)((IDictionary)item["Device"])["hd_specs"])["general_aliases"]) {
                        int result = alias_item.IndexOf(key);
                        if (result == 0) {
                            temp.Add(alias_item.Replace(key, ""));
                        }
                    }
                }
            }
            this.reply = new Dictionary<string, object>();
            this.reply["model"] = temp;
            this.reply["status"] = 0;
            this.reply["message"] = "OK";
            this.setRawReply();
            return true;
        }


        /// <summary>
        /// Provides information on a handset given the vendor and model.
        /// </summary>
        /// <param name="vendor">vendor</param>
        /// <param name="model">model</param>
        /// <returns>true if successful, false otherwise</returns>
        public bool deviceView(string vendor, string model) {
            resetLog();
            try {
                if (this.UseLocal) {
                    return _localDeviceView(vendor, model);
                } else {
                    return Remote("/device/view/" + vendor + "/" + model + ".json", null);
                }
            } catch (Exception ex) {
                this.setError("Exception : " + ex.Message + " " + ex.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// Provides information on a handset given the vendor and model.
        /// </summary>
        /// <param name="vendor">vendor</param>
        /// <param name="model">model</param>
        /// <returns>true if successful, false otherwise</returns>
        private bool _localDeviceView(string vendor, string model) {
            Dictionary<string, object> data = _localGetSpecs();
            if (data == null)
                return false;
            vendor = vendor.ToLower();
            model = model.ToLower();
            foreach (Dictionary<string, object> item in (IEnumerable)data["devices"]) {
                if (vendor == (((IDictionary)((IDictionary)item["Device"])["hd_specs"])["general_vendor"].ToString().ToLower()) && model == ((IDictionary)((IDictionary)item["Device"])["hd_specs"])["general_model"].ToString().ToLower()) {
                    this.reply = new Dictionary<string, object>(); 
                    this.reply["device"] = ((IDictionary)item["Device"])["hd_specs"];
                    this.reply["status"] = 0;
                    this.reply["message"] = "OK";
                    this.setRawReply();
                    return true;
                }
            }
            this.reply = new Dictionary<string, object>();
            this.reply["status"] = 301;
            this.reply["message"] = "Nothing found";
            this.setRawReply();
            return false;
        }

        /// <summary>
        /// Provides information on a handset given the key and value.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <returns></returns>
        public bool deviceWhatHas(string key, string value) {
            resetLog();
            try {
                if (this.UseLocal) {
                    return _localDeviceWhatHas(key, value);
                } else {
                    return Remote("/device/whathas/" + key + "/" + value + ".json", null);
                }
            } catch (Exception ex) {
                this.setError("Exception : " + ex.Message + " " + ex.StackTrace);
                return false;
            }
        }


        /// <summary>
        /// Provides information on a what device given the key and value.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <returns></returns>
        private bool _localDeviceWhatHas(string key, string value) {
            Dictionary<string, object> data = this._localGetSpecs();
            if (data == null)
                return false;
            value = value.ToLower();
            key = key.ToLower();
            string s="";
            Type sType = s.GetType();
            var temp = new ArrayList();
            foreach (Dictionary<string, object> item in (IEnumerable)data["devices"])
            {
                if (((IDictionary)((IDictionary)item["Device"])["hd_specs"])[key].ToString() == "")
                    continue;                
                var match = false;
                if (((IDictionary)((IDictionary)item["Device"])["hd_specs"])[key].GetType() == sType) {
                    string check = ((IDictionary)((IDictionary)item["Device"])["hd_specs"])[key].ToString().ToLower();
                    if (check.IndexOf(value) >= 0)
                        match = true;
                } else {
                    foreach (string check in (IEnumerable)((IDictionary)((IDictionary)item["Device"])["hd_specs"])[key]) {
                        string tmpcheck = check.ToLower();
                        if (tmpcheck.IndexOf(value) >= 0)
                            match = true;
                    }                    
                }                
                if (match == true) {
                    Dictionary<string, string> sublist = new Dictionary<string, string>();
                    sublist.Add("id", ((IDictionary)item["Device"])["_id"].ToString());
                    sublist.Add("general_vendor", ((IDictionary)((IDictionary)item["Device"])["hd_specs"])["general_vendor"].ToString());
                    sublist.Add("general_model", ((IDictionary)((IDictionary)item["Device"])["hd_specs"])["general_model"].ToString());
                    temp.Add(sublist);
                }
            }
            this.reply = new Dictionary<string, object>();
            this.reply["device"] = temp;
            this.reply["status"] = 0;
            this.reply["message"] = "OK";
            this.setRawReply();
            return true;
        }


        /// <summary>
        /// site detect
        /// </summary>
        /// <returns>return site detect specs</returns>
        public bool siteDetect()
        {
            return siteDetect("hd_specs");
        }

        /// <summary>
        /// site detect
        /// </summary>
        /// <param name="options">options</param>
        /// <returns>return true if site detect specs</returns>
        public bool siteDetect(string options) {
            resetLog();
            if (this.m_detectRequest.ContainsKey("user-agent")) {                
                Regex reg = new Regex(NonMobile, RegexOptions.IgnoreCase);
                if (reg.IsMatch(this.m_detectRequest["user-agent"].ToString())) {
#if HD3_DEBUG
                    this._log("FastFail : Probable bot, sprider, script, or desktop");
#endif
                    this.reply = new Dictionary<string, object>();
                    this.reply["status"] = 301;
                    this.reply["message"] = "FastFail : Probable bot, sprider, script, or desktop";
                    this.setRawReply();
                    return false;
                } else {
#if HD3_DEBUG
                    this._log("No fastfail found");
#endif
                }
            } else {
#if HD3_DEBUG
                this._log("user-agent not set");
#endif
            }
 
            try {
                if (this.UseLocal) {                    
                    return _localSiteDetect(this.m_detectRequest);
                } else {
                    this.AddKey("options", options);
                    return Remote("/site/detect/" + this.SiteId + ".json", this.m_detectRequest);
                }
            } catch (Exception ex) {
                this.setError("Exception : " + ex.Message + " " + ex.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        private bool _localSiteDetect(Dictionary<string, string> headers) {            
            Dictionary<string, object> device = null;
            Dictionary<string, object> platform = null;
            Dictionary<string, object> browser = null;
            int id = _getDevice(headers);            
            if (id > 0) {
                device = _getCacheSpecs(id, "device");                
			    if (device == null) {
                    this.reply = new Dictionary<string, object>();
				    this.reply["status"] = 225;
				    this.reply["class"] = "Unknown";
				    this.reply["message"] = "Unable to write cache or main datafile.";
				    this.setError(this.reply["message"].ToString());
                    this.setRawReply();
				    return false;
			    }
                // Perform Browser & OS (platform) detection
			    int platform_id = _getExtra("platform", headers);
			    int browser_id = _getExtra("browser", headers);
			    if (platform_id > 0) 
				    platform = _getCacheSpecs(platform_id, "extra");
			    if (browser_id > 0)
				    browser = _getCacheSpecs(browser_id, "extra");				
			    // Selective merge
			    if (browser != null && browser.ContainsKey("general_browser")) {
				    platform["general_browser"] = browser["general_browser"];
				    platform["general_browser_version"] = browser["general_browser_version"];
			    }	
			    if (platform != null && platform.ContainsKey("general_platform")) {
				    device["general_platform"] = platform["general_platform"];
				    device["general_platform_version"] = platform["general_platform_version"];	
			    }
			    if (platform != null && platform.ContainsKey("general_browser")) {
				    device["general_browser"] = platform["general_browser"];
				    device["general_browser_version"] = platform["general_browser_version"];	
			    }
                if (!device.ContainsKey("general_browser")) {
                    device["general_browser"] = "";
                    device["general_browser_version"] = "";
                }
                if (!device.ContainsKey("general_platform")) {
                    device["general_platform"] = "";
                    device["general_platform_version"] = "";
                }
                var jss = new JavaScriptSerializer();
                jss.MaxJsonLength = this.maxJsonLength;
#if HD3_DEBUG
                this._log(jss.Serialize(device));
#endif
                this.reply = new Dictionary<string, object>();								
			    this.reply["hd_specs"] = device;
			    this.reply["status"] = 0;
			    this.reply["message"] = "OK";
			    this.reply["class"] = (device["general_type"].ToString() == "" ? "Unknown" : device["general_type"]);
                this.setRawReply();
			    return true;
		    }
		    if (this.reply == null || ! this.reply.ContainsKey("status")) {
                this.reply = new Dictionary<string, object>();
			    this.reply["status"] = 301;
			    this.reply["class"] = "Unknown";
			    this.reply["message"] = "Nothing found";
			    this.setError("Error: 301, Nothing Found");
                this.setRawReply();
		    }
		    return false;
	    }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        private int _getDevice(Dictionary<string, string> headers) {            
            int id;
		    // Remember the agent for generic matching later.
		    string genericAgent = "";
            if (headers.ContainsKey("user-agent"))
                genericAgent = headers["user-agent"];
#if HD3_DEBUG
		    this._log("Working with headers of " + _helperDictToString(headers));
		    this._log("Start Checking Opera Special headers");
#endif
            // Opera mini puts the vendor # model in the header - nice! ... sometimes it puts ? # ? in as well :(
		    if (headers.ContainsKey("x-operamini-phone") && headers["x-operamini-phone"].ToString() != "? # ?") {
			    id = this._tryHeader(ref headers, "x-operamini-phone", "x-operamini-phone");
			    if (id > 0)
				    return id;
		    }
             // Profile header matching
            id = this._tryHeader(ref headers, "profile", "profile");
            if (id > 0) return id;
			id = this._tryHeader(ref headers, "profile", "x-wap-profile");
			if (id > 0) return id;
            // Various types of user-agent x-header matching, order is important here (for the first 3).
            id = this._tryHeader(ref headers, "user-agent", "x-operamini-phone-ua");
            if (id > 0) return id;
            id = this._tryHeader(ref headers, "user-agent", "x-mobile-ua");
            if (id > 0) return id;
            id = this._tryHeader(ref headers, "user-agent", "user-agent");
            if (id > 0) return id;
            // Try anything else thats left		
		    foreach(KeyValuePair <string, string> item in headers) {
                if (item.Value != null && item.Key != null && item.Key.Length > 2 && item.Key[0].Equals('x')) {
                    id = this._tryHeader(ref headers, "user-agent", item.Key);
                    if (id > 0) return id;
                }
		    }

		    // Generic matching - Match of last resort.
#if HD3_DEBUG
            this._log("Trying Generic Match");
#endif                        
            return this._matchDevice("user-agent", genericAgent, true);
	    }
        
        private int _tryHeader(ref Dictionary<string, string> headers, string field, string httpheader) {
            int id;
#if HD3_DEBUG
            this._log("Start device "+field+"/"+httpheader+" check against "+field);
#endif
            if (headers.ContainsKey(httpheader) && headers[httpheader] != null) {
			    id = this._matchDevice(field, headers[httpheader], false);
			    if (id > 0) {
#if HD3_DEBUG
                    this._log("End "+httpheader+" check against "+field+" Found");
#endif
                    return id;
			    }
			    headers.Remove(field);
		    }
#if HD3_DEBUG
            this._log("End device " + httpheader + " check against " + field + " Not Found");
#endif
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="value"></param>
        /// <param name="generic"></param>
        /// <returns></returns>
        private int _matchDevice(string header, string value, bool generic) {
		    // Strip unwanted chars from lower case version of value
            StringBuilder b = new StringBuilder(value.ToLower());
            foreach(char c in MatchFilter) {
                b.Replace(c.ToString(), string.Empty);
            }
            value = b.ToString();
            string treetag;
            if (generic == true)
                treetag = header+"1";
            else
                treetag = header+"0";            
            return this._match(header, value, treetag);
	    }   

	    // Tries headers in diffferent orders depending on the extra $class.
	    private int _getExtra(string extraclass, Dictionary <string, string> headers) {
            int id;
		    if (extraclass == "platform") {
                id = this._tryExtra(ref headers, "user-agent", "x-operamini-phone-ua", extraclass);
                if (id > 0) return id;
                id = this._tryExtra(ref headers, "user-agent", "user-agent", extraclass);
                if (id > 0) return id;
                // Try anything else thats left		
		        foreach(KeyValuePair <string, string> item in headers) {
                    if (item.Value != null) {
                        id = this._tryExtra(ref headers, "user-agent", item.Value, extraclass);
                        if (id > 0) return id;
                    }
		        }
		    } else if (extraclass == "browser") {
                id = this._tryExtra(ref headers, "user-agent", "user-agent", extraclass);
                if (id > 0) return id;
                // Try anything else thats left		
		        foreach(KeyValuePair <string, string> item in headers) {
                    if (item.Value != null) {
                        id = this._tryExtra(ref headers, "user-agent", item.Value, extraclass);
                        if (id > 0) return id;
                    }
		        }
		    }
            return 0;
	    }
		
	    private int _tryExtra(ref Dictionary <string, string> headers, string matchfield, string httpheader, string extraclass) {
            int id;
#if HD3_DEBUG
            this._log("Start Extra "+matchfield+"/"+httpheader+" check");
#endif
            if (headers.ContainsKey(httpheader)) {
                string value = headers[httpheader].ToLower().Replace(" ","");
                string treetag = matchfield + extraclass;
			    id = this._match(httpheader, value, treetag);
			    if (id > 0) {
#if HD3_DEBUG
                    this._log("End "+matchfield+" check. Found");
#endif
                    return id;
			    }
			    headers.Remove(matchfield);
		    }
		    // this._log("End Extra "+matchfield+"check - not found");
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="newvalue"></param>
        /// <param name="treetag"></param>
        /// <returns></returns>
        private int _match(string header, string newvalue, string treetag) {            
		
		    int f = 0,r = 0;		
#if HD3_DEBUG
            this._log("Loading "+treetag+" match "+newvalue); 
#endif
		    if (newvalue == "") {
#if HD3_DEBUG
                this._log("Value empty - returning false");
#endif
                return 0;
		    }
		
		    if (newvalue.Length < 4) {
#if HD3_DEBUG
                this._log("Value " +newvalue+ " too small - returning false");
#endif
                return 0;
		    }

#if HD3_DEBUG
		    this._log("Loading match branch "+treetag);
#endif            
            Dictionary<string, object> branch = this._getBranch(treetag);                  
		    if (branch == null) {
#if HD3_DEBUG
                this._log("Match branch "+treetag+" empty - returning false");
#endif                
                return 0;
		    }
#if HD3_DEBUG
		    this._log("Match branch loaded");		
#endif                                               
		    if (header == "user-agent") {                
			    // Sieve matching strategy
                foreach (KeyValuePair<string, object> orders in branch)
                {                    
#if HD3_DEBUG
                    this._log("OK using order " + orders.Key + " in " + newvalue);
#endif                    
                    foreach (KeyValuePair<string, object> filters in (IEnumerable)orders.Value)
                    {
					    f++;
#if HD3_DEBUG
                        this._log("FK Looking for "+filters.Key+" in "+newvalue);
#endif                        
					    if (newvalue.IndexOf(filters.Key) >= 0) {
                            foreach (KeyValuePair<string, object> matches in (IEnumerable)filters.Value)
                            {
                                r++;
#if HD3_DEBUG
                                this._log("MK Looking for " + matches.Key + " in "+newvalue);
#endif
							    if (newvalue.IndexOf(matches.Key) >= 0) {
#if HD3_DEBUG
                                    this._log("Match Found : "+filters.Key+ "/"+matches.Key+"/" +matches.Value+" wins on "+newvalue+" ("+f+"/"+r+")");
#endif                                    
                                    return Convert.ToInt32(matches.Value.ToString());
							    }
						    }
					    }
				    }
			    }
		    } else {                
			    // Direct matching strategy
                try {
                    int id = Convert.ToInt32(branch[newvalue]);
#if HD3_DEBUG
                    this._log("Match found : " + treetag + " " + newvalue + " (" + f + "/" + r + ")");
#endif                    
                    return id;
                } catch (Exception ex) {
                }
		    }
		
#if HD3_DEBUG
            this._log("No Match Found for "+treetag+" "+newvalue+"("+f+"/"+r+")");
#endif
            return 0;
	    }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="treetag"></param>
        /// <returns></returns>
        private Dictionary<string, object> _getBranch(string treetag)
        {            
		    // See if its in the class
            if (this.tree.ContainsKey(treetag)) {
#if HD3_DEBUG
                this._log(treetag + " fetched from memory");
#endif                
                return (Dictionary<string, object>) this.tree[treetag];
		    }            
            // Not in class - try Cache.
            Dictionary<string, object> obj = myCache.read(treetag);            
            if (obj != null && obj.Count != 0) {                
#if HD3_DEBUG
                this._log(treetag + " fetched from cache. count : "+obj.Count);
#endif
                this.tree[treetag] = obj;
                return obj;
            }            
            // Its in neither - so populate both.            
            this._setCachecArchives();
            
            // If it doesnt exist after immediate refresh then something is wrong.
		    if (! this.tree.ContainsKey(treetag))
                this.tree[treetag] = new Dictionary<string, object>();
      
#if HD3_DEBUG
            this._log(treetag + " built and cached");
#endif
            return (Dictionary<string, object>) this.tree[treetag];
	    }

        #endregion
      
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool siteFetchArchive()
        {
            resetLog();
            bool status = this.Remote("/site/fetcharchive/" + this.SiteId + ".json", null);            
            if (!status)
                return false;
            try {
                if (!this.reply.ContainsKey("status") || (int)this.reply["status"] != 0) {
                    this.setError("siteFetchArchive API call failed: " + this.reply["message"].ToString());
                    return false;
                }
                // Write rawreply to file hd3specs.json file.
                _localPutSpecs();
            } catch (Exception ex) {
                this.setError("Exception : " + ex.Message + " " + ex.StackTrace);
                return false;
            }
            return true; // _setCachecArchives();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool _setCachecArchives()
        {
            //Dictionary<string, object> data = _localGetSpecs();
            var jss = new JavaScriptSerializer();
            jss.MaxJsonLength = this.maxJsonLength;
            string[] fileNames = Directory.GetFiles(FileDirectory, "Device_*.json");
            if (fileNames.Length == 0)
            {
                this.reply = new Dictionary<string, object>();
                this.reply["status"] = 299;
                this.reply["message"] = "Unable to open file devices";
                this.setError("Error : 299, Message : _setCachecArchives cannot open files. Is it there ? Is it world readable ?");
                this.setRawReply();
                return false;
            }
            // Cache Devices
            /*foreach (string fileName in fileNames)
            {
                string contents = System.IO.File.ReadAllText(fileName);
                Dictionary<string, object> device = jss.Deserialize<Dictionary<string, dynamic>>(contents);
                string device_id = device["Device"]["_id"].ToString();
                string key = "Device_" + device_id;
                if (device != null && device["Device"] != null && device["Device"]["hd_specs"] != null && key != null)
                {
                    this.devices[key] = device["Device"]["hd_specs"];
                    myCache.write(key, this.devices[key]);
                }        
            }*/
            /*foreach (Dictionary<string, object> device in (IEnumerable)data["devices"]) {
                string device_id = ((IDictionary)device["Device"])["_id"].ToString();
                string key = "device" + device_id;
                if (device != null && device["Device"] != null && ((IDictionary)device["Device"])["hd_specs"] != null && key != null)
                {
                    this.specs[key] = ((IDictionary)device["Device"])["hd_specs"];
                    // Save to Application Cache
                    myCache.write(key, (Dictionary<string, object>)this.specs[key]);
                }
            }
            // Cache Extras
            foreach (Dictionary<string, object> extra in (IEnumerable)data["extras"]) {
                string extra_id = ((IDictionary)extra["Extra"])["_id"].ToString();
                string key = "extra" + extra_id;
                if (extra["Extra"] != null && ((IDictionary)extra["Extra"])["hd_specs"] != null)
                {
                    this.specs[key] = ((IDictionary)extra["Extra"])["hd_specs"] as Dictionary<string, object>;
                    // Save to Applications Cache
                    myCache.write(key, (Dictionary<string, object>)this.specs[key]);
                }
            }*/
            return true;
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private Dictionary<string, object> _getCacheSpecs(int id, string type)
        {
            // Read local first
            string key = type + Convert.ToInt32(id);
            if (this.specs.ContainsKey(key)) {
#if HD3_DEBUG
                this._log(key + " fetched from memory");
#endif
                return (Dictionary<string, object>) this.specs[key];
            }

            // Try Cache
            Dictionary<string, object> obj = myCache.read(key);
            if (obj != null && obj.Count != 0) {
#if HD3_DEBUG
                this._log(key + " fetched from cache");
#endif
                this.specs[key] = obj;
                return obj;
            }

            // re-cache & re-read local.
#if HD3_DEBUG
            this._log(key + " not found - rebuilding");
#endif            
            this._setCachecArchives();
            if (this.specs.ContainsKey(key)) {
#if HD3_DEBUG
                this._log(key + " found after rebuilding");
#endif
                return (Dictionary<string, object>) this.specs[key];
            }
#if HD3_DEBUG
            this._log(key + " not found");
#endif
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, object> _localGetSpecs()
        {            
            var jss = new JavaScriptSerializer();
            jss.MaxJsonLength = this.maxJsonLength;            
            try {
                string jsonText = System.IO.File.ReadAllText(FileDirectory + "\\hd3specs.json");
                Dictionary<string, object> data = jss.Deserialize<Dictionary<string, object>>(jsonText);
                return data;
            } catch (Exception ex) {
                this.setError("Exception : " + ex.Message + " " + ex.StackTrace);
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool siteFetchTrees()
        {
            resetLog();
            this.isDownloadableFiles = true;
            bool status = this.Remote("/site/fetchtrees/" + this.SiteId + ".json", null);
            if (!status)
                return false;
            try
            {
                if (!this.reply.ContainsKey("status") || (int)this.reply["status"] != 0)
                {
                    this.setError("siteFetchSpecs API call failed: " + this.reply["message"].ToString());
                    return false;
                }
                // Write rawreply to file hd3trees.json file.
                _localPutTrees();
            }
            catch (Exception ex)
            {
                this.setError("Exception : " + ex.Message + " " + ex.StackTrace);
                return false;
            }
            return _setCacheTrees();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool _setCacheTrees()
        {
            Dictionary<string, object> data = _localGetTrees();
            if (data == null || !data.ContainsKey("trees"))
            {
                this.reply = new Dictionary<string, object>();
                this.reply["status"] = 299;
                this.reply["message"] = "Unable to open specs file hd3trees.json";
                this.setError("Error : 299, Message : _setCacheTrees cannot open hd3trees.json. Is it there ? Is it world readable ?");
                this.setRawReply();
                return false;
            }
            foreach (KeyValuePair<string, object> branch in (IEnumerable)data["trees"])
            {
                this.tree[branch.Key] = branch.Value as Dictionary<string, object>;
                // Write to memory cache
                myCache.write(branch.Key, (Dictionary<string, object>)this.tree[branch.Key]);
            }
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool siteFetchSpecs()
        {
            resetLog();
            bool status = this.Remote("/site/fetchspecs/" + this.SiteId + ".json", null);
            if (!status)
                return false;

            try
            {
                if (!this.reply.ContainsKey("status") || (int)this.reply["status"] != 0)
                {
                    this.setError("siteFetchSpecs API call failed: " + this.reply["message"].ToString());
                    return false;
                }
                // Write rawreply to file hd3specs.json file.
                _localPutSpecs();
            }
            catch (Exception ex)
            {
                this.setError("Exception : " + ex.Message + " " + ex.StackTrace);
                return false;
            }
            return _setCacheSpecs();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool _setCacheSpecs()
        {
            Dictionary<string, object> data = _localGetSpecs();
            if (data == null)
            {
                this.reply = new Dictionary<string, object>();
                this.reply["status"] = 299;
                this.reply["message"] = "Unable to open specs file hd3specs.json";
                this.setError("Error : 299, Message : _setCacheSpecs cannot open hd3specs.json. Is it there ? Is it world readable ?");
                this.setRawReply();
                return false;
            }
            // Cache Devices
            foreach (Dictionary<string, object> device in (IEnumerable)data["devices"])
            {
                string device_id = ((IDictionary)device["Device"])["_id"].ToString();
                string key = "device" + device_id;
                if (device != null && device["Device"] != null && ((IDictionary)device["Device"])["hd_specs"] != null && key != null)
                {
                    this.specs[key] = ((IDictionary)device["Device"])["hd_specs"];
                    // Save to Application Cache
                    myCache.write(key, (Dictionary<string, object>)this.specs[key]);
                }
            }
            // Cache Extras
            foreach (Dictionary<string, object> extra in (IEnumerable)data["extras"])
            {
                string extra_id = ((IDictionary)extra["Extra"])["_id"].ToString();
                string key = "extra" + extra_id;
                if (extra["Extra"] != null && ((IDictionary)extra["Extra"])["hd_specs"] != null)
                {
                    this.specs[key] = ((IDictionary)extra["Extra"])["hd_specs"] as Dictionary<string, object>;
                    // Save to Applications Cache
                    myCache.write(key, (Dictionary<string, object>)this.specs[key]);
                }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, object> _localGetTrees()
        {
            var jss = new JavaScriptSerializer();
            jss.MaxJsonLength = this.maxJsonLength;
            try {
                string jsonText = System.IO.File.ReadAllText(FileDirectory + @"\\hd3trees.json");
                Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonText); //jss.Deserialize<Dictionary<string, object>>(jsonText);
                return data;
            } catch (Exception ex) {
                this.setError("Exception : " + ex.Message + " " + ex.StackTrace);
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool _localPutSpecs() {            
            try {                
                System.IO.File.WriteAllText(Request.PhysicalApplicationPath + "\\hd3specs.json", this.rawreply.ToString());
                return true;
            } catch (Exception ex) {
                this.setError("Exception : " + ex.Message + " " + ex.StackTrace);
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool _localPutTrees() {
            try {
                if (!Directory.Exists(FileDirectory)) {
                    System.IO.Directory.CreateDirectory(FileDirectory);
                }                 
                BinaryWriter bw = new BinaryWriter(new FileStream(FileDirectory + @"\\hd3trees.json", FileMode.Create), Encoding.UTF8);
                byte[] buff = new byte[1024];
                int c = 1;
                while (c > 0) {
                    c = this.reader.Read(buff, 0, 1024);
                    for (int i = 0; i < c; i++)
                        bw.Write(buff[i]);
                }
                bw.Close();
                responseStream.Close();                
                return true;
            } catch (Exception ex) {
                this.setError("Exception : " + ex.Message + " " + ex.StackTrace);
            }
            return false;
        }

        // From : http://www.dotnetperls.com/convert-dictionary-string
        private string _helperDictToString(Dictionary<string, string> d) {
	        // Build up each line one-by-one and them trim the end
	        StringBuilder builder = new StringBuilder();
	        foreach (KeyValuePair<string, string> pair in d) {
	            builder.Append(pair.Key).Append(":").Append(pair.Value).Append(',');
	        }
	        string result = builder.ToString();
	        // Remove the final delimiter
	        result = result.TrimEnd(',');
	        return result;
        }

        // From : http://blogs.msdn.com/b/csharpfaq/archive/2006/10/09/how-do-i-calculate-a-md5-hash-from-a-string_3f00_.aspx
        private string _helperMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);
 
            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++) {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString().ToLower();
        }
    }       
}