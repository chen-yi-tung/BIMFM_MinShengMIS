using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using MiniExcelLibs.OpenXml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

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

        private static readonly HashSet<string> AllowedPdfContentTypes = new HashSet<string>
        {
            "application/pdf"
        };

        private static readonly HashSet<string> AllowedPdfExtensions = new HashSet<string>
        {
            ".PDF"
        };

        private static readonly HashSet<string> AllowedImageContentTypes = new HashSet<string>
        {
            "image/jpeg", "image/jpg", "image/png"
        };

        private static readonly HashSet<string> AllowedImageExtensions = new HashSet<string>
        {
            ".jpeg", ".jpg", ".png", ".JPEG", ".JPG", ".PNG"
        };

        public static bool IsConformedForPdf(string contentType, string extension)
        {
            return AllowedPdfContentTypes.Contains(contentType) && AllowedPdfExtensions.Contains(extension);
        }

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
        /// <param name="reName">重新命名的名稱，若不須改名，則不填寫</param>
        /// <returns>檔案是否上傳成功</returns>
        public static bool UploadFile(HttpPostedFileBase file, string folderpath, string reName = null)
        {
            try
            {
                string filename = Path.GetFileName(file.FileName);

                if (!Directory.Exists(folderpath))
                    System.IO.Directory.CreateDirectory(folderpath);  // 如果不存在，則建立資料夾
                // Rename file
                if (!string.IsNullOrEmpty(reName))
                {
                    string extension = Path.GetExtension(file.FileName); // 文件副檔名
                    filename = $"{reName}{extension}";
                }
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
        /// <param name="folderpath">欲刪除檔案的資料夾路徑</param>
        /// <param name="filename">欲刪除檔案的名稱，若不指定則輸入"*"</param>
        /// <param name="extension">欲刪除檔案的副檔名，若不指定則輸入"*"，若檔案名稱已包含副檔名則輸入null</param>
        public static void DeleteFile(string folderpath, string filename, string extension)
        {
            string[] deleteFile ;
            if (!Directory.Exists(folderpath)) /*throw new Exception("檔案路徑不存在");*/ return;
            // 找出符合的檔案
            if (!string.IsNullOrEmpty(extension)) deleteFile = Directory.GetFiles(folderpath, $"{filename}.{extension}");
            else deleteFile = Directory.GetFiles(folderpath, filename);
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

        #region 取得檔案路徑
        /// <summary>
        /// 以資料夾路徑取得所有或指定檔案路徑
        /// </summary>
        /// <param name="path">資料夾"相對"路徑</param>
        /// <param name="filename">指定檔案名稱(不含副檔名)</param>
        /// <returns></returns>
        public static List<string> GetFilePath(string path, HttpServerUtilityBase ser, string filename)
        {
            string folderpath = ser.MapPath($"~/{path}/");
            if (Directory.Exists(folderpath))  // 資料夾路徑存在
            {
                List<string> files = new List<string>();
                DirectoryInfo di = new DirectoryInfo(folderpath);
                if (!string.IsNullOrEmpty(filename))
                    foreach (var fi in di.GetFiles($"{filename}.*"))
                        files.Add(UrlMaker(path, fi.Name));
                else
                    foreach (var fi in di.GetFiles())
                        files.Add(UrlMaker(path, fi.Name));

                return files;
            }
            else
                return null;  // 資料路徑不存在，表示未有檔案
        }
        #endregion

        #region 匯出Excel
        /// <summary>
        /// 匯出Excel
        /// </summary>
        /// <param name="ser">伺服器位址取得</param>
        /// <param name="data">匯出資料</param>
        /// <param name="ctrlName">控制器名稱</param>
        /// <returns></returns>
        public static Object ExportExcel(HttpServerUtilityBase ser, JToken data, string ctrlName)
        {
            IEnumerable<Dictionary<string, object>> rows = data.ToObject<IEnumerable<Dictionary<string, object>>>();

            // 讀取Json file匯入config
            var jo = Helper.ReadJsonFile(ser, ExcelConfigJsonPath);
            var setting = jo[ctrlName];

            var config = new OpenXmlConfiguration
            {
                DynamicColumns = setting["Config"].Select((x, index) => new DynamicExcelColumn($"{x["Column"]}")
                {
                    Index = index,
                    Name = x["Name"].ToString(),
                    Width = Convert.ToInt32(x["Width"])
                }).ToArray()
            };

            // 從rows篩選符合setting的字典
            var setting_col = setting["Config"].Select(x => x["Column"].ToString()).ToList();
            rows = rows.Select(x =>
            {
                return x.Where(k => setting_col.Contains(k.Key)).ToDictionary(k => k.Key, v => v.Value);
            }).ToList();

            var memoryStream = new MemoryStream();
            memoryStream.SaveAs(rows, configuration: config);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return new
            {
                FileDownloadName = $"{setting["Title"]}.xlsx",
                FileContents = memoryStream.ToArray()
            };
        }
        #endregion

        #region 產生下一個ID/SN
        /// <summary>
        /// 根據指定的格式表達式和最新 ID 生成下一個唯一 ID
        /// </summary>
        /// <param name="formatExpress">格式表達式</param>
        /// <param name="latestID">最近一次使用的 ID</param>
        /// <returns>新生成的 ID</returns>
        public static string CreateNextID(string formatExpress, string latestID)
        {
            if (string.IsNullOrEmpty(formatExpress))
                throw new ArgumentException("格式表達式不能為空或 null。", nameof(formatExpress));

            var idBuilder = new StringBuilder();

            for (int i = 0; i < formatExpress.Length; i++)
            {
                switch (formatExpress[i])
                {
                    case '!':
                        i = AppendDateFormat(formatExpress, idBuilder, ref i);
                        break;
                    case '%':
                        i = AppendSequence(formatExpress, idBuilder, latestID, ref i);
                        break;
                    default:
                        idBuilder.Append(formatExpress[i]);
                        break;
                }
            }

            return idBuilder.ToString();
        }

        /// <summary>
        /// 處理日期格式佔位符
        /// </summary>
        private static int AppendDateFormat(string formatExpress, StringBuilder idBuilder, ref int index)
        {
            int endIndex = formatExpress.IndexOf('}', index);
            if (endIndex == -1)
                throw new FormatException("日期格式表達式無效");

            string dateFormat = formatExpress.Substring(index + 2, endIndex - index - 2);
            idBuilder.Append(DateTime.Now.ToString(dateFormat));
            return endIndex;
        }

        /// <summary>
        /// 處理序列號佔位符
        /// </summary>
        private static int AppendSequence(string formatExpress, StringBuilder idBuilder, string latestID, ref int index)
        {
            int endIndex = formatExpress.IndexOf('}', index);
            if (endIndex == -1)
                throw new FormatException("序列號格式表達式無效");

            string sequenceLengthText = formatExpress.Substring(index + 2, endIndex - index - 2);
            if (!int.TryParse(sequenceLengthText, out int sequenceLength))
                throw new FormatException("序列號長度格式無效");

            string currentDatePart = idBuilder.ToString();
            string nextSequence = GenerateNextSequence(currentDatePart, latestID, sequenceLength);

            idBuilder.Append(nextSequence);
            return endIndex;
        }

        /// <summary>
        /// 生成下一個序列號，支持按日期重置
        /// </summary>
        private static string GenerateNextSequence(string currentDatePart, string latestID, int sequenceLength)
        {
            // 如果沒有最近的 ID 或日期部分不同，重置為 1
            if (string.IsNullOrEmpty(latestID) || !latestID.StartsWith(currentDatePart))
            {
                return "1".PadLeft(sequenceLength, '0');
            }

            // 提取並遞增序列號
            string sequencePart = latestID.Substring(currentDatePart.Length, sequenceLength);

            if (!int.TryParse(sequencePart, out int currentSequence))
                throw new FormatException("最近的 ID 中序列號格式無效");

            int nextSequence = currentSequence + 1;
            return nextSequence.ToString().PadLeft(sequenceLength, '0');
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

        #region 參數
        private static readonly string ExcelConfigJsonPath = "~/App_Data/ExcelConfig.json";
        #endregion
    }
}