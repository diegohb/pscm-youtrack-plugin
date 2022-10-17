using Codice.Client.IssueTracker;
using MMG.PlasticExtensions.YouTrackPlugin.Core.Models;
using YouTrackSharp.Issues;

namespace MMG.PlasticExtensions.YouTrackPlugin
{
    public interface IPlasticYouTrackTranslationService
    {
        PlasticTask GetPlasticTaskFromIssue(Issue pIssue);

        /// <summary>
        ///     This method will take the mapping setting and allow mapping different authentication methods' usernames to issue
        ///     username.
        /// </summary>
        /// <param name="pAssignee">The username specified for configuration value UserId</param>
        /// <returns>the username to pass to youtrack to filter for issues assignee.</returns>
        string GetYouTrackUsernameFromPlasticUsername(string pAssignee);

        string GetPlasticUsernameFromYouTrackUser(string pYouTrackUsername);
        string GetPlasticUsernameFromYouTrackUser(YoutrackUser pUser);
        YoutrackUser GetAssigneeFromYouTrackIssue(Issue pIssue, string pUserFieldName = "Assignee");
        string GetPlasticUserFromYouTrackIssue(Issue pIssue, string pUserFieldName);
        string GetMarkAsOpenTransitionFromIssue(Issue pIssue);
    }
}