using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Plugin.DownloadManager.Abstractions;
using Windows.Networking.BackgroundTransfer;
using System.Linq;

namespace Plugin.DownloadManager
{
    /// <summary>
    /// The UWP implementation of the download manager.
    /// </summary>
    public class DownloadManagerImplementation : IDownloadManager
    {
        private readonly IList<IDownloadFile> _queue;

        public IEnumerable<IDownloadFile> Queue
        {
            get
            {
                lock (_queue)
                {
                    return _queue.ToList();
                }
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public Func<IDownloadFile, string> PathNameForDownloadedFile { get; set; }

        public DownloadManagerImplementation()
        {
            _queue = new List<IDownloadFile>();

            // Enumerate outstanding downloads.
            BackgroundDownloader.GetCurrentDownloadsAsync().AsTask().ContinueWith((downloadOperationsTask) => {
                foreach (var downloadOperation in downloadOperationsTask.Result)
                {
                    var downloadFile = new DownloadFileImplementation(downloadOperation);
                    downloadFile.PropertyChanged += UpdateFileProperty;
                    AddFile(downloadFile);
                }
            });
        }

        public IDownloadFile CreateDownloadFile(string url)
        {
            return CreateDownloadFile(url, new Dictionary<string, string>());
        }

        public IDownloadFile CreateDownloadFile(string url, IDictionary<string, string> headers)
        {
            return new DownloadFileImplementation(url, headers);
        }

        public void Start(IDownloadFile i, bool mobileNetworkAllowed = true)
        {
            var file = (DownloadFileImplementation)i;

            file.PropertyChanged += UpdateFileProperty;
            AddFile(file);

            string destinationPathName = null;
            if (PathNameForDownloadedFile != null)
            {
                destinationPathName = PathNameForDownloadedFile(file);
            }

            //Do not await here otherwise it will never return
            file.StartDownloadAsync(destinationPathName, mobileNetworkAllowed);
        }

        public void Abort(IDownloadFile i)
        {
            var file = (DownloadFileImplementation)i;

            file.PropertyChanged -= UpdateFileProperty;
            file.Cancel();

            RemoveFile(file);
        }

        public void AbortAll()
        {
            foreach (var file in Queue)
            {
                Abort(file);
            }
        }

        protected internal void AddFile(IDownloadFile file)
        {
            lock (_queue)
            {
                _queue.Add(file);
            }

            CollectionChanged?.Invoke(Queue, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, file));
        }

        protected internal void RemoveFile(IDownloadFile file)
        {
            lock (_queue)
            {
                _queue.Remove(file);
            }

            CollectionChanged?.Invoke(Queue, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, file));
        }

        private void UpdateFileProperty(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IDownloadFile.Status))
            {
                var downloadFile = (IDownloadFile)sender;

                if (downloadFile.Status == DownloadFileStatus.COMPLETED ||
                    downloadFile.Status == DownloadFileStatus.FAILED ||
                    downloadFile.Status == DownloadFileStatus.CANCELED)
                {
                    RemoveFile(downloadFile);
                }
            }
        }
    }
}
