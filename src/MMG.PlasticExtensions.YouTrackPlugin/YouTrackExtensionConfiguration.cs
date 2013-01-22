// *************************************************
// MMG.PlasticExtensions.YouTrackPlugin.YouTrackExtensionConfiguration.cs
// Last Modified: 01/21/2013 9:53 PM
// Modified By: Bustamante, Diego (bustamd1)
// *************************************************

using Codice.Client.Extension;

namespace MMG.PlasticExtensions.YouTrackPlugin
{
    public class YouTrackExtensionConfiguration : BaseExtensionConfiguration
    {
        public string Host = "";
        public int? CustomPort = null;
        public string Username = "";
        public string Password = "";
        public bool UseSSL = false;
    }
}