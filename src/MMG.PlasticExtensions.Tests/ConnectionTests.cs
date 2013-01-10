// *************************************************
// MMG.Plastic_Extensions.Tests.Class1.cs
// Last Modified: 11/05/2012 12:55 PM
// Modified By: Bustamante, Diego (bustamd1)
// *************************************************

using System;
using System.IO;
using System.Net;
using System.Xml;
using NUnit.Framework;


namespace MMG.Plastic_Extensions.Tests
{


    [TestFixture]
    public class ConnectionTests
    {
        [Test]
        public void AuthenticateToYouTrack()
        {
            string issue;
            using (var client = new WebClient()) {
                var result = client.UploadString("http://issues.ketchum.com/rest/user/login?login=dbustamante&password=cocoliso", "POST", "");
                Assert.AreEqual(@"<login>ok</login>", result);
                var authCookies = client.ResponseHeaders.Get("Set-Cookie");
                client.Headers.Add("Cookie", authCookies);
                try
                {
                    issue = client.DownloadString(string.Format("http://issues.ketchum.com/rest/issue/{0}", "SAMPLEDEPLOY-2"));
                    Assert.IsNotEmpty(issue);
                    Console.WriteLine(issue);
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(issue);
                    Console.WriteLine("Assignee: " + xmlDoc.SelectSingleNode(string.Format("//field[@name='{0}']/value", "Assignee")).InnerText);

                }
                catch (WebException e)
                {
                    Assert.Fail(e.ToString());
                }
            }
            
        }

        [Test]
        public void GetIssues()
        {
           
        }
    }
}