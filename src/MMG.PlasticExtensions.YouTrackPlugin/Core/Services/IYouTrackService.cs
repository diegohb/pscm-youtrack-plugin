using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Codice.Client.IssueTracker;
using MMG.PlasticExtensions.YouTrackPlugin.Core.Models;

namespace MMG.PlasticExtensions.YouTrackPlugin.Core.Services
{
    public interface IYouTrackService
    {
        Task<PlasticTask> GetPlasticTask(string pTaskID);
        Task<IEnumerable<PlasticTask>> GetPlasticTasks(string[] pTaskIDs);

        Task<IEnumerable<PlasticTask>> GetUnresolvedPlasticTasks(string pAssignee = "",
            int pMaxCount = 500);

        string GetIssueWebUrl(string pIssueID);
        Task<YoutrackUser> GetAuthenticatedUser();
        void Authenticate();
        Task EnsureIssueInProgress(string pIssueID);

        Task AddCommentToIssue
        (string pIssueID, string pRepositoryServer, string pRepository, Uri pWebGui, string pBranch, long pChangeSetId,
            string pComment,
            Guid pChangeSetGuid);

        Task AssignIssue(string pIssueID, string pAssignee, bool pAddComment = true);
    }
}