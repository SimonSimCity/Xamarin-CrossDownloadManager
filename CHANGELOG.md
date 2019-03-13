## Changes

### 1.4.0

  * Feature [#94](https://github.com/SimonSimCity/Xamarin-CrossDownloadManager/issues/94): Added DestinationPathName to IDownloadFile which returns the path of the local file
  * Bug [#88](https://github.com/SimonSimCity/Xamarin-CrossDownloadManager/issues/88): [Android] TotalBytesExpected and TotalBytesWritten is converted as integers but should be float

### 1.3.6

  * Fixed bug [#77](https://github.com/SimonSimCity/Xamarin-CrossDownloadManager/issues/77): [Android] Download are cancelled with no reason

### 1.3.5

  * *changes in documentation*

### 1.3.4

  * Add possibility to prevent files from showing up in download manager

### 1.3.3

  * *no changes*

### 1.3.2

  * Fixed bug [#69](https://github.com/SimonSimCity/Xamarin-CrossDownloadManager/issues/69): Using PathNameForDownloadedFile: Android suffixes filename by '-1' if file already exists

### 1.3.1

  * Fixed bug [#64](https://github.com/SimonSimCity/Xamarin-CrossDownloadManager/issues/64): Cancel button on Notification does not change file status to `DownloadFileStatus.CANCELED`

### 1.3.0

  * Fixed download-queue on UWP
  * Added option for Android to disable notification

### 1.2.0

  * Added property `CrossDownloadManager.AvoidDiscretionaryDownloadInBackground` to avoid setting `discretionary` to `true` if download starts while app is in background (see https://github.com/SimonSimCity/Xamarin-CrossDownloadManager/pull/50/commits/bd94398623043cd3e29f743201302103d9d48ba1#diff-a606a1aeb07f8c9297bce51968abc4c6R32 for details)
  * Fixed calling of DownloadCompletedBroadcastReceiver in background by adding an annotation (and thereby dropped requirement to register class ActivityLifecycleCallbacks)

### 1.1.1

  * Fixed automated build-process - nothing functional to this plugin ...

### 1.1.0

  * Added first implementation for UWP
  * [BC] Updated naming of methods to follow C# guidelines

### 1.0.0

  * Allowed implementation of Mac (without crashing at once)

### 0.10.2

  * Fixed NullReferenceException

### 0.10.1

  * Small fixes

### 0.10.0

  * [BC] Introduced new status in DownloadFileStatus enum. Please check your switch-case statements on `DownloadFileStatus`. Be aware of that items can also be in the `Queue` while still having the status `DownloadFileStatus.INITIALIZED`.
  * [BC] Refactored queue to be threadsafe. `Queue` is implemented as `IEnumerable<T>` and the `CollectionChanged` listener is implemented on the `CrossDownloadManager` itself.

### 0.9.1

  * Small fixes

### 0.9.0

  * [Bug] Updated session-identifier on iOS. (If another app is running, using the same extension, you don't have accesss to their downloads anymore)
  * [Feature] Option to disallow downloading via mobile network
  * [Bug] Fixed bug on iOS when reestablishing the queue after the app was shut down
