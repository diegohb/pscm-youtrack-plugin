// *************************************************
// MMG.PlasticExtensions.YouTrackPlugin.YouTrackExtensionFactory.cs
// Last Modified: 12/27/2015 3:47 PM
// Modified By: Bustamante, Diego (bustamd1)
// *************************************************

namespace MMG.PlasticExtensions.YouTrackPlugin
{
    using Codice.Client.IssueTracker;
    using log4net;

    public class YouTrackExtensionFactory : IPlasticIssueTrackerExtensionFactory
    {
        private static readonly ILog _log = LogManager.GetLogger("extensions");

        public YouTrackExtensionFactory()
        {
            _log.Debug("YouTrackExtensionFactory: ctor called");
        }

        public IssueTrackerConfiguration GetConfiguration(IssueTrackerConfiguration pStoredConfiguration)
        {
            _log.Debug("YouTrackExtensionFactory: GetConfiguration - start");

            var configFacade = pStoredConfiguration != null
                ? new YouTrackExtensionConfigFacade(pStoredConfiguration)
                : new YouTrackExtensionConfigFacade();

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