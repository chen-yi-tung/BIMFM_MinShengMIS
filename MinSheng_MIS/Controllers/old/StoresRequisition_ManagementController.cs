using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
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
            if (sr_info.StoresRequisitionItem.GroupBy(x => x.SISN).Any(g => g.Count() > 1)) return Content($"<br>同品項品名請統整為單筆領用項目！", "application/json; charset=utf-8");

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
            SR_ViewModel<Read_ItemViewModel> model = new SR_ViewModel<Read_ItemViewModel>
            {
                SRSN = request.SRSN,
                SRMyName = db.AspNetUsers.FirstOrDefaultAsync(x => x.UserName == request.SRUserName)?.Result.MyName,
                SRDept = request.SRDept,
                SRContent = request.SRContent,
                StoresRequisitionItem = request.StoresRequisitionItem.Select(x => new Read_ItemViewModel
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
        public ActionResult Edit(string id)
        {
            ViewBag.id = id;
            return View();
        }

        public async Task<ActionResult> Edit_Data(string id)
        {
            var request = await db.StoresRequisition.FirstOrDefaultAsync(x => x.SRSN == id);
            if (request == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "SRSN is Undefined.");

            var UnitDics = Surface.Unit();
            SR_ViewModel<Edit_ItemViewModel> model = new SR_ViewModel<Edit_ItemViewModel>
            {
                SRSN = request.SRSN,
                SRUserName = request.SRUserName,
                SRDept = request.SRDept,
                SRContent = request.SRContent,
                StoresRequisitionItem = request.StoresRequisitionItem.Select(x => new Edit_ItemViewModel
                {
                    SISN = x. SISN,
                    StockType = x.ComputationalStock?.StockType,
                    StockName = x.ComputationalStock?.StockName,
                    Amount = x.Amount,
                    Unit = UnitDics.TryGetValue(x.ComputationalStock?.Unit, out var mapUnit) ? mapUnit : "undefined",
                    SRContent = x.SRContent
                }).ToList()
            };
            return Content(JsonConvert.SerializeObject(model), "application/json");
        }

        [HttpPost]
        public async Task<ActionResult> EditStoresRequisition(SR_Info sr_info)
        {
            if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);  // Data Annotation未通過
            var request = await db.StoresRequisition.Where(x => x.SRSN == sr_info.SRSN).FirstOrDefaultAsync();
            if (request == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "SRSN is Undefined.");
            else if (request.SRState != "1") return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Cannot be Edited!");
            if (sr_info.StoresRequisitionItem.GroupBy(x => x.SISN).Any(g => g.Count() > 1)) return Content($"<br>同品項品名請統整為單筆領用項目！", "application/json; charset=utf-8");

            // 編輯領用申請單
            request.SRUserName = sr_info.SRUserName;
            request.SRDept = sr_info.SRDept;
            request.SRContent = sr_info.SRContent;
            // 編輯領用申請單項目
            db.StoresRequisitionItem.RemoveRange(request.StoresRequisitionItem);
            ICollection<StoresRequisitionItem> items = AddOrUpdateList<StoresRequisitionItem>(sr_info.StoresRequisitionItem, request.SRSN);
            request.StoresRequisitionItem = items;

            db.StoresRequisition.AddOrUpdate(request);
            await db.SaveChangesAsync();

            return Content("Succeed");
        }
        #endregion

        #region 審核領用申請
        public ActionResult Audit(string id)
        {
            ViewBag.id = id;
            return View();
        }

        public async Task<ActionResult> Audit_Data(string id)
        {
            var request = await db.StoresRequisition.FirstOrDefaultAsync(x => x.SRSN == id);
            if (request == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "SRSN is Undefined.");

            var TypeDics = Surface.StockType();
            var UnitDics = Surface.Unit();
            SR_ViewModel<Audit_ItemViewModel> model = new SR_ViewModel<Audit_ItemViewModel>
            {
                SRSN = request.SRSN,
                SRMyName = db.AspNetUsers.FirstOrDefaultAsync(x => x.UserName == request.SRUserName)?.Result.MyName,
                SRDept = request.SRDept,
                SRContent = request.SRContent,
                StoresRequisitionItem = request.StoresRequisitionItem.Select(x => new Audit_ItemViewModel
                {
                    SRISN = x.SRSN,
                    StockType = TypeDics.TryGetValue(x.ComputationalStock?.StockType, out var mapType) ? mapType : "undefined",
                    StockName = x.ComputationalStock?.StockName,
                    Amount = x.Amount,
                    RemainingAmount = (double)x.ComputationalStock?.StockAmount,
                    Unit = UnitDics.TryGetValue(x.ComputationalStock?.Unit, out var mapUnit) ? mapUnit : "undefined",
                    SRContent = x.SRContent
                }).ToList()
            };
            return Content(JsonConvert.SerializeObject(model), "application/json");
        }

        [HttpPost]
        public async Task<ActionResult> AuditStoresRequisition(SR_Audit sr_audit)
        {
            if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);  // Data Annotation未通過
            var request = await db.StoresRequisition.Where(x => x.SRSN == sr_audit.SRSN).FirstOrDefaultAsync();
            if (request == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "SRSN is Undefined.");
            else if (request.SRState != "1") return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Cannot be Audited!");
            else if (sr_audit.AuditResult.Count != request.StoresRequisitionItem.Count) return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Then count of AuditResult is not correct!");

            // 編輯領用申請單審核部分
            request.AuditUserID = User.Identity.Name;
            request.AuditDate = DateTime.Now;
            request.AuditContent = sr_audit.AuditContent;
            request.SRState = "2"; //=審核完成
            // 編輯領用申請單項目審核結果
            for (int i = 0; i < request.StoresRequisitionItem.Count; i++)
            {
                var item = request.StoresRequisitionItem.ElementAt(i);
                item.AuditResult = sr_audit.AuditResult[i];
                switch (item.AuditResult)
                {
                    case "2":
                        item.PickUpStatus = "2";
                        break;
                    case "3":
                        item.PickUpStatus = "3";
                        break;
                    default: break;
                }
            }

            db.StoresRequisition.AddOrUpdate(request);
            await db.SaveChangesAsync();

            return Content("Succeed");
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