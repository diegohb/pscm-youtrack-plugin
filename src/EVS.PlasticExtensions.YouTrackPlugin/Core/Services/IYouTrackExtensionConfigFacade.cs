// *************************************************
// MMG.PlasticExtensions.YouTrackPlugin.IYouTrackExtensionConfigFacade.cs
// Last Modified: 09/09/2019 11:17 AM
// Modified By: Diego Bustamante (dbustamante)
// *************************************************

using System;
using System.Collections.Generic;
using Codice.Client.IssueTracker;

namespace EVS.PlasticExtensions.YouTrackPlugin.Core.Services
{
    public interface IYouTrackExtensionConfigFacade
    {
        string BranchPrefix { get; }
        Uri HostUri { get; }
        Uri WebGuiRootUrl { get; }
        string UsernameMapping { get; }
        string AuthToken { get; }
        bool UseSsl { get; }
        bool ShowIssueStateInBranchTitle { get; }
        bool PostCommentsToTickets { get; }

        /// <summary>
        /// Issue state(s) to not display in branch title when ShowIssueStateInBranchTitle = true.
        /// </summary>
        /// <remarks>Use commas to separate multiple states.</remarks>
        string IgnoreIssueStateForBranchTitle { get; }

        string CreateBranchIssueQuery { get; }

        string CreateBranchTransitions { get; }

        ExtensionWorkingMode WorkingMode { get; }

        List<IssueTrackerConfigurationParameter> GetYouTrackParameters();

        string GetDecryptedPassword();
    }
}