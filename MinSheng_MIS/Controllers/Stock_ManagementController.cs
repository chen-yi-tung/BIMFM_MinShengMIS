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
    public class Stock_ManagementController : Controller
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
        // GET: Stock_Management
        #region 庫存管理
        public ActionResult Management()
        {
            return View();
        }
        #endregion

        #region 庫存詳情
        public ActionResult Read(string id)
        {
            ViewBag.id = id;
            return View();
        }

        public async Task<ActionResult> Read_Data(string id)
        {
            var C_Stock = await db.ComputationalStock.FirstOrDefaultAsync(x => x.SISN == id);
            if (C_Stock == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "SISN is Undefined.");

            var UserDics = await db.AspNetUsers.Where(x => x.IsEnabled == true).ToDictionaryAsync(k => k.UserName, v => v.MyName);
            Stock_ViewModel model = new Stock_ViewModel
            {
                SISN = C_Stock.SISN,
                StockType = Surface.StockType()[C_Stock.StockType],
                StockName = C_Stock.StockName,
                StockAmount = C_Stock.StockAmount,
                Unit = Surface.Unit()[C_Stock.Unit],
                MinStockAmount = C_Stock.MinStockAmount,
                ExpiryDate = C_Stock.ExpiryDate?.ToString("yyyy-MM-dd"),
                StockItem = C_Stock.Stock.Select(x => new StockItem
                {
                    SSN = x.SSN,
                    Brand = x.StockInRecord.Brand,
                    Model = x.StockInRecord.Model,
                    StockInDateTime = x.StockInRecord.StockInDateTime.ToString("yyyy/MM/dd"),
                    ExpiryDate = x.ExpiryDate?.ToString("yyyy/MM/dd"),
                    Location = x.Location,
                    SIRSN = x.SIRSN,
                    RemainingAmount = x.RemainingAmount,
                    StockInMyName = UserDics[x.StockInRecord.StockInUserName]
                }).Where(x => x.RemainingAmount > 0).ToList()
            };
            return Content(JsonConvert.SerializeObject(model), "application/json");
        }
        #endregion

        #region 庫存警戒值設定
        public ActionResult MinStockAmountSetting(string id)
        {
            ViewBag.id = id;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> SetWarningValue(SetWarning setting)
        {
            if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);  // Data Annotation未通過
            var C_Stock = await db.ComputationalStock.FirstOrDefaultAsync(x => x.SISN == setting.SISN);
            if (C_Stock == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "SISN is Undefined.");

            // 設定到期日及警戒值
            C_Stock.ExpiryDate = setting.ExpiryDate;
            C_Stock.MinStockAmount = setting.MinStockAmount;
            db.ComputationalStock.AddOrUpdate(C_Stock);
            await db.SaveChangesAsync();

            return Json(new { Message = "Succeed" });
        }
        #endregion

        #region 庫存匯出
        [HttpPost]
        public ActionResult Export(FormCollection form)
        {
            var service = new DatagridService();
            var a = service.GetJsonForGrid_Stock_Management(form);
            string ctrlName = this.ControllerContext.RouteData.Values["controller"].ToString();
            var result = ComFunc.ExportExcel(Server, a["rows"], ctrlName);

            return Json(result);

            //IEnumerable<Dictionary<string, object>> rows = a["rows"].ToObject<IEnumerable<Dictionary<string, object>>>();

            //var config = new OpenXmlConfiguration
            //{
            //    DynamicColumns = new DynamicExcelColumn[] {
            //        new DynamicExcelColumn("SISN"){Index=0,Name="庫存編號",Width=10},
            //        new DynamicExcelColumn("MinStockAmount"){Index=1,Name="警戒值數量",Width=15},
            //        new DynamicExcelColumn("AvailableStockAmount"){Index=2,Name="庫存可用數量",Width=15},
            //        new DynamicExcelColumn("StockType"){Index=3,Name="品項",Width=15},
            //        new DynamicExcelColumn("StockName"){Index=4,Name="品名",Width=15},
            //        new DynamicExcelColumn("StockAmount"){Index=5,Name="庫存實際量",Width=15},
            //        new DynamicExcelColumn("Unit"){Index=6,Name="單位",Width=15},
            //    }
            //};

            //var memoryStream = new MemoryStream();
            //memoryStream.SaveAs(rows, configuration: config);
            //memoryStream.Seek(0, SeekOrigin.Begin);
            //return Json(new
            //{
            //    FileDownloadName = "demo.xlsx",
            //    FileContents = memoryStream.ToArray()
            //});
        }
        #endregion
    }
}