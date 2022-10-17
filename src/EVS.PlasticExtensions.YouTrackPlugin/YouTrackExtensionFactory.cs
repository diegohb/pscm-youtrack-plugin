using Codice.Client.IssueTracker;
using EVS.PlasticExtensions.YouTrackPlugin.Core.Services.Impl;
using log4net;

namespace EVS.PlasticExtensions.YouTrackPlugin
{
    #region

    #endregion

    public class YouTrackExtensionFactory : IPlasticIssueTrackerExtensionFactory
    {
        private static readonly ILog _log = LogManager.GetLogger("extensions");

        #region Ctors

        public YouTrackExtensionFactory()
        {
            _log.Debug("YouTrackExtensionFactory: ctor called");
        }

        #endregion

        public IssueTrackerConfiguration GetConfiguration(IssueTrackerConfiguration pStoredConfiguration)
        {
            _log.Debug("YouTrackExtensionFactory: GetConfiguration - start");

            var configFacade = pStoredConfiguration != null
                ? new YouTrackExtensionConfigFacade(pStoredConfiguration)
                : new YouTrackExtensionConfigFacade(new IssueTrackerConfiguration());

            var workingMode = configFacade.WorkingMode;
            var parameters = configFacade.GetYouTrackParameters();

            var issueTrackerConfiguration = new IssueTrackerConfiguration(workingMode, parameters);
            _log.Debug("YouTrackExtensionFactory: GetConfiguration - completed");
            return issueTrackerConfiguration;
        }

        public IPlasticIssueTrackerExtension GetIssueTrackerExtension(IssueTrackerConfiguration pConfiguration)
        {
            _log.Debug("YouTrackExtensionFactory: GetIssueTrackerExtension - start");
            var youtrackConfigFacade = new YouTrackExtensionConfigFacade(pConfiguration);
            var plasticIssueTrackerExtension = new YouTrackExtension(youtrackConfigFacade);
            _log.Debug("YouTrackExtensionFactory: GetIssueTrackerExtension - completed");
            return plasticIssueTrackerExtension;
        }

        public string GetIssueTrackerName()
        {
            return "YouTrack Issues";
        }
    }
}