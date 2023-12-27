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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http.Results;
using System.Web.Mvc;
using System.Web.Razor.Editor;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using static System.Data.Entity.Infrastructure.Design.Executor;

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
            if (!ModelState.IsValid)  // Data Annotation未通過
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Content(string.Join(Environment.NewLine, ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));
            }

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
            try
            {
                await db.SaveChangesAsync();

                return Content("Succeed");
            }
            catch (Exception ex)
            {
                return Content("Failed");
            }
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
            // Mapping
            //model.PurchaseRequisitionItem.ForEach(x => 
            //{
            //    if (KindDics.TryGetValue(x.Kind, out var mapKind)) x.Kind = mapKind;
            //    else x.Kind = "undefined";
            //    if (UnitDics.TryGetValue(x.Unit, out var mapUnit)) x.Unit = mapUnit;
            //    else x.Unit = "undefined";
            //});
            return Content(JsonConvert.SerializeObject(model), "application/json");
        }
        #endregion

        #region 請購編輯
        public ActionResult Edit()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> EditPurchaseRequisition(PR_Info pr_info)
        {
            if (!ModelState.IsValid)  // Data Annotation未通過
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Content(string.Join(Environment.NewLine, ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));
            }

            // 編輯請購單
            var request = await db.PurchaseRequisition.Where(x => x.PRN == pr_info.PRN).FirstOrDefaultAsync();
            if (request == null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Content("PRN is Undefined.");
            }

            request.PRUserName = pr_info.PRUserName;
            request.PRState = pr_info.PRState;
            request.PRDept = pr_info.PRDept;
            request.AuditDate = pr_info.AuditDate;
            request.AuditResult = pr_info.AuditResult;

            // 檔案處理，目前只提供單個檔案上傳
            if (pr_info.File != null && pr_info.File.ContentLength > 0)
            {
                string extension = Path.GetExtension(pr_info.File.FileName); // 檔案副檔名
                if (ComFunc.IsConformedForDocument(pr_info.File.ContentType, extension) || ComFunc.IsConformedForImage(pr_info.File.ContentType, extension)) // 檔案白名單檢查
                {
                    string folderpath = Server.MapPath("~/Files/PurchaseRequisition/");

                    // 若有檔案，先進行刪除
                    if (Directory.Exists(folderpath))
                    {
                        string[] oldFile = Directory.GetFiles(folderpath, $"{request.PRN}.*");
                        if (oldFile.Length > 0)
                            ComFunc.DeleteFile(oldFile, folderpath);
                    }
                    // 檔案上傳
                    if (!ComFunc.UploadFile(pr_info.File, folderpath, request.PRN))
                    {
                        Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        return Content("檔案上傳過程出錯!");
                    }
                }
                else
                {
                    Response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;
                    return Content("非系統可接受的檔案格式!");
                }
            }

            // 編輯請購單項目
            int i = 1;
            ICollection<PurchaseRequisitionItem> pr_items = new List<PurchaseRequisitionItem>();
            foreach (var item in pr_info.PurchaseRequisitionItem)
            {
                PurchaseRequisitionItem pr_item;
                if (request.PurchaseRequisitionItem.Count >= i)
                    pr_item = request.PurchaseRequisitionItem.ElementAtOrDefault(i-1); // 保留其餘欄位;
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
            db.PurchaseRequisition.AddOrUpdate(request);
            await db.SaveChangesAsync();

            return Json(new { Message = "Succeed" });
        }
        #endregion

    }
}