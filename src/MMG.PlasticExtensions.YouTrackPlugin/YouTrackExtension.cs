namespace MMG.PlasticExtensions.YouTrackPlugin
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Codice.Client.IssueTracker;
    using log4net;

    #endregion

    public class YouTrackExtension : IPlasticIssueTrackerExtension
    {
        private static readonly ILog _log = LogManager.GetLogger("extensions");
        private readonly IYouTrackExtensionConfigFacade _config;
        private readonly YouTrackService _ytService;

        #region Ctors

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

        #region IPlasticIssueTrackerExtension implementation

        public string GetExtensionName()
        {
            return "YouTrack";
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

        public async void LogCheckinResult(PlasticChangeset pChangeset, List<PlasticTask> pTasks)
        {
            if (!_config.PostCommentsToTickets)
                return;
            foreach (var task in pTasks)
            {
                await _ytService.AddCommentToIssue
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

            return AsyncHelpers.RunSync(() => _ytService.GetPlasticTask(getTicketIDFromTaskBranchName(taskBranchName)));
        }

        public Dictionary<string, PlasticTask> GetTasksForBranches(List<string> pFullBranchNames)
        {
            var data = pFullBranchNames
                .Where(pBranch => pBranch.Split('/').Last().StartsWith(_config.BranchPrefix))
                .Select
                (x => new
                {
                    FullBranchName = x,
                    Task = AsyncHelpers.RunSync(() =>_ytService.GetPlasticTask(getTicketIDFromTaskBranchName(getBranchName(x))))
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
            var plasticTasks = AsyncHelpers.RunSync(() => _ytService.GetPlasticTasks(pTaskIds.ToArray())).ToList();
            _log.DebugFormat("YouTrackExtension: Loaded {0} YouTrack plastic tasks.", plasticTasks.Count);
            return plasticTasks;
        }

        public List<PlasticTask> GetPendingTasks()
        {
            var plasticTasks = AsyncHelpers.RunSync(()=> _ytService.GetUnresolvedPlasticTasks()).ToList();
            _log.DebugFormat("YouTrackExtension: Loaded {0} YouTrack unresolved plastic tasks.", plasticTasks.Count);
            return plasticTasks;
        }

        public List<PlasticTask> GetPendingTasks(string pAssignee)
        {
            var plasticTasks = AsyncHelpers.RunSync(() => _ytService.GetUnresolvedPlasticTasks(pAssignee)).ToList();
            _log.DebugFormat("YouTrackExtension: Loaded {0} YouTrack unresolved plastic tasks.", plasticTasks.Count);
            return plasticTasks;
        }

        public async void MarkTaskAsOpen(string pTaskId, string pAssignee)
        {
            try
            {
                await _ytService.AssignIssue(pTaskId, pAssignee, false);
            }
            catch (Exception e)
            {
                _log.Error($"Failed to assign issue '{pTaskId}'.", e);
            }

            try
            {
                _ytService.EnsureIssueInProgress(pTaskId);
            }
            catch (Exception e)
            {
                _log.Error($"Failed to transition issue '{pTaskId}'.", e);
            }
        }

        #endregion
    }
}