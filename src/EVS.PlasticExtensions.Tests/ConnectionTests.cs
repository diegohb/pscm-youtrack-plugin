// *************************************************
// MMG.PlasticExtensions.Tests.ConnectionTests.cs
// Last Modified: 09/08/2019 5:23 PM
// Modified By: Diego Bustamante (dbustamante)
// *************************************************

using System.Configuration;
using System.Threading.Tasks;
using NUnit.Framework;
using YouTrackSharp;

namespace EVS.PlasticExtensions.Tests
{
    [TestFixture]
    public class ConnectionTests
    {
        [Test]
        [Ignore("Run manually.")]
        public async Task YTSharp_VerifyTicketAccess()
        {
            //arange
            var ticketId = ConfigurationManager.AppSettings["test.issueKey"];
            var fieldName = ConfigurationManager.AppSettings["test.fieldName"];
            var expectedValue = ConfigurationManager.AppSettings["test.fieldValue"];

            //act
            var baseHost = ConfigurationManager.AppSettings["host"];
            var authToken = ConfigurationManager.AppSettings["auth.token"];
            var connection = new BearerTokenConnection(baseHost, authToken);
            var issueSvc = connection.CreateIssuesService();
            var issue = await issueSvc.GetIssue(ticketId);

            //assert
            Assert.IsNotNull(issue);
            var actualValue = issue.GetField(fieldName).Value;
            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void GetIssues() { }
    }
}