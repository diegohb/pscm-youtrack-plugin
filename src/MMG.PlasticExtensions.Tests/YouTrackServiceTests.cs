// *************************************************
// MMG.PlasticExtensions.Tests.YouTrackServiceTests.cs
// Last Modified: 03/17/2016 11:16 AM
// Modified By: Green, Brett (greenb1)
// *************************************************

namespace MMG.PlasticExtensions.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using Codice.Client.IssueTracker;
    using Moq;
    using NUnit.Framework;
    using YouTrackPlugin;
    using YouTrackSharp.Issues;
    using System.Text;

    [TestFixture]
    public class YouTrackServiceTests
    {
        [Test]
        [Ignore("These aren't real unit tests and must be run manually after configuring app.config values.")]
        public void GetAuthenticatedUser_ShouldReturnUserWithEmail()
        {
            var svc = new YouTrackService(getTestConfig());
            svc.Authenticate();
            var user = svc.GetAuthenticatedUser();
            Assert.IsNotNull(user);

            var expectedValue = ConfigurationManager.AppSettings["test.authUserEmail"];
            var actualValue = user.Email;
            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        [Ignore("These aren't real unit tests and must be run manually after configuring app.config values.")]
        public void GetPlasticTask_ShouldReturnLinkedTask()
        {
            var svc = new YouTrackService(getTestConfig());
            var expectedIssueKey = ConfigurationManager.AppSettings["test.issueKey"];
            var actualTask = svc.GetPlasticTask(expectedIssueKey);
            Assert.IsNotNull(actualTask);
            Assert.AreEqual(expectedIssueKey, actualTask.Id);
            Assert.IsTrue(actualTask.CanBeLinked);
        }

        [Test]
        [Ignore("These aren't real unit tests and must be run manually after configuring app.config values.")]
        public void BeginWorkOnIssue_ShouldUpdateTicketToInProgress()
        {
            var config = getTestConfig();
            var svc = new YouTrackService(config);
            svc.Authenticate();
            Assert.IsNotNull(svc.GetAuthenticatedUser());

            var testIssueID = ConfigurationManager.AppSettings["test.issueKey"];
            svc.EnsureIssueInProgress(testIssueID);
        }

        [Test]
        [Ignore("These aren't real unit tests and must be run manually after configuring app.config values.")]
        public void AssignIssue_ShouldUpdateTicketToAssigned()
        {
            var config = getTestConfig();
            var svc = new YouTrackService(config);
            svc.Authenticate();
            Assert.IsNotNull(svc.GetAuthenticatedUser());

            var testIssueID = ConfigurationManager.AppSettings["test.issueKey"];
            svc.AssignIssue(testIssueID, "dbustamante");
        }

        [Test]
        [Ignore("These aren't real unit tests and must be run manually after configuring app.config values.")]
        public void GetUnresolvedIssues_ShouldReturnTicketsUnresolved()
        {
            var config = getTestConfig();
            var svc = new YouTrackService(config);
            var issues = svc.GetUnresolvedPlasticTasks().ToList();
            CollectionAssert.IsNotEmpty(issues);
            var assigneeName = ConfigurationManager.AppSettings["test.fieldValue"];
            Assert.IsTrue(issues.Any(pIssue => !pIssue.Owner.Equals(assigneeName, StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        [Ignore("These aren't real unit tests and must be run manually after configuring app.config values.")]
        public void GetUnresolvedIssuesByAssignee_ShouldReturnTicketsForAssignee()
        {
            var config = getTestConfig();
            var svc = new YouTrackService(config);
            var assigneeName = ConfigurationManager.AppSettings["test.fieldValue"];
            var issues = svc.GetUnresolvedPlasticTasks(assigneeName).ToList();
            CollectionAssert.IsNotEmpty(issues);
            Assert.IsTrue(issues.All(pIssue => pIssue.Owner.Equals(assigneeName, StringComparison.InvariantCultureIgnoreCase)));
        }

        private YouTrackExtensionConfigFacade getTestConfig()
        {
            var testStoredConfig = new IssueTrackerConfiguration()
            {
                WorkingMode = ExtensionWorkingMode.TaskOnBranch,
                Parameters = getTestConfigParams()
            };
            var ytConfig = new YouTrackExtensionConfigFacade(testStoredConfig);
            return ytConfig;
        }

        private IssueTrackerConfigurationParameter[] getTestConfigParams()
        {
            var parameters = new List<IssueTrackerConfigurationParameter>
            {
                new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.BranchPrefix,
                    Value = "yt",
                    Type = IssueTrackerConfigurationParameterType.BranchPrefix,
                    IsGlobal = true
                },
                new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.HostUri,
                    Value = ConfigurationManager.AppSettings["host"],
                    Type = IssueTrackerConfigurationParameterType.Host,
                    IsGlobal = true
                },
                new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.UserId,
                    Value = ConfigurationManager.AppSettings["username"],
                    Type = IssueTrackerConfigurationParameterType.User,
                    IsGlobal = false
                },
                new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.Password,
                    Value = ConfigurationManager.AppSettings["auth.token"],
                    Type = IssueTrackerConfigurationParameterType.Password,
                    IsGlobal = false
                },
                new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.ShowIssueStateInBranchTitle,
                    Value = "false",
                    Type = IssueTrackerConfigurationParameterType.Boolean,
                    IsGlobal = false
                },
                new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.ClosedIssueStates,
                    Value = "Completed,Approved",
                    Type = IssueTrackerConfigurationParameterType.Text,
                    IsGlobal = false
                }
            };

            return parameters.ToArray();
        }

        [Test]
        public void HydratePlasticTask_Base()
        {
            var facade = GetConfigFacade("http://test.com");
            facade.SetupGet(x => x.ShowIssueStateInBranchTitle).Returns(false);
            facade.SetupGet(x => x.IgnoreIssueStateForBranchTitle).Returns("");
            var sut = new YouTrackService(facade.Object);

            dynamic issue = new Issue();
            issue.Id = "ABC1234";
            issue.Summary = "Issue Summary";
            issue.State = "In Progress";
            issue.AssigneeName = "jdoe";
            issue.Description = "Issue Description";

            var task = sut.hydratePlasticTaskFromIssue(issue);
            Assert.AreEqual("ABC1234", task.Id);
            Assert.AreEqual("Issue Summary", task.Title);
            Assert.AreEqual("In Progress", task.Status);
            Assert.AreEqual("jdoe", task.Owner);
            Assert.AreEqual("Issue Description", task.Description);
        }

        [Test]
        public void TestCommentFormatting()
        {
            var host = "acme.website.int:5656/";
            var webGui = new Uri($"https://{host}");
            var repository = "Test.Repository";
            var branch = "/yt_TEST-60";
            long changeSetId = 969;
            var comment = "This is my test comment";
            var nl = Environment.NewLine;
            var guid = Guid.NewGuid();

            var generatedComment = YouTrackService.FormatComment(host, repository, webGui, branch, changeSetId, comment, guid);

            var mdComment = $"{{color:darkgreen}}*PSCM - CODE COMMIT #{changeSetId}*{{color}}";

            var changeSetUriBuilder = new UriBuilder(webGui);
            if (string.IsNullOrEmpty(changeSetUriBuilder.Scheme) ||
                (!changeSetUriBuilder.Scheme.Equals("https", StringComparison.CurrentCultureIgnoreCase) &&
                 !changeSetUriBuilder.Scheme.Equals("http", StringComparison.CurrentCultureIgnoreCase)))
                changeSetUriBuilder.Scheme = "http";

            changeSetUriBuilder.Path = $"webui/repos/{repository}/diff/changeset/{guid}";

            var hostName = host.StartsWith("localhost", StringComparison.CurrentCultureIgnoreCase) ||
                           host.StartsWith("127.0.0.", StringComparison.CurrentCultureIgnoreCase)
                ? Environment.MachineName + (host.Contains(":") ? host.Substring(host.IndexOf(":")) : "")
                : host;

            var tildes = "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~";

            var commentBuilder = new StringBuilder();
            commentBuilder.Append($"{comment}{nl}{nl}");
            commentBuilder.Append($"{tildes}{nl}");
            commentBuilder.Append($"[{mdComment}|{changeSetUriBuilder}]{nl}");
            //commentBuilder.Append($"{{monospace}}");
            commentBuilder.Append($"{guid} @ {branch} @ {repository} @ {hostName}");
            //commentBuilder.Append($"{{monospace}}");

            var expectedComment = commentBuilder.ToString();
            Console.WriteLine("\nActual:\n" + generatedComment);
            Assert.AreEqual(expectedComment, generatedComment);
        }

        [Test]
        public void TestMarkTaskAsOpen_Comment()
        {
            var msg = "{color:darkgreen}*PSCM - BRANCH CREATED*{color}";
            Assert.AreEqual(msg, YouTrackService.GetBranchCreationMessage());
        }

        private static Mock<IYouTrackExtensionConfigFacade> GetConfigFacade(string pUri)
        {
            var facade = new Mock<IYouTrackExtensionConfigFacade>();
            var uri = new Uri(pUri);
            facade.SetupGet(x => x.HostUri).Returns(uri);
            return facade;
        }
    }
}