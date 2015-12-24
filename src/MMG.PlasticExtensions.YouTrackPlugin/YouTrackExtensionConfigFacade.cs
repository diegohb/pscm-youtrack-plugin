// *************************************************
// MMG.PlasticExtensions.YouTrackPlugin.YouTrackExtensionConfigFacade.cs
// Last Modified: 12/24/2015 2:29 PM
// Modified By: Bustamante, Diego (bustamd1)
// *************************************************

namespace MMG.PlasticExtensions.YouTrackPlugin
{
    using System;
    using System.Collections.Generic;
    using Codice.Client.IssueTracker;

    public class YouTrackExtensionConfigFacade
    {
        private readonly IssueTrackerConfiguration _config;
        private Uri _hostUri;

        public YouTrackExtensionConfigFacade(IssueTrackerConfiguration pConfig)
        {
            _config = pConfig;
        }

        internal class ParameterNames
        {
            internal const string BranchPrefix = "Branch Name Prefix";
            internal const string UserID = "User ID";
            internal const string Password = "Password";
            internal const string Host = "Host";
            internal const string ShowIssueStateInBranchTitle = "Show issues state in branch title";
            internal const string ClosedIssueStates = "Issue states considered closed";
        }

        public string BranchPrefix
        {
            get { return getValidParameterValue(ParameterNames.BranchPrefix); }
        }

        public Uri Host
        {
            get
            {
                var hostValue = getValidParameterValue(ParameterNames.Host);
                if (!Uri.TryCreate(hostValue, UriKind.Absolute, out _hostUri))
                    throw new ApplicationException(string.Format("Unable to parse host URL '{0}'.", hostValue));

                return _hostUri;
            }
        }

        public string UserID
        {
            get { return getValidParameterValue(ParameterNames.UserID); }
        }

        public string Password
        {
            get { return getValidParameterValue(ParameterNames.Password); }
        }

        public bool UseSSL
        {
            get { return _hostUri.Scheme == "https"; }
        }

        public bool ShowIssueStateInBranchTitle
        {
            get { return bool.Parse(getValidParameterValue(ParameterNames.ShowIssueStateInBranchTitle, "false")); }
        }

        /// <summary>
        /// Issue state(s) to not display in branch title when ShowIssueStateInBranchTitle = true.
        /// </summary>
        /// <remarks>Use commas to separate multiple states.</remarks>
        public string IgnoreIssueStateForBranchTitle
        {
            get { return getValidParameterValue(ParameterNames.ClosedIssueStates, "Completed"); }
        }

        public ExtensionWorkingMode WorkingMode
        {
            get
            {
                if (_config == null)
                    return ExtensionWorkingMode.TaskOnBranch;

                return _config.WorkingMode == ExtensionWorkingMode.None
                    ? ExtensionWorkingMode.TaskOnBranch
                    : _config.WorkingMode;
            }
        }

        public List<IssueTrackerConfigurationParameter> GetYouTrackParameters()
        {
            var parameters = new List<IssueTrackerConfigurationParameter>();

            parameters.Add
                (new IssueTrackerConfigurationParameter
                {
                    Name = ParameterNames.BranchPrefix,
                    Value = BranchPrefix,
                    Type = IssueTrackerConfigurationParameterType.BranchPrefix,
                    IsGlobal = true
                });
            parameters.Add
                (new IssueTrackerConfigurationParameter
                {
                    Name = ParameterNames.Host,
                    Value = Host.ToString(),
                    Type = IssueTrackerConfigurationParameterType.Host,
                    IsGlobal = true
                });

            parameters.Add
                (new IssueTrackerConfigurationParameter
                {
                    Name = ParameterNames.UserID,
                    Value = UserID,
                    Type = IssueTrackerConfigurationParameterType.User,
                    IsGlobal = false
                });
            parameters.Add
                (new IssueTrackerConfigurationParameter
                {
                    Name = ParameterNames.Password,
                    Value = Password,
                    Type = IssueTrackerConfigurationParameterType.Password,
                    IsGlobal = false
                });
            parameters.Add
                (new IssueTrackerConfigurationParameter
                {
                    Name = ParameterNames.ShowIssueStateInBranchTitle,
                    Value = ShowIssueStateInBranchTitle.ToString(),
                    Type = IssueTrackerConfigurationParameterType.Boolean,
                    IsGlobal = false
                });
            parameters.Add
                (new IssueTrackerConfigurationParameter
                {
                    Name = ParameterNames.ClosedIssueStates,
                    Value = IgnoreIssueStateForBranchTitle,
                    Type = IssueTrackerConfigurationParameterType.Text,
                    IsGlobal = false
                });

            return parameters;
        }

        private string getValidParameterValue(string pParamName, string pDefaultValue = "")
        {
            var configValue = _config.GetValue(pParamName);

            if (string.IsNullOrEmpty(pDefaultValue) && string.IsNullOrEmpty(configValue))
                throw new ApplicationException(string.Format("The configuration value for '{0}' is required but was not provided!", pParamName));

            return string.IsNullOrEmpty(configValue)
                ? pDefaultValue
                : configValue;
        }
    }
}