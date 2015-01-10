// *************************************************
// MMG.PlasticExtensions.YouTrackPlugin.YouTrackHandler.cs
// Last Modified: 01/21/2013 9:53 PM
// Modified By: Bustamante, Diego (bustamd1)
// *************************************************

using System.Net;
using System.Xml;
using Codice.Client.Extension;
using log4net;

namespace MMG.PlasticExtensions.YouTrackPlugin
{
    internal class YouTrackHandler
    {
        private static readonly ILog _log = LogManager.GetLogger("extensions");
        private readonly YouTrackExtensionConfiguration _config;
        private string _authData;
        private int _authRetryCount = 0 ;

        public YouTrackHandler(YouTrackExtensionConfiguration pConfig)
        {
            _config = pConfig;
            authenticate();
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
                    result.Status = getTextFromXPathElement(xmlDoc, "State");
                    result.Title = getTextFromXPathElement(xmlDoc, "summary");
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
            var protocol = _config.UseSSL ? "https" : "http";
            var port = _config.CustomPort.HasValue ? _config.CustomPort : _config.UseSSL ? 443 : 80;
            var serverHost = port != 80 ? string.Format("{0}:{1}", _config.Host, port) : _config.Host;
            return string.Format("{0}://{1}", protocol, serverHost);
        }

        #region Support Methods

        private static string getTextFromXPathElement(XmlDocument pXMLDoc, string pFieldName)
        {
            var node = pXMLDoc.SelectSingleNode(string.Format("//field[@name='{0}']/value", pFieldName));
            return node != null ? node.InnerText : string.Empty;
        }

        private void authenticate()
        {
            _authRetryCount++;

            using (var client = new WebClient())
            {
                var requestURL = string.Format
                    ("{0}/rest/user/login?login={1}&password={2}", GetBaseURL(), _config.Username, _config.Password);
                try
                {
                    var result = client.UploadString(requestURL, "POST", "");
                    if (result == @"<login>ok</login>")
                    {
                        _authData = client.ResponseHeaders.Get("Set-Cookie");
                        _log.DebugFormat("YouTrackHandler: Successfully authenticated in {0} attempt(s).", _authRetryCount);
                        _authRetryCount = 0;
                    }
                }
                catch (WebException exWeb)
                {
                    _log.Error(string.Format("YouTrackHandler: Failed to authenticate using request '{0}'.", requestURL), exWeb);
                }
            }
        }

        #endregion

    }
}