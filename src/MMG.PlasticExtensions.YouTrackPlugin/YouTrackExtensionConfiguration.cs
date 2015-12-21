// *************************************************
// MMG.PlasticExtensions.YouTrackPlugin.YouTrackExtensionConfiguration.cs
// Last Modified: 12/20/2015 6:36 PM
// Modified By: Bustamante, Diego (bustamd1)
// *************************************************

namespace MMG.PlasticExtensions.YouTrackPlugin
{
    using System.Collections.Generic;
    using Codice.Client.IssueTracker;

    public class YouTrackExtensionConfiguration : IPlasticIssueTrackerExtensionFactory
    {
        public string Host = "";
        public int? CustomPort = null;
        public string Username = "";
        public string Password = "";
        public bool UseSSL = false;

        public bool ShowIssueStateInBranchTitle = true;

        /// <summary>
        /// Issue state(s) to not display in branch title when ShowIssueStateInBranchTitle = true.
        /// </summary>
        /// <remarks>Use commas to separate multiple states.</remarks>
        public string IgnoreIssueStateForBranchTitle = "";

        public IssueTrackerConfiguration GetConfiguration
            (
            IssueTrackerConfiguration pStoredConfiguration)
        {
            var parameters = new List<IssueTrackerConfigurationParameter>();

            var workingMode = GetWorkingMode(pStoredConfiguration);

            var user = GetValidParameterValue
                (
                    pStoredConfiguration, SampleExtension.USER_KEY, "1");

            var prefix = GetValidParameterValue
                (
                    pStoredConfiguration, SampleExtension.BRANCH_PREFIX_KEY, "scm");

            var userIdParam = new IssueTrackerConfigurationParameter()
            {
                Name = SampleExtension.USER_KEY,
                Value = GetValidParameterValue
                    (
                        pStoredConfiguration, SampleExtension.USER_KEY, "1"),
                Type = IssueTrackerConfigurationParameterType.User,
                IsGlobal = false
            };

            var branchPrefixParam =
                new IssueTrackerConfigurationParameter()
                {
                    Name = SampleExtension.BRANCH_PREFIX_KEY,
                    Value = GetValidParameterValue
                        (
                            pStoredConfiguration, SampleExtension.BRANCH_PREFIX_KEY, "sample"),
                    Type = IssueTrackerConfigurationParameterType.BranchPrefix,
                    IsGlobal = true
                };

            parameters.Add(userIdParam);
            parameters.Add(branchPrefixParam);

            return new IssueTrackerConfiguration(workingMode, parameters);
        }

        public IPlasticIssueTrackerExtension GetIssueTrackerExtension
            (IssueTrackerConfiguration pConfiguration)
        {
            return new YouTrackExtension(pConfiguration);
        }

        public string GetIssueTrackerName()
        {
            return "YouTrack Issue Tracker";
        }

        ExtensionWorkingMode GetWorkingMode(IssueTrackerConfiguration pConfig)
        {
            if (pConfig == null)
                return ExtensionWorkingMode.TaskOnBranch;

            if (pConfig.WorkingMode == ExtensionWorkingMode.None)
                return ExtensionWorkingMode.TaskOnBranch;

            return pConfig.WorkingMode;
        }

        string GetValidParameterValue
            (
            IssueTrackerConfiguration pConfig, string pParamName, string pDefaultValue)
        {
            var configValue = (pConfig != null) ? pConfig.GetValue(pParamName) : null;
            if (string.IsNullOrEmpty(configValue))
                return pDefaultValue;
            return configValue;
        }
    }
}