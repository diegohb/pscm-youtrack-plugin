// *************************************************
// MMG.PlasticExtensions.YouTrackPlugin.IYouTrackExtensionConfigFacade.cs
// Last Modified: 03/28/2016 1:57 PM
// Modified By: Green, Brett (greenb1)
// *************************************************

namespace MMG.PlasticExtensions.YouTrackPlugin
{
    using System;
    using System.Collections.Generic;
    using Codice.Client.IssueTracker;

    public interface IYouTrackExtensionConfigFacade
    {
        string BranchPrefix { get; }
        Uri HostUri { get; }
        Uri WebGuiRootUrl { get; }
        string UsernameMapping { get; }
        string UserId { get; }
        string Password { get; }
        bool UseSsl { get; }
        bool ShowIssueStateInBranchTitle { get; }
        bool PostCommentsToTickets { get; }

        /// <summary>
        /// Issue state(s) to not display in branch title when ShowIssueStateInBranchTitle = true.
        /// </summary>
        /// <remarks>Use commas to separate multiple states.</remarks>
        string IgnoreIssueStateForBranchTitle { get; }

        ExtensionWorkingMode WorkingMode { get; }

        List<IssueTrackerConfigurationParameter> GetYouTrackParameters();

        string GetDecryptedPassword();
    }
}