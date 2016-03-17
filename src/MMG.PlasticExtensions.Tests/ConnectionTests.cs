// *************************************************
// MMG.PlasticExtensions.Tests.ConnectionTests.cs
// Last Modified: 04/03/2013 9:24 PM
// Modified By: Bustamante, Diego (bustamd1)
// *************************************************

using System;
using System.Configuration;
using System.Net;
using System.Xml;
using NUnit.Framework;

namespace MMG.PlasticExtensions.Tests
{
    [TestFixture]
    public class ConnectionTests
    {
        [Test]
        public void AuthenticateToYouTrack()
        {
            var baseHost = ConfigurationManager.AppSettings["host"];
            var username = ConfigurationManager.AppSettings["username"];
            var password = ConfigurationManager.AppSettings["password"];
            string issue;
            using (var client = new WebClient())
            {
                var result = client.UploadString
                    (string.Format("{0}/rest/user/login?login={1}&password={2}", baseHost, username, password), "POST", "");
                Assert.AreEqual(@"<login>ok</login>", result);
                var authCookies = client.ResponseHeaders.Get("Set-Cookie");
                client.Headers.Add("Cookie", authCookies);
                try
                {
                    var testIssueKey = ConfigurationManager.AppSettings["test.issueKey"];
                    issue = client.DownloadString(string.Format("{0}/rest/issue/{1}", baseHost, testIssueKey));
                    Assert.IsNotEmpty(issue);
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(issue);

                    var testFieldName = ConfigurationManager.AppSettings["test.fieldName"];
                    var expectedValue = ConfigurationManager.AppSettings["test.fieldValue"];
                    var actualFieldValue = xmlDoc.SelectSingleNode(string.Format("//field[@name='{0}']/value", testFieldName)).InnerText;
                    Console.WriteLine("{0}: {1}", testFieldName, actualFieldValue);
                    Assert.AreEqual(expectedValue, actualFieldValue);
                }
                catch (WebException e)
                {
                    Assert.Fail(e.ToString());
                }
            }
        }

        [Test]
        public void GetIssues() {}
    }
}