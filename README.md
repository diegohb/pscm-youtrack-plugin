# pscm-youtrack-plugin
====================

A plugin for PlasticSCM 6.x that will map branches to YouTrack issues and allow creation of branches from tickets.

* [You Track API documentation](https://confluence.jetbrains.com/display/YTD4/YouTrack+REST+API+Reference)
* [Plastic SCM Custom Extensions documentation](https://www.plasticscm.com/documentation/extensions/plastic-scm-version-control-task-and-issue-tracking-guide.shtml#WritingPlasticSCMcustomextensions)


## Features
* Create branches from YouTrack tickets.
* Place ticket in-progress on create branch.
* Log operation comments to ticket (create branch, commits, comments w/ commits)
* Branch explorer integration shows ticket title, state, and quick 1-click access to ticket page in YouTrack.

## How to install
...

## How to update existing installation
1. Build the solution with Release configuration.
2. Copy files from Extension project's ~/bin/release folder: *PlasticExtensions.YouTrackPlugin.*
3. Place them at c:\Program Files\PlasticSCM5\client\extensions\youtrack\ .