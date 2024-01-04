using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using Newtonsoft.Json;
using System;
using System.Data.Entity;
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

        // GET: LaboratoryMaintenance_Management
        #region 實驗室維護管理
        public ActionResult Management()
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
            //var count = await db.LaboratoryMaintenance.Where(x => (x.UploadDateTime ?? DateTime.MinValue).Date == now.Date).CountAsync() + 1;  // 實驗室維護流水碼
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
            string folderpath = Server.MapPath("~/Files/LaboratoryMaintenance/");
            if (lm_info.MFile != null && lm_info.MFile.ContentLength > 0) // 上傳
			{
                string extension = Path.GetExtension(lm_info.MFile.FileName); // 檔案副檔名
                if (ComFunc.IsConformedForDocument(lm_info.MFile.ContentType, extension) || ComFunc.IsConformedForImage(lm_info.MFile.ContentType, extension)) // 檔案白名單檢查
                {
                    // 檔案上傳
                    if (!ComFunc.UploadFile(lm_info.MFile, folderpath, maintenance.LMSN))
                        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "檔案上傳過程出錯!");
                    maintenance.MFile = maintenance.LMSN + extension;
                }
                else
                    return new HttpStatusCodeResult(HttpStatusCode.UnsupportedMediaType, "非系統可接受的檔案格式!");
            }
            db.LaboratoryMaintenance.Add(maintenance);
            await db.SaveChangesAsync();

            return Content("Succeed");
        }
        #endregion

        #region 編輯實驗室維護管理
        public ActionResult Edit()
		{
			return View();
		}
		#endregion

		#region 實驗室維護管理詳情
		public ActionResult Read(string id)
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
                MType = maintenance.MType,
                MTitle = maintenance.MTitle,
                MContent = maintenance.MContent,
                UploadUserName = db.AspNetUsers.FirstOrDefaultAsync(x => x.UserName == maintenance.UploadUserName)?.Result.MyName,
                UploadDateTime = maintenance.UploadDateTime?.ToString("yyyy/MM/dd HH:mm:ss"),
                FilePath = !string.IsNullOrEmpty(maintenance.MFile) ? ComFunc.UrlMaker("Files/LaboratoryMaintenance", maintenance.MFile) : null,
            };
            return Content(JsonConvert.SerializeObject(model), "application/json");
        }
        #endregion
    }
}