// *************************************************
// MMG.PlasticExtensions.YouTrackPlugin.YouTrackExtension.cs
// Last Modified: 03/28/2016 1:46 PM
// Modified By: Green, Brett (greenb1)
// *************************************************

using System.Threading.Tasks;

namespace MMG.PlasticExtensions.YouTrackPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Codice.Client.IssueTracker;
    using log4net;

    public class YouTrackExtension : IPlasticIssueTrackerExtension
    {
        private static readonly ILog _log = LogManager.GetLogger("extensions");
        private readonly YouTrackService _ytService;
        private readonly IYouTrackExtensionConfigFacade _config;

        public YouTrackExtension(IYouTrackExtensionConfigFacade pConfig)
        {
            try
            {
                _config = pConfig;
                _ytService = new YouTrackService(pConfig);
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("YouTrackExtension: {0}\n\t{1}", ex.Message, ex.StackTrace);
            }
        }

        #region IPlasticIssueTrackerExtension implementation

        public string GetExtensionName()
        {
            return "YouTrack Issues Viewer";
        }

        public void Connect()
        {
            //no active connection held.
        }

        public void Disconnect()
        {
            //no active connection held.
        }

        public bool TestConnection(IssueTrackerConfiguration pConfiguration)
        {
            _log.Debug("YouTrackExtension: TestConnection - start");

            try
            {
                var config = new YouTrackExtensionConfigFacade(pConfiguration);
                YouTrackService.VerifyConfiguration(config);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void LogCheckinResult(PlasticChangeset pChangeset, List<PlasticTask> pTasks)
        {
            if (!_config.PostCommentsToTickets)
            {
                return;
            }
            foreach (var task in pTasks)
            {
                _ytService.AddCommentToIssue
                (task.Id, pChangeset.RepositoryServer, pChangeset.Repository,
                    _config.WebGuiRootUrl ?? new Uri($"http://{pChangeset.RepositoryServer}"),
                    pChangeset.Branch, pChangeset.Id, pChangeset.Comment, pChangeset.Guid);
            }
        }

        public void UpdateLinkedTasksToChangeset(PlasticChangeset pChangeset, List<string> pTasks)
        {
            //TODO: Implement
        }

        public PlasticTask GetTaskForBranch(string pFullBranchName)
        {
            var taskBranchName = getBranchName(pFullBranchName);
            if (!taskBranchName.StartsWith(_config.BranchPrefix))
                return null;

            return _ytService.GetPlasticTask(getTicketIDFromTaskBranchName(taskBranchName));
        }

        public Dictionary<string, PlasticTask> GetTasksForBranches(List<string> pFullBranchNames)
        {
            var data = pFullBranchNames.Select
                (x => new
                {
                    FullBranchName = x,
                    Task = _ytService.GetPlasticTask(getTicketIDFromTaskBranchName(getBranchName(x)))
                }).AsParallel();
            var result = data.ToDictionary(x => x.FullBranchName, x => x.Task);
            return result;
        }

        public void OpenTaskExternally(string pTaskId)
        {
            var issueWebUrl = _ytService.GetIssueWebUrl(pTaskId);
            _log.DebugFormat("YouTrackExtension: Open task '{0}' at {1}", pTaskId, issueWebUrl);
            Process.Start(issueWebUrl);
        }

        public List<PlasticTask> LoadTasks(List<string> pTaskIds)
        {
            var plasticTasks = _ytService.GetPlasticTasks(pTaskIds.ToArray()).ToList();
            _log.DebugFormat("YouTrackExtension: Loaded {0} YouTrack plastic tasks.", plasticTasks.Count);
            return plasticTasks;
        }

        public List<PlasticTask> GetPendingTasks()
        {
            var plasticTasks = _ytService.GetUnresolvedPlasticTasks().ToList();
            _log.DebugFormat("YouTrackExtension: Loaded {0} YouTrack unresolved plastic tasks.", plasticTasks.Count);
            return plasticTasks;
        }

        public List<PlasticTask> GetPendingTasks(string pAssignee)
        {
            var plasticTasks = _ytService.GetUnresolvedPlasticTasks(pAssignee).ToList();
            _log.DebugFormat("YouTrackExtension: Loaded {0} YouTrack unresolved plastic tasks.", plasticTasks.Count);
            return plasticTasks;
        }

        public void MarkTaskAsOpen(string pTaskId, string pAssignee)
        {
            try
            {
                _ytService.EnsureIssueInProgress(pTaskId);
                _ytService.AssignIssue(pTaskId, pAssignee, false);
            }
            catch (Exception e)
            {
                _log.Error(string.Format("Failed to set issue '{0}' open and assigned.", pTaskId), e);
            }
        }

        #endregion

        #region Support Methods

        private string getBranchName(string pFullBranchName)
        {
            var lastSeparatorIndex = pFullBranchName.LastIndexOf('/');

            if (lastSeparatorIndex < 0)
                return pFullBranchName;

            return lastSeparatorIndex == pFullBranchName.Length - 1
                ? string.Empty
                : pFullBranchName.Substring(lastSeparatorIndex + 1);
        }

        private string getTicketIDFromTaskBranchName(string pTaskBranchName)
        {
            return !string.IsNullOrEmpty(_config.BranchPrefix)
                   && pTaskBranchName.StartsWith(_config.BranchPrefix, StringComparison.InvariantCultureIgnoreCase)
                ? pTaskBranchName.Substring(_config.BranchPrefix.Length)
                : pTaskBranchName;
        }

        #endregion
    }
}