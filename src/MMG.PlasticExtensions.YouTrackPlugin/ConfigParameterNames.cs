// *************************************************
// MMG.PlasticExtensions.YouTrackPlugin.ConfigParameterNames.cs
// Last Modified: 09/09/2019 10:54 AM
// Modified By: Diego Bustamante (dbustamante)
// *************************************************

namespace MMG.PlasticExtensions.YouTrackPlugin
{
    public static class ConfigParameterNames
    {
        public const string UserId = "User ID";
        public const string BranchPrefix = "Branch Name Prefix";
        public const string AuthToken = "Auth Token";
        public const string HostUri = "Host";
        public const string WebGuiRootUrl = "Plastic WebGUI Root URL";
        public const string UsernameMapping = "Username Mapping";
        public const string ShowIssueStateInBranchTitle = "Show issues state in branch title";
        public const string PostCommentsToTickets = "Add checkin comments to ticket(s)";
        public const string ClosedIssueStates = "Issue states considered closed";
        public static string CreateBranchIssueQuery = "Create branch issue query";
        public static string CreateBranchTransitions = "Create branch transitions";
    }
}