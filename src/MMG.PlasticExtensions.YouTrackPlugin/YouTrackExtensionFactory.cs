// *************************************************
// MMG.PlasticExtensions.YouTrackPlugin.YouTrackExtensionFactory.cs
// Last Modified: 12/24/2015 11:22 AM
// Modified By: Bustamante, Diego (bustamd1)
// *************************************************

namespace MMG.PlasticExtensions.YouTrackPlugin
{
    using Codice.Client.IssueTracker;

    public class YouTrackExtensionFactory : IPlasticIssueTrackerExtensionFactory
    {
        public IssueTrackerConfiguration GetConfiguration(IssueTrackerConfiguration pStoredConfiguration)
        {
            var configFacade = new YouTrackExtensionConfigFacade(pStoredConfiguration);

            var workingMode = configFacade.WorkingMode;
            var parameters = configFacade.GetYouTrackParameters();

            return new IssueTrackerConfiguration(workingMode, parameters);
        }

        public IPlasticIssueTrackerExtension GetIssueTrackerExtension(IssueTrackerConfiguration pConfiguration)
        {
            var youtrackConfigFacade = new YouTrackExtensionConfigFacade(pConfiguration);
            return new YouTrackExtension(youtrackConfigFacade);
        }

        public string GetIssueTrackerName()
        {
            return "YouTrack Issues";
        }
    }
}