using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
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
		public ActionResult Read()
		{
			return View();
		}
		#endregion
	}
}