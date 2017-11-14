//// *************************************************
//// MMG.PlasticExtensions.YouTrackPlugin.YouTrackExtensionConfigFacade.cs
//// Last Modified: 03/28/2016 1:57 PM
//// Modified By: Green, Brett (greenb1)
//// *************************************************

//namespace MMG.PlasticExtensions.YouTrackPlugin
//{
//    using System;
//    using System.Collections.Generic;
//    using Codice.Client.IssueTracker;
//    using Codice.Utils;
//    using log4net;

//    public class oldYouTrackExtensionConfigFacade //: IYouTrackExtensionConfigFacade
//    {
//        private static readonly ILog _log = LogManager.GetLogger("extensions");
//        private readonly IssueTrackerConfiguration _config;
//        private readonly Uri _hostUri;
//        private readonly Uri _webGUI_RootURL;
//        private readonly string _branchPrefix;
//        private readonly string _userID;
//        private readonly string _password;
//        private readonly bool _showIssueStateInTitle;
//        private readonly bool _postCommentsToTickets;
//        private readonly string _closedIssueStates;
//        private readonly string _usernameMapping;


//        internal YouTrackExtensionConfigFacade()
//        {
//            _branchPrefix = "yt_";
//            _hostUri = new Uri("http://issues.domain.com");
//            _userID = "";
//            _password = "";
//            _showIssueStateInTitle = false;
//            _closedIssueStates = "Completed";
//            _usernameMapping = "";
//            _webGUI_RootURL = new Uri("http://plastic-gui.domain.com");
//            _log.Debug("YouTrackExtensionConfigFacade: empty ctor called");
//        }

//        public YouTrackExtensionConfigFacade(IssueTrackerConfiguration pConfig)
//        {
//            _config = pConfig;

//            _branchPrefix = getValidParameterValue(ConfigParameterNames.BranchPrefix);
//            _hostUri = validateStringParamAsUri(ConfigParameterNames.HostUri);
//            _webGUI_RootURL = validateStringParamAsUri(ConfigParameterNames.WebGuiRootUrl);
            
//            _userID = getValidParameterValue(ConfigParameterNames.UserId);
//            _password = getValidParameterValue(ConfigParameterNames.Password);
//            _showIssueStateInTitle =
//                bool.Parse(getValidParameterValue(ConfigParameterNames.ShowIssueStateInBranchTitle, "false"));
//            _postCommentsToTickets =
//                bool.Parse(getValidParameterValue(ConfigParameterNames.PostCommentsToTickets, "true"));
//            _closedIssueStates =
//                getValidParameterValue(ConfigParameterNames.IgnoreIssueStateForBranchTitle, "Completed");
//            _usernameMapping = getValidParameterValue(ConfigParameterNames.UsernameMapping);

//            _log.Debug("YouTrackExtensionConfigFacade: ctor called");
//        }

//        #region Properties

//        public string BranchPrefix { get { return _branchPrefix; } }

//        public Uri HostUri
//        {
//            get { return _hostUri; }
//        }

//        public string WebGuiRootUrl { get { return _webGUI_RootURL.ToString(); } }

//        public string UsernameMapping { get { return _usernameMapping; } }

//        public string UserId { get { return _userID; } }

//        public string Password { get { return _password; } }


//        public bool UseSsl
//        {
//            get { return _hostUri.Scheme == "https"; }
//        }

//        public bool ShowIssueStateInBranchTitle { get { return _showIssueStateInTitle; } }

//        public bool PostCommentsToTickets { get { return _postCommentsToTickets; } }

//        /// <summary>
//        /// Issue state(s) to not display in branch title when ShowIssueStateInBranchTitle = true.
//        /// </summary>
//        /// <remarks>Use commas to separate multiple states.</remarks>
//        public string IgnoreIssueStateForBranchTitle { get { return _closedIssueStates; } }
        
//        public ExtensionWorkingMode WorkingMode
//        {
//            get
//            {
//                if (_config == null)
//                    return ExtensionWorkingMode.TaskOnBranch;

//                return _config.WorkingMode == ExtensionWorkingMode.None
//                    ? ExtensionWorkingMode.TaskOnBranch
//                    : _config.WorkingMode;
//            }
//        }
//        #endregion

//        public List<IssueTrackerConfigurationParameter> GetYouTrackParameters()
//        {
//            var parameters = new List<IssueTrackerConfigurationParameter>();

//            parameters.Add
//                (new IssueTrackerConfigurationParameter
//                {
//                    Name = ConfigParameterNames.BranchPrefix,
//                    Value = BranchPrefix,
//                    Type = IssueTrackerConfigurationParameterType.BranchPrefix,
//                    IsGlobal = true
//                });
//            parameters.Add
//                (new IssueTrackerConfigurationParameter
//                {
//                    Name = ConfigParameterNames.HostUri,
//                    Value = HostUri.ToString(),
//                    Type = IssueTrackerConfigurationParameterType.HostUri,
//                    IsGlobal = true
//                });
//            parameters.Add
//                (new IssueTrackerConfigurationParameter
//                {
//                    Name = ConfigParameterNames.WebGuiRootUrl,
//                    Value = WebGuiRootUrl,
//                    Type = IssueTrackerConfigurationParameterType.Text,
//                    IsGlobal = true
//                });
//            parameters.Add
//                (new IssueTrackerConfigurationParameter
//                {
//                    Name = ConfigParameterNames.UserId,
//                    Value = UserId,
//                    Type = IssueTrackerConfigurationParameterType.User,
//                    IsGlobal = false
//                });
//            parameters.Add
//                (new IssueTrackerConfigurationParameter
//                {
//                    Name = ConfigParameterNames.UsernameMapping,
//                    Value = UsernameMapping,
//                    Type = IssueTrackerConfigurationParameterType.Text,
//                    IsGlobal = true
//                });
//            parameters.Add
//                (new IssueTrackerConfigurationParameter
//                {
//                    Name = ConfigParameterNames.Password,
//                    Value = Password,
//                    Type = IssueTrackerConfigurationParameterType.Password,
//                    IsGlobal = false
//                });
//            parameters.Add
//                (new IssueTrackerConfigurationParameter
//                {
//                    Name = ConfigParameterNames.ShowIssueStateInBranchTitle,
//                    Value = ShowIssueStateInBranchTitle.ToString(),
//                    Type = IssueTrackerConfigurationParameterType.Boolean,
//                    IsGlobal = false
//                });
//            parameters.Add
//                (new IssueTrackerConfigurationParameter
//                {
//                    Name = ConfigParameterNames.PostCommentsToTickets,
//                    Value = PostCommentsToTickets.ToString(),
//                    Type = IssueTrackerConfigurationParameterType.Boolean,
//                    IsGlobal = true
//                });
//            parameters.Add
//                (new IssueTrackerConfigurationParameter
//                {
//                    Name = ConfigParameterNames.IgnoreIssueStateForBranchTitle,
//                    Value = IgnoreIssueStateForBranchTitle,
//                    Type = IssueTrackerConfigurationParameterType.Text,
//                    IsGlobal = false
//                });


//            return parameters;
//        }

//        public string GetDecryptedPassword()
//        {
//            if (_config == null)
//                throw new ApplicationException("The configuration has not yet been initialized!");

//            if (string.IsNullOrEmpty(Password))
//                throw new ApplicationException("Password value can not be empty!");

//            var decryptedPassword = CryptoServices.GetDecryptedPassword(Password);
//            return decryptedPassword;
//        }

//        #region Support Methods

//        private string getValidParameterValue(string pParamName, string pDefaultValue = "")
//        {
//            if (_config == null)
//                throw new ApplicationException("The configuration has not yet been initialized!");

//            var configValue = _config.GetValue(pParamName);

//            return string.IsNullOrEmpty(configValue)
//                ? pDefaultValue
//                : configValue;
//        }
        
//        private Uri validateStringParamAsUri(string pConfigParamName)
//        {
//            Uri paramUri;
//            var paramValue = getValidParameterValue(pConfigParamName);
//            if (!Uri.TryCreate(paramValue, UriKind.Absolute, out paramUri))
//                throw new ApplicationException(string.Format("Unable to parse parameter '{0}' with value '{1}' as valid Uri.", pConfigParamName, paramValue));

//            return paramUri;
//        }

//        #endregion
//    }
//}