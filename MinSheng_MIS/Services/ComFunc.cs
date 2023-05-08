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

                foreach (var item in file) //上傳檔案
                {
                    if (File.Exists(Path.Combine(FullPath, item.FileName))) //檢查是否有相同名稱的資料名稱，不然會壞掉
                    {
                        File.Delete(Path.Combine(FullPath, item.FileName));
                    }

                    string newFileName = string.Concat(
                    Path.GetFileNameWithoutExtension(item.FileName),
                    Path.GetExtension(item.FileName).ToLower()); ;

                    string fullFilePath = Path.Combine(FullPath, newFileName);
                    item.SaveAs(fullFilePath);

                    pathlist.Add("/" + Path.Combine(MainDir,DirectName,newFileName));
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion
    }
}