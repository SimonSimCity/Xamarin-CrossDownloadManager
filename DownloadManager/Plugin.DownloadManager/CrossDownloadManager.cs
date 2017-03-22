using System;
using Plugin.DownloadManager.Abstractions;

namespace Plugin.DownloadManager
{
    /// <summary>
    /// Cross platform DownloadManager implemenations
    /// </summary>
    public class CrossDownloadManager
    {
        private static Lazy<IDownloadManager> Implementation = new Lazy<IDownloadManager> (() => CreateDownloadManager (), System.Threading.LazyThreadSafetyMode.PublicationOnly);

#if __IOS__
        /// <summary>
        /// Set the background session completion handler.
        /// @see: https://developer.xamarin.com/guides/ios/application_fundamentals/backgrounding/part_4_ios_backgrounding_walkthroughs/background_transfer_walkthrough/#Handling_Transfer_Completion
        /// </summary>
        public static Action BackgroundSessionCompletionHandler;

        /// <summary>
        /// The URL session download delegate.
        /// @see https://developer.apple.com/library/ios/documentation/Foundation/Reference/NSURLSessionDownloadDelegate_protocol/#//apple_ref/occ/intfm/NSURLSessionDownloadDelegate/URLSession:downloadTask:didResumeAtOffset:expectedTotalBytes:
        /// </summary>
        public static UrlSessionDownloadDelegate UrlSessionDownloadDelegate;
#endif

        /// <summary>
        /// The platform-implementation
        /// </summary>
        public static IDownloadManager Current {
            get {
                var ret = Implementation.Value;
                if (ret == null) {
                    throw NotImplementedInReferenceAssembly ();
                }
                return ret;
            }
        }

        private static IDownloadManager CreateDownloadManager ()
        {
#if __IOS__
            return new DownloadManagerImplementation (UrlSessionDownloadDelegate ?? new UrlSessionDownloadDelegate());
#elif __ANDROID__ || __UNIFIED__ || WINDOWS_UWP
            return new DownloadManagerImplementation();
#else
            return null;
#endif
        }

        internal static Exception NotImplementedInReferenceAssembly ()
        {
            return new NotImplementedException ("This functionality is not implemented in the portable version of this assembly.  You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
        }
    }
}
