// *************************************************
// MMG.PlasticExtensions.YouTrackPlugin.YouTrackExtension.cs
// Last Modified: 12/20/2015 5:06 PM
// Modified By: Bustamante, Diego (bustamd1)
// *************************************************

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
        private readonly YouTrackExtensionConfigFacade _config;

        public YouTrackExtension(YouTrackExtensionConfigFacade pConfig)
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
            _ytService.Authenticate();
            _log.InfoFormat("Connected successfully to host '{0}'.", _config.Host);
        }

        public void Disconnect()
        {
            _ytService.ClearAuthentication();
            _log.DebugFormat("Disconnected successfully from host '{0}'.", _config.Host);
        }

        public bool TestConnection(IssueTrackerConfiguration pConfiguration)
        {
            try
            {
                var config = new YouTrackExtensionConfigFacade(pConfiguration);
                _ytService.VerifyConfiguration(config);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void LogCheckinResult(PlasticChangeset pChangeset, List<PlasticTask> pTasks)
        {
            //TODO: Implement
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
            var ticketIDs = pFullBranchNames.Select(getBranchName)
                .Where(pBranchName => pBranchName.StartsWith(_config.BranchPrefix))
                .Select(getTicketIDFromTaskBranchName);
            var plasticTasks = _ytService.GetPlasticTasks(ticketIDs.ToArray());

            var result = plasticTasks.ToDictionary(pTask => pTask.Id, pTask => pTask);
            return result;
        }

        public void OpenTaskExternally(string pTaskId)
        {
            _log.DebugFormat("YouTrackExtension: Open task '{0}'", pTaskId);

            Process.Start(string.Format("{0}/issue/{1}", _ytService.GetBaseURL(), pTaskId));
        }

        public List<PlasticTask> LoadTasks(List<string> pTaskIds)
        {
            //TODO: Implement
            return new List<PlasticTask>();
        }

        public List<PlasticTask> GetPendingTasks()
        {
            //TODO: Implement
            return new List<PlasticTask>();
        }

        public List<PlasticTask> GetPendingTasks(string pAssignee)
        {
            //TODO: Implement
            return new List<PlasticTask>();
        }

        public void MarkTaskAsOpen(string pTaskId, string pAssignee)
        {
            //TODO: Implement
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