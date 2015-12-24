// *************************************************
// MMG.PlasticExtensions.YouTrackPlugin.YouTrackService.cs
// Last Modified: 12/24/2015 3:37 PM
// Modified By: Bustamante, Diego (bustamd1)
// *************************************************

namespace MMG.PlasticExtensions.YouTrackPlugin
{
    using System;
    using System.Collections;
    using System.Net;
    using Codice.Client.IssueTracker;
    using log4net;
    using Models;
    using YouTrackSharp.Infrastructure;
    using YouTrackSharp.Issues;

    public class YouTrackService
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

            var result = new PlasticTask {Id = pTaskID, CanBeLinked = false};

            try
            {
                dynamic issue = _ytIssues.GetIssue(pTaskID);
                if (issue != null)
                {
                    result.Owner = issue.Assignee.ToString();
                    result.Status = issue.State.ToString();
                    result.Title = getBranchTitle(result.Status, issue.Summary.ToString());
                    result.Description = issue.Description.ToString();
                    result.CanBeLinked = true;
                }
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

        public YoutrackUser GetAuthenticatedUser()
        {
            var authUser = _ytConnection.GetCurrentAuthenticatedUser();
            var user = new YoutrackUser(authUser.Username, authUser.FullName, authUser.Email);
            return user;
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