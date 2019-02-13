using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Codice.Client.IssueTracker;
using Codice.Utils;
using log4net;

namespace MMG.PlasticExtensions.YouTrackPlugin
{
    public class YouTrackExtensionConfigFacade : IYouTrackExtensionConfigFacade
    {
        private static readonly ILog _log = LogManager.GetLogger("extensions");
        public virtual IssueTrackerConfiguration Config { get; private set; }

        #region config values
        public virtual Uri HostUri { get; private set; }
        public virtual Uri WebGuiRootUrl { get; private set; }
        public virtual string BranchPrefix { get; private set; }
        public virtual string UserId { get; private set; }
        public virtual string Password { get; private set; }
        public virtual bool ShowIssueStateInBranchTitle { get; private set; }
        public virtual bool PostCommentsToTickets { get; private set; }
        public virtual string IgnoreIssueStateForBranchTitle { get; private set; }
        public virtual string CreateBranchIssueQuery { get; private set; }
        public virtual string UsernameMapping { get; private set; }
        public virtual bool UseSsl => HostUri.Scheme.Equals("https", StringComparison.CurrentCultureIgnoreCase);
        public virtual ExtensionWorkingMode WorkingMode { get; private set; }
        #endregion

        internal YouTrackExtensionConfigFacade()
        {
            _log.Debug("YouTrackExtensionConfigFacade: empty ctor called");
            LoadDefaultValues();
            _log.Debug("YouTrackExtensionConfigFacade: empty ctor completed");
        }

        public YouTrackExtensionConfigFacade(IssueTrackerConfiguration pConfig) : this()
        {
            _log.Debug("YouTrackExtensionConfigFacade: configured ctor called");
            Config = pConfig;
            BranchPrefix = GetValidParameterValue(Config, ConfigParameterNames.BranchPrefix, pDefaultValue: BranchPrefix);
            HostUri = GetValidParameterValue(Config, ConfigParameterNames.HostUri, pDefaultValue: HostUri, converter: new UriTypeConverter());
            UserId = GetValidParameterValue(Config, ConfigParameterNames.UserId, pDefaultValue: UserId);
            Password = GetValidParameterValue(Config, ConfigParameterNames.Password, pDefaultValue: Password);
            ShowIssueStateInBranchTitle = GetValidParameterValue(Config, ConfigParameterNames.ShowIssueStateInBranchTitle, pDefaultValue: ShowIssueStateInBranchTitle);
            PostCommentsToTickets = GetValidParameterValue(Config, ConfigParameterNames.PostCommentsToTickets, pDefaultValue: PostCommentsToTickets);
            IgnoreIssueStateForBranchTitle = GetValidParameterValue(Config, ConfigParameterNames.ClosedIssueStates, pDefaultValue: IgnoreIssueStateForBranchTitle);
            UsernameMapping = GetValidParameterValue(Config, ConfigParameterNames.UsernameMapping, pDefaultValue: UsernameMapping);
            WebGuiRootUrl = GetValidParameterValue(Config, ConfigParameterNames.WebGuiRootUrl, pDefaultValue: WebGuiRootUrl, converter: new UriTypeConverter());
            WorkingMode = GetValidParameterValue(Config, nameof(ExtensionWorkingMode), pDefaultValue: ExtensionWorkingMode.TaskOnBranch);
            CreateBranchIssueQuery = GetValidParameterValue(Config, nameof(CreateBranchIssueQuery), pDefaultValue: CreateBranchIssueQuery);
            _log.Debug("YouTrackExtensionConfigFacade: configured ctor completed");
        }

        private void LoadDefaultValues()
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
        }

        public List<IssueTrackerConfigurationParameter> GetYouTrackParameters() =>
            new List<IssueTrackerConfigurationParameter>
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
                    Name = ConfigParameterNames.CreateBranchQueryForAll,
                    Value = CreateBranchIssueQuery,
                    Type = IssueTrackerConfigurationParameterType.Text,
                    IsGlobal = true
                }
            };

        protected static T GetValidParameterValue<T>(IssueTrackerConfiguration pConfig, string pParamName, TypeConverter converter = null, T pDefaultValue = default(T))
        {
            if (pConfig == null)
                throw new ApplicationException("The configuration parameter cannot be null!");

            if (string.IsNullOrEmpty(pParamName))
                throw new ApplicationException("The parameter name cannot be null or empty!");

            var configValue = pConfig.GetValue(pParamName);

            try
            {
                return string.IsNullOrEmpty(configValue)
                    ? pDefaultValue
                    : converter != null
                        ? (T)converter.ConvertFromString(configValue)
                        : (T)Convert.ChangeType(configValue, typeof(T));
            }
            catch (Exception e)
            {
                _log.Error(e.Message);
                return pDefaultValue;
            }
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
    }
}
