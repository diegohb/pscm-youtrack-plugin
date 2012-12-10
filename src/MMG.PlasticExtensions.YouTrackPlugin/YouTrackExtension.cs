// *************************************************
// MMG.PlasticExtensions.YouTrackPlugin.YouTrackExtension.cs
// Last Modified: 11/05/2012 11:30 AM
// Modified By: Bustamante, Diego (bustamd1)
// *************************************************

using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Xml;
using Codice.Client.Extension;

using log4net;

namespace MMG.PlasticExtensions.YouTrackPlugin
{
    public class YouTrackExtension : BasePlasticExtension
    {
        private static readonly ILog _log = LogManager.GetLogger("extensions");
        private YouTrackExtensionConfiguration _config;
        private string _configFile = "youtrackextension.conf";
        private YouTrackHandler _handler;

        public YouTrackExtension()
        {
            try
            {
                bool bSuccess = false;
                _config = (YouTrackExtensionConfiguration)ExtensionServices.LoadConfig(_configFile, typeof(YouTrackExtensionConfiguration), out bSuccess);
                if (!bSuccess)
                {
                    _log.WarnFormat
                        ("YouTrackExtension: Unable to load configuration file: {0}", _configFile);
                    _config = new YouTrackExtensionConfiguration();
                }
                else
                {
                    _config.SetDefaultAttributePrefix("yt");
                    mBaseConfig = this._config;
                    _handler = new YouTrackHandler(_config);
                    _log.InfoFormat("YouTrackExtension: Successfully loaded configuration file: {0}", _configFile);
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("YouTrackExtension: {0}\n\t{1}", ex.Message, ex.StackTrace);
            }
        }

        public override string GetName()
        {
            return "YouTrack Extension";
        }

        public override void OpenTask(string id, string repName)
        {
            _log.DebugFormat("YouTrackExtension: Open task '{0}'", id);
            System.Diagnostics.Process.Start(string.Format("http://{0}:{1}/issue/{2}", _config.Host, _config.Port, id));
        }

        public override PlasticTask[] LoadTask(string[] pTaskIDs, string pRepoName)
        {
            _log.DebugFormat("YouTrackExtension: Load tasks {0}", string.Join(",", pTaskIDs));
            if (pTaskIDs[0] == null || pTaskIDs[0] == String.Empty)
                return null;

            var result = new List<PlasticTask>();
            foreach (var taskID in pTaskIDs)
            {
                result.Add(_handler.GetPlasticTaskFromTaskID(taskID));
            }
            return result.ToArray();
        }

        public override string GetTaskIdForBranch(string pFullBranchName, string repName)
        {
            return getTaskNameWithoutBranchPrefix(ExtensionServices.GetTaskNameFromBranch(pFullBranchName));
        }

        public override PlasticTaskConfiguration[] GetTaskConfiguration(string task)
        {
            _log.DebugFormat("YouTrackExtension: GettaskConfiguration for task '{0}'", task);
            throw new NotImplementedException();
        }

        private string getTaskNameWithoutBranchPrefix(string pTaskFullName)
        {
            return !string.IsNullOrEmpty(_config.BranchPrefix)
                   && pTaskFullName.StartsWith(_config.BranchPrefix, StringComparison.InvariantCultureIgnoreCase)
                       ? pTaskFullName.Substring(_config.BranchPrefix.Length)
                       : pTaskFullName;
        }
    }

    internal class YouTrackHandler
    {
        private static readonly ILog _log = LogManager.GetLogger("extensions");
        private readonly YouTrackExtensionConfiguration _config;
        private string _authData;

        public YouTrackHandler(YouTrackExtensionConfiguration pConfig)
        {
            _config = pConfig;
            using (var client = new WebClient())
            {
                var requestURL = string.Format
                    ("http://{0}:{1}/rest/user/login?login={2}&password={3}", _config.Host, _config.Port, _config.Username, _config.Password);
                try
                {
                    var result = client.UploadString(requestURL, "POST", "");
                    if (result == @"<login>ok</login>")
                        _authData = client.ResponseHeaders.Get("Set-Cookie");
                }
                catch (WebException exWeb)
                {
                    _log.Error(string.Format("Failed to authenticate using request '{0}'.", requestURL), exWeb);
                }
            }
        }

        public PlasticTask GetPlasticTaskFromTaskID(string pTaskID)
        {
            _log.DebugFormat("YouTrackHandler: GetPlasticTaskFromTaskID {0}", pTaskID);
            var result = new PlasticTask { Id = pTaskID };
            using (var client = new WebClient())
            {
                var requestURL = string.Format("http://{0}:{1}/rest/issue/{2}", _config.Host, _config.Port, pTaskID);
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
                catch (System.Net.WebException exWeb)
                {
                    _log.WarnFormat("YouTrackHandler: Failed to find youtrack issue '{0}' due to {1}", pTaskID, exWeb);
                }

            }
            return result;
        }

        private static string getTextFromXPathElement(XmlDocument pXMLDoc, string pFieldName)
        {
            var node = pXMLDoc.SelectSingleNode(string.Format("//field[@name='{0}']/value", pFieldName));
            return node != null ? node.InnerText : string.Empty;
        }
    }
}