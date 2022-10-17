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
        public void GetYouTrackUsernameFromPlasticUsername_WithoutUserMapping_ShouldReturnOriginalName()
        {
            var facade = GetConfigFacade("http://test.com");
            facade.SetupGet(x => x.ShowIssueStateInBranchTitle).Returns(false);
            facade.SetupGet(x => x.IgnoreIssueStateForBranchTitle).Returns("");
            facade.SetupGet(x => x.UsernameMapping).Returns(string.Empty);
            var sut = new PlasticYouTrackTranslationService(facade.Object);

            const string plasticUsername = "john.doe";
            var actualYoutrackUsername = sut.GetYouTrackUsernameFromPlasticUsername(plasticUsername);

            Assert.AreEqual(plasticUsername, actualYoutrackUsername);
        }

        [Test]
        public void GetYouTrackUsernameFromPlasticUsername_WithUserMapping_ShouldReturnMappedName()
        {
            const string plasticUsername = "john.doe";
            const string youtrackUsername = "jdoe";

            var facade = GetConfigFacade("http://test.com");
            facade.SetupGet(x => x.ShowIssueStateInBranchTitle).Returns(false);
            facade.SetupGet(x => x.IgnoreIssueStateForBranchTitle).Returns("");
            facade.SetupGet(x => x.UsernameMapping).Returns($"{plasticUsername}:{youtrackUsername}");
            var sut = new PlasticYouTrackTranslationService(facade.Object);

            var actualYoutrackUsername = sut.GetYouTrackUsernameFromPlasticUsername(plasticUsername);

            Assert.AreEqual(youtrackUsername, actualYoutrackUsername);
        }

        [Test]
        public void GetPlasticUsernameFromYouTrackUser_ByName_WithoutUserMapping_ShouldReturnOriginalName()
        {
            var facade = GetConfigFacade("http://test.com");
            facade.SetupGet(x => x.ShowIssueStateInBranchTitle).Returns(false);
            facade.SetupGet(x => x.IgnoreIssueStateForBranchTitle).Returns("");
            facade.SetupGet(x => x.UsernameMapping).Returns(string.Empty);
            var sut = new PlasticYouTrackTranslationService(facade.Object);

            const string plasticUsername = "john.doe";
            var actualPlasticUsername = sut.GetPlasticUsernameFromYouTrackUser(plasticUsername);

            Assert.AreEqual(plasticUsername, actualPlasticUsername);
        }

        [Test]
        public void GetPlasticUsernameFromYouTrackUser_ByName_WithUserMapping_ShouldReturnMappedName()
        {
            const string plasticUsername = "john.doe";
            const string youtrackUsername = "jdoe";

            var facade = GetConfigFacade("http://test.com");
            facade.SetupGet(x => x.ShowIssueStateInBranchTitle).Returns(false);
            facade.SetupGet(x => x.IgnoreIssueStateForBranchTitle).Returns("");
            facade.SetupGet(x => x.UsernameMapping).Returns($"{plasticUsername}:{youtrackUsername}");
            var sut = new PlasticYouTrackTranslationService(facade.Object);

            var actualPlasticUsername = sut.GetPlasticUsernameFromYouTrackUser(youtrackUsername);

            Assert.AreEqual(plasticUsername, actualPlasticUsername);
        }

        [Test]
        public void GetAssigneeFromYouTrackIssue_WhenAssigneeFieldDoesntExist_ShouldReturnUnassigned()
        {
            const string plasticUsername = "john.doe";
            const string youtrackUsername = "jdoe";

            var facade = GetConfigFacade("http://test.com");
            facade.SetupGet(x => x.ShowIssueStateInBranchTitle).Returns(false);
            facade.SetupGet(x => x.IgnoreIssueStateForBranchTitle).Returns("");
            facade.SetupGet(x => x.UsernameMapping).Returns($"{plasticUsername}:{youtrackUsername}");
            var sut = new PlasticYouTrackTranslationService(facade.Object);
            
            dynamic issue = new Issue();
            issue.Id = "ABC1234";
            issue.Summary = "Issue Summary";
            
            var actualYoutrackUser = sut.GetAssigneeFromYouTrackIssue(issue);

            Assert.AreEqual("Unassigned", actualYoutrackUser.Username);
        }

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

        [Test]
        public void GetMarkAsOpenTransitionFromIssue_WhenPropertyDoesntExist_ShouldReturnEmptyString()
        {
            var facade = GetConfigFacade("http://test.com");
            facade.SetupGet(x => x.ShowIssueStateInBranchTitle).Returns(true);
            facade.SetupGet(x => x.CreateBranchTransitions).Returns("Planned:Start Work");
            var sut = new PlasticYouTrackTranslationService(facade.Object);

            dynamic issue = new Issue();
            issue.Id = "ABC1234";
            issue.Summary = "Issue Summary";

            var actualTransitionVerb = sut.GetMarkAsOpenTransitionFromIssue(issue);
            Assert.AreEqual(string.Empty, actualTransitionVerb);
        }

        [Test]
        public void GetMarkAsOpenTransitionFromIssue_GivenSingleMapping_ShouldReturnVerb()
        {
            var facade = GetConfigFacade("http://test.com");
            facade.SetupGet(x => x.ShowIssueStateInBranchTitle).Returns(true);
            facade.SetupGet(x => x.CreateBranchTransitions).Returns("Planned:Start Work");
            var sut = new PlasticYouTrackTranslationService(facade.Object);

            dynamic issue = new Issue();
            issue.Id = "ABC1234";
            issue.Summary = "Issue Summary";
            issue.State = "Planned";

            var actualTransitionVerb = sut.GetMarkAsOpenTransitionFromIssue(issue);
            Assert.AreEqual("Start Work", actualTransitionVerb);

        }

        [Test]
        public void GetMarkAsOpenTransitionFromIssue_GivenSingleMapping_ShouldReturnEmptyWhenNotMatched()
        {
            var facade = GetConfigFacade("http://test.com");
            facade.SetupGet(x => x.ShowIssueStateInBranchTitle).Returns(true);
            facade.SetupGet(x => x.CreateBranchTransitions).Returns("Planned:Start Work");
            var sut = new PlasticYouTrackTranslationService(facade.Object);

            dynamic issue = new Issue();
            issue.Id = "ABC1234";
            issue.Summary = "Issue Summary";
            issue.State = "New";

            var actualTransitionVerb = sut.GetMarkAsOpenTransitionFromIssue(issue);
            Assert.AreEqual(string.Empty, actualTransitionVerb);

        }

        [Test]
        public void GetPlasticTaskFromIssue_WhenStateFieldIsMissing_ShouldReturnPlasticTask()
        {
            var facade = GetConfigFacade("http://test.com");
            facade.SetupGet(x => x.ShowIssueStateInBranchTitle).Returns(true);
            facade.SetupGet(x => x.IgnoreIssueStateForBranchTitle).Returns("Completed");
            var sut = new PlasticYouTrackTranslationService(facade.Object);

            var issue = new Issue();
            issue.Id = "ABC1234";
            issue.Summary = "Issue Summary";

            var task = sut.GetPlasticTaskFromIssue(issue);
            Assert.AreEqual("ABC1234", task.Id);
            Assert.AreEqual("Issue Summary", task.Title);
            Assert.AreEqual("Unknown", task.Status);
        }
        
        [Test]
        public void GetPlasticTaskFromIssue_WhenIssueRequiredFieldsMissing_ShouldReturnPlasticTask()
        {
            var facade = GetConfigFacade("http://test.com");
            facade.SetupGet(x => x.ShowIssueStateInBranchTitle).Returns(true);
            facade.SetupGet(x => x.IgnoreIssueStateForBranchTitle).Returns("Completed");
            var sut = new PlasticYouTrackTranslationService(facade.Object);

            var issue = new Issue();

            var task = sut.GetPlasticTaskFromIssue(issue);
            Assert.AreEqual(false, task.CanBeLinked);
        }


        [Test]
        public void GetPlasticTaskFromIssue_WhenIssueIsNull_ShouldReturnPlasticTask()
        {
            var facade = GetConfigFacade("http://test.com");
            facade.SetupGet(x => x.ShowIssueStateInBranchTitle).Returns(true);
            facade.SetupGet(x => x.IgnoreIssueStateForBranchTitle).Returns("Completed");
            var sut = new PlasticYouTrackTranslationService(facade.Object);

            Assert.Throws<ArgumentNullException>(() => sut.GetPlasticTaskFromIssue(null));

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