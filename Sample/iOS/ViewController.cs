using Plugin.DownloadManager;
using Plugin.DownloadManager.Abstractions;
using System;
using UIKit;

namespace DownloadExample.iOS
{
    public partial class ViewController : UIViewController
    {
        public ViewController(IntPtr handle) : base(handle)
        {
            // If you want to do some extensional things - like showing a dialog after all files finished downloading.
            // If you set this after `CrossDownloadManager.Current` has been called, this will be silently ignored!
            CrossDownloadManager.UrlSessionDownloadDelegate = new ExtendedUrlSessionDownloadDelegate();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // If you want to take full control of the saved file, see `ExtendedUrlSessionDownloadDelegate`
            //CrossDownloadManager.Current.PathNameForDownloadedFile = new Func<IDownloadFile, string> (file => {
            //    string fileName = (new NSUrl (file.Url, false)).LastPathComponent;
            //    return Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments), fileName);
            //});

            // Perform any additional setup after loading the view, typically from a nib.
            Button.AccessibilityIdentifier = "myButton";
            Button.TouchUpInside += ButtonOnTouchUpInside;
        }

        private async void ButtonOnTouchUpInside(object control, EventArgs eventArgs)
        {
            var foo = new Downloader();

            // If already downloading, abort it.
            if (foo.IsDownloading())
            {
                foo.AbortDownloading();
                Button.SetTitle("Download aborted.", UIControlState.Normal);
                return;
            }

            Button.SetTitle("Start downloading ...", UIControlState.Normal);

            var rootFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            await foo.InitializeDownload(rootFolder);

            foo.File.PropertyChanged += (sender, e) =>
            {
                // Update UI text-fields
                var downloadFile = ((IDownloadFile)sender);
                switch (e.PropertyName)
                {
                    case nameof(IDownloadFile.Status):
                        InvokeOnMainThread(() =>
                        {
                            StatusTextField.Text = downloadFile.Status.ToString();
                        });
                        break;

                    case nameof(IDownloadFile.StatusDetails):
                        InvokeOnMainThread(() =>
                        {
                            StatusDetailsTextField.Text = downloadFile.StatusDetails;
                        });
                        break;

                    case nameof(IDownloadFile.TotalBytesExpected):
                        InvokeOnMainThread(() =>
                        {
                            TotalBytesExpectedTextField.Text = downloadFile.TotalBytesExpected.ToString();
                        });
                        break;

                    case nameof(IDownloadFile.TotalBytesWritten):
                        InvokeOnMainThread(() =>
                        {
                            TotalBytesWrittenTextField.Text = downloadFile.TotalBytesWritten.ToString();
                        });
                        break;
                }

                // Update UI if download-status changed.
                if (e.PropertyName == "Status")
                {
                    switch (((IDownloadFile)sender).Status)
                    {
                        case DownloadFileStatus.COMPLETED:
                        case DownloadFileStatus.FAILED:
                        case DownloadFileStatus.CANCELED:
                            InvokeOnMainThread(() =>
                            {
                                Button.SetTitle("Downloading finished.", UIControlState.Normal);
                            });
                            break;
                    }
                }

                // Update UI while donwloading.
                if (e.PropertyName == "TotalBytesWritten" || e.PropertyName == "TotalBytesExpected")
                {
                    var bytesExpected = ((IDownloadFile)sender).TotalBytesExpected;
                    var bytesWritten = ((IDownloadFile)sender).TotalBytesWritten;

                    if (bytesExpected > 0)
                    {
                        var percentage = Math.Round(bytesWritten / bytesExpected * 100);
                        InvokeOnMainThread(() =>
                        {
                            Button.SetTitle("Downloading (" + percentage + "%)", UIControlState.Normal);
                        });
                    }
                }
            };

            foo.StartDownloading(Switch.On);
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}