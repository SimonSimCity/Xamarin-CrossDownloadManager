using System.Linq;
using Foundation;
using Plugin.DownloadManager.Abstractions;

namespace Plugin.DownloadManager
{
    public class UrlSessionDownloadDelegate : NSUrlSessionDownloadDelegate
    {
        public DownloadManagerImplementation Controller;

        protected DownloadFileImplementation GetDownloadFileByTask (NSUrlSessionTask downloadTask)
        {
            return Controller.Queue
                .Cast<DownloadFileImplementation> ()
                .FirstOrDefault (
                    i => i.Task != null &&
                    (int)i.Task.TaskIdentifier == (int)downloadTask.TaskIdentifier
                );
        }

        /**
         * A Task was resumed (or started ..)
         */
        public override void DidResume (NSUrlSession session, NSUrlSessionDownloadTask downloadTask, long resumeFileOffset, long expectedTotalBytes)
        {
            var file = GetDownloadFileByTask (downloadTask);
            if (file == null) {
                downloadTask.Cancel ();
                return;
            }

            file.Status = DownloadFileStatus.RUNNING;
        }

        public override void DidCompleteWithError (NSUrlSession session, NSUrlSessionTask task, NSError error)
        {
            var file = GetDownloadFileByTask (task);
            if (file == null)
                return;

            file.StatusDetails = error.LocalizedDescription;
            file.Status = DownloadFileStatus.FAILED;

            Controller.RemoveFile (file);
        }

        /**
         * The Task keeps receiving data. Keep track of the current progress ...
         */
        public override void DidWriteData (NSUrlSession session, NSUrlSessionDownloadTask downloadTask, long bytesWritten, long totalBytesWritten, long totalBytesExpectedToWrite)
        {
            var file = GetDownloadFileByTask (downloadTask);
            if (file == null) {
                downloadTask.Cancel ();
                return;
            }

            file.Status = DownloadFileStatus.RUNNING;

            file.TotalBytesExpected = totalBytesExpectedToWrite;
            file.TotalBytesWritten = totalBytesWritten;
        }

        public override void DidFinishDownloading (NSUrlSession session, NSUrlSessionDownloadTask downloadTask, NSUrl location)
        {
            var file = GetDownloadFileByTask (downloadTask);
            if (file == null) {
                downloadTask.Cancel ();
                return;
            }

            // On iOS 9 and later, this method is called even so the response-code is 400 or higher. See https://github.com/cocos2d/cocos2d-x/pull/14683
            var response = downloadTask.Response as NSHttpUrlResponse;
            if (response != null && response.StatusCode >= 400) {
                file.StatusDetails = "Error.HttpCode: " + response.StatusCode;
                file.Status = DownloadFileStatus.FAILED;
            
                Controller.RemoveFile (file);
                return;
            }

            var success = true;
            var destinationPathName = Controller.PathNameForDownloadedFile?.Invoke (file);
            if (destinationPathName != null) {
                success = MoveDownloadedFile (file, location, destinationPathName);
                file.DestinationPathName = destinationPathName;
            }
            else
            {
                file.DestinationPathName = location.ToString();
            }

            // If the file destination is unknown or was moved successfully ...
            if (success) {
                file.Status = DownloadFileStatus.COMPLETED;
            }

            Controller.RemoveFile (file);
        }

        /**
         * Move the downloaded file to it's destination
         */
        public bool MoveDownloadedFile (DownloadFileImplementation file, NSUrl location, string destinationPathName)
        {
            var fileManager = NSFileManager.DefaultManager;

            var destinationUrl = new NSUrl (destinationPathName, false);
            NSError removeCopy;
            NSError errorCopy;

            fileManager.Remove (destinationUrl, out removeCopy);
            var success = fileManager.Copy (location, destinationUrl, out errorCopy);

            if (!success) {
                file.StatusDetails = errorCopy.LocalizedDescription;
                file.Status = DownloadFileStatus.FAILED;
            }

            return success;
        }

        public override void DidFinishEventsForBackgroundSession(NSUrlSession session)
        {
            var handler = CrossDownloadManager.BackgroundSessionCompletionHandler;
            if (handler != null)
            {
                CrossDownloadManager.BackgroundSessionCompletionHandler = null;
                handler();
            }
        }
    }
}
