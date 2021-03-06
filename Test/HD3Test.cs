﻿using HD3;
using System;
using System.Diagnostics;
using System.IO;
using System.Configuration;
using System.Reflection;
using System.Xml;
using System.Collections;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Web;
using NUnit.Framework;

namespace HD3Test
{    
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]        
    public class HD3Test
    {
        private HD3.HD3 hd3;

        private List<string> notFoundHeaders = new List<string>(
               new string[] { "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.0; Trident/4.0; GTB7.1; SLCC1; .NET CLR 2.0.50727; Media Center PC 5.0; InfoPath.2; .NET CLR 3.5.30729; .NET4.0C; .NET CLR 3.0.30729; AskTbFWV5/5.12.2.16749; 978803803",
                              "Mozilla/5.0 (Windows; U; Windows NT 5.1; fr; rv:1.9.2.22) Gecko/20110902 Firefox/3.6.22 ( .NET CLR 3.5.30729) Swapper 1.0.4",
                              "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 5.1; Trident/4.0; Sky Broadband; GTB7.1; SeekmoToolbar 4.8.4; Sky Broadband; Sky Broadband; AskTbBLPV5/5.9.1.14019)" 
               });

        private Dictionary<string, string> h1 = new Dictionary<string, string>() { 
            { "user-agent", "Mozilla/5.0 (Linux; U; Android 2.2.2; en-us; SCH-M828C[3373773858] Build/FROYO) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1" },
            { "x-wap-profile", "http://www-ccpp.tcl-ta.com/files/ALCATEL_one_touch_908.xml" },
            { "match", "AlcatelOT-908222" }
        };

        private Dictionary<string, string> h2 = new Dictionary<string, string>() {
		    { "user-agent","Mozilla/5.0 (Linux; U; Android 2.2.2; en-us; SCH-M828C[3373773858] Build/FROYO) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1" },
		    { "match","SamsungSCH-M828C" }
	    };

        private Dictionary<string, string> h3 = new Dictionary<string, string>() {
		    { "x-wap-profile","http://www-ccpp.tcl-ta.com/files/ALCATEL_one_touch_908.xml" },
		    { "match","AlcatelOT-90822" }
	    };

        private Dictionary<string, string> h4 = new Dictionary<string, string>() {
		    { "user-agent","Mozilla/5.0 (Linux; U; Android 2.3.3; es-es; GT-P1000N Build/GINGERBREAD) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1" },
		    { "x-wap-profile","http://wap.samsungmobile.com/uaprof/GT-P1000.xml" },
		    { "match","SamsungGT-P1000" }
	    };

        private Dictionary<string, string> h5 = new Dictionary<string, string>() {
		    { "user-agent","Opera/9.80 (J2ME/MIDP; Opera Mini/5.21076/26.984; U; en) Presto/2.8.119 Version/10.54" },
		    { "match","GenericOperaMini" }
	    };

        private Dictionary<string, string> h6 = new Dictionary<string, string>() {
		    { "user-agent","Opera/9.80 (iPhone; Opera Mini/6.1.15738/26.984; U; tr) Presto/2.8.119 Version/10.54" },
		    { "match","AppleiPhone" }
	    };

        private Dictionary<string, string> h7 = new Dictionary<string, string>() {
		    { "user-agent","Mozilla/5.0 (Linux; U; Android 2.1-update1; cs-cz; SonyEricssonX10i Build/2.1.B.0.1) AppleWebKit/530.17 (KHTML, like Gecko) Version/4.0 Mobile Safari/530.17" },
		    { "match","SonyEricssonX10I" }
	    };

        private Dictionary<string, Dictionary<string, string>> map;

        /// <summary>
        /// 
        /// </summary>
        public string nokiaN95 = "{\"general_vendor\":\"Nokia\","
            + "\"general_model\":\"N95\","
            + "\"general_platform\":\"Symbian\","
            + "\"general_platform_version\":\"9.2\","
            + "\"general_browser\":\"\","
            + "\"general_browser_version\":\"\","
            + "\"general_image\":\"nokian95-1403496370-0.gif\","
            + "\"general_aliases\":[],"
            + "\"general_eusar\":\"0.50\","
            + "\"general_battery\":[\"Li-Ion 950 mAh\",\"BL-5F\"],"
            + "\"general_type\":\"Mobile\","
            + "\"general_cpu\":[\"Dual ARM 11\",\"332Mhz\"],"
            + "\"design_formfactor\":\"Dual Slide\","
            + "\"design_dimensions\":\"99 x 53 x 21\","
            + "\"design_weight\":\"120\","
            + "\"design_antenna\":\"Internal\","
            + "\"design_keyboard\":\"Numeric\","
            + "\"design_softkeys\":\"2\","
            + "\"design_sidekeys\":[\"Volume\",\"Camera\"],"
            + "\"display_type\":\"TFT\","
            + "\"display_color\":\"Yes\","
            + "\"display_colors\":\"16M\","
            + "\"display_size\":\"2.6\\\"\","
            + "\"display_x\":\"240\","
            + "\"display_y\":\"320\","
            + "\"display_other\":[],"
            + "\"memory_internal\":[\"160MB\",\"64MB RAM\",\"256MB ROM\"],"
            + "\"memory_slot\":[\"microSD\",\"8GB\",\"128MB\"],"
            + "\"network\":[\"GSM850\",\"GSM900\",\"GSM1800\",\"GSM1900\",\"UMTS2100\",\"HSDPA2100\",\"Infrared port\",\"Bluetooth 2.0\",\"802.11b\",\"802.11g\",\"GPRS Class 10\",\"EDGE Class 32\"],"
            + "\"media_camera\":[\"5MP\",\"2592x1944\"],"
            + "\"media_secondcamera\":[\"QVGA\"],"
            + "\"media_videocapture\":[\"VGA@30fps\"],"
            + "\"media_videoplayback\":[\"MPEG4\",\"H.263\",\"H.264\",\"3GPP\",\"RealVideo 8\",\"RealVideo 9\",\"RealVideo 10\"],"
            + "\"media_audio\":[\"MP3\",\"AAC\",\"AAC+\",\"eAAC+\",\"WMA\"],"
            + "\"media_other\":[\"Auto focus\",\"Video stabilizer\",\"Video calling\",\"Carl Zeiss optics\",\"LED Flash\"],"
            + "\"features\":[\"Unlimited entries\",\"Multiple numbers per contact\",\"Picture ID\",\"Ring ID\",\"Calendar\",\"Alarm\",\"To-Do\",\"Document viewer\",\"Calculator\",\"Notes\",\"UPnP\",\"Computer sync\",\"VoIP\",\"Music ringtones (MP3)\",\"Vibration\",\"Phone profiles\",\"Speakerphone\",\"Accelerometer\",\"Voice dialing\",\"Voice commands\",\"Voice recording\",\"Push-to-Talk\",\"SMS\",\"MMS\",\"Email\",\"Instant Messaging\",\"Stereo FM radio\",\"Visual radio\",\"Dual slide design\",\"Organizer\",\"Word viewer\",\"Excel viewer\",\"PowerPoint viewer\",\"PDF viewer\",\"Predictive text input\",\"Push to talk\",\"Voice memo\",\"Games\"],"
            + "\"connectors\":[\"USB\",\"miniUSB\",\"3.5mm Headphone\",\"TV Out\"]}";

        private static string AlcatelOT_908222 = "{\"general_vendor\":\"Alcatel\","
            + "\"general_model\":\"OT-908\","
            + "\"general_platform\":\"Android\","
            + "\"general_platform_version\":\"2.2.2\","
            + "\"general_browser\":\"Android Webkit\","
            + "\"general_browser_version\":\"4.0\","
            + "\"general_image\":\"\","
            + "\"general_aliases\":[\"Alcatel One Touch 908\"],"
            + "\"general_eusar\":\"\","
            + "\"general_battery\":[\"Li-Ion 1300 mAh\"],"
            + "\"general_type\":\"Mobile\","
            + "\"general_cpu\":[\"600Mhz\"],"
            + "\"design_formfactor\":\"Bar\","
            + "\"design_dimensions\":\"110 x 57.4 x 12.4\","
            + "\"design_weight\":\"120\","
            + "\"design_antenna\":\"Internal\","
            + "\"design_keyboard\":\"Screen\","
            + "\"design_softkeys\":\"\","
            + "\"design_sidekeys\":[\"Lock/Unlock\",\"Volume\"],"
            + "\"display_type\":\"TFT\","
            + "\"display_color\":\"Yes\","
            + "\"display_colors\":\"262K\","
            + "\"display_size\":\"2.8\\\"\","
            + "\"display_x\":\"240\","
            + "\"display_y\":\"320\","
            + "\"display_other\":[\"Capacitive\",\"Touch\",\"Multitouch\"],"
            + "\"memory_internal\":[\"150MB\"],"
            + "\"memory_slot\":[\"microSD\",\"microSDHC\",\"32GB\",\"2GB\"],"
            + "\"network\":[\"GSM850\",\"GSM900\",\"GSM1800\",\"GSM1900\",\"UMTS900\",\"UMTS2100\",\"HSDPA900\",\"HSDPA2100\",\"Bluetooth 3.0\",\"802.11b\",\"802.11g\",\"802.11n\",\"GPRS Class 12\",\"EDGE Class 12\"],"
            + "\"media_camera\":[\"2MP\",\"1600x1200\"],"
            + "\"media_secondcamera\":[],"
            + "\"media_videocapture\":[\"Yes\"],"
            + "\"media_videoplayback\":[\"MPEG4\",\"H.263\",\"H.264\"],"
            + "\"media_audio\":[\"MP3\",\"AAC\",\"AAC+\",\"WMA\"],"
            + "\"media_other\":[\"Geo-tagging\"],"
            + "\"features\":[\"Unlimited entries\",\"Caller groups\",\"Multiple numbers per contact\",\"Search by both first and last name\",\"Picture ID\",\"Ring ID\",\"Calendar\",\"Alarm\",\"Calculator\",\"Computer sync\",\"OTA sync\",\"Music ringtones (MP3)\",\"Polyphonic ringtones (64 voices)\",\"Vibration\",\"Flight mode\",\"Silent mode\",\"Speakerphone\",\"Accelerometer\",\"Compass\",\"Voice recording\",\"SMS\",\"MMS\",\"Email\",\"Push Email\",\"IM\",\"Stereo FM radio with RDS\",\"SNS integration\",\"Google Search\",\"Maps\",\"Gmail\",\"YouTube\",\"Google Talk\",\"Picasa integration\",\"Organizer\",\"Document viewer\",\"Voice memo\",\"Voice dialing\",\"Predictive text input\",\"Games\"],"
            + "\"connectors\":[\"USB 2.0\",\"microUSB\",\"3.5mm Headphone\"]}";

        private static string SamsungSCH_M828C = "{\"general_vendor\":\"Samsung\","
            + "\"general_model\":\"SCH-M828C'\","
            + "\"general_platform\":\"Android\","
            + "\"general_platform_version\":\"2.2.2\","
            + "\"general_browser\":\"Android Webkit\","
            + "\"general_browser_version\":\"4.0\","
            + "\"general_image\":\"samsungsch-m828c-1355919519-0.jpg\","
            + "\"general_aliases\":[\"Samsung Galaxy Prevail\", \"Samsung Galaxy Precedent\"],"
            + "\"general_eusar\":\"\","
            + "\"general_battery\":[\"Li-Ion 1500 mAh\"],"
            + "\"general_type\":\"Mobile\","
            + "\"general_cpu\":[\"800Mhz\"],"
            + "\"design_formfactor\":\"Bar\","
            + "\"design_dimensions\":\"113 x 57 x 12\","
            + "\"design_weight\":\"108\","
            + "\"design_antenna\":\"Internal\","
            + "\"design_keyboard\":\"Screen\","
            + "\"design_softkeys\":\"\","
            + "\"design_sidekeys\":[],"
            + "\"display_type\":\"TFT\","
            + "\"display_color\":\"Yes\","
            + "\"display_colors\":\"262K\","
            + "\"display_size\":\"3.2\\\"\","
            + "\"display_x\":\"320\","
            + "\"display_y\":\"480\","
            + "\"display_other\":[\"Capacitive\",\"Touch\",\"Multitouch\", \"Touch Buttons\"],"
            + "\"memory_internal\":[\"117MB\"],"
            + "\"memory_slot\":[\"microSD\",\"microSDHC\",\"32GB\",\"2GB\"],"
            + "\"network\":[\"CDMA800\",\"CDMA1900\",\"Bluetooth 3.0\"],"
            + "\"media_camera\":[\"2MP\",\"1600x1200\"],"
            + "\"media_secondcamera\":[],"
            + "\"media_videocapture\":[\"QVGA\"],"
            + "\"media_videoplayback\":[\"MP3\",\"WAV\",\"eAAC+\"],"
            + "\"media_audio\":[\"MP4\",\"H.264\",\"H.263\"],"
            + "\"media_other\":[\"Geo-tagging\"],"
            + "\"features\":[\"Unlimited entries\",\"Caller groups\",\"Multiple numbers per contact\",\"Search by both first and last name\",\"Picture ID\",\"Ring ID\",\"Calendar\",\"Alarm\",\"Document viewer\",\"Calculator\",\"Computer sync\",\"OTA sync\",\"Music ringtones (MP3)\",\"Polyphonic ringtones\",\"Vibration\",\"Flight mode\",\"Silent mode\",\"Speakerphone\",\"Accelerometer\",\"Voice dialing\",\"Voice recording\",\"SMS\",\"Threaded viewer\",\"MMS\",\"Email\",\"Push Email\",\"IM\",\"Organizer\",\"Google Search\",\"Maps\",\"Gmail\",\"YouTube\",\"Google Talk\",\"Picasa integration\",\"Voice memo\",\"Predictive text input (Swype)\",\"Games\"],"
            + "\"connectors\":[\"USB 2.0\",\"microUSB\",\"3.5mm Headphone\"]}";

        private static string AlcatelOT_90822 = "{\"general_vendor\":\"Alcatel\","
            + "\"general_model\":\"OT-908\","
            + "\"general_platform\":\"Android\","
            + "\"general_platform_version\":\"2.2\","
            + "\"general_browser\":\"\","
            + "\"general_browser_version\":\"\","
            + "\"general_image\":\"\","
            + "\"general_aliases\":[\"Alcatel One Touch 908\"],"
            + "\"general_eusar\":\"\","
            + "\"general_battery\":[\"Li-Ion 1300 mAh\"],"
            + "\"general_type\":\"Mobile\","
            + "\"general_cpu\":[\"600Mhz\"],"
            + "\"design_formfactor\":\"Bar\","
            + "\"design_dimensions\":\"110 x 57.4 x 12.4\","
            + "\"design_weight\":\"120\","
            + "\"design_antenna\":\"Internal\","
            + "\"design_keyboard\":\"Screen\","
            + "\"design_softkeys\":\"\","
            + "\"design_sidekeys\":[\"Lock/Unlock\",\"Volume\"],"
            + "\"display_type\":\"TFT\","
            + "\"display_color\":\"Yes\","
            + "\"display_colors\":\"262K\","
            + "\"display_size\":\"2.8\\\"\","
            + "\"display_x\":\"240\","
            + "\"display_y\":\"320\","
            + "\"display_other\":[\"Capacitive\",\"Touch\",\"Multitouch\"],"
            + "\"memory_internal\":[\"150MB\"],"
            + "\"memory_slot\":[\"microSD\",\"microSDHC\",\"32GB\",\"2GB\"],"
            + "\"network\":[\"GSM850\",\"GSM900\",\"GSM1800\",\"GSM1900\",\"UMTS900\",\"UMTS2100\",\"HSDPA900\",\"HSDPA2100\",\"Bluetooth 3.0\",\"802.11b\",\"802.11g\",\"802.11n\",\"GPRS Class 12\",\"EDGE Class 12\"],"
            + "\"media_camera\":[\"2MP\",\"1600x1200\"],"
            + "\"media_secondcamera\":[],"
            + "\"media_videocapture\":[\"Yes\"],"
            + "\"media_videoplayback\":[\"MPEG4\",\"H.263\",\"H.264\"],"
            + "\"media_audio\":[\"MP3\",\"AAC\",\"AAC+\",\"WMA\"],"
            + "\"media_other\":[\"Geo-tagging\"],"
            + "\"features\":[\"Unlimited entries\",\"Caller groups\",\"Multiple numbers per contact\",\"Search by both first and last name\",\"Picture ID\",\"Ring ID\",\"Calendar\",\"Alarm\",\"Calculator\",\"Computer sync\",\"OTA sync\",\"Music ringtones (MP3)\",\"Polyphonic ringtones (64 voices)\",\"Vibration\",\"Flight mode\",\"Silent mode\",\"Speakerphone\",\"Accelerometer\",\"Compass\",\"Voice recording\",\"SMS\",\"MMS\",\"Email\",\"Push Email\",\"IM\",\"Stereo FM radio with RDS\",\"SNS integration\",\"Google Search\",\"Maps\",\"Gmail\",\"YouTube\",\"Google Talk\",\"Picasa integration\",\"Organizer\",\"Document viewer\",\"Voice memo\",\"Voice dialing\",\"Predictive text input\",\"Games\"],"
            + "\"connectors\":[\"USB 2.0\",\"microUSB\",\"3.5mm Headphone\"]}";

        private static string SamsungGT_P1000 = "{\"general_vendor\":\"Samsung\","
            + "\"general_model\":\"GT-P1000'\","
            + "\"general_platform\":\"Android\","
            + "\"general_platform_version\":\"2.3.3\","
            + "\"general_browser\":\"Android Webkit\","
            + "\"general_browser_version\":\"4.0\","
            + "\"general_image\":\"samsunggt-p1000-1368755043-0.jpg\","
            + "\"general_aliases\":[\"Samsung Galaxy Tab\"],"
            + "\"general_eusar\":\"1.07\","
            + "\"general_battery\":[\"Li-Ion 4000 mAh\"],"
            + "\"general_type\":\"Tablet\","
            + "\"general_cpu\":[\"1000Mhz\"],"
            + "\"design_formfactor\":\"Bar\","
            + "\"design_dimensions\":\"190.1 x 120.45 x 11.98\","
            + "\"design_weight\":\"380\","
            + "\"design_antenna\":\"Internal\","
            + "\"design_keyboard\":\"Screen\","
            + "\"design_softkeys\":\"\","
            + "\"design_sidekeys\":[],"
            + "\"display_type\":\"TFT\","
            + "\"display_color\":\"Yes\","
            + "\"display_colors\":\"16M\","
            + "\"display_size\":\"7\\\"\","
            + "\"display_x\":\"1024\","
            + "\"display_y\":\"600\","
            + "\"display_other\":[\"Capacitive\",\"Touch\",\"Multitouch\", \"Touch Buttons\", \"Gorilla Glass\", \"TouchWiz\"],"
            + "\"memory_internal\":[\"16GB\",\"32GB\",\"512MB RAM\"],"
            + "\"memory_slot\":[\"microSD\",\"microSDHC\",\"32GB\"],"
            + "\"network\":[\"GSM850\",\"GSM900\",\"GSM1800\", \"GSM1900\", \"UMTS900\", \"UMTS1900\", \"UMTS2100\", \"HSDPA900\", \"HSDPA1900\", \"HSDPA2100\", \"Bluetooth 3.0\", \"802.11b\",  \"802.11g\",  \"802.11n\",  \"GPRS\",  \"EDGE\",],"
            + "\"media_camera\":[\"3.15MP\",\"2048x1536\"],"
            + "\"media_secondcamera\":[\"1.3MP\"],"
            + "\"media_videocapture\":[\"720x480@30fps\"],"
            + "\"media_videoplayback\":[\"MPEG4\",\"H.264\",\"DivX\", \"XviD\"],"
            + "\"media_audio\":[\"MP3\",\"AAC\",\"FLAC\",\"WMA\",\"WAV\",\"AMR\",\"OGG\",\"MIDI\"],"
            + "\"media_other\":[\"Auto focus\",\"Video calling\",\"Geo-tagging\",\"LED Flash\"],"
            + "\"features\":[\"Unlimited entries\",\"Caller groups\",\"Multiple numbers per contact\",\"Search by both first and last name\",\"Picture ID\",\"Ring ID\",\"Calendar\",\"Alarm\",\"Document viewer\",\"Calculator\",\"DLNA\",\"Computer sync\",\"OTA sync\",\"Music ringtones (MP3)\",\"Flight mode\",\"Silent mode\",\"Speakerphone\",\"Accelerometer\",\"Voice commands\",\"Voice recording\",\"SMS\",\"Threaded viewer\",\"MMS\",\"Email\",\"Push Mail\",\"IM\",\"RSS\",\"Social networking integration\",\"Full HD video playback\",\"Up to 7h movie playback\",\"Organizer\",\"Image/video editor\",\"Thinkfree Office\",\"Word viewer\",\"Excel viewer\",\"PowerPoint viewer\",\"PDF viewer\",\"Google Search\",\"Maps\",\"Gmail\",\"YouTube\",\"Google Talk\",\"Picasa integration\",\"Readers/Media/Music Hub\",\"Voice memo\",\"Voice dialing\",\"Predictive text input (Swype)\",\"Games\"],"
            + "\"connectors\":[\"USB\",\"3.5mm Headphone\",\"3.5mm Headphone\",\"TV Out\",\"MHL\"]}";

        private static string GenericOperaMini = "{\"general_vendor\":\"Generic\","
            + "\"general_model\":\"Opera Mini 5\","
            + "\"general_platform\":\"\","
            + "\"general_platform_version\":\"\","
            + "\"general_browser\":\"Opera Mini\","
            + "\"general_browser_version\":\"5.2\","
            + "\"general_image\":\"\","
            + "\"general_aliases\":[],"
            + "\"general_eusar\":\"\","
            + "\"general_battery\":[],"
            + "\"general_type\":\"Mobile\","
            + "\"general_cpu\":[],"
            + "\"design_formfactor\":\"\","
            + "\"design_dimensions\":\"\","
            + "\"design_weight\":\"\","
            + "\"design_antenna\":\"\","
            + "\"design_keyboard\":\"\","
            + "\"design_softkeys\":\"\","
            + "\"design_sidekeys\":[],"
            + "\"display_type\":\"TFT\","
            + "\"display_color\":\"\","
            + "\"display_colors\":\"\","
            + "\"display_size\":\"\","
            + "\"display_x\":\"\","
            + "\"display_y\":\"\","
            + "\"display_other\":[],"
            + "\"memory_internal\":[],"
            + "\"memory_slot\":[],"
            + "\"network\":[],"
            + "\"media_camera\":[],"
            + "\"media_secondcamera\":[],"
            + "\"media_videocapture\":[],"
            + "\"media_videoplayback\":[],"
            + "\"media_audio\":[],"
            + "\"media_other\":[],"
            + "\"features\":[],"
            + "\"connectors\":[]}";

        private static string AppleiPhone = "{\"general_vendor\":\"Apple\","
                + "\"general_model\":\"iPhone\","
                + "\"general_platform\":\"iOS\","
                + "\"general_image\":\"apple^iphone.jpg\","
                + "\"general_aliases\":[],"
                + "\"general_eusar\":\"0.97\","
                + "\"general_battery\":[\"Li-Ion 1300 mAh\"],"
                + "\"general_type\":\"Mobile\","
                + "\"general_cpu\":[\"ARM 11\",\"412Mhz\"],"
                + "\"design_formfactor\":\"Bar\","
                + "\"design_dimensions\":\"115 x 61 x 11.6\","
                + "\"design_weight\":\"135\","
                + "\"design_antenna\":\"Internal\","
                + "\"design_keyboard\":\"Screen\","
                + "\"design_softkeys\":\"\","
                + "\"design_sidekeys\":[\"Volume\"],"
                + "\"display_type\":\"TFT\","
                + "\"display_color\":\"Yes\","
                + "\"display_colors\":\"16M\","
                + "\"display_size\":\"3.5\\\"\","
                + "\"display_x\":\"320\","
                + "\"display_y\":\"480\","
                + "\"display_other\":[\"Capacitive\",\"Touch\",\"Multitouch\",\"Gorilla Glass\"],"
                + "\"memory_internal\":[\"4GB\",\"8GB\",\"16GB RAM\"],"
                + "\"memory_slot\":[],"
                + "\"network\":[\"GSM850\",\"GSM900\",\"GSM1800\",\"GSM1900\",\"Bluetooth 2.0\",\"802.11b\",\"802.11g\",\"GPRS\",\"EDGE\"],"
                + "\"media_camera\":[\"2MP\",\"1600x1200\"],"
                + "\"media_secondcamera\":[],"
                + "\"media_videocapture\":[],"
                + "\"media_videoplayback\":[\"MPEG4\",\"H.264\"],"
                + "\"media_audio\":[\"MP3\",\"AAC\",\"WAV\"],"
                + "\"media_other\":[],"
                + "\"features\":[\"Unlimited entries\",\"Multiple numbers per contact\",\"Picture ID\",\"Ring ID\",\"Calendar\",\"Alarm\",\"Document viewer\",\"Calculator\",\"Timer\",\"Stopwatch\",\"Computer sync\",\"OTA sync\",\"Polyphonic ringtones\",\"Vibration\",\"Phone profiles\",\"Flight mode\",\"Silent mode\",\"Speakerphone\",\"Accelerometer\",\"Voice recording\",\"Light sensor\",\"Proximity sensor\",\"SMS\",\"Threaded viewer\",\"Email\",\"Google Maps\",\"Audio/video player\",\"Games\"],"
                + "\"connectors\":[\"USB\",\"3.5mm Headphone\",\"TV Out\"],"
                + "\"general_platform_version\":\"\","
                + "\"general_browser\":\"Opera Mini\","
                + "\"general_browser_version\":\"6.1\"}";

        private static string SonyEricssonX10I = "[{\"general_vendor\":\"SonyEricsson\","
                + "\"general_model\":\"X10I\","
                + "\"general_platform\":\"Android\","
                + "\"general_platform_version\":\"2.1.1\","
                + "\"general_browser\":\"Android Webkit\","
                + "\"general_browser_version\":\"4.0\","
                + "\"general_image\":\"\","
                + "\"general_aliases\":[\"SonyEricsson Xperia X10\",\"SonyEricsson X10\"],"
                + "\"general_eusar\":\"\","
                + "\"general_battery\":[\"Li-Po 1500 mAh\",\"BST-41\"],"
                + "\"general_type\":\"Mobile\","
                + "\"general_cpu\":[\"1000Mhz\"],"
                + "\"design_formfactor\":\"Bar\","
                + "\"design_dimensions\":\"119 x 63 x 13\","
                + "\"design_weight\":\"135\","
                + "\"design_antenna\":\"Internal\","
                + "\"design_keyboard\":\"Screen\","
                + "\"design_softkeys\":\"\","
                + "\"design_sidekeys\":[\"Volume\",\"Camera\"],"
                + "\"display_type\":\"TFT\","
                + "\"display_color\":\"Yes\","
                + "\"display_colors\":\"65K\","
                + "\"display_size\":\"4\\\"\","
                + "\"display_x\":\"480\","
                + "\"display_y\":\"854\","
                + "\"display_other\":[\"Capacitive\",\"Touch\",\"Multitouch\"],"
                + "\"memory_internal\":[\"1GB\",\"384MB RAM\"],"
                + "\"memory_slot\":[\"microSD\",\"microSDHC\",\"32GB\",\"8GB\"],"
                + "\"network\":[\"GSM850\",\"GSM900\",\"GSM1800\",\"GSM1900\",\"UMTS900\",\"UMTS1700\",\"UMTS2100\",\"HSDPA900\",\"HSDPA1700\",\"HSDPA2100\",\"Bluetooth 2.1\",\"802.11b\",\"802.11g\",\"GPRS Class 10\",\"EDGE Class 10\"],"
                + "\"media_camera\":[\"8MP\",\"3264x2448\"],"
                + "\"media_secondcamera\":[],"
                + "\"media_videocapture\":[\"WVGA@30fps\"],"
                + "\"media_videoplayback\":[\"MPEG4\"],"
                + "\"media_audio\":[\"MP3\",\"AAC\",\"AAC+\",\"WMA\",\"WAV\"],"
                + "\"media_other\":[\"Auto focus\",\"Image stabilizer\",\"Video stabilizer\",\"Face detection\",\"Smile detection\",\"Digital zoom\",\"Geo-tagging\",\"Touch focus\",\"LED Flash\"],"
                + "\"features\":[\"Unlimited entries\",\"Caller groups\",\"Multiple numbers per contact\",\"Search by both first and last name\",\"Picture ID\",\"Ring ID\",\"Calendar\",\"Alarm\",\"Document viewer\",\"Calculator\",\"World clock\",\"Stopwatch\",\"Notes\",\"Computer sync\",\"OTA sync\",\"Music ringtones (MP3)\",\"Polyphonic ringtones\",\"Vibration\",\"Flight mode\",\"Silent mode\",\"Speakerphone\",\"Voice recording\",\"Accelerometer\",\"Compass\",\"Timescape/Mediascape UI\",\"SMS\",\"Threaded viewer\",\"MMS\",\"Email\",\"Push email\",\"IM\",\"Google Search\",\"Maps\",\"Gmail\",\"YouTube\",\"Google Talk\",\"Facebook and Twitter integration\",\"Voice memo\",\"Games\"],"
                + "\"connectors\":[\"USB 2.0\",\"microUSB\",\"3.5mm Headphone\"]}]";


        private static string Device_10 = "{\"Device\":{\"_id\":\"10\","
                + "\"hd_specs\":{\"general_vendor\":\"Samsung\","
                + "\"general_model\":\"SPH-A680\","
                + "\"general_platform\":\"\","
                + "\"general_platform_version\":\"\","
                + "\"general_browser\":\"\","
                + "\"general_browser_version\":\"\","
                + "\"general_image\":\"samsungsph-a680-1403617960-0.jpg\","
                + "\"general_aliases\":[\"Samsung VM-A680\"],"
                + "\"general_eusar\":\"\","
                + "\"general_battery\":[\"Li-Ion 900 mAh\"],"
                + "\"general_type\":\"Mobile\","
                + "\"general_cpu\":[],"
                + "\"design_formfactor\":\"Clamshell\","
                + "\"design_dimensions\":\"83 x 46 x 24\","
                + "\"design_weight\":\"96\","
                + "\"design_antenna\":\"Internal\","
                + "\"design_keyboard\":\"Numeric\","
                + "\"design_softkeys\":\"2\","
                + "\"design_sidekeys\":[],"
                + "\"display_type\":\"TFT\","
                + "\"display_color\":\"Yes\","
                + "\"display_colors\":\"65K\","
                + "\"display_size\":\"\","
                + "\"display_x\":\"128\","
                + "\"display_y\":\"160\","
                + "\"display_other\":[\"Second External TFT\"],"
                + "\"memory_internal\":[],"
                + "\"memory_slot\":[],"
                + "\"network\":[\"CDMA800\",\"CDMA1900\",\"AMPS800\"],"
                + "\"media_camera\":[\"VGA\",\"640x480\"],"
                + "\"media_secondcamera\":[],"
                + "\"media_videocapture\":[\"Yes\"],"
                + "\"media_videoplayback\":[],"
                + "\"media_audio\":[],"
                + "\"media_other\":[\"Exposure control\",\"White balance\",\"Multi shot\",\"Self-timer\",\"LED Flash\"],"
                + "\"features\":[\"300 entries\",\"Multiple numbers per contact\",\"Picture ID\",\"Ring ID\",\"Calendar\",\"Alarm\",\"To-Do\",\"Calculator\",\"Stopwatch\",\"SMS\",\"T9\",\"Computer sync\",\"Polyphonic ringtones (32 voices)\",\"Vibration\",\"Voice dialing (Speaker independent)\",\"Voice recording\",\"TTY\\/TDD\",\"Games\"],"
                + "\"connectors\":[\"USB\"]}}}";

        private Dictionary<string, string> headers = new Dictionary<string, string>() { 
            { "AlcatelOT-908222", AlcatelOT_908222 },
		    { "SamsungSCH-M828C", SamsungSCH_M828C },
		    { "AlcatelOT-90822", AlcatelOT_90822 },
		    { "SamsungGT-P1000", SamsungGT_P1000 },
		    { "GenericOperaMini", GenericOperaMini },
		    { "AppleiPhone", AppleiPhone },
		    { "SonyEricssonX10I", SonyEricssonX10I }
        };

        /// <summary>
        /// initialize objects
        /// </summary>
        [TestFixtureSetUp]
        public void Initialize()
        {                                                  
            map = new Dictionary<string, Dictionary<string, string>>();
            map.Add("h1", h1);
            map.Add("h2", h2);
            map.Add("h3", h3);
            map.Add("h4", h4);
            map.Add("h5", h5);
            map.Add("h6", h6);
            map.Add("h7", h7);
            hd3 = new HD3.HD3("your_api_username", "your_api_secret", "your_api_siteId", false);
        }
        
        /// <summary>
        /// Test for runtime exception
        /// </summary>
        [Test]
        public void Test_UsernameRequired()
        {
            hd3.Username = "";
            Assert.AreEqual("", hd3.Username);
        }

        /// <summary>
        /// Test for runtime exception
        /// </summary>
        [Test]
        public void Test_SecretRequired()
        {
            hd3.Secret = "";
            Assert.AreEqual("", hd3.Secret);
        }

        /// <summary>
        /// Test for a config passed to the constructor
        /// </summary>
        [Test]
        public void Test_PassedConfig()
        {
            hd3.Username = "jones";
            hd3.Secret = "jango";
            hd3.SiteId = "78";
            hd3.ProxyServer = "127.0.0.1";
            hd3.ProxyPort = 8080;
            hd3.ProxyUser = "bob";
            hd3.ProxyPass = "123abc";
            Assert.AreEqual(hd3.Username, "jones");
            Assert.AreEqual(hd3.Secret, "jango");
            Assert.AreEqual(hd3.SiteId, "78");
            Assert.AreEqual(hd3.ProxyServer, 8080);
            Assert.AreEqual(hd3.ProxyUser, "bob");
            Assert.AreEqual(hd3.ProxyPass, "123abc");
        }

        /// <summary>
        /// Test for default config readon from config file
        /// </summary>
        [Test]
        public void Test_DefaultFileConfig()
        {
            hd3.UseProxy = false;
            hd3.UseLocal = false;
            Assert.NotNull(hd3.Username);
            Assert.NotNull(hd3.Secret);
            Assert.NotNull(hd3.SiteId);
            Assert.NotNull(hd3.ApiServer);
        }

        /// <summary>
        /// Test for default http headers read when a new object is instantiated
        /// </summary>
        [Test]
        public void Test_DefaultSetup()
        {
            string header = "Mozilla/5.0 (SymbianOS/9.2; U; Series60/3.1 NokiaN95-3/20.2.011 Profile/MIDP-2.0 Configuration/CLDC-1.1 ) AppleWebKit/413";
            string profile = "http://nds1.nds.nokia.com/uaprof/NN95-1r100.xml";
            string ipaddress = "127.0.0.1";
            JObject data = new JObject();
            data.Add("user-agent", header);
            data.Add("x-wap-profile", profile);
            data.Add("ipaddress", ipaddress);
            HttpRequest request = new HttpRequest(null, "http://localhost", null);
            var hd = new HD3.HD3(request);
            hd.UseLocal = false;
            hd.setDetectVar("user-agent", header);
            hd.setDetectVar("x-wap-profile", profile);
            hd.setDetectVar("ipaddress", ipaddress);
            Assert.AreEqual(data["user-agent"], hd.m_detectRequest["user-agent"]);
            Assert.AreEqual(data["x-wap-profile"], hd.m_detectRequest["x-wap-profile"]);
            Assert.AreEqual(data["ipaddress"], hd.m_detectRequest["ipaddress"]);
        }

        /// <summary>
        /// Test for manual setting of http headers
        /// </summary>
        [Test]
        public void Test_ManualSetup()
        {
            string header = "Mozilla/5.0 (SymbianOS/9.2; U; Series60/3.1 NokiaN95-3/20.2.011 Profile/MIDP-2.0 Configuration/CLDC-1.1 ) AppleWebKit/413";
            string profile = "http://nds1.nds.nokia.com/uaprof/NN95-1r100.xml";
            JObject data = new JObject();
            data.Add("user-agent", header);
            data.Add("x-wap-profile", profile);
            hd3.UseLocal = true;
            hd3.setDetectVar("user-agent", header);
            hd3.setDetectVar("x-wap-profile", profile);
            Assert.AreEqual(data["user-agent"], hd3.m_detectRequest["user-agent"]);
            Assert.AreEqual(data["x-wap-profile"], hd3.m_detectRequest["x-wap-profile"]);
        }

        /// <summary>
        /// Test for invalis API credentials
        /// </summary>
        [Test]
        public void Test_InvalidCredentials()
        {
            hd3.Username = "jones";
            hd3.Secret = "jipple";
            hd3.UseLocal = false;
            hd3.SiteId = "57";
            bool reply = hd3.deviceVendors();            
            string data = hd3.getRawReply();
            JObject json = JObject.Parse(data);
            Assert.IsFalse(reply);
            Assert.AreEqual("200", json["status"].ToString());
        }

        /// <summary>
        /// The list is continually growing so ensure its a min length and common vendors are present
        /// </summary>
        /// <param name="local">True if running in local mode, false otherwise</param>
        /// <param name="proxy">True if using proxy for API queries</param>
        [Test]
        public void DeviceVendors(bool local, bool proxy)
        {
            List<String> vendors = new List<string>( new string[] { "Apple", "Sony", "Samsung", "Nokia", "LG", "HTC", "Karbonn" });
            hd3.UseLocal = local;
            hd3.UseProxy = proxy;
            bool reply = hd3.deviceVendors();
            Assert.IsTrue(reply);
            string data = hd3.getRawReply();
            JObject json = JObject.Parse(data);
            Assert.AreEqual("OK", json["message"].ToString());
            Assert.AreEqual(0, int.Parse(json["status"].ToString()));
            foreach(string vendor in vendors)
            {
                Assert.IsTrue(InJsonList(vendor, "vendor", data));
            }
        }

        /// <summary>
        /// The list is continually growing so ensure its a min length and common vendors are present
        /// </summary>
        [Test]
        public void Test_DeviceVendorsFail()
        {
            List<String> vendors = new List<string>(new string[] { "Oracle", "Linux", "Azure" });
            hd3.deviceVendors();
            string data = hd3.getRawReply();
            foreach (string vendor in vendors)
            {
                Assert.IsTrue(InJsonList(vendor, "vendor", data));
            }
        }

        /// <summary>
        /// This list is also continually growing so ensure its a minimum length
        /// </summary>
        /// <param name="local">True if running in local mode, false otherwise</param>
        /// <param name="proxy">True if using proxy for API queries</param>
        [Test]
        public void DeviceModels(bool local, bool proxy)
        {
            hd3.UseLocal = local;
            hd3.UseProxy = proxy;
            bool reply = hd3.deviceModels("Nokia");
            string data = hd3.getRawReply();
            JObject json = JObject.Parse(data);
            Assert.IsTrue(reply);
            Assert.AreEqual("OK", json["message"].ToString());
            Assert.AreEqual(0, int.Parse(json["status"].ToString()));
        }

        /// <summary>
        /// View detailed information about one device
        /// </summary>
        /// <param name="local">True if running in local mode, false otherwise</param>
        /// <param name="proxy">True if using proxy for API queries</param>
        [Test]
        public void DeviceView(bool local, bool proxy)
        {
            hd3.UseLocal = local;
            hd3.UseProxy = proxy;
            bool reply = hd3.deviceView("Nokia", "N95");
            string data = hd3.getRawReply();
            JObject json = JObject.Parse(data);
            Assert.IsTrue(reply);
            Assert.AreEqual("OK", json["message"].ToString());
            Assert.AreEqual(0, int.Parse(json["status"].ToString()));
            Test_CompareDevices(json["device"].ToString(), nokiaN95);
        }

        /// <summary>
        /// Find which devices have a specific property
        /// </summary>
        /// <param name="local">True if running in local mode, false otherwise</param>
        /// <param name="proxy">True if using proxy for API queries</param>
        [Test]
        public void DeviceWhatHas(bool local, bool proxy)
        {
            hd3.UseLocal = local;
            hd3.UseProxy = proxy;
            bool reply = hd3.deviceWhatHas("design_dimensions", "101 x 44 x 16");
            string data = hd3.getRawReply();
            JObject json = JObject.Parse(data);
            List<string> specs = new List<string>() { "Asus", "V80", "Spice", "S900", "Voxtel", "RX800" };
            Assert.IsTrue(reply);
            foreach (string spec in specs)
            {
                var retSpec = (from pt in json["devices"]
                         where (string)pt["id"] == spec || (string)pt["general_vendor"] == spec || (string)pt["general_model"] == spec
                         select pt).Any();

                Assert.IsTrue(retSpec);
            }
        }

        /// <summary>
        /// Perform a battery of detection tests
        /// </summary>
        /// <param name="local">True if running in local mode, false otherwise</param>
        /// <param name="proxy">True if using proxy for API queries</param>
		[Test]
        public void SiteDetect(bool local, bool proxy)
        {
            hd3.UseLocal = local;
            hd3.UseProxy = proxy;
            foreach(string header in notFoundHeaders)
            {
                hd3.setDetectVar("user-agent", header);
                bool reply = hd3.siteDetect();
                string data = hd3.getRawReply();
                JObject json = JObject.Parse(data);
                Assert.IsFalse(reply);
                Assert.AreEqual(301, int.Parse(json["status"].ToString()));
            }
            foreach (KeyValuePair<string, Dictionary<string, string>> entries in map)
            {
                string agent = entries.Value["user-agent"];
                string profile = entries.Value["x-wap-profile"];
                string matchKey = entries.Value["match"];
                if (agent.Length > 0)
                {                    
					hd3.setDetectVar("user-agent", agent);
                }
                if (profile.Length > 0)
                {                    
					hd3.setDetectVar("x-wap-profile", profile);
                }
                bool reply = hd3.siteDetect();                
                string data = hd3.getRawReply();
                JObject json = JObject.Parse(data);                                
                Assert.AreEqual(0, int.Parse(json["status"].ToString()));
                Assert.AreEqual("OK", json["message"]);
                Assert.AreEqual(json["class"], json["hd_specs"]["general_type"]);
                var enumerator = headers.GetEnumerator();
                while(enumerator.MoveNext())
                {
                    string key = enumerator.Current.Key;
                    string value = enumerator.Current.Value;
                    if(key.Equals(matchKey))
                    {
                        json.Remove("general_language");
                        json.Remove("general_language_full");
                        this.Test_CompareDevices(json["hd_specs"].ToString(), value);
                    }                    
                }                
            }
        }

        /// <summary>
        /// Test device nokia browser detect
        /// </summary>
        [Test]
        public void Test_NokiaSiteDetect() {
		    hd3.setDetectVar("user-agent","Mozilla/5.0 (SymbianOS/9.2; U; Series60/3.1 NokiaN95-3/20.2.011 Profile/MIDP-2.0 Configuration/CLDC-1.1 ) AppleWebKit/413");
		    hd3.setDetectVar("x-wap-profile","http://nds1.nds.nokia.com/uaprof/NN95-1r100.xml");
		    hd3.siteDetect();
            string reply = hd3.getRawReply();
            JObject json = JObject.Parse(reply);
		    Assert.AreEqual("Nokia", json["hd_specs"]["general_vendor"]);
		    Assert.AreEqual("Symbian", json["hd_specs"]["general_platform"]);
	    }

        /// <summary>
        /// Test device vendors found
        /// </summary>
        [Test]
        public void Test_DeviceVendorsFound()
        {
            hd3.deviceVendors();
            string reply = hd3.getRawReply();
            string key = "vendor";
            Assert.IsTrue(InJsonList("Asus", key, reply));
            Assert.IsTrue(InJsonList("Satellite", key, reply));
            Assert.IsTrue(InJsonList("Tecno", key, reply));
        }

        /// <summary>
        /// Test device vendors not found
        /// </summary>
        [Test]
        public void Test_DeviceVendorsNotFound()
        {
            hd3.deviceVendors();
            string reply = hd3.getRawReply();
            string key = "vendor";
            Assert.IsFalse(InJsonList("Flame", key, reply));
            Assert.IsFalse(InJsonList("Xeon", key, reply));
            Assert.IsFalse(InJsonList("Advance", key, reply));
        }

        /// <summary>
        /// Test device nokia models; true
        /// </summary>
        [Test]
        public void Test_DeviceModelsNokiaPass()
        {
            hd3.deviceModels("Nokia");
            string reply = hd3.getRawReply();
            string key = "model";
            Assert.IsTrue(InJsonList("3310i", key, reply));
            Assert.IsTrue(InJsonList("Lumia 610 NFC", key, reply));
            Assert.IsTrue(InJsonList("2720 Fold", key, reply));
            Assert.IsTrue(InJsonList("1110i", key, reply));
        }

        /// <summary>
        /// Test device nokia models; false
        /// </summary>
        [Test]
        public void Test_DeviceModelsNokiaFail()
        {
            hd3.deviceModels("Nokia");
            string reply = hd3.getRawReply();
            string key = "model";
            Assert.IsFalse(InJsonList("5050i", key, reply));
            Assert.IsFalse(InJsonList("x120", key, reply));
            Assert.IsFalse(InJsonList("10101", key, reply));
            Assert.IsFalse(InJsonList("abc123", key, reply));
        }

        /// <summary>
        /// Test device view nokia n95
        /// </summary>
        [Test]
        public void Test_DeviceViewNokia95()
        {
            Assert.IsTrue(hd3.deviceView("Nokia", "N95"));
            string reply = hd3.getRawReply();
            JObject json = JObject.Parse(reply);
            Assert.AreEqual(json["device"]["general_vendor"], "Nokia");
            Assert.AreEqual(json["device"]["general_model"], "N95");
            Assert.AreEqual(json["device"]["general_platform"], "Symbian");
            Assert.IsTrue(InJsonMultiList("Alarm", "device", "features", reply));
            Assert.IsTrue(InJsonMultiList("Push-to-Talk", "device", "features", reply));
            Assert.IsTrue(InJsonMultiList("Computer sync", "device", "features", reply));
            Assert.IsTrue(InJsonMultiList("VoIP", "device", "features", reply));
        }

        /// <summary>
        /// Test device view apple iphone 5s
        /// </summary>
        [Test]
        public void Test_DeviceViewAppleIPhone5s()
        {
            Assert.IsTrue(hd3.deviceView("Apple", "IPhone 5s"));
            string reply = hd3.getRawReply();
            JObject json = JObject.Parse(reply);
            Assert.AreEqual(json["device"]["general_vendor"], "Apple");
            Assert.AreEqual(json["device"]["general_model"], "iPhone 5S");
            Assert.IsTrue(InJsonMultiList("Video Call", "device", "features", reply));
            Assert.IsTrue(InJsonMultiList("AGPS", "device", "features", reply));
            Assert.IsTrue(InJsonMultiList("LED Flash", "device", "features", reply));
            Assert.IsTrue(InJsonMultiList("Electronic Compass", "device", "features", reply));
        }
        
        /// <summary>
        /// Runs the Api tests against the Cloud web service
        /// </summary>
        [Test]
        public void Test_CloudApiCalls()
        {
            this.DeviceVendors(false, false);
            this.DeviceModels(false, false);
            this.DeviceView(false, false);
            this.DeviceWhatHas(false, false);
            this.SiteDetect(false, false);
        }

        /// <summary>
        /// Runs the same tests as testCloudApiCalls() but through a proxy
        /// </summary>
        [Test]
        public void Test_CloudProxyApiCalls()
        {
            hd3.UseLocal = false;
            hd3.UseProxy = true;
            Assert.IsNotNull(hd3.ProxyServer);
            Assert.IsNotNull(hd3.ProxyPort);
            Assert.IsNotNull(hd3.ProxyUser);
            Assert.IsNotNull(hd3.ProxyPass);
            this.DeviceVendors(false, true);
            this.DeviceModels(false, true);
            this.DeviceView(false, true);
            this.DeviceWhatHas(false, true);
            this.SiteDetect(false, true);
        }

        /// <summary>
        /// Test fetching the detection trees
        /// </summary>
        [Test]
        public void Test_UltimateFetchTrees()
        {
            hd3.UseLocal = true;
            hd3.UseProxy = false;
            hd3.ReadTimeout = 120;
            hd3.FileDirectory = "tmp";
            bool reply = hd3.siteFetchTrees();
            Assert.IsTrue(reply);
            Assert.AreEqual(true, File.Exists(hd3.FileDirectory + "\\hd3trees.json"));
            string[] devices = { "user-agent0.json", "user-agent1.json", "user-agentplatform.json", "user-agentbrowser.json", "profile0.json" };
            foreach (string device in devices)
            {
                Assert.AreEqual(true, File.Exists(hd3.FileDirectory + "\\" + device));
            }        
        }

        /// <summary>
        /// Test invalid credentials for accessing fetchTrees
        /// </summary>
        [Test]
        public void Test_UltimateFetchTreesFail()
        {
            hd3.Username = "bob";
            hd3.Secret = "cowcowcow";
            hd3.SiteId = "76";
            hd3.ReadTimeout = 120;
            hd3.UseLocal = true;
            hd3.UseProxy = false;
            bool reply = hd3.siteFetchTrees();
            Assert.AreEqual(false, reply);
        }

        /// <summary>
        /// Fetch the device specs
        /// </summary>
        [Test]
        public void Test_UltimateFetchSpecs()
        {
            hd3.UseLocal = true;
            hd3.UseProxy = false;
            hd3.ReadTimeout = 120;
            hd3.FileDirectory = "tmp";
            bool reply = hd3.siteFetchSpecs();
            Assert.IsTrue(reply);
            Assert.AreEqual(true, File.Exists(hd3.FileDirectory + "\\hd3specs.json"));
            string[] filenames = { "Device_10.json", "Extra_546.json", "Device_46142.json", "Extra_9.json", "Extra_102.json", "user-agent0.json", "user-agent1.json", "user-agentplatform.json", "user-agentbrowser.json", "profile0.json" };
            foreach (string filename in filenames)
            {
                Assert.AreEqual(true, File.Exists(hd3.FileDirectory + "\\" + filename));
            }
            string content = System.IO.File.ReadAllText(hd3.FileDirectory + "\\device_10.json");
            JObject json1 = JObject.Parse(Device_10);
            JObject json2 = JObject.Parse(content);
            Assert.IsTrue(JToken.DeepEquals(json1, json2));
        }

        /// <summary>
        /// Test invalid credentials for accessing fetchSpecs
        /// </summary>
        [Test]
        public void Test_UltimateFetchSpecsFail()
        {
            hd3.Username = "bob";
            hd3.Secret = "cowcowcow";
            hd3.SiteId = "76";
            hd3.ReadTimeout = 120;
            hd3.UseLocal = true;
            hd3.UseProxy = false;
            hd3.FileDirectory = "tmp";
            bool reply = hd3.siteFetchTrees();
            Assert.IsFalse(reply);
        }

        /// <summary>
        /// Test fetchArchive
        /// </summary>
        [Test]
        public void Test_UltimateFetchArchive()
        {
            string[] filenames = { "Device_10.json", "Extra_546.json", "Device_46142.json", "Extra_9.json", "Extra_102.json", "user-agent0.json", "user-agent1.json", "user-agentplatform.json", "user-agentbrowser.json", "profile0.json" };
            hd3.UseLocal = true;
            hd3.UseProxy = false;
            hd3.ReadTimeout = 120;
            bool reply = hd3.siteFetchArchive();
            Assert.AreEqual(true, reply);
            foreach (string filename in filenames)
            {
                Assert.AreEqual(true, File.Exists(hd3.FileDirectory + "\\" + filename));
            }
            string content = System.IO.File.ReadAllText(hd3.FileDirectory + "\\device_10.json");
            JObject json1 = JObject.Parse(Device_10);
            JObject json2 = JObject.Parse(content);
            Assert.IsTrue(JToken.DeepEquals(json1, json2));
        }

        /// <summary>
        /// Test Ultimate mode for API calls
        /// </summary>
        [Test]
        public void Test_UltimateApiCalls()
        {
            this.DeviceVendors(true, false);
            this.DeviceModels(true, false);
            this.DeviceView(true, false);
            this.DeviceWhatHas(true, false);
            this.SiteDetect(true, false);
        }

        /// <summary>
        /// Compare two json contents
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        [Test]
        public void Test_CompareDevices(string value1, string value2)
        {
            JObject json1 = JObject.Parse(value1);
            JObject json2 = JObject.Parse(value2);            
            Assert.AreEqual(true, JToken.DeepEquals(json1, json2));
        }

        /// <summary>
        /// Clean object
        /// </summary>
        [TestFixtureTearDown]
        public void TearDown() { hd3.cleanUp();  }

        /// <summary>
        /// Check if key exist
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <param name="reply"></param>
        /// <returns></returns>
        [Ignore]
        public bool InJsonList(string value, string key, string reply)
        {
            JObject json = JObject.Parse(reply);
            foreach (string data in json[key])
            {
                if (data == value)
                    return true;
            }
            return true;
        }

        /// <summary>
        /// Check if keys exist
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key1"></param>
        /// <param name="key2"></param>
        /// <param name="reply"></param>
        /// <returns>boolean</returns>
        [Ignore]
        public bool InJsonMultiList(string value, string key1, string key2, string reply)
        {
            JObject json = JObject.Parse(reply);
            foreach (string data in json[key1][key2])
            {
                if (data == value)
                    return true;
            }
            return false;
        }

    }   
}

