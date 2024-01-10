using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Models;
using MinSheng_MIS.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Data.Entity.Migrations;
using System.IO;
using Newtonsoft.Json;

namespace MinSheng_MIS.Controllers
{
	public class ExperimentData_ManagementController : Controller
	{
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
        static readonly string folderPath = "Files/ExperimentalDataRecord";
        // GET: ExperimentData_Management
        #region 實驗數據管理
        public ActionResult Management()
		{
			return View();
		}
		#endregion

		#region 新增實驗數據紀錄
		public ActionResult Create()
		{
			return View();
		}

        [HttpPost]
        public async Task<ActionResult> CreateExperimentData(EDRecord edRecord)
        {
            ModelState.Remove("EDRSN");
            if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);  // Data Annotation未通過
            //else if (edRecord.ExperimentalDataItem.Where(x => x.DataName.Trim().Length > 200).Count() > 0) return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "數據標題 的長度最多200個字元。");

            DateTime now = DateTime.Now;
            // 新增實驗數據紀錄
            var count = await db.ExperimentalDataRecord.Where(x => DbFunctions.TruncateTime(x.UploadDateTime) == now.Date).CountAsync() + 1;  // 實驗標籤流水碼
            var data = new ExperimentalDataRecord
            {
                EDRSN = now.ToString("yyMMdd") + count.ToString().PadLeft(3, '0'),
                TAWSN = edRecord.TAWSN,
                EDate = edRecord.EDate,
                UploadUserName = User.Identity.Name,
                UploadDateTime = now,
                EDFile = "undefined"
            };
            db.ExperimentalDataRecord.Add(data);
            // 新增實驗數據紀錄資料
            ICollection<ExperimentalData> items = AddOrUpdateList<ExperimentalData>(edRecord.ExperimentalDataItem, data.EDRSN);
            db.ExperimentalData.AddRange(items);
            // [實驗數據檔案]檔案處理，目前只提供單個檔案上傳
            if (edRecord.EDFile != null && edRecord.EDFile.ContentLength > 0) // 上傳
            {
                var File = edRecord.EDFile;
                string extension = Path.GetExtension(File.FileName); // 檔案副檔名
                if (ComFunc.IsConformedForDocument(File.ContentType, extension) || ComFunc.IsConformedForImage(File.ContentType, extension)) // 檔案白名單檢查
                {
                    // 檔案上傳
                    if (!ComFunc.UploadFile(File, Server.MapPath($"~/{folderPath}/"), data.EDRSN))
                        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "檔案上傳過程出錯!");
                    data.EDFile = data.EDRSN + extension;
                    db.ExperimentalDataRecord.AddOrUpdate(data);
                }
                else
                    //return new HttpStatusCodeResult(HttpStatusCode.UnsupportedMediaType, "非系統可接受的檔案格式!");
                    return Content("<br>非系統可接受的檔案格式!<br>僅支援上傳圖片、Word或PDF!", "application/json; charset=utf-8");
            }
            await db.SaveChangesAsync();

            return Content("Succeed");
        }
        #endregion

        #region 編輯實驗數據紀錄
        public ActionResult Edit(string id)
		{
            ViewBag.id = id;
            return View();
		}

        [HttpPost]
        public async Task<ActionResult> EditExperimentData(EDRecord edRecord)
        {
            if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);  // Data Annotation未通過
            //else if (el_info.LabelName.Where(x => x.Trim().Length > 200).Count() > 0) return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "標籤名稱 的長度最多200個字元。");

            var data = await db.ExperimentalDataRecord.FirstOrDefaultAsync(x => x.EDRSN == edRecord.EDRSN);
            if (data == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "EDRSN is Undefined.");

            // 編輯實驗數據紀錄
            data.TAWSN = edRecord.TAWSN;
            data.EDate = edRecord.EDate;
            // 編輯實驗數據紀錄資料
            db.ExperimentalData.RemoveRange(data.ExperimentalData);
            ICollection<ExperimentalData> data_items = AddOrUpdateList<ExperimentalData>(edRecord.ExperimentalDataItem, data.EDRSN);
            data.ExperimentalData = data_items;
            // [實驗數據檔案]檔案處理，目前只提供單個檔案上傳且刪除
            if (edRecord.EDFile != null && edRecord.EDFile.ContentLength > 0) // 上傳
            {
                var newFile = edRecord.EDFile;
                string extension = Path.GetExtension(newFile.FileName); // 檔案副檔名
                if (ComFunc.IsConformedForDocument(newFile.ContentType, extension) || ComFunc.IsConformedForImage(newFile.ContentType, extension)) // 檔案白名單檢查
                {
                    // 舊檔案刪除
                    ComFunc.DeleteFile(Server.MapPath($"~/{folderPath}/"), data.EDFile, null);
                    // 檔案上傳
                    if (!ComFunc.UploadFile(newFile, Server.MapPath($"~/{folderPath}/"), data.EDRSN))
                        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "檔案上傳過程出錯!(舊檔案已刪除)");
                    data.EDFile = data.EDRSN + extension;
                }
                else
                    //return new HttpStatusCodeResult(HttpStatusCode.UnsupportedMediaType, "非系統可接受的檔案格式!");
                    return Content("<br>非系統可接受的檔案格式!<br>僅支援上傳圖片、Word或PDF!", "application/json; charset=utf-8");
            }

            db.ExperimentalDataRecord.AddOrUpdate(data);
            await db.SaveChangesAsync();

            return Content("Succeed");
        }
        #endregion

        #region 實驗數據紀錄詳情
        public ActionResult Read(string id)
		{
            ViewBag.id = id;
            return View();
		}

        public async Task<ActionResult> Read_Data(string id)
        {
            var data = await db.ExperimentalDataRecord.FirstOrDefaultAsync(x => x.EDRSN == id);
            if (data == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "EDRSN is Undefined.");

            ED_ViewModel model = new ED_ViewModel
            {
                ExperimentType = data.TestingAndAnalysisWorkflow?.ExperimentType,
                ExperimentName = data.TestingAndAnalysisWorkflow?.ExperimentName,
                TAWSN = data.TAWSN,
                EDate = data.EDate.ToString("yyyy-MM-dd"),
                UploadUserName = db.AspNetUsers.FirstOrDefaultAsync(x => x.UserName == data.UploadUserName)?.Result.MyName,
                UploadDateTime = data.UploadDateTime.ToString("yyyy/MM/dd"),
                FilePath = !string.IsNullOrEmpty(data.EDFile) ? ComFunc.UrlMaker(folderPath, data.EDFile) : null,
                ExperimentalDataItem = data.ExperimentalData.Select(x => new ED_Info { DataName = x.DataName, Data = x.Data }).ToList()
            };
            return Content(JsonConvert.SerializeObject(model), "application/json");
        }
        #endregion

        #region Helper
        private static ICollection<T> AddOrUpdateList<T>(List<ED_Info> list, string EDRSN) where T : ExperimentalData, new()
        {
            ICollection<T> result = list.Where(x => !string.IsNullOrEmpty(x.DataName.Trim()) && !string.IsNullOrEmpty(x.Data.Trim())).Select(x => new T
            {
                EDSN = EDRSN + "_" + (list.IndexOf(x) + 1).ToString().PadLeft(3, '0'),
                EDRSN = EDRSN,
                DataName = x.DataName,
                Data = x.Data
            }).ToList();
            return result;
        }
        #endregion
    }
}