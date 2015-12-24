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
            //TODO: Implement
        }

        public void Disconnect()
        {
            //TODO: Implement
        }

        public bool TestConnection(IssueTrackerConfiguration pConfiguration)
        {
            //TODO: Implement
            return true;
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
            throw new NotImplementedException();
        }

        public Dictionary<string, PlasticTask> GetTasksForBranches(List<string> pFullBranchNames)
        {
            throw new NotImplementedException();
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

        private string getTaskNameWithoutBranchPrefix(string pTaskFullName)
        {
            return !string.IsNullOrEmpty(_config.BranchPrefix)
                   && pTaskFullName.StartsWith(_config.BranchPrefix, StringComparison.InvariantCultureIgnoreCase)
                ? pTaskFullName.Substring(_config.BranchPrefix.Length)
                : pTaskFullName;
        }

        #endregion
    }
}