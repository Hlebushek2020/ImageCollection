using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageCollection.Structures
{
    public struct OpenFolderArgs
    {
        public bool ContinueExecution { get; }
        public string BaseDirectory { get; }
        public SearchOption SearchOption { get; }
        public string SearchMask { get; }
        public string DistributionDirectory { get; }

        public OpenFolderArgs(bool continueExecution, string baseDirectory, bool recursiveSearch, string searchMask, string distributionDirectory)
        {
            ContinueExecution = continueExecution;
            BaseDirectory = baseDirectory;
            if (recursiveSearch)
                SearchOption = SearchOption.AllDirectories;
            else
                SearchOption = SearchOption.TopDirectoryOnly;
            SearchMask = searchMask;
            DistributionDirectory = distributionDirectory;
        }
    }
}
