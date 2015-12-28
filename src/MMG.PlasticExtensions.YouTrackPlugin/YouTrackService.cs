// *************************************************
// MMG.PlasticExtensions.YouTrackPlugin.YouTrackService.cs
// Last Modified: 12/27/2015 2:51 PM
// Modified By: Bustamante, Diego (bustamd1)
// *************************************************

namespace MMG.PlasticExtensions.YouTrackPlugin
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
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
            validateConfig(pConfig);

            _config = pConfig;
            _ytConnection = new Connection(_config.Host.DnsSafeHost, _config.Host.Port, _config.UseSSL);
            _ytIssues = new IssueManagement(_ytConnection);
            _log.Debug("YouTrackService: ctor called");
        }

        public PlasticTask GetPlasticTask(string pTaskID)
        {
            ensureAuthenticated();

            //TODO: implement this as async.
            _log.DebugFormat("YouTrackService: GetPlasticTask {0}", pTaskID);

            var result = new PlasticTask { Id = pTaskID, CanBeLinked = false };

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

                _log.Warn(string.Format("YouTrackService: Failed to find youtrack issue '{0}' due to error.", pTaskID), ex);
            }

            return result;
        }

        public IEnumerable<PlasticTask> GetPlasticTasks(string[] pTaskIDs)
        {
            ensureAuthenticated();

            _log.DebugFormat("YouTrackService: GetPlasticTasks - {0} task ID(s) supplied", pTaskIDs.Length);

            var result = pTaskIDs.Select(pTaskID => GetPlasticTask(pTaskID)).AsParallel();
            return result;
        }

        public string GetBaseURL()
        {
            return _config.Host.ToString();
        }

        public YoutrackUser GetAuthenticatedUser()
        {
            ensureAuthenticated();

            var authUser = _ytConnection.GetCurrentAuthenticatedUser();
            var user = new YoutrackUser(authUser.Username, authUser.FullName, authUser.Email);
            return user;
        }

        public void Authenticate()
        {
            if (_ytConnection.IsAuthenticated)
            {
                _log.DebugFormat("YouTrackService: Authenticate() was called but already authenticated.");
                return;
            }

            _authRetryCount++;
            var creds = new NetworkCredential(_config.UserID, _config.Password);

            try
            {
                _ytConnection.Authenticate(creds);
            }
            catch (Exception ex)
            {
                _log.Error(string.Format("YouTrackService: Failed to authenticate with YouTrack server '{0}'.", _config.Host.DnsSafeHost), ex);
                throw;
            }
        }

        public void ClearAuthentication()
        {
            ensureAuthenticated();

            _ytConnection.Logout();
        }

        public void VerifyConfiguration(YouTrackExtensionConfigFacade pConfig)
        {
            validateConfig(pConfig);

            try
            {
                var testConnection = new Connection(pConfig.Host.DnsSafeHost, pConfig.Host.Port, pConfig.UseSSL);
                testConnection.Authenticate(pConfig.UserID, pConfig.Password);
                testConnection.Logout();
            }
            catch (Exception e)
            {
                _log.Warn(string.Format("Failed to verify configuration against host '{0}'.", pConfig.Host), e);
                throw new ApplicationException(string.Format("Failed to authenticate against the host. Message: {0}", e.Message), e);
            }
        }

        public void EnsureIssueInProgress(string pIssueID)
        {
            ensureAuthenticated();

            if (!checkIssueExistenceAndLog(pIssueID)) return;

            try
            {
                dynamic issue = _ytIssues.GetIssue(pIssueID);
                if (issue.State.ToString() != "In Progress")
                    _ytIssues.ApplyCommand(pIssueID, "State: In Progress", string.Format("User '{0}' has created a branch for this task.", GetAuthenticatedUser().Username));
                else
                    _log.InfoFormat("Issue '{0}' already marked in-progress.", pIssueID);
            }
            catch (Exception ex)
            {
                _log.Error("Unable to mark issue '{0}' in-progress.", ex);
                throw new ApplicationException("Error occurred marking issue in-progress.", ex);
            }
        }

        public void AssignIssue(string pIssueID, string pAssignee, bool pAddComment = true)
        {
            ensureAuthenticated();

            if (!checkIssueExistenceAndLog(pIssueID)) return;

            try
            {
                dynamic issue = _ytIssues.GetIssue(pIssueID);
                string currentAssignee;

                try
                {
                    currentAssignee = issue.Assignee.ToString();
                }
                catch (Exception)
                {
                    currentAssignee = string.Empty;
                }

                if (!string.Equals(currentAssignee, pAssignee, StringComparison.InvariantCultureIgnoreCase))
                {
                    var comment = string.Format("Assigned by PlasticSCM to user '{0}'.", pAssignee);
                    _ytIssues.ApplyCommand(pIssueID, string.Format("for {0}", pAssignee), pAddComment ? comment : string.Empty);
                }
            }
            catch (Exception ex)
            {
                _log.Error(string.Format("Unable to assign issue '{0}' to '{1}'.", pIssueID, pAssignee), ex);
                throw new ApplicationException("Error occurred marking issue assigned.", ex);
            }
        }


        #region Support Methods

        private bool checkIssueExistenceAndLog(string pTicketID)
        {
            if (_ytIssues.CheckIfIssueExists(pTicketID)) return true;

            _log.WarnFormat("Unable to start work on ticket '{0}' because it cannot be found.", pTicketID);
            return false;
        }

        private void ensureAuthenticated()
        {
            if (_ytConnection.IsAuthenticated)
                return;

            Authenticate();
        }

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

        private void validateConfig(YouTrackExtensionConfigFacade pConfig)
        {
            /*//validate URL
            var testConnection = new Connection(pConfig.Host.DnsSafeHost, pConfig.Host.Port, pConfig.UseSSL);
            testConnection.Head("/rest/user/login");*/

            if (pConfig.Host == null)
                throw new ApplicationException(string.Format("YouTrack setting '{0}' cannot be null or empty!", ConfigParameterNames.Host));

            throwErrorIfRequiredStringSettingIsMissing(pConfig.BranchPrefix, ConfigParameterNames.BranchPrefix);
            throwErrorIfRequiredStringSettingIsMissing(pConfig.UserID, ConfigParameterNames.UserID);
            throwErrorIfRequiredStringSettingIsMissing(pConfig.Password, ConfigParameterNames.Password);

        }

        private void throwErrorIfRequiredStringSettingIsMissing(string pSettingValue, string pSettingName)
        {
            if (string.IsNullOrWhiteSpace(pSettingValue))
                throw new ApplicationException(string.Format("YouTrack setting '{0}' cannot be null or empty!", pSettingName));
        }

        #endregion

    }
}