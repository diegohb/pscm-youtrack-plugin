using System;
using System.Collections.Generic;
using System.ComponentModel;
using Codice.Client.IssueTracker;
using Codice.Utils;
using log4net;
using MMG.PlasticExtensions.YouTrackPlugin.Core.Models;

namespace MMG.PlasticExtensions.YouTrackPlugin.Core.Services.Impl
{
    #region

    #endregion

    public class YouTrackExtensionConfigFacade : IYouTrackExtensionConfigFacade
    {
        private static readonly ILog _log = LogManager.GetLogger("extensions");
        private readonly string _ignoreIssueStateForBranchTitle;
        private readonly string _createBranchIssueQuery;
        private readonly string _createBranchTransitions;
        private readonly string _usernameMapping;

        #region Ctors

        public YouTrackExtensionConfigFacade(IssueTrackerConfiguration pConfig)
        {
            _log.Debug("YouTrackExtensionConfigFacade: configured ctor called");
            Config = pConfig;
            UserId = getValidParameterValue(Config, ConfigParameterNames.UserId, pDefaultValue: "");
            BranchPrefix = getValidParameterValue(Config, ConfigParameterNames.BranchPrefix, pDefaultValue: "yt_");
            HostUri = getValidParameterValue(Config, ConfigParameterNames.HostUri, pDefaultValue: new Uri("http://issues.domain.com"), converter: new UriTypeConverter());
            AuthToken = getValidParameterValue(Config, ConfigParameterNames.AuthToken, pDefaultValue: string.Empty);
            ShowIssueStateInBranchTitle = getValidParameterValue
                (Config, ConfigParameterNames.ShowIssueStateInBranchTitle, pDefaultValue: true);
            PostCommentsToTickets = getValidParameterValue(Config, ConfigParameterNames.PostCommentsToTickets, pDefaultValue: true);
            _ignoreIssueStateForBranchTitle = getValidParameterValue
                (Config, ConfigParameterNames.ClosedIssueStates, pDefaultValue: "Completed");
            _usernameMapping = getValidParameterValue(Config, ConfigParameterNames.UsernameMapping, pDefaultValue: "plasticusr1:ytusername1;plasticusr2:ytusername2");
            WebGuiRootUrl = getValidParameterValue
                (Config, ConfigParameterNames.WebGuiRootUrl, pDefaultValue: new Uri("http://plastic-gui.domain.com:7178/"), converter: new UriTypeConverter());
            WorkingMode = getValidParameterValue(Config, nameof(ExtensionWorkingMode), pDefaultValue: ExtensionWorkingMode.TaskOnBranch);
            _createBranchIssueQuery = getValidParameterValue
                (Config, ConfigParameterNames.CreateBranchIssueQuery, pDefaultValue: "#unresolved order by: updated desc");
            _createBranchTransitions = getValidParameterValue
                (Config, ConfigParameterNames.CreateBranchTransitions, pDefaultValue: "Submitted:Plan;Planned:Start Work;Incomplete:Start Work");
            _log.Debug("YouTrackExtensionConfigFacade: configured ctor completed");
        }

        #endregion

        #region Properties

        public virtual IssueTrackerConfiguration Config { get; }
        public virtual Uri HostUri { get; }
        public virtual Uri WebGuiRootUrl { get; }
        public virtual string BranchPrefix { get; }
        public virtual string UserId { get; private set; }
        public virtual string AuthToken { get; }
        public virtual bool ShowIssueStateInBranchTitle { get; }
        public virtual bool PostCommentsToTickets { get; }

        public virtual string IgnoreIssueStateForBranchTitle => _ignoreIssueStateForBranchTitle;

        public virtual string CreateBranchIssueQuery => _createBranchIssueQuery;

        public virtual string CreateBranchTransitions => _createBranchTransitions;

        public virtual string UsernameMapping => _usernameMapping;

        public virtual bool UseSsl => HostUri.Scheme.Equals("https", StringComparison.CurrentCultureIgnoreCase);

        public virtual ExtensionWorkingMode WorkingMode { get; }

        #endregion

        public List<IssueTrackerConfigurationParameter> GetYouTrackParameters()
        {
            return new List<IssueTrackerConfigurationParameter>
            {
                new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.UserId,
                    Value = UserId,
                    Type = IssueTrackerConfigurationParameterType.User,
                    IsGlobal = false
                },
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
                    Name = ConfigParameterNames.UsernameMapping,
                    Value =  UsernameMapping,
                    Type = IssueTrackerConfigurationParameterType.Text,
                    IsGlobal = true
                },
                new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.AuthToken,
                    Value = AuthToken,
                    Type = IssueTrackerConfigurationParameterType.Password,
                    IsGlobal = false
                },
                new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.ShowIssueStateInBranchTitle,
                    Value = ShowIssueStateInBranchTitle.ToString(),
                    Type = IssueTrackerConfigurationParameterType.Boolean,
                    IsGlobal = true
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
                    Value =  IgnoreIssueStateForBranchTitle,
                    Type = IssueTrackerConfigurationParameterType.Text,
                    IsGlobal = true
                },
                new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.CreateBranchIssueQuery,
                    Value =  CreateBranchIssueQuery,
                    Type = IssueTrackerConfigurationParameterType.Text,
                    IsGlobal = true
                },
                new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.CreateBranchTransitions,
                    Value =  CreateBranchTransitions,
                    Type = IssueTrackerConfigurationParameterType.Text,
                    IsGlobal = true
                }
            };
        }

        public string GetDecryptedPassword()
        {
            if (Config == null)
                throw new ApplicationException("The configuration has not yet been initialized!");

            if (string.IsNullOrEmpty(AuthToken))
                throw new ApplicationException("AuthToken value can not be empty!");

            var decryptedPassword = CryptoServices.GetDecryptedPassword(AuthToken);
            return decryptedPassword;
        }

        protected static T getValidParameterValue<T>
            (IssueTrackerConfiguration pConfig, string pParamName, TypeConverter converter = null, T pDefaultValue = default(T))
        {
            if (string.IsNullOrEmpty(pParamName))
                throw new ApplicationException("The parameter name cannot be null or empty!");

            if (pConfig == null || pConfig.Parameters.Length == 0)
                return pDefaultValue;
            
            if (pParamName == nameof(ExtensionWorkingMode))
                return pDefaultValue;

            var configValue = pConfig.GetValue(pParamName);

            if (string.IsNullOrEmpty(configValue))
                return pDefaultValue;
            
            try
            {
                return converter != null
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
        
        #endregion
    }
}