// *************************************************
// MMG.PlasticExtensions.YouTrackPlugin.YouTrackHandler.cs
// Last Modified: 12/24/2015 11:39 AM
// Modified By: Bustamante, Diego (bustamd1)
// *************************************************

namespace MMG.PlasticExtensions.YouTrackPlugin
{
    using System.Collections;
    using System.Net;
    using System.Xml;
    using Codice.Client.IssueTracker;
    using log4net;
    using YouTrackSharp.Infrastructure;
    using YouTrackSharp.Issues;

    internal class YouTrackHandler
    {
        private static readonly ILog _log = LogManager.GetLogger("extensions");
        private readonly Connection _ytConnection;
        private readonly IssueManagement _ytIssues;
        private readonly YouTrackExtensionConfigFacade _config;
        private string _authData;
        private int _authRetryCount = 0;

        public YouTrackHandler(YouTrackExtensionConfigFacade pConfig)
        {
            _config = pConfig;
            _ytConnection = new Connection(_config.Host.DnsSafeHost, _config.Host.Port, _config.UseSSL);
            authenticate();
            _ytIssues = new IssueManagement(_ytConnection);
            
        }

        public PlasticTask GetPlasticTaskFromTaskID(string pTaskID)
        {
            _log.DebugFormat("YouTrackHandler: GetPlasticTaskFromTaskID {0}", pTaskID);
            var result = new PlasticTask {Id = pTaskID};
            using (var client = new WebClient())
            {
                var requestURL = string.Format("{0}/rest/issue/{1}", GetBaseURL(), pTaskID);
                client.Headers.Add("Cookie", _authData);
                try
                {
                    var xml = client.DownloadString(requestURL);
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(xml);
                    result.Owner = getTextFromXPathElement(xmlDoc, "Assignee");
                    var issueState = getTextFromXPathElement(xmlDoc, "State");
                    result.Status = issueState;
                    result.Title = getBranchTitle(issueState, getTextFromXPathElement(xmlDoc, "summary"));
                    result.Description = getTextFromXPathElement(xmlDoc, "description");
                }
                catch (WebException exWeb)
                {
                    if (exWeb.Message.Contains("Unauthorized.") && _authRetryCount < 3)
                    {
                        _log.WarnFormat
                            ("YouTrackHandler: Failed to fetch youtrack issue '{0}' due to authentication error. Will retry after authentication again. Details: {1}",
                                pTaskID, exWeb);
                        authenticate();
                        return GetPlasticTaskFromTaskID(pTaskID);
                    }

                    _log.WarnFormat("YouTrackHandler: Failed to find youtrack issue '{0}' due to {1}", pTaskID, exWeb);
                }
            }
            return result;
        }

        public string GetBaseURL()
        {
            return _config.Host.ToString();
        }

        #region Support Methods

        private string getBranchTitle(string pIssueState, string pIssueSummary)
        {
            //if feature is disabled, return ticket summary.
            if (!_config.ShowIssueStateInBranchTitle)
                return pIssueSummary;

            //if feature is enabled but no states are ignored, return default format.
            if (string.IsNullOrEmpty(_config.IgnoreIssueStateForBranchTitle.Trim()))
                return string.Format("{0} [{1}]", pIssueSummary, pIssueState);

            //otherwise, consider the ignore list.
            var ignoreStates = new ArrayList(_config.IgnoreIssueStateForBranchTitle.Trim().Split(','));
            return ignoreStates.Contains(pIssueState)
                ? pIssueSummary
                : string.Format("{0} [{1}]", pIssueSummary, pIssueState);
        }

        private static string getTextFromXPathElement(XmlDocument pXMLDoc, string pFieldName)
        {
            var node = pXMLDoc.SelectSingleNode(string.Format("//field[@name='{0}']/value", pFieldName));
            return node != null ? node.InnerText : string.Empty;
        }

        private void authenticate()
        {
            _authRetryCount++;
            var creds = new NetworkCredential(_config.UserID, _config.Password);
            _ytConnection.Authenticate(creds);
        }

        #endregion
    }
}