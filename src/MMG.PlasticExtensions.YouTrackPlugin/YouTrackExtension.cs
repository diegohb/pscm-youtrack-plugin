// *************************************************
// MMG.PlasticExtensions.YouTrackPlugin.YouTrackExtension.cs
// Last Modified: 01/10/2015 8:53 PM
// Modified By: Bustamante, Diego (bustamd1)
// *************************************************

namespace MMG.PlasticExtensions.YouTrackPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Codice.Client.Extension;
    using log4net;

    public class YouTrackExtension : BasePlasticExtension
    {
        private static readonly ILog _log = LogManager.GetLogger("extensions");
        private readonly YouTrackExtensionConfiguration _config;
        private const string configFile = "youtrackextension.conf";
        private readonly YouTrackHandler _handler;

        public YouTrackExtension()
        {
            try
            {
                bool bSuccess;
                _config =
                    (YouTrackExtensionConfiguration) ExtensionServices.LoadConfig(configFile, typeof (YouTrackExtensionConfiguration), out bSuccess);
                if (!bSuccess)
                {
                    _log.WarnFormat
                        ("YouTrackExtension: Unable to load configuration file: {0}", configFile);
                    _config = new YouTrackExtensionConfiguration();
                }
                else
                {
                    _config.SetDefaultAttributePrefix("yt");
                    mBaseConfig = this._config;
                    _handler = new YouTrackHandler(_config);
                    _log.InfoFormat("YouTrackExtension: Successfully loaded configuration file: {0}", configFile);
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("YouTrackExtension: {0}\n\t{1}", ex.Message, ex.StackTrace);
            }
        }

        public override string GetName()
        {
            return "YouTrack Extension";
        }

        public override void OpenTask(string id, string repName)
        {
            _log.DebugFormat("YouTrackExtension: Open task '{0}'", id);

            Process.Start(string.Format("{0}/issue/{1}", _handler.GetBaseURL(), id));
        }

        public override PlasticTask[] LoadTask(string[] pTaskIDs, string pRepoName)
        {
            _log.DebugFormat("YouTrackExtension: Load tasks {0}", string.Join(",", pTaskIDs));
            if (pTaskIDs[0] == null || pTaskIDs[0] == String.Empty)
                return null;

            var result = new List<PlasticTask>();
            foreach (var taskID in pTaskIDs)
            {
                result.Add(_handler.GetPlasticTaskFromTaskID(taskID));
            }
            return result.ToArray();
        }

        public override string GetTaskIdForBranch(string pFullBranchName, string repName)
        {
            return getTaskNameWithoutBranchPrefix(ExtensionServices.GetTaskNameFromBranch(pFullBranchName));
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