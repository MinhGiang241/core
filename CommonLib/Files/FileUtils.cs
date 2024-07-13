using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace CommonLibCore.CommonLib.Files
{
    public static class FileUtils
    {
        /* lọc ra các tệp tin hợp lệ (tồn tại) từ một tập hợp đường dẫn tệp tin (filePaths) */
        public static string[] GetValidFile(this IEnumerable<string> filePaths)
        {
            List<FileInfo> validfiles = new List<FileInfo>();
            foreach (var filePath in filePaths)
            {
                FileInfo file = new FileInfo(filePath);
                if (file.Exists)
                {
                    if (!validfiles.Exists(f => f.Name == file.Name))
                        validfiles.Add(file);
                }
            }
            return validfiles.Select(f => f.FullName).ToArray();
        }

        public static string GetExtention(this string filename)
        {
            if (filename.IndexOf(".") > 0)
            {
                int dotindex = filename.LastIndexOf(".");
                string ext = filename.Substring(dotindex);
                return ext;
            }
            return "";
        }

        public static string GetContentType(string path)
        {
            try
            {
                var types = GetMimeTypes();
                var ext = Path.GetExtension(path).ToLowerInvariant();
                return types[ext];
            }
            catch (Exception ex)
            {
                return "application/octet-stream";
            }
        }

        public static Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                { ".txt", "text/plain" },
                { ".wav", "audio/wav" },
                { ".pdf", "application/pdf" },
                { ".doc", "application/vnd.ms-word" },
                { ".docx", "application/vnd.ms-word" },
                { ".xls", "application/vnd.ms-excel" },
                { ".xlsx", "application/vnd.openxmlformats" },
                { ".png", "image/png" },
                { ".jpg", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".gif", "image/gif" },
                { ".csv", "text/csv" },
                { ".m4a", "audio/mp4" },
                { ".m4v", "video/m4v" },
                { ".mov", "video/quicktime" },
                { ".3gp", "video/3gpp" },
                { ".mp4", "video/mp4" }
            };
        }

        //check and create if not existed
        public static void CheckDirectory(params string[] folderPaths)
        {
            foreach (var folderPath in folderPaths)
            {
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
            }
        }

        //copy from folder to folder
        public static void CopyDirectory(string SourcePath, string DestinationPath)
        {
            //Now Create all of the directories
            foreach (
                string dirPath in Directory.GetDirectories(
                    SourcePath,
                    "*",
                    SearchOption.AllDirectories
                )
            )
                Directory.CreateDirectory(dirPath.Replace(SourcePath, DestinationPath));

            //Copy all the files & Replaces any files with the same name
            foreach (
                string newPath in Directory.GetFiles(SourcePath, "*.*", SearchOption.AllDirectories)
            )
                File.Copy(newPath, newPath.Replace(SourcePath, DestinationPath), true);
        }

        //copy file
        public static void CopyFile(string source, string target)
        {
            if (File.Exists(target))
            {
                RemoveFile(target);
            }
            if (File.Exists(source))
            {
                string FolderPath = target.GetFolderPath();

                if (!Directory.Exists(FolderPath))
                    Directory.CreateDirectory(FolderPath);
                // overwrite the destination file if it already exists.
                File.Copy(source, target, true);
            }
        }

        public static void MoveFile(string source, string target)
        {
            CopyFile(source, target);
            RemoveFile(source);
        }

        public static void RemoveFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                // overwrite the destination file if it already exists.
                File.Delete(filePath);
            }
        }

        //create folder if not existed
        public static void ValidateFolder(String FolderPath)
        {
            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);
        }

        /// <summary>
        /// remove all file with name contain pattern
        /// </summary>
        /// <param name="pattern"></param>
        public static void RemoveFiles(String FolderPath, String pattern)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(FolderPath);
            FileInfo[] Files = directoryInfo.GetFiles($"*{pattern}*");
            foreach (var file in Files)
            {
                file.Delete();
            }
        }

        public static void UploadImage(String FilePath, byte[] bytes)
        {
            string FolderPath = FilePath.GetFolderPath();
            if (FolderPath.StartsWith("/"))
            {
                FolderPath = FolderPath.Substring(1);
            }
            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);
            if (File.Exists(FilePath))
                File.Delete(FilePath);
            File.WriteAllBytes(FilePath, bytes);
        }

        public static string GetFolderPath(this string FilePath)
        {
            string Folder = "";

            try
            {
                Uri uri;
                if (Regex.IsMatch(FilePath, ":"))
                    uri = new Uri(FilePath);
                else
                    uri = new Uri(
                        "http://domain.com/"
                            + (FilePath.StartsWith("/") ? FilePath.Substring(1) : FilePath)
                    );

                if (uri.Segments.Count() > 0)
                {
                    Folder = uri.Segments[0];
                    for (int i = 1; i < uri.Segments.Length - 1; i++)
                    {
                        string segment = uri.Segments[i];
                        Folder = string.Join("/", Folder, HttpUtility.UrlDecode(segment));
                    }
                }
            }
            catch { }
            return Folder.Replace("//", "/");
        }

        public static void Download(String linkdownload, String filepath)
        {
            using (WebClient Client = new WebClient())
            {
                string FolderPath = filepath.GetFolderPath();
                if (FolderPath.StartsWith("/"))
                {
                    FolderPath = FolderPath.Substring(1);
                }
                if (!Directory.Exists(FolderPath))
                    Directory.CreateDirectory(FolderPath);
                Client.DownloadFile(linkdownload, filepath);
            }
        }

        /// <summary>
        /// ghi nội dung text lên file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="content"></param>
        /// <param name="append"></param>
        public static void WriteToFile(string filePath, string content, bool append = false)
        {
            if (!append)
            {
                using (StreamWriter writer = File.CreateText(filePath))
                {
                    writer.WriteLine(content);
                }
            }
            else
            {
                using (StreamWriter writer = File.AppendText(filePath))
                {
                    writer.WriteLine(content);
                }
            }
        }

        public static string ReadAllText(string filePath)
        {
            try
            {
                string text = File.ReadAllText(filePath);
                return text;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
    }
}
