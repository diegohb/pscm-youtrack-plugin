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
               //init
                _config = pConfig;
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("YouTrackExtension: {0}\n\t{1}", ex.Message, ex.StackTrace);
            }
        }

        public string GetName()
        {
            return "YouTrack Extension";
        }

        public void OpenTask(string id, string repName)
        {
            _log.DebugFormat("YouTrackExtension: Open task '{0}'", id);

            Process.Start(string.Format("{0}/issue/{1}", _ytService.GetBaseURL(), id));
        }
        
        public string GetTaskIdForBranch(string pFullBranchName, string repName)
        {
            throw new NotImplementedException(); 
        }


        private string getTaskNameWithoutBranchPrefix(string pTaskFullName)
        {
            return !string.IsNullOrEmpty(_config.BranchPrefix)
                   && pTaskFullName.StartsWith(_config.BranchPrefix, StringComparison.InvariantCultureIgnoreCase)
                ? pTaskFullName.Substring(_config.BranchPrefix.Length)
                : pTaskFullName;
        }

        #region IPlasticIssueTrackerExtension implementation

        public string GetExtensionName()
        {
            throw new NotImplementedException();
        }

        public void Connect()
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public bool TestConnection(IssueTrackerConfiguration configuration)
        {
            throw new NotImplementedException();
        }

        public void LogCheckinResult(PlasticChangeset changeset, List<PlasticTask> tasks)
        {
            throw new NotImplementedException();
        }

        public void UpdateLinkedTasksToChangeset(PlasticChangeset changeset, List<string> tasks)
        {
            throw new NotImplementedException();
        }

        public PlasticTask GetTaskForBranch(string fullBranchName)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, PlasticTask> GetTasksForBranches(List<string> fullBranchNames)
        {
            throw new NotImplementedException();
        }

        public void OpenTaskExternally(string taskId)
        {
            throw new NotImplementedException();
        }

        public List<PlasticTask> LoadTasks(List<string> taskIds)
        {
            throw new NotImplementedException();
        }

        public List<PlasticTask> GetPendingTasks()
        {
            throw new NotImplementedException();
        }

        public List<PlasticTask> GetPendingTasks(string assignee)
        {
            throw new NotImplementedException();
        }

        public void MarkTaskAsOpen(string taskId, string assignee)
        {
            throw new NotImplementedException();
        }



        #endregion

    }
}