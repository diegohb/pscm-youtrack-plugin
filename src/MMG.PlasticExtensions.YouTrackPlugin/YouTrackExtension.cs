// *************************************************
// MMG.PlasticExtensions.YouTrackPlugin.YouTrackExtension.cs
// Last Modified: 12/20/2015 7:26 PM
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
        private static readonly ILog _log = LogManager.GetLogger("extensions-youtrack");
        private const string configFile = "youtrackextension.conf";
        private readonly YouTrackHandler _handler;

        readonly IssueTrackerConfiguration _config;

        internal YouTrackExtension(IssueTrackerConfiguration pConfig)
        {
            _config = pConfig;

            _log.InfoFormat("YouTrackExtension: Successfully loaded configuration file: {0}", configFile);
        }

        public YouTrackExtension()
        {
            try
            {
                bool bSuccess;
                var config =
                    (YouTrackExtensionConfiguration) ExtensionServices.LoadConfig(configFile, typeof (YouTrackExtensionConfiguration), out bSuccess);
                if (!bSuccess)
                {
                    _log.WarnFormat
                        ("YouTrackExtension: Unable to load configuration file: {0}", configFile);
                    config = new YouTrackExtensionConfiguration();
                }
                else
                {
                    config.SetDefaultAttributePrefix("yt");
                    _handler = new YouTrackHandler(config);
                    _log.InfoFormat("YouTrackExtension: Successfully loaded configuration file: {0}", configFile);
                }

                _config = config;
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("YouTrackExtension: {0}\n\t{1}", ex.Message, ex.StackTrace);
            }
        }

        public string GetExtensionName()
        {
            return "YouTrack Issue Extension";
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

        public void OpenTaskExternally(string pTaskID)
        {
            _log.DebugFormat("YouTrackExtension: Open task '{0}'", pTaskID);

            Process.Start(string.Format("{0}/issue/{1}", _handler.GetBaseURL(), pTaskID));
        }

        public List<PlasticTask> LoadTasks(List<string> pTaskIDs)
        {
            if (pTaskIDs.Count == 0 || string.IsNullOrEmpty(pTaskIDs[0]))
                return new List<PlasticTask>();

            _log.DebugFormat("YouTrackExtension: Load tasks {0}", string.Join(",", pTaskIDs.ToArray()));
            var result = new List<PlasticTask>();
            foreach (var taskID in pTaskIDs)
            {
                var plasticTask = default(PlasticTask); //add an empty plastictask so that array result matches order of task IDs passed in.
                if (taskID.ToLower().StartsWith(GetBranchPrefix(pRepoName)))
                {
                    var taskIDWithoutPrefix = getTaskNameWithoutBranchPrefix(taskID);
                    plasticTask = _handler.GetPlasticTaskFromTaskID(taskIDWithoutPrefix);
                }
                result.Add(plasticTask);
            }
            return result;
        }

        public override string GetTaskIdForBranch(string pFullBranchName, string repName)
        {
            return ExtensionServices.GetTaskNameFromBranch(pFullBranchName);
        }

        public override PlasticTaskConfiguration[] GetTaskConfiguration(string task)
        {
            _log.DebugFormat("YouTrackExtension: GettaskConfiguration for task '{0}'", task);
            throw new NotImplementedException();
        }

        private string getTaskNameWithoutBranchPrefix(string pTaskFullName)
        {
            return !string.IsNullOrEmpty(_config.BranchPrefix)
                   && pTaskFullName.StartsWith(_config.BranchPrefix, StringComparison.InvariantCultureIgnoreCase)
                ? pTaskFullName.Substring(_config.BranchPrefix.Length)
                : pTaskFullName;
        }
    }
}