// *************************************************
// MMG.PlasticExtensions.Tests.YouTrackServiceTests.cs
// Last Modified: 12/24/2015 3:37 PM
// Modified By: Bustamante, Diego (bustamd1)
// *************************************************

namespace MMG.PlasticExtensions.Tests
{
    using System.Collections.Generic;
    using System.Configuration;
    using Codice.Client.IssueTracker;
    using NUnit.Framework;
    using YouTrackPlugin;

    [TestFixture, Ignore("These aren't real unit tests and must be run manually after configuring app.config values.")]
    public class YouTrackServiceTests
    {
        
        [Test]
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
        public void GetUnresolvedIssues_ShouldReturnTicketsUnresolved()
        {
            var config = getTestConfig();
            var svc = new YouTrackService(config);
            var issues = svc.GetUnresolvedPlasticTasks();
            CollectionAssert.IsNotEmpty(issues);
        }

        [Test]
        public void GetUnresolvedIssuesByAssignee_ShouldReturnTicketsForAssignee()
        {
            var config = getTestConfig();
            var svc = new YouTrackService(config);
            var assigneeName = ConfigurationManager.AppSettings["test.fieldValue"];
            var issues = svc.GetUnresolvedPlasticTasks(assigneeName);
            CollectionAssert.IsNotEmpty(issues);
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
                    Name = ConfigParameterNames.Host,
                    Value = ConfigurationManager.AppSettings["host"],
                    Type = IssueTrackerConfigurationParameterType.Host,
                    IsGlobal = true
                },
                new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.UserID,
                    Value = ConfigurationManager.AppSettings["username"],
                    Type = IssueTrackerConfigurationParameterType.User,
                    IsGlobal = false
                },
                new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.Password,
                    Value = ConfigurationManager.AppSettings["password"],
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
    }
}