using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BigFiles1
{
    public class FileSizeProvider
    {
        public void CancelSearch(Action<string> reportCancellationStatus)
        {
            try
            {
                if (searchThread != null && searchThread.IsAlive)
                {
                    searchThread.Abort();
                }
            }
            catch
            {
                reportCancellationStatus?.Invoke("Search cancelled");
            }
        }

        Thread searchThread;
        public void Search(
            string directoryPath,
            long sizeLimit,
            Action<FileInfo> reportResult,
            Action<string> reportError,
            Action<bool> searchCompleted)
        {
            searchThread = new Thread(() =>
            {
                try
                {
                    SearchDir(directoryPath, sizeLimit, reportResult, reportError, searchCompleted);

                    searchCompleted?.Invoke(true);
                }
                catch (Exception f)
                {
                    reportError?.Invoke(f.Message);
                    searchCompleted?.Invoke(false);
                }
            });

            searchThread.Start();
        }

        void SearchDir(string directoryPath,
            long sizeLimit,
            Action<FileInfo> reportResult,
            Action<string> reportError,
            Action<bool> searchCompleted)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(directoryPath);

                SearchFiles(dir.FullName, sizeLimit, reportResult, reportError, searchCompleted);

                foreach (var subDirectory in dir.EnumerateDirectories())
                {
                    SearchDir(subDirectory.FullName, sizeLimit, reportResult, reportError, searchCompleted);
                }

            }
            catch (Exception f)
            {
                reportError?.Invoke(f.Message);
            }
        }

        void SearchFiles(string directoryPath,
            long sizeLimit,
            Action<FileInfo> reportResult,
            Action<string> reportError,
            Action<bool> searchCompleted)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(directoryPath);

                foreach (var file in dir.EnumerateFiles())
                {
                    long size = 0;
                    try
                    {
                        size = file.Length;
                    }
                    catch (Exception e)
                    {
                        reportError(e.Message);
                    }

                    if (size > sizeLimit)
                    {
                        reportResult?.Invoke(file);
                    }
                }
            }
            catch (Exception f)
            {
                reportError?.Invoke(f.Message);
            }
        }

    }
}
