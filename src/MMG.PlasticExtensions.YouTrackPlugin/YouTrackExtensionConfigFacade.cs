namespace MMG.PlasticExtensions.YouTrackPlugin
{
    #region

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text;
    using Codice.Client.IssueTracker;
    using Codice.Utils;
    using log4net;

    #endregion

    public class YouTrackExtensionConfigFacade : IYouTrackExtensionConfigFacade
    {
        private static readonly ILog _log = LogManager.GetLogger("extensions");

        #region Ctors

        internal YouTrackExtensionConfigFacade()
        {
            _log.Debug("YouTrackExtensionConfigFacade: empty ctor called");
            loadDefaultValues();
            _log.Debug("YouTrackExtensionConfigFacade: empty ctor completed");
        }

        public YouTrackExtensionConfigFacade(IssueTrackerConfiguration pConfig) : this()
        {
            _log.Debug("YouTrackExtensionConfigFacade: configured ctor called");
            Config = pConfig;
            BranchPrefix = getValidParameterValue(Config, ConfigParameterNames.BranchPrefix, pDefaultValue: BranchPrefix);
            HostUri = getValidParameterValue(Config, ConfigParameterNames.HostUri, pDefaultValue: HostUri, converter: new UriTypeConverter());
            UserId = getValidParameterValue(Config, ConfigParameterNames.UserId, pDefaultValue: UserId);
            Password = getValidParameterValue(Config, ConfigParameterNames.Password, pDefaultValue: Password);
            ShowIssueStateInBranchTitle = getValidParameterValue
                (Config, ConfigParameterNames.ShowIssueStateInBranchTitle, pDefaultValue: ShowIssueStateInBranchTitle);
            PostCommentsToTickets = getValidParameterValue(Config, ConfigParameterNames.PostCommentsToTickets, pDefaultValue: PostCommentsToTickets);
            IgnoreIssueStateForBranchTitle = getValidParameterValue
                (Config, ConfigParameterNames.ClosedIssueStates, pDefaultValue: IgnoreIssueStateForBranchTitle);
            UsernameMapping = getValidParameterValue(Config, ConfigParameterNames.UsernameMapping, pDefaultValue: UsernameMapping);
            WebGuiRootUrl = getValidParameterValue
                (Config, ConfigParameterNames.WebGuiRootUrl, pDefaultValue: WebGuiRootUrl, converter: new UriTypeConverter());
            WorkingMode = getValidParameterValue(Config, nameof(ExtensionWorkingMode), pDefaultValue: ExtensionWorkingMode.TaskOnBranch);
            CreateBranchIssueQuery = getValidParameterValue
                (Config, ConfigParameterNames.CreateBranchIssueQuery, pDefaultValue: CreateBranchIssueQuery);
            CreateBranchTransitions = getValidParameterValue
                (Config, ConfigParameterNames.CreateBranchTransitions, pDefaultValue: CreateBranchTransitions);
            _log.Debug("YouTrackExtensionConfigFacade: configured ctor completed");
        }

        #endregion

        #region Properties

        public virtual IssueTrackerConfiguration Config { get; private set; }
        public virtual Uri HostUri { get; private set; }
        public virtual Uri WebGuiRootUrl { get; private set; }
        public virtual string BranchPrefix { get; private set; }
        public virtual string UserId { get; private set; }
        public virtual string Password { get; private set; }
        public virtual bool ShowIssueStateInBranchTitle { get; private set; }
        public virtual bool PostCommentsToTickets { get; private set; }
        public virtual string IgnoreIssueStateForBranchTitle { get; private set; }
        public virtual string CreateBranchIssueQuery { get; private set; }
        public virtual string CreateBranchTransitions { get; private set; }
        public virtual string UsernameMapping { get; private set; }
        public virtual bool UseSsl => HostUri.Scheme.Equals("https", StringComparison.CurrentCultureIgnoreCase);
        public virtual ExtensionWorkingMode WorkingMode { get; private set; }

        #endregion

        public List<IssueTrackerConfigurationParameter> GetYouTrackParameters()
        {
            return new List<IssueTrackerConfigurationParameter>
            {
                new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.BranchPrefix,
                    Value = BranchPrefix,
                    Type = IssueTrackerConfigurationParameterType.BranchPrefix,
                    IsGlobal = true
                },
                new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.HostUri,
                    Value = HostUri.ToString(),
                    Type = IssueTrackerConfigurationParameterType.Host,
                    IsGlobal = true
                },
                new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.WebGuiRootUrl,
                    Value = WebGuiRootUrl.ToString(),
                    Type = IssueTrackerConfigurationParameterType.Text,
                    IsGlobal = true
                },
                new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.UserId,
                    Value = UserId,
                    Type = IssueTrackerConfigurationParameterType.User,
                    IsGlobal = false
                },
                new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.UsernameMapping,
                    Value = UsernameMapping,
                    Type = IssueTrackerConfigurationParameterType.Text,
                    IsGlobal = true
                },
                new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.Password,
                    Value = Password,
                    Type = IssueTrackerConfigurationParameterType.Password,
                    IsGlobal = false
                },
                new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.ShowIssueStateInBranchTitle,
                    Value = ShowIssueStateInBranchTitle.ToString(),
                    Type = IssueTrackerConfigurationParameterType.Boolean,
                    IsGlobal = false
                },
                new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.PostCommentsToTickets,
                    Value = PostCommentsToTickets.ToString(),
                    Type = IssueTrackerConfigurationParameterType.Boolean,
                    IsGlobal = true
                },
                new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.ClosedIssueStates,
                    Value = IgnoreIssueStateForBranchTitle,
                    Type = IssueTrackerConfigurationParameterType.Text,
                    IsGlobal = false
                },
                new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.CreateBranchIssueQuery,
                    Value = CreateBranchIssueQuery,
                    Type = IssueTrackerConfigurationParameterType.Text,
                    IsGlobal = true
                },
                new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.CreateBranchTransitions,
                    Value = CreateBranchTransitions,
                    Type = IssueTrackerConfigurationParameterType.Text,
                    IsGlobal = true
                }
            };
        }

        public string GetDecryptedPassword()
        {
            if (Config == null)
                throw new ApplicationException("The configuration has not yet been initialized!");

            if (string.IsNullOrEmpty(Password))
                throw new ApplicationException("Password value can not be empty!");

            var decryptedPassword = CryptoServices.GetDecryptedPassword(Password);
            return decryptedPassword;
        }
        

        protected static T getValidParameterValue<T>
            (IssueTrackerConfiguration pConfig, string pParamName, TypeConverter converter = null, T pDefaultValue = default(T))
        {
            if (pConfig == null)
                throw new ApplicationException("The configuration parameter cannot be null!");

            if (string.IsNullOrEmpty(pParamName))
                throw new ApplicationException("The parameter name cannot be null or empty!");

            string configValue = pConfig.GetValue(pParamName);

            if (pParamName.Equals(ConfigParameterNames.CreateBranchIssueQuery) ||
                pParamName.Equals(ConfigParameterNames.CreateBranchTransitions))
                configValue = isBase64(configValue) ? base64Decode(configValue) : base64Encode(configValue);

            try
            {
                return string.IsNullOrEmpty(configValue)
                    ? pDefaultValue
                    : converter != null
                        ? (T) converter.ConvertFromString(configValue)
                        : (T) Convert.ChangeType(configValue, typeof(T));
            }
            catch (Exception e)
            {
                _log.Error(e.Message);
                return pDefaultValue;
            }
        }

        #region Support Methods

        private static string base64Decode(string pBase64EncodedData)
        {
            if (String.IsNullOrEmpty(pBase64EncodedData))
                return String.Empty;
            var base64EncodedBytes = Convert.FromBase64String(pBase64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private static string base64Encode(string pPlainText)
        {
            if (String.IsNullOrEmpty(pPlainText))
                return String.Empty;

            var plainTextBytes = Encoding.UTF8.GetBytes(pPlainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        private static bool isBase64(string pBase64String)
        {
            // Credit: oybek https://stackoverflow.com/users/794764/oybek
            if (string.IsNullOrEmpty(pBase64String) || pBase64String.Length % 4 != 0
                                                   || pBase64String.Contains(" ") || pBase64String.Contains("\t") || pBase64String.Contains
                                                       ("\r") || pBase64String.Contains("\n"))
                return false;

            try
            {
                Convert.FromBase64String(pBase64String);
                return true;
            }
            catch (Exception exception)
            {
                // Handle the exception
            }

            return false;
        }

        private void loadDefaultValues()
        {
            _log.Debug("YouTrackExtensionConfigFacade: loading default values...");
            BranchPrefix = "yt_";
            HostUri = new Uri("http://issues.domain.com");
            UserId = "";
            Password = "";
            ShowIssueStateInBranchTitle = false;
            PostCommentsToTickets = true;
            IgnoreIssueStateForBranchTitle = "Completed";
            UsernameMapping = "";
            WebGuiRootUrl = new Uri("http://plastic-gui.domain.com");
            WorkingMode = ExtensionWorkingMode.TaskOnBranch;
            CreateBranchIssueQuery = "#unresolved order by: updated desc";
            CreateBranchTransitions = "Submitted:Start Work;Planned:Start Work;Incomplete:Start Work";
        }

        #endregion
    }
}