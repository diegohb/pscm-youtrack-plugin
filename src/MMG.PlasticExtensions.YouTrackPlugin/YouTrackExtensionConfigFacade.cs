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
        private readonly string _ignoreIssueStateForBranchTitle;
        private readonly string _createBranchIssueQuery;
        private readonly string _createBranchTransitions;
        private readonly string _usernameMapping;

        #region Ctors

        public YouTrackExtensionConfigFacade(IssueTrackerConfiguration pConfig)
        {
            _log.Debug("YouTrackExtensionConfigFacade: configured ctor called");
            Config = pConfig;
            BranchPrefix = getValidParameterValue(Config, ConfigParameterNames.BranchPrefix, pDefaultValue: "yt_");
            HostUri = getValidParameterValue(Config, ConfigParameterNames.HostUri, pDefaultValue: new Uri("http://issues.domain.com"), converter: new UriTypeConverter());
            AuthToken = getValidParameterValue(Config, ConfigParameterNames.AuthToken, pDefaultValue: string.Empty);
            ShowIssueStateInBranchTitle = getValidParameterValue
                (Config, ConfigParameterNames.ShowIssueStateInBranchTitle, pDefaultValue: true);
            PostCommentsToTickets = getValidParameterValue(Config, ConfigParameterNames.PostCommentsToTickets, pDefaultValue: true);
            _ignoreIssueStateForBranchTitle = getValidParameterValue
                (Config, ConfigParameterNames.ClosedIssueStates, pDefaultValue: "Completed");
            _usernameMapping = getValidParameterValue(Config, ConfigParameterNames.UsernameMapping, pDefaultValue: "plasticusr:ytusername");
            WebGuiRootUrl = getValidParameterValue
                (Config, ConfigParameterNames.WebGuiRootUrl, pDefaultValue: new Uri("http://plastic-gui.domain.com:7178/"), converter: new UriTypeConverter());
            WorkingMode = getValidParameterValue(Config, nameof(ExtensionWorkingMode), pDefaultValue: ExtensionWorkingMode.TaskOnBranch);
            _createBranchIssueQuery = getValidParameterValue
                (Config, ConfigParameterNames.CreateBranchIssueQuery, pDefaultValue: "#unresolved order by: updated desc");
            _createBranchTransitions = getValidParameterValue
                (Config, ConfigParameterNames.CreateBranchTransitions, pDefaultValue: "Submitted:Start Work;Planned:Start Work;Incomplete:Start Work");
            _log.Debug("YouTrackExtensionConfigFacade: configured ctor completed");
        }

        #endregion

        #region Properties

        public virtual IssueTrackerConfiguration Config { get; 
            private set; }
        public virtual Uri HostUri { get; private set; }
        public virtual Uri WebGuiRootUrl { get; private set; }
        public virtual string BranchPrefix { get; private set; }
        public virtual string UserId { get; private set; }
        public virtual string AuthToken { get; private set; }
        public virtual bool ShowIssueStateInBranchTitle { get; private set; }
        public virtual bool PostCommentsToTickets { get; private set; }

        public virtual string IgnoreIssueStateForBranchTitle
        {
            get { return getInMemDecodedPropertyValue(ConfigParameterNames.ClosedIssueStates, _ignoreIssueStateForBranchTitle); }
        }

        public virtual string CreateBranchIssueQuery
        {
            get { return getInMemDecodedPropertyValue(ConfigParameterNames.CreateBranchIssueQuery,_createBranchIssueQuery); }
        }

        public virtual string CreateBranchTransitions
        {
            get { return getInMemDecodedPropertyValue(ConfigParameterNames.CreateBranchTransitions, _createBranchTransitions); }
        }

        public virtual string UsernameMapping
        {
            get { return getInMemDecodedPropertyValue(ConfigParameterNames.UsernameMapping, _usernameMapping); }
        }

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
                    Name = ConfigParameterNames.UsernameMapping,
                    Value =  base64Encode(UsernameMapping),
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
                    Value =  base64Encode(IgnoreIssueStateForBranchTitle),
                    Type = IssueTrackerConfigurationParameterType.Text,
                    IsGlobal = false
                },
                new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.CreateBranchIssueQuery,
                    Value =  base64Encode(CreateBranchIssueQuery),
                    Type = IssueTrackerConfigurationParameterType.Text,
                    IsGlobal = true
                },
                new IssueTrackerConfigurationParameter
                {
                    Name = ConfigParameterNames.CreateBranchTransitions,
                    Value =  base64Encode(CreateBranchTransitions),
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

        protected string getInMemDecodedPropertyValue(string pParamName, string pOriginalValue)
        {
            if (Config == null || Config.Parameters.Length == 0)
                return pOriginalValue;

            var configParam = Config[pParamName];
            if (configParam.Type == IssueTrackerConfigurationParameterType.Text && isBase64(configParam.Value))
            {
                //NOTE: workaround for https://github.com/diegohb/pscm-youtrack-plugin/issues/6
                var configValue = base64Decode(configParam.Value);
                //_log.DebugFormat($"Value for setting '{pParamName}' encoded. Decoded to '{configValue}'.");
                return configValue;
            }

            return pOriginalValue;
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
        
        //TODO: re-implement once factory interface exposes separate methods for loading/saving.

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

        #endregion
    }
}