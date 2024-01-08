using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
	public class LaboratoryLabel_ManagementController : Controller
	{
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();

        // GET: LaboratoryLabel_Management
        #region 實驗室標籤管理
        public ActionResult Management()
		{
			return View();
		}
		#endregion

		#region 新增實驗標籤
		public ActionResult Create()
		{
			return View();
		}

        [HttpPost]
        public async Task<ActionResult> CreateLaboratoryLabel(EL_Info el_info)
		{
            ModelState.Remove("ELSN");
            if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);  // Data Annotation未通過

            DateTime now = DateTime.Now;
            // 新增實驗標籤
            var count = await db.ExperimentalLabel.Where(x => DbFunctions.TruncateTime(x.UploadDateTime) == now.Date).CountAsync() + 1;  // 實驗標籤流水碼
			var label = new ExperimentalLabel
			{
				ELSN = now.ToString("yyMMdd") + count.ToString().PadLeft(3, '0'),
				TAWSN = el_info.TAWSN,
				EDate = el_info.EDate,
                UploadUserName = User.Identity.Name,
                UploadDateTime = now
            };
            db.ExperimentalLabel.Add(label);
            // 新增實驗標籤項目
            ICollection<ExperimentalLabel_Item> items = AddOrUpdateList<ExperimentalLabel_Item>(el_info.LabelName, label.ELSN);
            db.ExperimentalLabel_Item.AddRange(items);
            await db.SaveChangesAsync();

            return Content("Succeed");
        }
        #endregion

        #region 編輯實驗標籤
        public ActionResult Edit(string id)
		{
            ViewBag.id = id;
            return View();
		}

        [HttpPost]
        public async Task<ActionResult> EditLaboratoryLabel(EL_Info el_info)
        {
            if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);  // Data Annotation未通過

            var label = await db.ExperimentalLabel.FirstOrDefaultAsync(x => x.ELSN == el_info.ELSN);
            if (label == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "ELSN is Undefined.");

            // 編輯實驗標籤
            label.TAWSN = el_info.TAWSN;
            label.EDate = el_info.EDate;
            // 編輯實驗標籤項目
            db.ExperimentalLabel_Item.RemoveRange(label.ExperimentalLabel_Item);
            ICollection<ExperimentalLabel_Item> label_items = AddOrUpdateList<ExperimentalLabel_Item>(el_info.LabelName, label.TAWSN);
            label.ExperimentalLabel_Item = label_items;

            db.ExperimentalLabel.AddOrUpdate(label);
            await db.SaveChangesAsync();

            return Content("Succeed");
        }
        #endregion

        #region 實驗標籤詳情
        public ActionResult Read(string id)
		{
            ViewBag.id = id;
            return View();
		}

        public async Task<ActionResult> Read_Data(string id)
        {
            var label = await db.ExperimentalLabel.FirstOrDefaultAsync(x => x.ELSN == id);
            if (label == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "ELSN is Undefined.");

            EL_ViewModel model = new EL_ViewModel
            {
                ExperimentType = label.TestingAndAnalysisWorkflow?.ExperimentType,
                ExperimentName = label.TestingAndAnalysisWorkflow?.ExperimentName,
                TAWSN = label.TAWSN,
                EDate = label.EDate.ToString("yyyy-MM-dd"),
                UploadUserName = User.Identity.Name,
                UploadDateTime = label.UploadDateTime.ToString("yyyy/MM/dd"),
                LaboratoryLabelItem = label.ExperimentalLabel_Item.Select(x => new EL_Item { ELISN = x.ELISN, LabelName = x.LabelName}).ToList()
            };
            return Content(JsonConvert.SerializeObject(model), "application/json");
        }
        #endregion

        #region Helper
        private static ICollection<T> AddOrUpdateList<T>(List<string> list, string ELSN) where T : ExperimentalLabel_Item, new()
        {
            ICollection<T> result = list.Select(x => new T
            {
                ELISN = ELSN + "_" + (list.IndexOf(x) + 1).ToString().PadLeft(3, '0'),
                ELSN = ELSN,
                LabelName = x
            }).ToList();
            //for (int i = 0; i < list.Count; i++)
            //{
            //    T item = new T
            //    {
            //        ELISN = ELSN + "_" + (i + 1).ToString().PadLeft(3, '0'),
            //        ELSN = ELSN,
            //        LabelName = list[i]
            //    };
            //    result.Add((T)item);
            //}
            return result;
        }
        #endregion
    }
}