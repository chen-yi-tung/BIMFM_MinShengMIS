using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class StoresRequisition_ManagementController : Controller
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
        // GET: StoresRequisition_Management
        #region 領用申請管理
        public ActionResult Management()
        {
            return View();
        }
        #endregion

        #region 領用申請
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> CreateStoresRequisition([Bind(Exclude = "SRSN")] SR_Info sr_info)
        {
            ModelState.Remove("SRSN");
            if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);  // Data Annotation未通過

            DateTime now = DateTime.Now;
            // 新增領用申請單
            var count = await db.StoresRequisition.Where(x => DbFunctions.TruncateTime(x.SRDateTime) == now.Date).CountAsync() + 1;  // 領用申請單流水碼
            var request = new StoresRequisition
            {
                SRSN = "R" + now.ToString("yyMMdd") + count.ToString().PadLeft(3, '0'),
                SRState = "1", //=待審核
                SRUserName = sr_info.SRUserName,
                SRDept = sr_info.SRDept,
                SRDateTime = now,
                SRContent = sr_info.SRContent
            };
            db.StoresRequisition.Add(request);
            // 新增領用申請單項目
            ICollection<StoresRequisitionItem> items = AddOrUpdateList<StoresRequisitionItem>(sr_info.StoresRequisitionItem, request.SRSN);
            db.StoresRequisitionItem.AddRange(items);
            await db.SaveChangesAsync();

            return Content("Succeed");
        }
        #endregion

        #region 領用申請詳情
        public ActionResult Read(string id)
        {
            ViewBag.id = id;
            return View();
        }

        public async Task<ActionResult> Read_Data(string id)
        {
            var request = await db.StoresRequisition.FirstOrDefaultAsync(x => x.SRSN == id);
            if (request == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "SRSN is Undefined.");

            var TypeDics = Surface.StockType();
            var UnitDics = Surface.Unit();
            var ItemStateDics = Surface.PickUpStatus();
            SR_ViewModel model = new SR_ViewModel
            {
                SRSN = request.SRSN,
                SRMyName = db.AspNetUsers.FirstOrDefaultAsync(x => x.UserName == request.SRUserName)?.Result.MyName,
                SRDept = request.SRDept,
                SRContent = request.SRContent,
                StoresRequisitionItem = request.StoresRequisitionItem.Select(x => new SR_Item_ViewModel
                {
                    PickUpStatusName = ItemStateDics.TryGetValue(x.PickUpStatus, out var mapPick) ? mapPick : "undefined",
                    StockType = TypeDics.TryGetValue(x.ComputationalStock?.StockType, out var mapType) ? mapType : "undefined",
                    StockName = x.ComputationalStock?.StockName,
                    Amount = x.Amount,
                    Unit = UnitDics.TryGetValue(x.ComputationalStock?.Unit, out var mapUnit) ? mapUnit : "undefined",
                    SRContent = x.SRContent
                }).ToList()
            };
            return Content(JsonConvert.SerializeObject(model), "application/json");
        }
        #endregion

        #region 編輯領用申請
        public ActionResult Edit()
        {
            return View();
        }
        #endregion

        #region 審核領用申請
        public ActionResult Audit()
        {
            return View();
        }
        #endregion

        #region Helper
        private static ICollection<T> AddOrUpdateList<T>(List<SR_Item> list, string SRSN) where T : StoresRequisitionItem, new()
        {
            ICollection<T> result = list.Select(x => new T
            {
                SRISN = SRSN + (list.IndexOf(x) + 1).ToString().PadLeft(3, '0'),
                SRSN = SRSN,
                SISN = x.SISN,
                Amount = x.Amount,
                TakeAmount = 0,
                SRContent = x.SRContent,
                AuditResult = "1",
                PickUpStatus = "1",
            }).ToList();
            return result;
        }
        #endregion
    }
}