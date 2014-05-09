using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HD3;
using HD3.Test;
using System.Diagnostics;
using System.IO;
using System.Configuration;
using System.Reflection;
using System.Xml;
using System.Collections;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace HD3Test
{    
    [TestClass]    
    public class HD3Test
    {
        private HD3.HD3 hd3;
        private SecretConfig secretConfig;

        [TestInitialize]
        public void Initialize()
        {
            hd3 = new HD3.HD3();
            secretConfig = new SecretConfig();
        }

        [TestMethod]
        public void Test_HD3WrongCredentials()
        {
            Assert.AreEqual<string>(hd3.Username, "your_api_username");
            Assert.AreEqual<string>(hd3.Secret, "your_api_secret");
            Assert.AreEqual<string>(hd3.SiteId, "your_api_siteId");
        }

        [TestMethod]
        public void Test_HD3CorrectCredentials()
        {
            Assert.AreEqual<string>(hd3.Username,
                secretConfig.GetConfigUsername());
            Assert.AreEqual<string>(hd3.Secret,
                secretConfig.GetConfigSecret());
            Assert.AreEqual<string>(hd3.SiteId,
                secretConfig.GetConfigSiteId());
        }

        [TestMethod]
        public void Test_SiteDetect()
        {
            Assert.IsFalse(hd3.siteDetect());
        }

        [TestMethod]
        public void Test_NokiaSiteDetect() {
		    hd3.setDetectVar("user-agent","Mozilla/5.0 (SymbianOS/9.2; U; Series60/3.1 NokiaN95-3/20.2.011 Profile/MIDP-2.0 Configuration/CLDC-1.1 ) AppleWebKit/413");
		    hd3.setDetectVar("x-wap-profile","http://nds1.nds.nokia.com/uaprof/NN95-1r100.xml");
		    hd3.siteDetect();
            string reply = hd3.getRawReply();
            JObject json = JObject.Parse(reply);
		    Assert.AreEqual("Nokia", json["hd_specs"]["general_vendor"]);
		    Assert.AreEqual("Symbian", json["hd_specs"]["general_platform"]);
	    }

        [TestMethod]
        public void Test_GeoipSiteDetect() {
		    hd3.setDetectVar("ipaddress","64.34.165.180");            
            Hashtable openWith = new Hashtable();
            openWith.Add("options", "geoip,hd_specs");
            hd3.siteDetect(openWith["options"].ToString());
		    string reply = hd3.getRawReply();
            JObject json = JObject.Parse(reply);
		    Assert.AreEqual("38.9266", json["geoip"]["latitude"]);
		    Assert.AreEqual("US", json["geoip"]["countrycode"]);
	    }
      
        [TestMethod]
        public void Test_SiteDetectLocal()
        {
            Assert.IsTrue(hd3.siteDetect());
        }

        [TestMethod]
        public void Test_DeviceVendorsWithWrongUsername()
        {
            Assert.AreEqual(hd3.Username, "your_api_username");
            Assert.IsFalse(hd3.deviceVendors());
        }

        [TestMethod]
        public void Test_DeviceVendorsWithCorrectUsername()
        {
            Assert.AreEqual(hd3.Username, secretConfig.GetConfigUsername());
            Assert.IsTrue(hd3.deviceVendors());
        }

        [TestMethod]
        public void Test_SamsungDeviceVendors() {
		    hd3.deviceVendors();
		    string reply = hd3.getRawReply();
            JObject json = JObject.Parse(reply);		    
            Assert.IsTrue(json["vendor"].Equals("Samsung"));
	    }

        [TestMethod]
        public void Test_DeviceModelsNokia()
        {
            hd3.deviceModels("Nokia");
            Assert.IsTrue(hd3.getRawReply().Contains("model"));
        }

        [TestMethod]
        public void Test_DeviceModelsLorem()
        {
            hd3.deviceModels("Lorem");
            Assert.IsFalse(hd3.getRawReply().Contains("model"));
        }

        [TestMethod]
        public void Test_DeviceViewNokia95()
        {
            Assert.IsTrue(hd3.deviceView("Nokia", "N95"));
            string reply = hd3.getRawReply();
            JObject json = JObject.Parse(reply);
            Assert.AreEqual(json["device"]["general_vendor"], "Nokia");
            Assert.AreEqual(json["device"]["general_model"], "N95");
            Assert.AreEqual(json["device"]["general_platform"], "Symbian");
        }

        [TestMethod]
        public void Test_DeviceViewXCode()
        {
            Assert.IsFalse(hd3.deviceView("XCode", "XC14"));
            string reply = hd3.getRawReply();     
            JObject json = JObject.Parse(reply);      
            Assert.AreEqual(json["device"]["general_vendor"], "Apple");
            Assert.AreEqual(json["device"]["general_model"], "XC14");
            Assert.AreEqual(json["device"]["general_platform"], "iOS");
        }

        [TestMethod]
        public void Test_DeviceWhatHasTrue()
        {
            hd3.ReadTimeout = 600;
            Assert.IsTrue(hd3.deviceWhatHas("network", "cdma"));
            string reply = hd3.getRawReply();
            JObject json = JObject.Parse(reply);               
            Assert.AreEqual("SCP-550CN", json["devices"][5]["general_model"]);
        }

        [TestMethod]
        public void Test_DeviceWhatHasFalse()
        {
            Assert.IsFalse(hd3.deviceWhatHas("cloud", "wifi"));
        } 
    }   
}

