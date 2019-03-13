using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using Plugin.DownloadManager;
using Plugin.DownloadManager.Abstractions;
using System;
using System.Linq;
using System.IO;

namespace DownloadExample.Droid
{
    [Activity (Label = "Download Example", Name = "com.example.download_example.MainActivity", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        void InitDownloadManager ()
        {
            // Define where the files should be stored. MUST be an external storage. (see https://github.com/SimonSimCity/Xamarin-CrossDownloadManager/issues/10)
            // If you skip this, you neither need the permission `WRITE_EXTERNAL_STORAGE`.
            //CrossDownloadManager.Current.PathNameForDownloadedFile = new Func<IDownloadFile, string> (file => {
            //    string fileName = Android.Net.Uri.Parse (file.Url).Path.Split ('/').Last ();
            //    return Path.Combine (ApplicationContext.GetExternalFilesDir (Android.OS.Environment.DirectoryDownloads).AbsolutePath, fileName);
            //});

            // In case you want to create your own notification :)
            //(CrossDownloadManager.Current as DownloadManagerImplementation).NotificationVisibility = DownloadVisibility.Hidden;

            // Prevents the file from appearing in the android download manager
            (CrossDownloadManager.Current as DownloadManagerImplementation).IsVisibleInDownloadsUi = true;
        }

        NotificationClickedBroadcastReceiver _receiverNotificationClicked;

        protected override void OnResume ()
        {
            base.OnResume ();

            _receiverNotificationClicked = new NotificationClickedBroadcastReceiver ();
            RegisterReceiver (
                _receiverNotificationClicked,
                new IntentFilter (DownloadManager.ActionNotificationClicked)
            );
        }

        protected override void OnPause ()
        {
            base.OnPause ();

            UnregisterReceiver (_receiverNotificationClicked);
        }

        protected override void OnCreate (Bundle savedInstanceState)
        {
            base.OnCreate (savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);

            InitDownloadManager ();

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button> (Resource.Id.myButton);

            var foo = new Downloader ();

            button.Click += delegate {
                // If already downloading, abort it.
                if (foo.IsDownloading()) {
                    foo.AbortDownloading ();
                    button.Text = "Download aborted.";
                    return;
                }

                button.Text = "Start downloading ...";

                foo.InitializeDownload();

                foo.File.PropertyChanged += (sender, e) => {
                    System.Diagnostics.Debug.WriteLine("[Property changed] " + e.PropertyName + " -> " + sender.GetType().GetProperty(e.PropertyName).GetValue(sender, null).ToString());

                    // Update UI text-fields
                    var downloadFile = ((IDownloadFile)sender);
                    switch (e.PropertyName) {
                        case nameof(IDownloadFile.Status):
                            FindViewById<TextView>(Resource.Id.value_status).Text = downloadFile.Status.ToString();
                            break;
                        case nameof(IDownloadFile.StatusDetails):
                            FindViewById<TextView>(Resource.Id.value_statusdetails).Text = downloadFile.StatusDetails;
                            break;
                        case nameof(IDownloadFile.TotalBytesExpected):
                            FindViewById<TextView>(Resource.Id.value_totalbytesexpected).Text = downloadFile.TotalBytesExpected.ToString();
                            break;
                        case nameof(IDownloadFile.TotalBytesWritten):
                            FindViewById<TextView>(Resource.Id.value_totalbyteswritten).Text = downloadFile.TotalBytesWritten.ToString();
                            break;
                    }

                    // Update UI if download-status changed.
                    if (e.PropertyName == "Status") {
                        switch (((IDownloadFile)sender).Status) {
                        case DownloadFileStatus.COMPLETED:
                        case DownloadFileStatus.FAILED:
                        case DownloadFileStatus.CANCELED:
                            button.Text = "Downloading finished.";

                            // Get the path this file was saved to. When you didn't set a custom path, this will be some temporary directory.
                            var nativeDownloadManager = (DownloadManager)ApplicationContext.GetSystemService (DownloadService);
                            System.Diagnostics.Debug.WriteLine (nativeDownloadManager.GetUriForDownloadedFile (((DownloadFileImplementation)sender).Id));

                            break;
                        }
                    }

                    // Update UI while donwloading.
                    if (e.PropertyName == "TotalBytesWritten" || e.PropertyName == "TotalBytesExpected") {
                        var bytesExpected = ((IDownloadFile)sender).TotalBytesExpected;
                        var bytesWritten = ((IDownloadFile)sender).TotalBytesWritten;

                        if (bytesExpected > 0) {
                            var percentage = Math.Round (bytesWritten / bytesExpected * 100);
                            button.Text = "Downloading (" + percentage + "%)";
                        }
                    }
                };

                try {
                    foo.StartDownloading(FindViewById<Switch>(Resource.Id.switch1).Checked);
                } catch (Java.Lang.IllegalArgumentException) {
                    foo.File = null;
                    button.Text = "Download crashed.";

                    try {
                        //Open the specific App Info page:
                        Intent intent = new Intent(Android.Provider.Settings.ActionApplicationDetailsSettings);
                        intent.SetData(Android.Net.Uri.Parse("package:com.android.providers.downloads"));
                        StartActivity(intent);
                    } catch (ActivityNotFoundException) {
                        //Open the generic Apps page:
                        Intent intent = new Intent(Android.Provider.Settings.ActionManageApplicationsSettings);
                        StartActivity(intent);
                    }
                }
            };
        }
    }
}


