using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI.WebControls;


namespace MinSheng_MIS.Controllers
{
    public class PurchaseRequisition_ManagementController : Controller
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();

        // GET: PurchaseRequisition_Management
        #region 請購管理
        public ActionResult Management()
        {
            return View();
        }
        #endregion

        #region 請購申請
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> CreatePurchaseRequisition([Bind(Exclude = "PRN, PRState, AuditDate, AuditResult, File")] PR_Info pr_info)
        {
            ModelState.Remove("PRN");
            ModelState.Remove("PRState");
            if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);  // Data Annotation未通過

            DateTime now = DateTime.Now;
            // 新增請購單
            var count = await db.PurchaseRequisition.Where(x => x.PRDate == now.Date).CountAsync() + 1;  // 請購單流水碼
            var request = new PurchaseRequisition
            {
                PRN = "PR" + now.ToString("yyMMdd") + count.ToString().PadLeft(2, '0'),
                PRUserName = pr_info.PRUserName,
                PRState = "1", //=待送審
                PRDept = pr_info.PRDept,
                PRDate = now
            };
            db.PurchaseRequisition.Add(request);
            // 新增請購單項目
            int i = 1;
            foreach (var item in pr_info.PurchaseRequisitionItem)
            {
                var pr_item = new PurchaseRequisitionItem
                {
                    PRIN = request.PRN + "_" + i.ToString().PadLeft(2, '0'),
                    PRN = request.PRN,
                    Kind = item.Kind,
                    ItemName = item.ItemName,
                    Size = item.Size,
                    PRAmount = item.PRAmount,
                    Unit = item.Unit,
                    ApplicationPurpose = item.ApplicationPurpose
                };
                db.PurchaseRequisitionItem.Add(pr_item);
                i ++;
            }
            await db.SaveChangesAsync();

            return Content("Succeed");
        }
        #endregion

        #region 請購詳情
        public ActionResult Read(string id)
        {
            ViewBag.id = id;
            return View();
        }

        public async Task<ActionResult> Read_Data(string id)
        {
            var request = await db.PurchaseRequisition.FirstOrDefaultAsync(x => x.PRN == id);
            if (request == null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Content("PRN is Undefined.");
            }
            //var StateDics = Surface.PRState();
            var KindDics = Surface.StockType();
            var UnitDics = Surface.Unit();
            PR_ViewModel model = new PR_ViewModel
            {
                PRN = request.PRN,
                PRUserName = db.AspNetUsers.FirstOrDefaultAsync(x => x.UserName == request.PRUserName)?.Result.MyName,
                PRUserAccount = request.PRUserName,
                PRState = request.PRState,
                PRStateName = Surface.PRState()[request.PRState],
                PRDept = request.PRDept,
                PRDate = request.PRDate.ToString("yyyy-MM-dd"),
                AuditDate = request.AuditDate?.ToString("yyyy-MM-dd"),
                AuditResult = request.AuditResult,
                FilePath = ComFunc.GetFilePath("Files/PurchaseRequisition", Server, request.PRN)?.FirstOrDefault(),
                PurchaseRequisitionItem = request.PurchaseRequisitionItem.Select(x => new PR_Item
                {
                    Kind = x.Kind,
                    KindName = KindDics.TryGetValue(x.Kind, out var mapKind) ? mapKind : "undefined",
                    ItemName = x.ItemName,
                    Size = x.Size,
                    PRAmount = x.PRAmount,
                    Unit = x.Unit,
                    UnitName = UnitDics.TryGetValue(x.Unit, out var mapUnit) ? mapUnit : "undefined",
                    ApplicationPurpose = x.ApplicationPurpose
                }).ToList()
            };
            return Content(JsonConvert.SerializeObject(model), "application/json");
        }
        #endregion

        #region 請購編輯
        public ActionResult Edit(string id)
        {
            ViewBag.id = id;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> EditPurchaseRequisition(PR_Info pr_info)
        {
            if (!ModelState.IsValidField("PRN")) return Helper.HandleInvalidModelState(this, "PRN");
            var request = await db.PurchaseRequisition.Where(x => x.PRN == pr_info.PRN).FirstOrDefaultAsync();
            if (request == null) 
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Content("PRN is Undefined.");
            }

            if (request.PRState != "1")  //當請購單狀態非"待審核"時，請購單申請時的資訊不可更改。
            {
                // 移除相關欄位的資料驗證。
                ModelState.Remove("PRUserName");
                ModelState.Remove("PurchaseRequisitionItem"); // 請購單項目
            }
            if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);  // Data Annotation未通過

            //當請購單狀態"待審核"時，請購單申請時的資訊才可更改。
            //需先編輯申請資訊 ，若先編輯審核資訊，可能會有狀態發生改變而造成申請資訊無法編輯
            #region 編輯請購單申請資訊 
            if (request.PRState == "1")
            {
                request.PRUserName = pr_info.PRUserName;
                request.PRDept = pr_info.PRDept;
                // 若整體採購項目減少，則刪除資料庫項目
                if (request.PurchaseRequisitionItem.Count > pr_info.PurchaseRequisitionItem.Count)
                {
                    int deletNum = request.PurchaseRequisitionItem.Count - pr_info.PurchaseRequisitionItem.Count();
                    var deletedItems = request.PurchaseRequisitionItem.OrderByDescending(x => x.PRIN).Take(deletNum).ToList();
                    db.PurchaseRequisitionItem.RemoveRange(deletedItems);
                }

                // 編輯請購單項目
                int i = 1;
                ICollection<PurchaseRequisitionItem> pr_items = new List<PurchaseRequisitionItem>();
                foreach (var item in pr_info.PurchaseRequisitionItem)
                {
                    PurchaseRequisitionItem pr_item;
                    if (request.PurchaseRequisitionItem.Count >= i)
                        pr_item = request.PurchaseRequisitionItem.ElementAtOrDefault(i - 1); // 保留其餘欄位;
                    else pr_item = new PurchaseRequisitionItem { PRIN = request.PRN + "_" + i.ToString().PadLeft(2, '0'), PRN = request.PRN };

                    pr_item.Kind = item.Kind;
                    pr_item.ItemName = item.ItemName;
                    pr_item.Size = item.Size;
                    pr_item.PRAmount = item.PRAmount;
                    pr_item.Unit = item.Unit;
                    pr_item.ApplicationPurpose = item.ApplicationPurpose;
                    pr_items.Add(pr_item);
                    i++;
                }
                request.PurchaseRequisitionItem = pr_items;
            }
            #endregion

            #region 編輯請購單審核資訊
            request.PRState = pr_info.PRState;
            request.AuditDate = pr_info.AuditDate;
            request.AuditResult = pr_info.AuditResult;
            // [相關文件]檔案處理，目前只提供單個檔案上傳及刪除
            string folderpath = Server.MapPath("~/Files/PurchaseRequisition/");
            if (pr_info.AFileName == null && !string.IsNullOrEmpty(request.FileName)) // 當使用者介面目前無檔案(不包含本次上傳的檔案)時，表示若此請購單具有相關文件，應刪除。
            {
                ComFunc.DeleteFile(folderpath, request.FileName, null);
                request.FileName = null;
            }
            if (pr_info.AFile != null && pr_info.AFile.ContentLength > 0) // 上傳
            {
                string extension = Path.GetExtension(pr_info.AFile.FileName); // 檔案副檔名
                if (ComFunc.IsConformedForDocument(pr_info.AFile.ContentType, extension) || ComFunc.IsConformedForImage(pr_info.AFile.ContentType, extension)) // 檔案白名單檢查
                {
                    // 檔案上傳
                    if (!ComFunc.UploadFile(pr_info.AFile, folderpath, request.PRN))
                    {
                        Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        return Content("檔案上傳過程出錯!");
                    }
                    request.FileName = request.PRN + extension;
                }
                else
                {
                    Response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;
                    return Content("非系統可接受的檔案格式!");
                }
            }
            #endregion

            db.PurchaseRequisition.AddOrUpdate(request);
            await db.SaveChangesAsync();

            return Json(new { Message = "Succeed" });
        }
        #endregion

        #region Helper
        //private ActionResult HandleInvalidModelState(string field)
        //{
        //    Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //    if (string.IsNullOrEmpty(field))
        //        return Content(string.Join(Environment.NewLine, ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));
        //    else
        //        return Content(string.Join(Environment.NewLine, ModelState[field].Errors.Select(e => e.ErrorMessage)));
        //}
        #endregion
    }
}