using System.Collections.Generic;
using System.IO;

namespace ModTools
{
    public static class FileUtil
    {

        public static List<string> ListFilesInDirectoryRecursively(string path, List<string> _filesMustBeNull = null)
        {
            _filesMustBeNull = _filesMustBeNull ?? new List<string>();

            foreach (string file in Directory.GetFiles(path))
            {
                _filesMustBeNull.Add(file);
            }

            foreach (string dir in Directory.GetDirectories(path))
            {
                ListFilesInDirectoryRecursively(dir, _filesMustBeNull);
            }

            return _filesMustBeNull;
        }

    }

}
