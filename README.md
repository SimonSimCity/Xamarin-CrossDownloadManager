## CrossDownloadManager

The CrossDownloadManager is a plugin that helps you downloading files in the background.

### Build Status:
![GitHub tag](https://img.shields.io/github/tag/SimonSimCity/xamarin-crossdownloadmanager.svg)
[![NuGet](https://img.shields.io/nuget/v/Xam.Plugins.DownloadManager.svg?label=NuGet)](https://www.nuget.org/packages/Xam.Plugins.DownloadManager/)

### Where can I use it?

|Platform|Supported|Version|
| ------------------- | :-----------: | :------------------: |
|Xamarin.iOS|Yes|iOS 7+|
|Xamarin.iOS Unified|Yes|iOS 7+|
|Xamarin.Android|Yes|API 16+|
|Windows 10 UWP|Yes|10.0.10240.0|
|Xamarin.Mac|No||

### Getting started

Add the nuget package to your cross-platform project and to every platform specific project. Now, you have to initialize the service for every platform. You also need to write some logic, which determines where the file will be saved.

#### iOS

_AppDelegate.cs_
```csharp
/**
 * Save the completion-handler we get when the app opens from the background.
 * This method informs iOS that the app has finished all internal processing and can sleep again.
 */
public override void HandleEventsForBackgroundUrl(UIApplication application, string sessionIdentifier, Action completionHandler)
{
    CrossDownloadManager.BackgroundSessionCompletionHandler = completionHandler;
}
```

As of iOS 9, your URL must be secured or you have to add the domain to the list of exceptions. See [https://developer.apple.com/library/ios/releasenotes/General/WhatsNewIniOS/Articles/iOS9.html#//apple_ref/doc/uid/TP40016198-SW14](https://developer.apple.com/library/ios/releasenotes/General/WhatsNewIniOS/Articles/iOS9.html#//apple_ref/doc/uid/TP40016198-SW14)

### Start downloading

You can now start a download by adding the following code:
```csharp
var downloadManager = CrossDownloadManager.Current;
var file = downloadManager.CreateDownloadFile(url);
downloadManager.Start(file);
```

This will add the file to a native library, which starts the download of that file. You can watch the properties of the `IDownloadFile` instance and execute some code if e.g. the status changes to `COMPLETED`, you can also watch the `IDownloadManager.Queue` and execute some code if the list of files, that will be downloaded or are currently downloading changes.

After a download has been completed, the instance of `IDownloadFile` is then removed from `IDownloadManager.Queue`.

You can also disallow downloading via a cellular network by setting the second parameter of `CrossDownloadManager.Current.Start()`.

### Where are the files stored?

#### Default Option - Temporary Location

When you choose not to provide your own path before starting the download, the downloaded files are stored at a temporary directory and may be removed by the OS e.g. when the system runs out of space. You can move this file to a decided destination by listening on whether the status of the files changes to `DownloadFileStatus.COMPLETED`. You can find an implementation in the sample: https://github.com/SimonSimCity/Xamarin-CrossDownloadManager/issues/27

#### Recommended Option - Custom Location

Usually, you would expect to set the path to the `IDownloadFile` instance, you get when calling `downloadManager.CreateDownloadFile(url)`. But, as this download manager even continues downloading when the app crashed, you have to be able to reconstruct the path in every stage of the app. The correct way is to register a method as early as possible, that, in every circumstance, can reconstruct the path that the file should be saved. This method could look like following:
```csharp
CrossDownloadManager.Current.PathNameForDownloadedFile = new System.Func<IDownloadFile, string> (file => {
#if __IOS__
    string fileName = (new NSUrl(file.Url, false)).LastPathComponent;
    return Path.Combine(Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments), fileName);
#elif __ANDROID__
    string fileName = Android.Net.Uri.Parse(file.Url).Path.Split('/').Last();
    return Path.Combine (ApplicationContext.GetExternalFilesDir (Android.OS.Environment.DirectoryDownloads).AbsolutePath, fileName);
#else
    string fileName = '';
    return Path.Combine(Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments), fileName);
#endif
});
```

**Warning:** Please check if the files exist. The CrossDownloadManager will overwrite the file if it already exists.

##### Additional for Andriod

On Android, the destination location must be a located outside of your Apps internal directory (see [#10](https://github.com/SimonSimCity/Xamarin-CrossDownloadManager/issues/10) for details). To allow your app to write to that location, you either have to add the permission `WRITE_EXTERNAL_STORAGE` to the mainfest.xml file to require it when installing the app
```xml
<uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" />
```

or to request it at runtime (See [#20](https://github.com/SimonSimCity/Xamarin-CrossDownloadManager/issues/20)).

All finished downloads are registered in a native `Downloads` application. If you want your finished download not to be listed there, see [#17](https://github.com/SimonSimCity/Xamarin-CrossDownloadManager/issues/17)

### I want to use $FAVORITE_IOC_LIBRARY

Just register the instance in `CrossDownloadManager.Current` in the library. Here's an example how to do it on MvvmCross:

```csharp
Mvx.RegisterSingleton<IDownloadManager>(() => CrossDownloadManager.Current);
```

### Can I have a look at a sample implementation?

I've created a quite basic implementation for UWP, iOS and Android which you can find [here](https://github.com/SimonSimCity/Xamarin-CrossDownloadManager/tree/develop/Sample). Feel free to fork this repository and play around with it. It's right within the `Sample` folder.

### Why do the files show up in the native Android Download Manager?

This is the default of the native download manager on Android. You can change this behavior by setting the property `IsVisibleInDownloadsUi` of the Android implementation of the download manager to `false`. Here's an example which you can copy into your Android application:

```csharp
(CrossDownloadManager.Current as DownloadManagerImplementation).IsVisibleInDownloadsUi = false;
```

### Contribute / Bugs / Features

Everything you tell me is contributing to this project and helps me improving it. If you found a bug or want a feature, please file an issue to let me know. For bugs, please include as much information as you know. If you can, please fork this repository, reproduce the bug in the sample projects and include a link in the issue. For features, feel free to start developing them. I'm always willing to contribute, help and give advice.

### Licensing

[This plugin is licensed under the MIT License](https://github.com/SimonSimCity/Xamarin-CrossDownloadManager/blob/develop/LICENSE.md)

### Contributors

* [SimonSimCity](https://github.com/SimonSimCity)
* [martijn00](https://github.com/martijn00)
* [fela98](https://github.com/fela98)
* [BtrJay](https://github.com/BtrJay)

### Changes

Moved the changelog to [CHANGELOG.md](https://github.com/SimonSimCity/Xamarin-CrossDownloadManager/blob/develop/CHANGELOG.md)

### Known issues

#### Exception `java.lang.IllegalArgumentException: Unknown URL content://downloads/my_downloads` on Android when calling one of `IDownloadManager.Start (downloadFile)`, `IDownloadManager.Abort (downloadFile)` or `IDownloadManager.AbortAll (downloadFile)`

This might occur because the user has disabled the system app `Download Manager`. You can kindly inform him using a message box after which you delegate him to the settings where he can enable it again. You can use the code-snippet I added in the sample (credits to [dashika](https://github.com/dashika) [#82](https://github.com/SimonSimCity/Xamarin-CrossDownloadManager/issues/82)).

https://github.com/SimonSimCity/Xamarin-CrossDownloadManager/blob/dad3082365ef9161e982fdf1a4dba5f83c63c3ad/Sample/Droid/MainActivity.cs#L127-L142
