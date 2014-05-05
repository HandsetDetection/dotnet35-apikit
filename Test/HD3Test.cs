using System;
using NUnit.Framework;
using HD3;
using System.Collections.Specialized;

namespace HD3Test
{    
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]    
    public class HD3Test
    {
        private HD3.HD3 hd3;

        /// <summary>
        /// 
        /// </summary>
        [SetUp]
        public void Init()
        {
            NameValueCollection appSettings = System.Configuration.ConfigurationManager.AppSettings;
            hd3 = new HD3.HD3();
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
	    public void testdeviceVendors() {
            Assert.False(hd3.UseLocal);
            Assert.True(hd3.UseLocal);
	    }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void testdeviceModels()
        {
            Assert.False(hd3.UseLocal);
            Assert.True(hd3.UseLocal);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ExecuteAssembly(
              @"C:\Program Files (x86)\NUnit 2.6.3\bin\nunit-console.exe",
              null,
              new string[] { System.Reflection.Assembly.GetExecutingAssembly().Location });
        }
    }   
}
