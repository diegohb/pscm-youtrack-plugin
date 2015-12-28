// *************************************************
// MMG.PlasticExtensions.YouTrackPlugin.YouTrackExtensionConfigFacade.cs
// Last Modified: 12/27/2015 3:47 PM
// Modified By: Bustamante, Diego (bustamd1)
// *************************************************

namespace MMG.PlasticExtensions.YouTrackPlugin
{
    using System;
    using System.Collections.Generic;
    using Codice.Client.IssueTracker;
    using log4net;

    public class YouTrackExtensionConfigFacade
    {
        private static readonly ILog _log = LogManager.GetLogger("extensions");
        private readonly IssueTrackerConfiguration _config;
        private readonly Uri _hostUri;
        private readonly string _branchPrefix;
        private readonly string _userID;
        private readonly string _password;
        private readonly bool _showIssueStateInTitle;
        private readonly string _closedIssueStates;
        private readonly bool _defaultInit;


        internal YouTrackExtensionConfigFacade()
        {
            _branchPrefix = "yt_";
            _hostUri = new Uri("http://issues.domain.com");
            _userID = "myusername";
            _password = "";
            _showIssueStateInTitle = false;
            _closedIssueStates = "Completed";

            _defaultInit = true;
            _log.Debug("YouTrackExtensionConfigFacade: empty ctor called");
        }

        public YouTrackExtensionConfigFacade(IssueTrackerConfiguration pConfig)
        {
            _config = pConfig;

            _branchPrefix = getValidParameterValue(ConfigParameterNames.BranchPrefix);
            var hostValue = getValidParameterValue(ConfigParameterNames.Host);
            if (!Uri.TryCreate(hostValue, UriKind.Absolute, out _hostUri))
                throw new ApplicationException(string.Format("Unable to parse host URL '{0}'.", hostValue));

            _userID = getValidParameterValue(ConfigParameterNames.UserID);
            _password = getValidParameterValue(ConfigParameterNames.Password);
            _showIssueStateInTitle = bool.Parse(getValidParameterValue(ConfigParameterNames.ShowIssueStateInBranchTitle, "false"));
            _closedIssueStates = getValidParameterValue(ConfigParameterNames.ClosedIssueStates, "Completed");

            _defaultInit = false;
            _log.Debug("YouTrackExtensionConfigFacade: ctor called");
        }

        public string BranchPrefix
        {
            get { return _branchPrefix; }
        }

        public Uri Host
        {
            get { return _hostUri; }
        }

        public string UserID
        {
            get { return _userID; }
        }

        public string Password
        {
            get { return _password; }
        }

        public bool UseSSL
        {
            get { return _hostUri.Scheme == "https"; }
        }

        public bool ShowIssueStateInBranchTitle
        {
            get { return _showIssueStateInTitle; }
        }

        /// <summary>
        /// Issue state(s) to not display in branch title when ShowIssueStateInBranchTitle = true.
        /// </summary>
        /// <remarks>Use commas to separate multiple states.</remarks>
        public string IgnoreIssueStateForBranchTitle
        {
            get { return _closedIssueStates; }
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
                    Name = ConfigParameterNames.BranchPrefix,
                    Value = BranchPrefix,
                    Type = IssueTrackerConfigurationParameterType.BranchPrefix,
                    IsGlobal = true
                });
            parameters.Add
                (new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.Host,
                    Value = Host.ToString(),
                    Type = IssueTrackerConfigurationParameterType.Host,
                    IsGlobal = true
                });

            parameters.Add
                (new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.UserID,
                    Value = UserID,
                    Type = IssueTrackerConfigurationParameterType.User,
                    IsGlobal = false
                });
            parameters.Add
                (new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.Password,
                    Value = Password,
                    Type = IssueTrackerConfigurationParameterType.Text, //TODO: figure out how to use Password setting (IssueTrackerConfigurationParameterType.Password)
                    IsGlobal = false
                });
            parameters.Add
                (new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.ShowIssueStateInBranchTitle,
                    Value = ShowIssueStateInBranchTitle.ToString(),
                    Type = IssueTrackerConfigurationParameterType.Boolean,
                    IsGlobal = false
                });
            parameters.Add
                (new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.ClosedIssueStates,
                    Value = IgnoreIssueStateForBranchTitle,
                    Type = IssueTrackerConfigurationParameterType.Text,
                    IsGlobal = false
                });

            return parameters;
        }

        private string getValidParameterValue(string pParamName, string pDefaultValue = "")
        {
            if (_config == null)
                throw new ApplicationException("The configuration has not yet been initialized!");

            var configValue = _config.GetValue(pParamName);

            return string.IsNullOrEmpty(configValue)
                ? pDefaultValue
                : configValue;
        }
    }
}