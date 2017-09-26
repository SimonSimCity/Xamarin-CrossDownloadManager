using PCLStorage;
using Plugin.DownloadManager;
using Plugin.DownloadManager.Abstractions;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DownloadExample
{
    public class Downloader
    {
        public IDownloadFile File;

        public Downloader()
        {
            CrossDownloadManager.Current.CollectionChanged += (sender, e) =>
                System.Diagnostics.Debug.WriteLine(
                    "[DownloadManager] " + e.Action +
                    " -> New items: " + (e.NewItems?.Count ?? 0) +
                    " at " + e.NewStartingIndex +
                    " || Old items: " + (e.OldItems?.Count ?? 0) +
                    " at " + e.OldStartingIndex
                );
        }

        public async Task InitializeDownload(string rootFolder)
        {
            var filePath = await RetrieveFilePath(rootFolder, "Audio.mp3");
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                CrossDownloadManager.Current.PathNameForDownloadedFile = downloadFile => filePath;
            }

            File = CrossDownloadManager.Current.CreateDownloadFile(
                "http://ipv4.download.thinkbroadband.com/10MB.zip"
            // If you need, you can add a dictionary of headers you need.
            //, new Dictionary<string, string> {
            //    { "Cookie", "LetMeDownload=1;" },
            //    { "Authorization", "Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ==" }
            //}
            );
        }

        public void StartDownloading(bool mobileNetworkAllowed)
        {
            CrossDownloadManager.Current.Start(File, mobileNetworkAllowed);
        }

        public void AbortDownloading()
        {
            CrossDownloadManager.Current.Abort(File);
        }

        public bool IsDownloading()
        {
            if (File == null) return false;

            switch (File.Status)
            {
                case DownloadFileStatus.INITIALIZED:
                case DownloadFileStatus.PAUSED:
                case DownloadFileStatus.PENDING:
                case DownloadFileStatus.RUNNING:
                    return true;

                case DownloadFileStatus.COMPLETED:
                case DownloadFileStatus.CANCELED:
                case DownloadFileStatus.FAILED:
                    return false;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public async Task<string> RetrieveFilePath(string rootFolder, string fileName)
        {
            try
            {
                var fileSystem = FileSystem.Current;
                var localStorage = FileSystem.Current.LocalStorage;
                if (!string.IsNullOrWhiteSpace(rootFolder))
                {
                    var downloadsFolder = await fileSystem.GetFolderFromPathAsync(rootFolder);
                    if (downloadsFolder != null)
                    {
                        localStorage = downloadsFolder;
                    }
                }

                var subFolder = await localStorage.CreateFolderAsync(Guid.NewGuid().ToString(), CreationCollisionOption.OpenIfExists);
                var file = await subFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                return file?.Path;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return string.Empty;
        }
    }
}