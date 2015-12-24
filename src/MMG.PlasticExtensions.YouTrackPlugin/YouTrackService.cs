// *************************************************
// MMG.PlasticExtensions.YouTrackPlugin.YouTrackService.cs
// Last Modified: 12/24/2015 2:22 PM
// Modified By: Bustamante, Diego (bustamd1)
// *************************************************

namespace MMG.PlasticExtensions.YouTrackPlugin
{
    using System;
    using System.Collections;
    using System.Net;
    using Codice.Client.IssueTracker;
    using log4net;
    using YouTrackSharp.Infrastructure;
    using YouTrackSharp.Issues;

    internal class YouTrackService
    {
        private static readonly ILog _log = LogManager.GetLogger("extensions");
        private readonly Connection _ytConnection;
        private readonly IssueManagement _ytIssues;
        private readonly YouTrackExtensionConfigFacade _config;
        private int _authRetryCount = 0;

        public YouTrackService(YouTrackExtensionConfigFacade pConfig)
        {
            _config = pConfig;
            _ytConnection = new Connection(_config.Host.DnsSafeHost, _config.Host.Port, _config.UseSSL);

            authenticate();
            _ytIssues = new IssueManagement(_ytConnection);
        }

        public PlasticTask GetPlasticTaskFromTaskID(string pTaskID)
        {
            _log.DebugFormat("YouTrackService: GetPlasticTaskFromTaskID {0}", pTaskID);

            var result = new PlasticTask {Id = pTaskID};

            try
            {
                dynamic issue = _ytIssues.GetIssue(pTaskID);
                if (issue == null)
                    return new PlasticTask() {Id = pTaskID, CanBeLinked = false};

                result.Owner = issue.Assignee;
                result.Status = issue.State;
                result.Title = getBranchTitle(issue.State, issue.Summary);
                result.Description = issue.Description;
            }
            catch (Exception ex)
            {
                /* if (exWeb.Message.Contains("Unauthorized.") && _authRetryCount < 3)
                {
                    _log.WarnFormat
                        ("YouTrackService: Failed to fetch youtrack issue '{0}' due to authentication error. Will retry after authentication again. Details: {1}",
                            pTaskID, exWeb);
                    authenticate();
                    return GetPlasticTaskFromTaskID(pTaskID);
                }*/

                _log.WarnFormat("YouTrackService: Failed to find youtrack issue '{0}' due to error: {1}", pTaskID, ex);
            }

            return result;
        }

        public string GetBaseURL()
        {
            return _config.Host.ToString();
        }

        #region Support Methods

        private string getBranchTitle(string pIssueState, string pIssueSummary)
        {
            //if feature is disabled, return ticket summary.
            if (!_config.ShowIssueStateInBranchTitle)
                return pIssueSummary;

            //if feature is enabled but no states are ignored, return default format.
            if (string.IsNullOrEmpty(_config.IgnoreIssueStateForBranchTitle.Trim()))
                return string.Format("{0} [{1}]", pIssueSummary, pIssueState);

            //otherwise, consider the ignore list.
            var ignoreStates = new ArrayList(_config.IgnoreIssueStateForBranchTitle.Trim().Split(','));
            return ignoreStates.Contains(pIssueState)
                ? pIssueSummary
                : string.Format("{0} [{1}]", pIssueSummary, pIssueState);
        }

        private void authenticate()
        {
            _authRetryCount++;
            var creds = new NetworkCredential(_config.UserID, _config.Password);
            _ytConnection.Authenticate(creds);
        }

        #endregion
    }
}