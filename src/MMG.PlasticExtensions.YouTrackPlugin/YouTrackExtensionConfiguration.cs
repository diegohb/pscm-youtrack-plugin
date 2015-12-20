// *************************************************
// MMG.PlasticExtensions.YouTrackPlugin.YouTrackExtensionConfiguration.cs
// Last Modified: 12/20/2015 5:06 PM
// Modified By: Bustamante, Diego (bustamd1)
// *************************************************

namespace MMG.PlasticExtensions.YouTrackPlugin
{
    public class YouTrackExtensionConfiguration : BaseExtensionConfiguration
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
    }
}