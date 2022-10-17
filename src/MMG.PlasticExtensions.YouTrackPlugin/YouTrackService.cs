// *************************************************
// MMG.PlasticExtensions.YouTrackPlugin.YouTrackService.cs
// Last Modified: 09/09/2019 10:50 AM
// Modified By: Diego Bustamante (dbustamante)
// *************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Codice.Client.IssueTracker;
using log4net;
using MMG.PlasticExtensions.YouTrackPlugin.Models;
using YouTrackSharp;
using YouTrackSharp.Generated;
using YouTrackSharp.Issues;

namespace MMG.PlasticExtensions.YouTrackPlugin
{
    
    public class YouTrackService
    {
        private static readonly ILog _log = LogManager.GetLogger("extensions");
        private readonly IYouTrackExtensionConfigFacade _config;
        private readonly PlasticYouTrackTranslationService _translationService;
        private readonly Connection _ytConnection;
        private readonly IIssuesService _ytIssues;

        #region Ctors

        public YouTrackService(IYouTrackExtensionConfigFacade pConfig)
        {
            _config = pConfig;
            _ytConnection = getServiceConnection(pConfig);
            _ytIssues = _ytConnection.CreateIssuesService();
            _log.Debug("YouTrackService: ctor called");
            _translationService = new PlasticYouTrackTranslationService(_config);
        }

        #endregion

        public async Task<PlasticTask> GetPlasticTask(string pTaskID)
        {
            _log.DebugFormat("YouTrackService: GetPlasticTask {0}", pTaskID);

            ensureAuthenticated();

            try
            {
                if (await _ytIssues.Exists(pTaskID))
                {
                    var issue = await _ytIssues.GetIssue(pTaskID);
                    if (issue != null)
                        return _translationService.GetPlasticTaskFromIssue(issue);
                }
            }
            catch (Exception ex)
            {
                _log.Warn($"YouTrackService: Failed to fetch youtrack issue '{pTaskID}' due to error.", ex);
            }

            return new PlasticTask { Id = pTaskID, CanBeLinked = false };
        }

        public async Task<IEnumerable<PlasticTask>> GetPlasticTasks(string[] pTaskIDs)
        {
            ensureAuthenticated();

            _log.DebugFormat("YouTrackService: GetPlasticTasks - {0} task ID(s) supplied", pTaskIDs.Length);

            var tasks = pTaskIDs.Select(GetPlasticTask);
            var results = await Task.WhenAll(tasks);
            return results;
        }

        public async Task<IEnumerable<PlasticTask>> GetUnresolvedPlasticTasks(string pAssignee = "",
            int pMaxCount = 500)
        {
            ensureAuthenticated();

            try
            {
                var assignee = _translationService.GetYouTrackUsernameFromPlasticUsername(pAssignee);
                var searchString = string.Format
                ("{0}{1}", string.IsNullOrWhiteSpace(assignee) ? string.Empty : string.Format("for: {0} ", assignee),
                    _config.CreateBranchIssueQuery);

                var issues = await _ytIssues.GetIssues(searchString, take: pMaxCount);
                if (!issues.Any())
                    return new List<PlasticTask>();

                var tasks = issues.Select(pIssue => _translationService.GetPlasticTaskFromIssue(pIssue));
                return tasks.ToList();
            }
            catch (Exception ex)
            {
                _log.Error("YouTrackService: Failed to fetch unresolved issues.", ex);
                throw;
            }
        }

        public string GetIssueWebUrl(string pIssueID)
        {
            return _config.HostUri.Segments.Length > 1
                ? string.Format("{0}/issue/{1}", _config.HostUri.ToString().TrimEnd('/'), pIssueID)
                : new Uri(_config.HostUri, string.Format("/issue/{0}", pIssueID)).ToString();
        }

        public async Task<YoutrackUser> GetAuthenticatedUser()
        {
            ensureAuthenticated();
            var http = await _ytConnection.GetAuthenticatedApiClient();
            var rsp = await http.UsersMeAsync("id,login,name,email");
            var user = new YoutrackUser(rsp.Login, rsp.FullName, rsp.Email);
            return user;
        }

        public void Authenticate()
        {
            validateConfig(_config);

            //no active connection held.
        }

        public void ClearAuthentication()
        {
            //no active connection held.
        }

        public static async Task VerifyConfiguration(YouTrackExtensionConfigFacade pConfig)
        {
            validateConfig(pConfig);

            try
            {
                var testConnection = getServiceConnection(pConfig);
                await testConnection.CreateIssuesService().GetIssueCount();
            }
            catch (Exception e)
            {
                _log.Warn($"Failed to verify configuration against host '{pConfig.HostUri}'.", e);
                throw new ApplicationException($"Failed to authenticate against the host. Message: {e.Message}", e);
            }
        }

        public static string GetBranchCreationMessage()
        {
            return "*PSCM - BRANCH CREATED*";
        }

        public async Task EnsureIssueInProgress(string pIssueID)
        {
            ensureAuthenticated();

            try
            {
                var issue = await _ytIssues.GetIssue(pIssueID);
                if (issue == null)
                    throw new NullReferenceException($"Unable to find issue by ID {pIssueID}.");

                var transitionTarget = _translationService.GetMarkAsOpenTransitionFromIssue(issue);

                if (!string.IsNullOrEmpty(transitionTarget))
                    await _ytIssues.ApplyCommand(pIssueID, transitionTarget, GetBranchCreationMessage());
                else
                    _log.InfoFormat("Issue '{0}' already marked in-progress.", pIssueID);
            }
            catch (YouTrackErrorException ex)
            {
                _log.Error($"Unable to mark issue '{pIssueID}' in-progress.", ex);
                throw new ApplicationException("Error occurred marking issue in-progress.", ex);
            }
        }

        public static string FormatComment
        (string pHost, string pRepository, Uri pWebGui, string pBranch,
            long pChangeSetId, string pComment, Guid pChangeSetGuid)
        {
            var nl = Environment.NewLine;
            var mdComment = $"*PSCM - CODE COMMIT #{pChangeSetId}*";

            var changeSetUriBuilder = new UriBuilder(pWebGui);
            if (string.IsNullOrEmpty(changeSetUriBuilder.Scheme) ||
                !changeSetUriBuilder.Scheme.Equals("https", StringComparison.CurrentCultureIgnoreCase) &&
                !changeSetUriBuilder.Scheme.Equals("http", StringComparison.CurrentCultureIgnoreCase))
                changeSetUriBuilder.Scheme = "http";

            changeSetUriBuilder.Path += $"repos/{pRepository}/diff/changeset/{pChangeSetGuid}";

            var hostName = pHost.StartsWith("localhost", StringComparison.CurrentCultureIgnoreCase) ||
                           pHost.StartsWith("127.0.0.", StringComparison.CurrentCultureIgnoreCase)
                ? Environment.MachineName + (pHost.Contains(":") ? pHost.Substring(pHost.IndexOf(":")) : "")
                : pHost;

            var tildes = "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~";

            var commentBuilder = new StringBuilder();
            commentBuilder.Append($"{pComment}{nl}{nl}");
            commentBuilder.Append($"{tildes}{nl}");
            commentBuilder.Append($"[{mdComment}]({changeSetUriBuilder}){nl}");
            //commentBuilder.Append($"{{monospace}}");
            commentBuilder.Append($"{pChangeSetGuid} @ {pBranch} @ {pRepository} @ {hostName}");
            //commentBuilder.Append($"{{monospace}}");

            return commentBuilder.ToString();
        }

        public async Task AddCommentToIssue
        (string pIssueID, string pRepositoryServer, string pRepository, Uri pWebGui, string pBranch, long pChangeSetId,
            string pComment,
            Guid pChangeSetGuid)
        {
            ensureAuthenticated();

            if (await _ytIssues.Exists(pIssueID) == false)
                return;

            try
            {
                var completeComment = FormatComment(pRepositoryServer, pRepository, pWebGui, pBranch, pChangeSetId,
                    pComment, pChangeSetGuid);
                await _ytIssues.AddCommentForIssue(pIssueID, completeComment);
            }
            catch (Exception ex)
            {
                _log.Error("Unable to add comment to issue '{0}'", ex);
            }
        }

        public async Task AssignIssue(string pIssueID, string pAssignee, bool pAddComment = true)
        {
            ensureAuthenticated();

            try
            {
                var mappedAssignee = _translationService.GetYouTrackUsernameFromPlasticUsername(pAssignee);
                var issue = await _ytIssues.GetIssue(pIssueID);
                if (issue == null)
                    throw new NullReferenceException($"Unable to find issue by ID {pIssueID}.");

                var currentAssignee = _translationService.GetAssigneeFromYouTrackIssue(issue).Username;

                if (!string.Equals(currentAssignee, mappedAssignee, StringComparison.InvariantCultureIgnoreCase))
                {
                    var comment = $"Assigned by PlasticSCM to user '{mappedAssignee}'.";
                    await _ytIssues.ApplyCommand(pIssueID, $"for {mappedAssignee}",
                        pAddComment ? comment : string.Empty);
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Unable to assign issue '{pIssueID}' to '{pAssignee}'.", ex);
                throw new ApplicationException("Error occurred marking issue assigned.", ex);
            }
        }

        #region Support Methods

        private void ensureAuthenticated()
        {
            Authenticate();
        }


        private static Connection getServiceConnection(IYouTrackExtensionConfigFacade pConfig)
        {
            var password = pConfig.GetDecryptedPassword();
            var serverUrl = pConfig.HostUri.ToString();
            //TODO: must be an api (starts with "perm:")
            return new BearerTokenConnection(serverUrl, password);
        }

        private static void throwErrorIfRequiredStringSettingIsMissing(string pSettingValue, string pSettingName)
        {
            if (string.IsNullOrWhiteSpace(pSettingValue))
                throw new ApplicationException(string.Format("YouTrack setting '{0}' cannot be null or empty!",
                    pSettingName));
        }

        private static void validateConfig(IYouTrackExtensionConfigFacade pConfig)
        {
            if (pConfig.HostUri.Host.Equals("issues.domain.com", StringComparison.InvariantCultureIgnoreCase))
                return;

            /*//validate URL
            var testConnection = new Connection(pConfig.HostUri.DnsSafeHost, pConfig.HostUri.Port, pConfig.UseSsl);
            testConnection.Head("/rest/user/login");*/

            if (pConfig.HostUri == null)
                throw new ApplicationException(string.Format("YouTrack setting '{0}' cannot be null or empty!",
                    ConfigParameterNames.HostUri));

            throwErrorIfRequiredStringSettingIsMissing(pConfig.BranchPrefix, ConfigParameterNames.BranchPrefix);
            throwErrorIfRequiredStringSettingIsMissing(pConfig.AuthToken, ConfigParameterNames.AuthToken);
        }

        #endregion
    }
}