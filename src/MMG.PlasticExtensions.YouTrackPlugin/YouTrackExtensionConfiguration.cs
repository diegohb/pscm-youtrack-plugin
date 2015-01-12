// *************************************************
// MMG.PlasticExtensions.YouTrackPlugin.YouTrackExtensionConfiguration.cs
// Last Modified: 01/10/2015 9:32 PM
// Modified By: Bustamante, Diego (bustamd1)
// *************************************************

namespace MMG.PlasticExtensions.YouTrackPlugin
{
    using Codice.Client.Extension;

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