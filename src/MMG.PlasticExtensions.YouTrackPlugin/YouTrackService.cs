// *************************************************
// MMG.PlasticExtensions.YouTrackPlugin.YouTrackService.cs
// Last Modified: 03/21/2016 3:34 PM
// Modified By: Green, Brett (greenb1)
// *************************************************

using System.Security;
using System.Text;
using YouTrackSharp;

namespace MMG.PlasticExtensions.YouTrackPlugin
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Codice.Client.IssueTracker;
    using log4net;
    using Models;
    using YouTrackSharp.Issues;

    public class YouTrackService
    {
        private static readonly ILog _log = LogManager.GetLogger("extensions");
        private readonly Connection _ytConnection;
        private readonly IssuesService _ytIssues;
        private readonly IYouTrackExtensionConfigFacade _config;

        public YouTrackService(IYouTrackExtensionConfigFacade pConfig)
        {
            _config = pConfig;
            _ytConnection = getServiceConnection((YouTrackExtensionConfigFacade) pConfig);
            _ytIssues = _ytConnection.CreateIssuesService();
            _log.Debug("YouTrackService: ctor called");
        }

        public PlasticTask GetPlasticTask(string pTaskID)
        {
            _log.DebugFormat("YouTrackService: GetPlasticTask {0}", pTaskID);

            ensureAuthenticated();

            //TODO: implement this as async.

            try
            {
                var issue = _ytIssues.GetIssue(pTaskID).Result;
                if (issue != null)
                    return hydratePlasticTaskFromIssue(issue);
            }
            catch (Exception ex)
            {
                _log.Warn(string.Format("YouTrackService: Failed to fetch youtrack issue '{0}' due to error.", pTaskID), ex);
            }

            return new PlasticTask { Id = pTaskID, CanBeLinked = false };
        }

        public IEnumerable<PlasticTask> GetPlasticTasks(string[] pTaskIDs)
        {
            ensureAuthenticated();

            _log.DebugFormat("YouTrackService: GetPlasticTasks - {0} task ID(s) supplied", pTaskIDs.Length);

            var result = pTaskIDs.Select(pTaskID => GetPlasticTask(pTaskID)).AsParallel();
            return result;
        }

        public IEnumerable<PlasticTask> GetUnresolvedPlasticTasks(string pAssignee = "", int pMaxCount = 500)
        {
            ensureAuthenticated();

            try
            {
                var assignee = applyUserMapping(pAssignee);
                var searchString = string.Format
                ("{0}{1}", string.IsNullOrWhiteSpace(assignee) ? string.Empty : string.Format("for: {0} ", assignee),
                    _config.CreateBranchIssueQuery);

                var issues = _ytIssues.GetIssues(searchString, take: pMaxCount).Result.ToList();
                if (!issues.Any())
                    return new List<PlasticTask>();

                var tasks = issues.Select(pIssue => hydratePlasticTaskFromIssue(pIssue));
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

        public YoutrackUser GetAuthenticatedUser()
        {
            ensureAuthenticated();

            var authUser = _ytConnection.CreateUserManagementService().GetUser(_config.UserId).Result;
            var user = new YoutrackUser(authUser.Username, authUser.FullName, authUser.Email);
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

        public static void VerifyConfiguration(YouTrackExtensionConfigFacade pConfig)
        {
            validateConfig(pConfig);

            try
            {
                var testConnection = getServiceConnection(pConfig);
                testConnection.CreateIssuesService().GetIssueCount().Wait(1000);
            }
            catch (Exception e)
            {
                _log.Warn(string.Format("Failed to verify configuration against host '{0}'.", pConfig.HostUri), e);
                throw new ApplicationException(string.Format("Failed to authenticate against the host. Message: {0}", e.Message), e);
            }
        }

        public static string GetBranchCreationMessage()
        {
            return "{color:darkgreen}*PSCM - BRANCH CREATED*{color}";
        }

        public async void EnsureIssueInProgress(string pIssueID)
        {
            ensureAuthenticated();

            try
            {
                var issue = await _ytIssues.GetIssue(pIssueID);
                if (issue == null)
                    throw new NullReferenceException(string.Format("Unable to find issue by ID {0}.", pIssueID));
                
                

                if (issue.GetField("State").AsString() != "In Progress")
                    await _ytIssues.ApplyCommand(pIssueID, "State: In Progress", GetBranchCreationMessage());
                else
                    _log.InfoFormat("Issue '{0}' already marked in-progress.", pIssueID);
            }
            catch (Exception ex)
            {
                _log.Error("Unable to mark issue '{0}' in-progress.", ex);
                throw new ApplicationException("Error occurred marking issue in-progress.", ex);
            }
        }

        public static string FormatComment(string pHost, string pRepository, Uri pWebGui, string pBranch,
            long pChangeSetId, string pComment, Guid pChangeSetGuid)
        {
            var nl = Environment.NewLine;
            var mdComment = $"{{color:darkgreen}}*PSCM - CODE COMMIT #{pChangeSetId}*{{color}}";

            var changeSetUriBuilder = new UriBuilder(pWebGui);
            if (string.IsNullOrEmpty(changeSetUriBuilder.Scheme) ||
                (!changeSetUriBuilder.Scheme.Equals("https", StringComparison.CurrentCultureIgnoreCase) &&
                 !changeSetUriBuilder.Scheme.Equals("http", StringComparison.CurrentCultureIgnoreCase)))
                changeSetUriBuilder.Scheme = "http";

            changeSetUriBuilder.Path = $"webui/repos/{pRepository}/diff/changeset/{pChangeSetGuid}";

            var hostName = pHost.StartsWith("localhost", StringComparison.CurrentCultureIgnoreCase) ||
                           pHost.StartsWith("127.0.0.", StringComparison.CurrentCultureIgnoreCase)
                ? Environment.MachineName + (pHost.Contains(":") ? pHost.Substring(pHost.IndexOf(":")) : "")
                : pHost;

            var tildes = "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~";

            var commentBuilder = new StringBuilder();
            commentBuilder.Append($"{pComment}{nl}{nl}");
            commentBuilder.Append($"{tildes}{nl}");
            commentBuilder.Append($"[{mdComment}|{changeSetUriBuilder}]{nl}");
            //commentBuilder.Append($"{{monospace}}");
            commentBuilder.Append($"{pChangeSetGuid} @ {pBranch} @ {pRepository} @ {hostName}");
            //commentBuilder.Append($"{{monospace}}");

            return commentBuilder.ToString();
        }

        public async void AddCommentToIssue
            (string pIssueID, string pRepositoryServer, string pRepository, Uri pWebGui, string pBranch, long pChangeSetId, string pComment, Guid pChangeSetGuid)
        {
            ensureAuthenticated();

            if (await _ytIssues.Exists(pIssueID) == false) return;

            try
            {
                var completeComment = FormatComment(pRepositoryServer, pRepository, pWebGui, pBranch, pChangeSetId, pComment, pChangeSetGuid);
                _ytIssues.ApplyCommand(pIssueID, "comment", completeComment, false).Wait(1000);
            }
            catch (Exception ex)
            {
                _log.Error("Unable to add comment to issue '{0}'", ex);
            }
        }

        public async void AssignIssue(string pIssueID, string pAssignee, bool pAddComment = true)
        {
            ensureAuthenticated();

            try
            {
                var mappedAssignee = applyUserMapping(pAssignee);
                var issue = await _ytIssues.GetIssue(pIssueID);
                if (issue == null)
                    throw new NullReferenceException(string.Format("Unable to find issue by ID {0}.", pIssueID));

                var currentAssignee = string.Empty;
                if (doesPropertyExist(issue, "Assignee"))
                    currentAssignee = getAssignee(issue).UserName;

                if (!string.Equals(currentAssignee, mappedAssignee, StringComparison.InvariantCultureIgnoreCase))
                {
                    var comment = $"Assigned by PlasticSCM to user '{mappedAssignee}'.";
                    await _ytIssues.ApplyCommand(pIssueID, string.Format("for {0}", mappedAssignee),
                        pAddComment ? comment : string.Empty);
                }
            }
            catch (Exception ex)
            {
                _log.Error(string.Format("Unable to assign issue '{0}' to '{1}'.", pIssueID, pAssignee), ex);
                throw new ApplicationException("Error occurred marking issue assigned.", ex);
            }
        }

        #region Support Methods


        private static Connection getServiceConnection(YouTrackExtensionConfigFacade pConfig)
        {
            var password = pConfig.GetDecryptedPassword();
            var serverUrl = pConfig.HostUri.ToString();
            return password.StartsWith("perm:")
                ? (Connection)new BearerTokenConnection(serverUrl, password)
                : new UsernamePasswordConnection(serverUrl, pConfig.UserId, password);
        }

        /// <summary>
        /// This method will take the mapping setting and allow mapping different authentication methods' usernames to issue username. 
        /// </summary>
        /// <param name="pAssignee">The username specified for configuration value UserId</param>
        /// <returns>the username to pass to youtrack to filter for issues assignee.</returns>
        private string applyUserMapping(string pAssignee)
        {
            if (string.IsNullOrEmpty(pAssignee))
                return string.Empty;

            return getValueFromKVPStringList(_config.UsernameMapping, pAssignee);
        }

        private string getValueFromKVPStringList(string pCSVList, string pKeyName)
        {
            try
            {
                var dictionary = pCSVList.Split(';')
                    .Select(pMapping => new KeyValuePair<string, string>(pMapping.Split(':')[0], pMapping.Split(':')[1]))
                    .ToDictionary(p => p.Key, p => p.Value);
                var value = dictionary[pKeyName];

                return string.IsNullOrEmpty(value) ? pKeyName : value;
            }
            catch (Exception e)
            {
                _log.Error("Error occurred trying to apply user mappings.", e);
                return pKeyName;
            }
        }

        public PlasticTask hydratePlasticTaskFromIssue(Issue pIssue)
        {
            if (pIssue == null)
                throw new ArgumentNullException("pIssue");

            var result = new PlasticTask();
            result.Id = pIssue.Id;
            var title = pIssue.Summary;
            var state = pIssue.GetField("state").AsCollection().First();
            result.Title = getBranchTitle(state, title);
            result.Status = state;

            try
            {
                result.Owner = getAssignee(pIssue).UserName;
            }
            catch (NullReferenceException)
            {
                result.Owner = "Unassigned";
            }

            if (pIssue.GetField("description") != null)
                result.Description = pIssue.GetField("description").AsString();

            result.CanBeLinked = true;
            return result;
        }

        private static Assignee getAssignee(Issue pIssue)
        {
            var field = pIssue.GetField("Assignee");
            if (field == null)
                throw new NullReferenceException("Ticket doesn't contain field 'Assignee' as expected.");

            var assignees = (List<Assignee>)field.Value;
            return assignees[0];
        }

        private void ensureAuthenticated()
        {
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

        private static void validateConfig(IYouTrackExtensionConfigFacade pConfig)
        {
            if (pConfig.HostUri.Host.Equals("issues.domain.com", StringComparison.InvariantCultureIgnoreCase))
                return;

            /*//validate URL
            var testConnection = new Connection(pConfig.HostUri.DnsSafeHost, pConfig.HostUri.Port, pConfig.UseSsl);
            testConnection.Head("/rest/user/login");*/

            if (pConfig.HostUri == null)
                throw new ApplicationException(string.Format("YouTrack setting '{0}' cannot be null or empty!", ConfigParameterNames.HostUri));

            throwErrorIfRequiredStringSettingIsMissing(pConfig.BranchPrefix, ConfigParameterNames.BranchPrefix);
            throwErrorIfRequiredStringSettingIsMissing(pConfig.UserId, ConfigParameterNames.UserId);
            throwErrorIfRequiredStringSettingIsMissing(pConfig.Password, ConfigParameterNames.Password);
        }

        private static void throwErrorIfRequiredStringSettingIsMissing(string pSettingValue, string pSettingName)
        {
            if (string.IsNullOrWhiteSpace(pSettingValue))
                throw new ApplicationException(string.Format("YouTrack setting '{0}' cannot be null or empty!", pSettingName));
        }

        private static bool doesPropertyExist(Issue pIssue, string pPropertyName)
        {
            return pIssue.GetField(pPropertyName) != null;
        }

        #endregion
    }
}