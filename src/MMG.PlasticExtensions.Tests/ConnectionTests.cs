// *************************************************
// MMG.PlasticExtensions.Tests.ConnectionTests.cs
// Last Modified: 04/03/2013 9:24 PM
// Modified By: Bustamante, Diego (bustamd1)
// *************************************************

using System;
using System.Configuration;
using System.Net;
using System.Security;
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
            var authToken = ConfigurationManager.AppSettings["auth.token"];
            using (var client = new WebClient())
            {
                client.Headers.Add("Authorization", $"Bearer {authToken}");
                try
                {
                    var testIssueKey = ConfigurationManager.AppSettings["test.issueKey"];
                    var issue = client.DownloadString($"{baseHost}/rest/issue/{testIssueKey}");
                    Assert.IsNotEmpty(issue);
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(issue);

                    var testFieldName = ConfigurationManager.AppSettings["test.fieldName"];
                    var expectedValue = ConfigurationManager.AppSettings["test.fieldValue"];
                    var actualFieldValue = xmlDoc.SelectSingleNode($"//field[@name='{testFieldName}']/value")?.InnerText;
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