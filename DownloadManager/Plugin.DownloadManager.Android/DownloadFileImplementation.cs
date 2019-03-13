using System;
using System.Collections.Generic;
using System.ComponentModel;
using Android.App;
using Android.Database;
using Plugin.DownloadManager.Abstractions;
using Uri = Android.Net.Uri;

namespace Plugin.DownloadManager
{
    public class DownloadFileImplementation : IDownloadFile
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public long Id;

        public string Url { get; set; }

        private string _destinationPathName;

        public string DestinationPathName
        {
            get { return _destinationPathName; }
            set
            {
                if (Equals(_destinationPathName, value)) return;
                _destinationPathName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DestinationPathName)));
            }
        }

        public IDictionary<string, string> Headers { get; }

        private DownloadFileStatus _status;

        public DownloadFileStatus Status {
            get {
                return _status;
            }
            set {
                if (Equals(_status, value)) return;
                _status = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            }
        }

        private string _statusDetails;

        public string StatusDetails {
            get {
                return _statusDetails;
            }
            set {
                if (Equals(_statusDetails, value)) return;
                _statusDetails = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusDetails)));
            }
        }

        private float _totalBytesExpected;

        public float TotalBytesExpected {
            get {
                return _totalBytesExpected;
            }
            set {
                if (Equals(_totalBytesExpected, value)) return;
                _totalBytesExpected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalBytesExpected)));
            }
        }

        private float _totalBytesWritten;

        public float TotalBytesWritten {
            get {
                return _totalBytesWritten;
            }
            set {
                if (Equals(_totalBytesWritten, value)) return;
                _totalBytesWritten = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalBytesWritten)));
            }
        }

        /**
         * Initializing a new object to add it to the download-queue
         */
        public DownloadFileImplementation(string url, IDictionary<string, string> headers)
        {
            Url = url;
            Headers = headers;

            Status = DownloadFileStatus.INITIALIZED;
        }

        /**
         * Reinitializing an object after the app restarted
         */
        public DownloadFileImplementation(ICursor cursor)
        {
            Id = cursor.GetLong(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnId));
            Url = cursor.GetString(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnUri));

            switch ((DownloadStatus) cursor.GetInt(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnStatus)))
            {
                case DownloadStatus.Failed:
                    Status = DownloadFileStatus.FAILED;
                    break;

                case DownloadStatus.Paused:
                    Status = DownloadFileStatus.PAUSED;
                    break;

                case DownloadStatus.Pending:
                    Status = DownloadFileStatus.PENDING;
                    break;

                case DownloadStatus.Running:
                    Status = DownloadFileStatus.RUNNING;
                    break;

                case DownloadStatus.Successful:
                    Status = DownloadFileStatus.COMPLETED;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void StartDownload(Android.App.DownloadManager downloadManager, string destinationPathName,
            bool allowedOverMetered, DownloadVisibility notificationVisibility, bool isVisibleInDownloadsUi)
        {
            using (var downloadUrl = Uri.Parse(Url))
            using (var request = new Android.App.DownloadManager.Request(downloadUrl))
            {
                if (Headers != null)
                {
                    foreach (var header in Headers)
                    {
                        request.AddRequestHeader(header.Key, header.Value);
                    }
                }

                if (destinationPathName != null)
                {
                    var file = new Java.IO.File(destinationPathName);
                    request.SetDestinationUri(Uri.FromFile(file));

                    if (file.Exists())
                    {
                        file.Delete();
                    }
                }

                request.SetVisibleInDownloadsUi(isVisibleInDownloadsUi);
                request.SetAllowedOverMetered(allowedOverMetered);

                request.SetNotificationVisibility(notificationVisibility);

                Id = downloadManager.Enqueue(request);
            }
        }
    }
}
