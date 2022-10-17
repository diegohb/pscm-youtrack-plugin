// *************************************************
// MMG.PlasticExtensions.Tests.YouTrackServiceTests.cs
// Last Modified: 09/09/2019 10:50 AM
// Modified By: Diego Bustamante (dbustamante)
// *************************************************

namespace MMG.PlasticExtensions.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Text;
    using Codice.Client.IssueTracker;
    using Moq;
    using NUnit.Framework;
    using YouTrackPlugin;
    using YouTrackSharp.Issues;

    [TestFixture]
    public class YouTrackServiceTests
    {
        [Test]
        [Category("Integration")]
        public void GetAuthenticatedUser_ShouldReturnUserWithEmail()
        {
            var svc = new YouTrackService(getTestConfig());
            svc.Authenticate();
            var user = AsyncHelpers.RunSync(() => svc.GetAuthenticatedUser());
            Assert.IsNotNull(user);

            var expectedValue = ConfigurationManager.AppSettings["test.authUserEmail"];
            var actualValue = user.Email;
            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        [Category("Integration")]
        public void GetPlasticTask_ShouldReturnLinkedTask()
        {
            var svc = new YouTrackService(getTestConfig());
            var expectedIssueKey = ConfigurationManager.AppSettings["test.issueKey"];
            var actualTask = AsyncHelpers.RunSync(()=> svc.GetPlasticTask(expectedIssueKey));
            Assert.IsNotNull(actualTask);
            Assert.AreEqual(expectedIssueKey, actualTask.Id);
            Assert.IsTrue(actualTask.CanBeLinked);
        }

        [Test]
        [Category("Integration")]
        public void BeginWorkOnIssue_ShouldUpdateTicketToInProgress()
        {
            var config = getTestConfig();
            var svc = new YouTrackService(config);
            svc.Authenticate();
            Assert.IsNotNull(svc.GetAuthenticatedUser());

            var testIssueID = ConfigurationManager.AppSettings["test.issueKey"];
            Assert.DoesNotThrow(() => svc.EnsureIssueInProgress(testIssueID));
        }

        [Test]
        [Category("Integration")]
        public void AssignIssue_ShouldUpdateTicketToAssigned()
        {
            var config = getTestConfig();
            var svc = new YouTrackService(config);
            svc.Authenticate();
            Assert.IsNotNull(svc.GetAuthenticatedUser());

            var testIssueID = ConfigurationManager.AppSettings["test.issueKey"];
            Assert.DoesNotThrow( () =>
            {
                svc.AssignIssue(testIssueID, ConfigurationManager.AppSettings["username"]).Wait(1000);
            });
        }

        [Test]
        [Category("Integration")]
        public void GetUnresolvedIssues_ShouldReturnTicketsUnresolved()
        {
            var config = getTestConfig();
            var svc = new YouTrackService(config);
            var issues = AsyncHelpers.RunSync(() => svc.GetUnresolvedPlasticTasks()).ToList();
            CollectionAssert.IsNotEmpty(issues);
            var assigneeName = ConfigurationManager.AppSettings["test.fieldValue"];
            Assert.IsTrue(issues.Any(pIssue => pIssue.Owner.Equals(assigneeName, StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        [Category("Integration")]
        public void GetUnresolvedIssuesByAssignee_ShouldReturnTicketsForAssignee()
        {
            var config = getTestConfig();
            var svc = new YouTrackService(config);
            var assigneeName = ConfigurationManager.AppSettings["test.fieldValue"];
            var issues = AsyncHelpers.RunSync(() => svc.GetUnresolvedPlasticTasks(assigneeName)).ToList();
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
                    Name = ConfigParameterNames.UserId,
                    Value = "dbustamante",
                    Type = IssueTrackerConfigurationParameterType.User,
                    IsGlobal = false
                },
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
                    Name = ConfigParameterNames.AuthToken,
                    Value = ConfigurationManager.AppSettings["auth.token"],
                    Type = IssueTrackerConfigurationParameterType.Password,
                    IsGlobal = false
                },
                new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.ShowIssueStateInBranchTitle,
                    Value = "false",
                    Type = IssueTrackerConfigurationParameterType.Boolean,
                    IsGlobal = true
                },
                new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.ClosedIssueStates,
                    Value = "Completed,Approved",
                    Type = IssueTrackerConfigurationParameterType.Text,
                    IsGlobal = true
                },
                new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.UsernameMapping,
                    Value = "pscmuser:ytuser",
                    Type = IssueTrackerConfigurationParameterType.Text,
                    IsGlobal = false
                },
                new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.CreateBranchTransitions,
                    Value = "Open:In Progress;Planned:In Progress",
                    Type = IssueTrackerConfigurationParameterType.Text,
                    IsGlobal = false
                },
                new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.CreateBranchIssueQuery,
                    Value = "#Unresolved",
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
            facade.SetupGet(x => x.UsernameMapping).Returns("john.doe:jdoe");
            var sut = new PlasticYouTrackTranslationService(facade.Object);

            dynamic issue = new Issue();
            issue.Id = "ABC1234";
            issue.Summary = "Issue Summary";
            issue.State = "In Progress";
            issue.Assignee = new List<Assignee>(new[] { new Assignee() { UserName = "jdoe", FullName = "John Doe" } });
            issue.Description = "Issue Description";

            var task = sut.GetPlasticTaskFromIssue(issue);
            Assert.AreEqual("ABC1234", task.Id);
            Assert.AreEqual("Issue Summary", task.Title);
            Assert.AreEqual("In Progress", task.Status);
            Assert.AreEqual("john.doe", task.Owner);
            Assert.AreEqual("Issue Description", task.Description);
        }

        [Test]
        public void TestCommentFormatting()
        {
            var host = "www.plasticscm.com/orgs/acme/";
            var webGui = new Uri($"https://{host}");
            var repository = "Test.Repository";
            var branch = "/yt_TEST-60";
            long changeSetId = 969;
            var comment = "This is my test comment";
            var nl = Environment.NewLine;
            var guid = Guid.NewGuid();

            var generatedComment = YouTrackService.FormatComment(host, repository, webGui, branch, changeSetId, comment, guid);

            var mdComment = $"*PSCM - CODE COMMIT #{changeSetId}*";

            var changeSetUriBuilder = new UriBuilder(webGui);
            if (string.IsNullOrEmpty(changeSetUriBuilder.Scheme) ||
                (!changeSetUriBuilder.Scheme.Equals("https", StringComparison.CurrentCultureIgnoreCase) &&
                 !changeSetUriBuilder.Scheme.Equals("http", StringComparison.CurrentCultureIgnoreCase)))
                changeSetUriBuilder.Scheme = "http";

            changeSetUriBuilder.Path += $"repos/{repository}/diff/changeset/{guid}";

            var hostName = host.StartsWith("localhost", StringComparison.CurrentCultureIgnoreCase) ||
                           host.StartsWith("127.0.0.", StringComparison.CurrentCultureIgnoreCase)
                ? Environment.MachineName + (host.Contains(":") ? host.Substring(host.IndexOf(":")) : "")
                : host;

            var tildes = "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~";

            var commentBuilder = new StringBuilder();
            commentBuilder.Append($"{comment}{nl}{nl}");
            commentBuilder.Append($"{tildes}{nl}");
            commentBuilder.Append($"[{mdComment}]({changeSetUriBuilder}){nl}");
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
            var msg = "*PSCM - BRANCH CREATED*";
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