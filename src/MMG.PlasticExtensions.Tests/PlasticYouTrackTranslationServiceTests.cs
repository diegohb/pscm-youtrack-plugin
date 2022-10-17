using System;
using System.Collections.Generic;
using MMG.PlasticExtensions.YouTrackPlugin;
using Moq;
using NUnit.Framework;
using YouTrackSharp.Issues;

namespace MMG.PlasticExtensions.Tests
{
    [TestFixture]
    public class PlasticYouTrackTranslationServiceTests
    {

        [Test]
        public void GetPlasticTaskFromIssue_WithoutUserMapping_ShouldReturnPlasticTaskWithMappedName()
        {
            var facade = GetConfigFacade("http://test.com");
            facade.SetupGet(x => x.ShowIssueStateInBranchTitle).Returns(false);
            facade.SetupGet(x => x.IgnoreIssueStateForBranchTitle).Returns("");
            facade.SetupGet(x => x.UsernameMapping).Returns(string.Empty);
            var sut = new PlasticYouTrackTranslationService(facade.Object);

            dynamic issue = new Issue();
            issue.Id = "ABC1234";
            issue.Summary = "Issue Summary";
            issue.State = "In Progress";
            issue.Assignee = new List<Assignee>(new[] { new Assignee { UserName = "jdoe", FullName = "John Doe" } });
            issue.Description = "Issue Description";

            var task = sut.GetPlasticTaskFromIssue(issue);
            Assert.AreEqual("ABC1234", task.Id);
            Assert.AreEqual("Issue Summary", task.Title);
            Assert.AreEqual("In Progress", task.Status);
            Assert.AreEqual("jdoe", task.Owner);
            Assert.AreEqual("Issue Description", task.Description);
        }

        [Test]
        public void GetPlasticTaskFromIssue_WithUserMapping_ShouldReturnPlasticTaskWithMappedName()
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
            issue.Assignee = new List<Assignee>(new[] { new Assignee { UserName = "jdoe", FullName = "John Doe" } });
            issue.Description = "Issue Description";

            var task = sut.GetPlasticTaskFromIssue(issue);
            Assert.AreEqual("ABC1234", task.Id);
            Assert.AreEqual("Issue Summary", task.Title);
            Assert.AreEqual("In Progress", task.Status);
            Assert.AreEqual("john.doe", task.Owner);
            Assert.AreEqual("Issue Description", task.Description);
        }

        [Test]
        public void GetPlasticTaskFromIssue_IncludeStateInTitle_ShouldIncludeStateInTitle()
        {
            var facade = GetConfigFacade("http://test.com");
            facade.SetupGet(x => x.ShowIssueStateInBranchTitle).Returns(true);
            facade.SetupGet(x => x.IgnoreIssueStateForBranchTitle).Returns("Completed");
            var sut = new PlasticYouTrackTranslationService(facade.Object);

            dynamic issue = new Issue();
            issue.Id = "ABC1234";
            issue.Summary = "Issue Summary";
            issue.State = "In Progress";

            var task = sut.GetPlasticTaskFromIssue(issue);
            Assert.AreEqual("ABC1234", task.Id);
            Assert.AreEqual("Issue Summary [In Progress]", task.Title);
            Assert.AreEqual("In Progress", task.Status);
        }

        [Test]
        public void GetPlasticTaskFromIssue_IncludeStateInTitle_ShouldExcludeStateInTitle()
        {
            var facade = GetConfigFacade("http://test.com");
            facade.SetupGet(x => x.ShowIssueStateInBranchTitle).Returns(true);
            facade.SetupGet(x => x.IgnoreIssueStateForBranchTitle).Returns("Completed");
            var sut = new PlasticYouTrackTranslationService(facade.Object);

            dynamic issue = new Issue();
            issue.Id = "ABC1234";
            issue.Summary = "Issue Summary";
            issue.State = "Completed";

            var task = sut.GetPlasticTaskFromIssue(issue);
            Assert.AreEqual("ABC1234", task.Id);
            Assert.AreEqual("Issue Summary", task.Title);
            Assert.AreEqual("Completed", task.Status);
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