using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Codice.Client.IssueTracker;
using log4net;
using MMG.PlasticExtensions.YouTrackPlugin.Models;
using YouTrackSharp.Issues;

namespace MMG.PlasticExtensions.YouTrackPlugin
{
    public class PlasticYouTrackTranslationService
    {
        private static readonly ILog _log = LogManager.GetLogger("extensions");
        private readonly string _emailDomain;
        private readonly string _ignoreIssueStateForBranchTitle;
        private readonly bool _showIssueStateInBranchTitle;
        private readonly Dictionary<string, string> _stateTransitions;
        private readonly Dictionary<string, string> _usernameMapping;

        public PlasticYouTrackTranslationService(IYouTrackExtensionConfigFacade pConfig)
        {
            _usernameMapping = convertStringListToKVPDictionary(pConfig.UsernameMapping);
            _stateTransitions = convertStringListToKVPDictionary(pConfig.CreateBranchTransitions);
            _showIssueStateInBranchTitle = pConfig.ShowIssueStateInBranchTitle;
            _ignoreIssueStateForBranchTitle = pConfig.IgnoreIssueStateForBranchTitle;
            _emailDomain = "company.com"; //TODO: pConfig.EmailDomain;
        }

        public PlasticTask GetPlasticTaskFromIssue(Issue pIssue)
        {
            if (pIssue == null)
                throw new ArgumentNullException("pIssue");

            if (string.IsNullOrEmpty(pIssue.Id) || string.IsNullOrEmpty(pIssue.Summary))
            {
                _log.WarnFormat("The youtrack issue returned a null ID or Summary field.");
                return new PlasticTask() { CanBeLinked = false };
            }

            var result = new PlasticTask();
            result.Id = pIssue.Id;
            var title = pIssue.Summary;

            if (doesPropertyExist(pIssue, "State"))
            {
                var state = pIssue.GetField("state").AsCollection().Single();
                result.Title = getBranchTitle(state, title);
                result.Status = state;
            }
            else
            {
                result.Title = title;
            }

            result.Owner = GetPlasticUserFromYouTrackIssue(pIssue, "Assignee");

            if (doesPropertyExist(pIssue, "description"))
                result.Description = pIssue.GetField("description").AsString();

            result.CanBeLinked = true;
            return result;
        }

        /// <summary>
        ///     This method will take the mapping setting and allow mapping different authentication methods' usernames to issue
        ///     username.
        /// </summary>
        /// <param name="pAssignee">The username specified for configuration value UserId</param>
        /// <returns>the username to pass to youtrack to filter for issues assignee.</returns>
        public string GetYouTrackUsernameFromPlasticUsername(string pAssignee)
        {
            if (string.IsNullOrEmpty(pAssignee))
                return string.Empty;

            if (_usernameMapping.Count == 0)
                return pAssignee;

            return _usernameMapping.TryGetValue(pAssignee, out var value) ? value : pAssignee;
        }

        public string GetPlasticUsernameFromYouTrackUser(YoutrackUser pUser)
        {
            if (pUser == null)
                return string.Empty;

            var username = pUser.Username;
            var value = _usernameMapping.ContainsValue(username)
                ? _usernameMapping.Single(kv =>
                    kv.Value.Equals(pUser.Username, StringComparison.InvariantCultureIgnoreCase)).Key
                : username;
            return value;
        }

        public YoutrackUser GetAssigneeFromYouTrackIssue(Issue pIssue, string pUserFieldName = "Assignee")
        {
            if (!doesPropertyExist(pIssue, pUserFieldName))
                return new YoutrackUser("Unassigned", "Unassigned", string.Empty);

            var field = pIssue.GetField(pUserFieldName);
            var assignees = (List<Assignee>)field.Value;
            var youtrackUser = assignees[0];
            return new YoutrackUser(youtrackUser.UserName, youtrackUser.FullName,
                $"{youtrackUser.UserName}@{_emailDomain}");
        }

        public string GetPlasticUserFromYouTrackIssue(Issue pIssue, string pUserFieldName)
        {
            if (!doesPropertyExist(pIssue, pUserFieldName))
                return string.Empty;

            var youtrackUser = GetAssigneeFromYouTrackIssue(pIssue, pUserFieldName);
            return GetPlasticUsernameFromYouTrackUser(youtrackUser);
        }

        public string GetMarkAsOpenTransitionFromIssue(Issue pIssue)
        {
            if (!doesPropertyExist(pIssue, "State"))
                return string.Empty;

            var issueCurrentState = pIssue.GetField("State").AsString();

            if (_stateTransitions.ContainsKey(issueCurrentState))
            {
                var transitionCommand = _stateTransitions[issueCurrentState];
                return transitionCommand;
            }

            return string.Empty;
        }

        #region Support Methods

        private static Dictionary<string, string> convertStringListToKVPDictionary(string pCSVList)
        {
            if (string.IsNullOrEmpty(pCSVList))
                return new Dictionary<string, string>();

            var dictionary = pCSVList.Split(';')
                .Select
                (pMapping =>
                {
                    var strings = pMapping.Split(':');
                    return new KeyValuePair<string, string>
                    (strings[0], strings.Length > 2
                        ? pMapping.Replace($"{strings[0]}:",
                            string.Empty) //accounts for commands like "state:in progress"
                        : strings[1]); //works for verbs in statemachine workflows
                })
                .ToDictionary(p => p.Key, p => p.Value);
            return dictionary;
        }

        private string getBranchTitle(string pIssueState, string pIssueSummary)
        {
            //if feature is disabled, return ticket summary.
            if (!_showIssueStateInBranchTitle)
                return pIssueSummary;

            //if feature is enabled but no states are ignored, return default format.
            if (string.IsNullOrEmpty(_ignoreIssueStateForBranchTitle.Trim()))
                return $"{pIssueSummary} [{pIssueState}]";

            //otherwise, consider the ignore list.
            var ignoreStates = new ArrayList(_ignoreIssueStateForBranchTitle.Trim().Split(','));
            return ignoreStates.Contains(pIssueState)
                ? pIssueSummary
                : $"{pIssueSummary} [{pIssueState}]";
        }

        private static bool doesPropertyExist(Issue pIssue, string pPropertyName)
        {
            return pIssue.GetField(pPropertyName) != null;
        }

        #endregion
    }
}