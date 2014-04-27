using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Diagnostics;
using HD3;

public partial class Tests : System.Web.UI.Page {
    
    protected void Page_Load(object sender, EventArgs e) {
        string log = "";
        string file = Request.PhysicalApplicationPath + "\\normal.txt";
        string[] lines = System.IO.File.ReadAllLines(file);
        char[] separator = new char[] { '|' };
        var hd3 = new HD3.HD3(Request);
        int i=0;
        /*
        // Display the file contents by using a foreach loop.
        Response.Write("Testing normal device http headers. Expect all normal agents to return 301 - Test begins.<br/>");
        foreach (string line in lines) {
            if (i++ > 30) break ;
            string[] strSplitArr = line.Split(separator);
            if (strSplitArr.Length > 1) {
                hd3.setDetectVar("user-agent", strSplitArr[0]);
                hd3.setDetectVar("x-wap-profile", strSplitArr[1]);
            } else {
                hd3.setDetectVar("user-agent", strSplitArr[0]);
            }
            hd3.setDetectVar("x-test-header", null);

            log = hd3.getLog();
            if (log != "") {
                Response.Write(log);
                Response.Write("<br/>");
            }
            hd3.cleanUp();       
        }
        Response.Write("Test complete.<br/>");
        */
        string file2 = Request.PhysicalApplicationPath + "\\mobile.txt";
        string[] lines2 = System.IO.File.ReadAllLines(file2);
        i = 0;

        Response.Write("<b> Start " + DateTime.Now +"<br/>");
        // Prime the cache
        hd3.setDetectVar("user-agent", "android SGH-9000");
        hd3.siteDetect();
        hd3.cleanUp();
    
        // Display the file contents by using a foreach loop.
        Response.Write("Expect all mobile agents to return JSON and display some device info.<br/>");
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        foreach (string line in lines2) {
           // if (i++ > 30) break;

            string[] strSplitArr = line.Split(separator);
            if (strSplitArr.Length > 1) {
                hd3.setDetectVar("user-agent", strSplitArr[0]);
                hd3.setDetectVar("x-wap-profile", strSplitArr[1]);
            } else {
                hd3.setDetectVar("user-agent", strSplitArr[0]);
            }
            //hd3.setDetectVar("x-test-header", null);

            if (hd3.siteDetect()) {
                string rawreply = hd3.getRawReply();
                var reply = (IDictionary)hd3.getReply();
                Response.Write("<b>" + DateTime.Now + " " + " Vendor " + ((IDictionary)reply["hd_specs"])["general_vendor"]);
                Response.Write(",Model " + ((IDictionary)reply["hd_specs"])["general_model"]);
                Response.Write(",Browser " + ((IDictionary)reply["hd_specs"])["general_browser"]);
                Response.Write(",Platform " + ((IDictionary)reply["hd_specs"])["general_platform"] + "</b><br/>");
                Response.Write("JSON object dump " + rawreply + "<br/>");    
            } else {
                string rawreply = hd3.getRawReply();
                Response.Write(i.ToString() + "<b>FAIL</b>" + rawreply + "<br/>");
            }
            Response.Write(hd3.getLog());
            hd3.cleanUp();
        }
        stopwatch.Stop();
        Response.Write("Detections "+i.ToString() + " Time " +stopwatch.Elapsed.ToString());
        Response.Write("<br/>");
        Response.Write(hd3.getLog());
        Response.Write("<br/>");
        Response.Write("Test complete.<br/>");
    }
}
