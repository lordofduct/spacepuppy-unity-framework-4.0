using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace com.spacepuppyeditor.Internal
{

    internal static class SPTempFolder
    {

        const string RELPATH_TEMPFOLDER = "Temp/Spacepuppy";

        static void EnsureTempFolderExists()
        {
            if (!Directory.Exists("Temp"))
            {
                Directory.CreateDirectory("Temp");
            }
            if (!Directory.Exists("Temp/Spacepuppy"))
            {
                Directory.CreateDirectory("Temp/Spacepuppy");
            }
        }

        public static void BackupFile(string filepath, string suffix = ".bak")
        {
            EnsureTempFolderExists();

            var filename = Path.GetFileName(filepath);
            var temppath = Path.Combine(RELPATH_TEMPFOLDER, filename + suffix);
            File.Copy(filepath, temppath, true);
        }

        public static void ResetFromBackup(string filepath, string suffix = ".bak")
        {
            var filename = Path.GetFileName(filepath);
            var temppath = Path.Combine(RELPATH_TEMPFOLDER, filename + suffix);
            if (File.Exists(temppath))
            {
                File.Copy(temppath, filepath, true);
            }
        }

        public static void ResetFromBackupAndPurge(string filepath, string suffix = ".bak")
        {
            var filename = Path.GetFileName(filepath);
            var temppath = Path.Combine(RELPATH_TEMPFOLDER, filename + suffix);
            if (File.Exists(temppath))
            {
                File.Copy(temppath, filepath, true);
                File.Delete(temppath);
            }
        }

        public static void PurgeBackup(string filepath, string suffix = ".bak")
        {
            var filename = Path.GetFileName(filepath);
            var temppath = Path.Combine(RELPATH_TEMPFOLDER, filename + suffix);
            if (File.Exists(temppath))
            {
                File.Delete(temppath);
            }
        }

        public static void WriteAllText(string filename, string stext)
        {
            EnsureTempFolderExists();
            File.WriteAllText(Path.Combine(RELPATH_TEMPFOLDER, filename), stext);
        }

        public static string ReadAllText(string filename)
        {
            var path = Path.Combine(RELPATH_TEMPFOLDER, filename);
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }
            return string.Empty;
        }

        public static bool Exists(string filename) => File.Exists(Path.Combine(RELPATH_TEMPFOLDER, filename));

        public static bool Delete(string filename)
        {
            var path = Path.Combine(RELPATH_TEMPFOLDER, filename);
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }
            else
            {
                return false;
            }
        }

    }

}
