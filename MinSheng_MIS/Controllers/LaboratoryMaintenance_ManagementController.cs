using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using Newtonsoft.Json;
using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
	public class LaboratoryMaintenance_ManagementController : Controller
	{
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
        static readonly string folderPath = "Files/LaboratoryMaintenance";
        // GET: LaboratoryMaintenance_Management
        #region 實驗室維護管理
        public ActionResult Index()
		{
			return View();
		}
		#endregion

		#region 新增實驗室維護管理
		public ActionResult Create()
		{
			return View();
		}

        [HttpPost]
        public async Task<ActionResult> CreateLaboratoryMaintenance(LM_Info lm_info)
		{
            ModelState.Remove("LMSN");
            if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);  // Data Annotation未通過

            DateTime now = DateTime.Now;
            // 新增實驗室維護
            var count = await db.LaboratoryMaintenance.Where(x => x.UploadDateTime.HasValue && DbFunctions.TruncateTime(x.UploadDateTime.Value) == now.Date).CountAsync() + 1;  // 實驗室維護流水碼
			var maintenance = new LaboratoryMaintenance
			{
				LMSN = now.ToString("yyMMdd") + count.ToString().PadLeft(3, '0'),
				MType = lm_info.MType,
				MTitle = lm_info.MTitle,
				MContent = lm_info.MContent,
                UploadUserName = User.Identity.Name,
                UploadDateTime = now
            };
            // [維護檔案]檔案處理，目前只提供單個檔案上傳
            if (lm_info.MFile != null && lm_info.MFile.ContentLength > 0) // 上傳
			{
                var File = lm_info.MFile;
                string extension = Path.GetExtension(File.FileName); // 檔案副檔名
                if (ComFunc.IsConformedForDocument(File.ContentType, extension) || ComFunc.IsConformedForImage(File.ContentType, extension)) // 檔案白名單檢查
                {
                    // 檔案上傳
                    if (!ComFunc.UploadFile(File, Server.MapPath($"~/{folderPath}/"), maintenance.LMSN))
                        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "檔案上傳過程出錯!");
                    maintenance.MFile = maintenance.LMSN + extension;
                }
                else
                    //return new HttpStatusCodeResult(HttpStatusCode.UnsupportedMediaType, "非系統可接受的檔案格式!");
                    return Content("<br>非系統可接受的檔案格式!<br>僅支援上傳圖片、Word或PDF!", "application/json; charset=utf-8");
            }
            db.LaboratoryMaintenance.Add(maintenance);
            await db.SaveChangesAsync();

            return Content("Succeed");
        }
        #endregion

        #region 編輯實驗室維護管理
        public ActionResult Edit(string id)
		{
            ViewBag.id = id;
            return View();
		}

        [HttpPost]
        public async Task<ActionResult> EditLaboratoryMaintenance(LM_Info lm_info)
        {
            if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);  // Data Annotation未通過

            var maintenance = await db.LaboratoryMaintenance.FirstOrDefaultAsync(x => x.LMSN == lm_info.LMSN);
            if (maintenance == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "LMSN is Undefined.");

            // 編輯實驗室維護資訊
            maintenance.MType = lm_info.MType;
            maintenance.MTitle = lm_info.MTitle;
            maintenance.MContent = lm_info.MContent;
            // [維護檔案]檔案處理，目前只提供單個檔案上傳及刪除
            if (lm_info.MFileName == null && !string.IsNullOrEmpty(maintenance.MFile)) // 當使用者介面目前無檔案(不包含本次上傳的檔案)時，若此實驗室維護具有維護檔案，應刪除。
            {
                ComFunc.DeleteFile(Server.MapPath($"~/{folderPath}/"), maintenance.MFile, null);
                maintenance.MFile = null;
            }
            if (lm_info.MFile != null && lm_info.MFile.ContentLength > 0) // 上傳
            {
                var newFile = lm_info.MFile;
                string extension = Path.GetExtension(newFile.FileName); // 檔案副檔名
                if (ComFunc.IsConformedForDocument(newFile.ContentType, extension) || ComFunc.IsConformedForImage(newFile.ContentType, extension)) // 檔案白名單檢查
                {
                    // 檔案上傳
                    if (!ComFunc.UploadFile(newFile, Server.MapPath($"~/{folderPath}/"), maintenance.LMSN))
                        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "檔案上傳過程出錯!");
                    maintenance.MFile = maintenance.LMSN + extension;
                }
                else
                    //return new HttpStatusCodeResult(HttpStatusCode.UnsupportedMediaType, "非系統可接受的檔案格式!");
                    return Content("<br>非系統可接受的檔案格式!<br>僅支援上傳圖片、Word或PDF!", "application/json; charset=utf-8");
            }

            db.LaboratoryMaintenance.AddOrUpdate(maintenance);
            await db.SaveChangesAsync();

            return Json(new { Message = "Succeed" });
        }
        #endregion

        #region 實驗室維護管理詳情
        public ActionResult Detail(string id)
		{
            ViewBag.id = id;
            return View();
		}

        public async Task<ActionResult> Read_Data(string id)
        {
            var maintenance = await db.LaboratoryMaintenance.FirstOrDefaultAsync(x => x.LMSN == id);
            if (maintenance == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "LMSN is Undefined.");

            LM_ViewModel model = new LM_ViewModel
            {
                LMSN = maintenance.LMSN,
                MType = maintenance.MType,
                MTitle = maintenance.MTitle,
                MContent = maintenance.MContent,
                UploadUserName = db.AspNetUsers.FirstOrDefaultAsync(x => x.UserName == maintenance.UploadUserName)?.Result.MyName,
                UploadDateTime = maintenance.UploadDateTime?.ToString("yyyy/MM/dd HH:mm:ss"),
                FilePath = !string.IsNullOrEmpty(maintenance.MFile) ? ComFunc.UrlMaker(folderPath, maintenance.MFile) : null
            };
            return Content(JsonConvert.SerializeObject(model), "application/json");
        }
        #endregion
    }
}