using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MinSheng_MIS.Services
{
    public class ComFunc
    {
        #region 更新資料夾內的檔案
        /// <summary>
        /// 
        /// </summary>
        /// <param name="DirectName">放置檔案的資料夾名稱</param>
        /// <param name="file">上傳的檔案</param>
        /// <param name="ser">伺服器位址取得</param>
        /// <param name="pathlist">要存入db的"相對"路徑</param>
        /// <param name="MainDir">最上層的放置所有檔案的資料夾名稱</param>
        /// <returns></returns>
        public static bool UpdateFile(List<HttpPostedFileBase> file, HttpServerUtilityBase ser,ref List<string> pathlist, string DirectName, string MainDir = "UploadDir")
        {
            try
            {
                if (file.Count != 0)
                {
                    string RootPath = ser.MapPath("~/");

                    string MainDirPath = Path.Combine(RootPath, MainDir); //上層資料夾
                    if (!Directory.Exists(MainDirPath))
                    {
                        Directory.CreateDirectory(MainDirPath);
                    }

                    string FullPath = Path.Combine(MainDirPath, DirectName); //該專案資料夾
                    if (!Directory.Exists(FullPath))
                    {
                        Directory.CreateDirectory(FullPath);
                    }
                    else 
                    {
                        string[] filePaths = Directory.GetFiles(FullPath);
                        foreach (string filePath in filePaths)
                        {
                            File.Delete(filePath);
                        }
                    }

                    foreach (var item in file) //上傳檔案
                    {
                        string newFileName = item.FileName;
                        int count = 1;
                        while (File.Exists(Path.Combine(FullPath, newFileName)))
                        {
                            newFileName = string.Concat(
                            Path.GetFileNameWithoutExtension(item.FileName) + "_" + count.ToString(),
                            Path.GetExtension(item.FileName).ToLower());
                            count++;
                        }

                        string fullFilePath = Path.Combine(FullPath, newFileName);
                        item.SaveAs(fullFilePath);

                        pathlist.Add("/" + MainDir + "/" + DirectName + "/" + newFileName);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region 檢查檔案類型及副檔名
        private static readonly HashSet<string> AllowedDocumentContentTypes = new HashSet<string>
        {
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/pdf"
        };

        private static readonly HashSet<string> AllowedDocumentExtensions = new HashSet<string>
        {
            ".doc", ".docx", ".pdf", ".DOC", ".DOCX", ".PDF"
        };

        private static readonly HashSet<string> AllowedImageContentTypes = new HashSet<string>
        {
            "image/jpeg", "image/jpg", "image/png"
        };

        private static readonly HashSet<string> AllowedImageExtensions = new HashSet<string>
        {
            ".jpeg", ".jpg", ".png", ".JPEG", ".JPG", ".PNG"
        };

        public static bool IsConformedForDocument(string contentType, string extension)
        {
            return AllowedDocumentContentTypes.Contains(contentType) && AllowedDocumentExtensions.Contains(extension);
        }

        public static bool IsConformedForImage(string contentType, string extension)
        {
            return AllowedImageContentTypes.Contains(contentType) && AllowedImageExtensions.Contains(extension);
        }
        #endregion

        #region 上傳檔案
        /// <summary>
        /// 將檔案重新命名後，上傳單個檔案至指定路徑
        /// </summary>
        /// <param name="file">上傳的檔案</param>
        /// <param name="folderpath">存入的"絕對"路徑</param>
        /// <param name="reName">重新命名的名稱</param>
        /// <returns>檔案是否上傳成功</returns>
        public static bool UploadFile(HttpPostedFileBase file, string folderpath, string reName)
        {
            try
            {
                if (!Directory.Exists(folderpath))
                    System.IO.Directory.CreateDirectory(folderpath);  // 如果不存在，則建立資料夾
                // Rename file
                string extension = Path.GetExtension(file.FileName); // 文件副檔名
                string filename = $"{reName}{extension}";
                string fileFullPath = Path.Combine(folderpath, filename);
                // 儲存檔案
                file.SaveAs(fileFullPath);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region 刪除檔案
        /// <summary>
        /// 刪除多個檔案(若不存在，則忽略)
        /// </summary>
        /// <param name="deleteFile">欲刪除檔案的名稱(須包含副檔名)</param>
        /// <param name="folderpath">欲刪除檔案的資料夾路徑</param>
        public static void DeleteFile(string[] deleteFile, string folderpath)
        {
            if (!Directory.Exists(folderpath)) /*throw new Exception("檔案路徑不存在");*/ return;
            // 刪除檔案
            foreach (var file in deleteFile)
            {
                string fileFullPath = Path.Combine(folderpath, file);
                if (System.IO.File.Exists(fileFullPath))
                    System.IO.File.Delete(fileFullPath);
                else
                    //throw new Exception("檔案不存在");
                    continue;
            }
        }
        #endregion

        #region Helper
        public static string UrlMaker(params string[] parts)
        {
            if (parts.Length == 0) return "";
            else
            {
                string url = string.Join("/", parts);
                return "/" + url;
            }
        }
        #endregion
    }
}