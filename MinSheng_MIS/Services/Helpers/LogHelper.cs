using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Services.Helpers
{
    public static class LogHelper
    {
        private static readonly string yearMonth = DateTime.Now.ToString("yyyyMM");
        private static readonly string LogDirectory = HttpContext.Current.Server.MapPath($"~/Logs/LoginLogs/{yearMonth}");
        private static readonly string ErrorLogDirectory = HttpContext.Current.Server.MapPath($"~/Logs/ErrorLogs/{yearMonth}");

        #region 記錄登入log
        public static void WriteLoginLog(object controller, string username, string result)
        {
            try
            {
                // 確保 Log 目錄存在
                if (!Directory.Exists(LogDirectory))
                {
                    Directory.CreateDirectory(LogDirectory);
                }

                // 產生每日的日誌檔案名稱 (例: 2025-03-31_login.txt)
                string logFileName = Path.Combine(LogDirectory, $"{DateTime.Now:yyyy-MM-dd}_login.txt");

                // 取得當前 Controller 名稱
                string controllerName = controller?.GetType().Name ?? "UnknownController";

                // 取得 Request IP
                string ip = HttpContext.Current?.Request?.UserHostAddress ?? "UnknownIP";

                // 組成 Log 訊息
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | TRACE | {controller} | Request IP : {ip} | UserName : {username} | {result}";

                // 將 Log 訊息寫入檔案
                using (StreamWriter writer = new StreamWriter(logFileName, true))
                {
                    writer.WriteLine(logEntry);
                }
            }
            catch (Exception ex)
            {
                // 若發生錯誤，可以考慮將錯誤訊息記錄到另一個錯誤日誌中
                File.AppendAllText(Path.Combine(LogDirectory, "error_log.txt"), $"{DateTime.Now}: {ex.Message}\n");
            }
        }
        #endregion

        #region 紀錄異常log
        public static void WriteErrorLog(object controller, string username, string errorMessage)
        {
            try
            {
                // 確保 Log 目錄存在
                if (!Directory.Exists(ErrorLogDirectory))
                {
                    Directory.CreateDirectory(ErrorLogDirectory);
                }

                // 產生每日的日誌檔案名稱 (例: 2025-03-31_login.txt)
                string logFileName = Path.Combine(ErrorLogDirectory, $"{DateTime.Now:yyyy-MM-dd}_error.txt");

                // 取得當前 Controller 名稱
                string controllerName = controller?.GetType().Name ?? "UnknownController";

                // 取得 Request IP
                string ip = HttpContext.Current?.Request?.UserHostAddress ?? "UnknownIP";

                // 組成 Log 訊息
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | ERROR | {controller} | Request IP : {ip} | UserName : {username} | {errorMessage}";

                // 將 Log 訊息寫入檔案
                using (StreamWriter writer = new StreamWriter(logFileName, true))
                {
                    writer.WriteLine(logEntry);
                }
            }
            catch (Exception ex)
            {
                // 若發生錯誤，可以考慮將錯誤訊息記錄到另一個錯誤日誌中
                File.AppendAllText(Path.Combine(ErrorLogDirectory, "error_log.txt"), $"{DateTime.Now}: {ex.Message}\n");
            }
        }
        #endregion
    }
}