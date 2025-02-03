using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Codice.Client.IssueTracker;
using EVS.PlasticExtensions.YouTrackPlugin.Core;
using EVS.PlasticExtensions.YouTrackPlugin.Core.Services;
using EVS.PlasticExtensions.YouTrackPlugin.Core.Services.Impl;
using EVS.PlasticExtensions.YouTrackPlugin.Infrastructure;
using log4net;

namespace EVS.PlasticExtensions.YouTrackPlugin
{
  #region

  #endregion

  public class YouTrackExtension : IPlasticIssueTrackerExtension
  {
    private static readonly ILog _log = LogManager.GetLogger("extensions");
    private readonly IYouTrackExtensionConfigFacade _config;
    private readonly IYouTrackService _ytService;

    #region Ctors

    public YouTrackExtension(IYouTrackExtensionConfigFacade pConfig)
    {
      try
      {
        _config = pConfig;
        _ytService = new YouTrackService(pConfig);
      }
      catch (Exception ex)
      {
        _log.ErrorFormat("YouTrackExtension: {0}\n\t{1}", ex.Message, ex.StackTrace);
      }
    }

    #endregion

    #region Support Methods

    private string getBranchName(string pFullBranchName)
    {
      var lastSeparatorIndex = pFullBranchName.LastIndexOf('/');

      if (lastSeparatorIndex < 0)
        return pFullBranchName;

      return lastSeparatorIndex == pFullBranchName.Length - 1
          ? string.Empty
          : pFullBranchName.Substring(lastSeparatorIndex + 1);
    }

    private string getTicketIDFromTaskBranchName(string pTaskBranchName)
    {
      return !string.IsNullOrEmpty(_config.BranchPrefix)
             && pTaskBranchName.StartsWith(_config.BranchPrefix, StringComparison.InvariantCultureIgnoreCase)
          ? pTaskBranchName.Substring(_config.BranchPrefix.Length)
          : pTaskBranchName;
    }

    #endregion

    #region IPlasticIssueTrackerExtension implementation

    public string GetExtensionName()
    {
      return "Jetbrains YouTrack Integration";
    }

    public void Connect()
    {
      //no active connection held.
    }

    public void Disconnect()
    {
      //no active connection held.
    }

    public bool TestConnection(IssueTrackerConfiguration pConfiguration)
    {
      _log.Debug("YouTrackExtension: TestConnection - start");

      try
      {
        var config = new YouTrackExtensionConfigFacade(pConfiguration);
        YouTrackService.VerifyConnection(config).RunSynchronously();
        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }

    public async void LogCheckinResult(PlasticChangeset pChangeset, List<PlasticTask> pTasks)
    {
      if (!_config.PostCommentsToTickets)
        return;
      foreach (var task in pTasks)
      {
        await _ytService.AddCommentToIssue
        (task.Id, pChangeset.RepositoryServer, pChangeset.Repository,
            _config.WebGuiRootUrl ?? new Uri($"http://{pChangeset.RepositoryServer}"),
            pChangeset.Branch, pChangeset.Id, pChangeset.Comment, pChangeset.Guid);
      }
    }

    public void UpdateLinkedTasksToChangeset(PlasticChangeset pChangeset, List<string> pTasks)
    {
      //TODO: Implement
    }

    public PlasticTask GetTaskForBranch(string pFullBranchName)
    {
      var taskBranchName = getBranchName(pFullBranchName);
      if (!taskBranchName.StartsWith(_config.BranchPrefix))
        return null;

      return _ytService.GetPlasticTask(getTicketIDFromTaskBranchName(taskBranchName)).Result;
    }

    public Dictionary<string, PlasticTask> GetTasksForBranches(List<string> pFullBranchNames)
    {
      var data = Task.WhenAll(pFullBranchNames
        .Where(pBranch => pBranch.Split('/').Last().StartsWith(_config.BranchPrefix))
        .Select(async x => new
        {
          FullBranchName = x,
          Task = await _ytService.GetPlasticTask(getTicketIDFromTaskBranchName(getBranchName(x)))
        })).Result;
      var result = data.ToDictionary(x => x.FullBranchName, x => x.Task);
      return result;

    }

    public void OpenTaskExternally(string pTaskId)
    {
      var issueWebUrl = _ytService.GetIssueWebUrl(pTaskId);
      _log.DebugFormat("YouTrackExtension: Open task '{0}' at {1}", pTaskId, issueWebUrl);
      Process.Start(issueWebUrl);
    }

    public List<PlasticTask> LoadTasks(List<string> pTaskIds)
    {
      try
      {
        var plasticTasks = _ytService.GetPlasticTasks(pTaskIds.ToArray()).GetAwaiter().GetResult().ToList();
        _log.DebugFormat("YouTrackExtension: Loaded {0} YouTrack plastic tasks.", plasticTasks.Count);
        return plasticTasks;
      }
      catch (NullReferenceException ex)
      {
        _log.Error("Error fetching tickets.", ex);
        return new List<PlasticTask>();
      }
    }

    public List<PlasticTask> GetPendingTasks()
    {
      try
      {
        var plasticTasks = _ytService.GetUnresolvedPlasticTasks().GetAwaiter().GetResult().ToList();
        _log.DebugFormat("YouTrackExtension: Loaded {0} YouTrack unresolved plastic tasks.", plasticTasks.Count);
        return plasticTasks;
      }
      catch (NullReferenceException ex)
      {
        _log.Error("Error fetching tickets.", ex);
        return new List<PlasticTask>();
      }


    }

    public List<PlasticTask> GetPendingTasks(string pAssignee)
    {
      try
      {
        var plasticTasks = _ytService.GetUnresolvedPlasticTasks(pAssignee).GetAwaiter().GetResult().ToList();
        _log.DebugFormat("YouTrackExtension: Loaded {0} YouTrack unresolved plastic tasks.", plasticTasks.Count);
        return plasticTasks;
      }
      catch (NullReferenceException ex)
      {
        _log.Error("Error fetching tickets.", ex);
        return new List<PlasticTask>();
      }

      return new List<PlasticTask>();
    }

    public void MarkTaskAsOpen(string pTaskId, string pAssignee)
    {
      try
      {
        _ytService.AssignIssue(pTaskId, pAssignee, false).RunSynchronously();
      }
      catch (Exception e)
      {
        _log.Error($"Failed to assign issue '{pTaskId}'.", e);
      }

      try
      {
        _ytService.EnsureIssueInProgress(pTaskId).RunSynchronously();
      }
      catch (Exception e)
      {
        _log.Error($"Failed to transition issue '{pTaskId}'.", e);
      }
    }

    #endregion
  }
}