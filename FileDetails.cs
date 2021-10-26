using System;
using System.IO;

namespace BigFiles1
{
    public class FileDetails
    {
        public FileDetails()
        {

        }
        public FileDetails(FileInfo f)
        {
            FileName = f.Name;
            Size = f.Length;
            FilePath = f.FullName;
        }
        public string FileName { get; private set; }
        public double Size { get; private set; }
        public string FilePath { get; private set; }
        public string FileExtn
        {
            get
            {
                return Path.GetExtension(FileName);
            }
        }
        public double SizeInMB
        {
            get
            {
                return Math.Round((double)Size / (1024 * 1024), 2);
            }
        }
    }
}
