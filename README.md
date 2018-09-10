# TFS Build Notifications
A notification and alert system for Microsoft Team Foundation Server builds.

## Overview

A Windows Forms application which provides notifications for TFS builds. Includes a website dashboard that allows you track the status of build definitions across multiple TFS connections and projects.

### Dashboard

The website dashboard gives a detailed overview of your monitored build definitions across multiple connections (via on-premises TFS installations and/or Visual Studio Online). Real time updates are applied to the dashboard when the status of a monitored build changes.

A recent build history is displayed underneath each build definition.

![Website dashboard](/docs/images/dashboard-example.png)

### Notifications

Build changes are polled at a configurable interval and display tray notifications (or Toast notifications in Windows 10) when a build starts, stops, fails or succeeds. If required, the application can be also configured to only display a notification if a build fails.

![Build started](/docs/images/build-started.png)

![Build failed](/docs/images/build-failed.png)

![Build succeeded](/docs/images/build-passed.png)

### Notification TTS (text to speech)

In addition to existing notifications you can activate voice output by setting TextToSpeech.Enabled to true in the app.config file.

### Application

.Net 4.5 Windows Forms application using Owin, Nancy and SignalR to self host and run the website dashboard.

#### Installer

Coming soon! For now, clone the repo and compile the Tfs.Build.Notifications.Tray project to run.
