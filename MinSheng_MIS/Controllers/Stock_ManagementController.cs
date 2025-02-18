using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;

namespace MinSheng_MIS.Controllers
{
    public class Stock_ManagementController : Controller
    {
        private readonly Bimfm_MinSheng_MISEntities _db;
        private readonly StockService _stockService;
        private readonly DatagridService _datagridService;

        public Stock_ManagementController()
        {
            _db = new Bimfm_MinSheng_MISEntities();
            _stockService = new StockService(_db);
            _datagridService = new DatagridService();
        }
        #region 庫存管理
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 新增 庫存品項
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CreateComputationalStock(ComputationalStockCreateModel data)
        {
            JsonResService<string> result = new JsonResService<string>();
            try
            {
                // Data Annotation
                //if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);  // Data Annotation未通過

                // 建立庫存
                result =  _stockService.Stock_Create(data);
                return Content(JsonConvert.SerializeObject(result), "application/json");
            }
            catch (MyCusResException ex)
            {
                result.AccessState = ResState.Failed;
                result.ErrorMessage = $"</br>{ex.Message}";
                return Content(JsonConvert.SerializeObject(result), "application/json");
            }
            catch (Exception)
            {
                result.AccessState = ResState.Failed;
                result.ErrorMessage = "</br>系統異常！";
                return Content(JsonConvert.SerializeObject(result), "application/json");
            }
        }
        #endregion

        #region 新增 入庫填報
        public ActionResult CreateInbound()
        {
            return View();
        }
        #region 新增一般入庫
        [HttpPost]
        public ActionResult CreateNormalComputationalStockIn(NomalComputationalStockInModel data)
        {
            JsonResService<string> result = new JsonResService<string>();
            try
            {
                // Data Annotation
                //if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);  // Data Annotation未通過

                // 新增一般入庫
                var lastSARSN = _db.StockChangesRecord.OrderByDescending(x => x.SARSN).FirstOrDefault()?.SARSN ?? (DateTime.Today.ToString("yyyyMMddHHmm") + "000");
                var SARSN = ComFunc.CreateNextID("!{yyMMddHHmm}%{3}", lastSARSN);
                string Filename = null;
                #region 採購單
                //檢查檔案格式
                if (data.PurchaseOrder != null)
                {
                    string extension = Path.GetExtension(data.PurchaseOrder.FileName).ToLower();
                    if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".pdf")
                    {
                        result.AccessState = ResState.Failed;
                        result.ErrorMessage = "圖片僅接受jpg、jpeg、png、pdf！";
                        return Content(JsonConvert.SerializeObject(result), "application/json");
                    }
                    string Folder = Server.MapPath("~/Files/PurchaseOrder");
                    if (!Directory.Exists(Folder))
                    {
                        System.IO.Directory.CreateDirectory(Folder);
                    }
                    string FolderPath = Server.MapPath("~/Files/PurchaseOrder");
                    Filename = SARSN + Path.GetExtension(data.PurchaseOrder.FileName);
                    System.IO.Directory.CreateDirectory(FolderPath);
                    string filefullpath = Path.Combine(FolderPath, Filename);
                    data.PurchaseOrder.SaveAs(filefullpath);
                }
                
                #endregion
                result = _stockService.NormalStockIn_Create(data, SARSN, User.Identity.Name, Filename);
                return Content(JsonConvert.SerializeObject(result), "application/json");
            }
            catch (MyCusResException ex)
            {
                result.AccessState = ResState.Failed;
                result.ErrorMessage = $"</br>{ex.Message}";
                return Content(JsonConvert.SerializeObject(result), "application/json");
            }
            catch (Exception)
            {
                result.AccessState = ResState.Failed;
                result.ErrorMessage = "</br>系統異常！";
                return Content(JsonConvert.SerializeObject(result), "application/json");
            }
        }
        #endregion
        #endregion

        #region 新增 出庫填報
        public ActionResult CreateOutbound()
        {
            return View();
        }
        #region 新增一般出庫
        [HttpPost]
        public ActionResult CreateNormalComputationalStockOut(NomalComputationalStockOutModel data)
        {
            JsonResService<string> result = new JsonResService<string>();
            try
            {
                // Data Annotation
                //if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);  // Data Annotation未通過

                // 新增一般入庫
                result = _stockService.NormalStockOut_Create(data, User.Identity.Name);
                return Content(JsonConvert.SerializeObject(result), "application/json");
            }
            catch (MyCusResException ex)
            {
                result.AccessState = ResState.Failed;
                result.ErrorMessage = $"</br>{ex.Message}";
                return Content(JsonConvert.SerializeObject(result), "application/json");
            }
            catch (Exception)
            {
                result.AccessState = ResState.Failed;
                result.ErrorMessage = "</br>系統異常！";
                return Content(JsonConvert.SerializeObject(result), "application/json");
            }
        }
        #endregion
        #endregion


        #region 編輯 庫存品項
        public ActionResult Edit(string id)
        {
            ViewBag.id = id;
            return View();
        }
        [HttpPost]
        public ActionResult EditComputationalStock(ComputationalStockEditModel data)
        {
            JsonResService<string> result = new JsonResService<string>();
            try
            {
                // Data Annotation
                //if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);  // Data Annotation未通過

                // 建立庫存
                result = _stockService.Stock_Edit(data);
                return Content(JsonConvert.SerializeObject(result), "application/json");
            }
            catch (MyCusResException ex)
            {
                result.AccessState = ResState.Failed;
                result.ErrorMessage = $"</br>{ex.Message}";
                return Content(JsonConvert.SerializeObject(result), "application/json");
            }
            catch (Exception)
            {
                result.AccessState = ResState.Failed;
                result.ErrorMessage = "</br>系統異常！";
                return Content(JsonConvert.SerializeObject(result), "application/json");
            }
        }
        #endregion

        #region 庫存品項 詳情
        public ActionResult Detail(string id)
        {
            ViewBag.id = id;
            return View();
        }
        [HttpGet]
        public ActionResult GetComputationalStockDetail(string id)
        {
            JsonResService<ComputationalStockDetailModel> result = new JsonResService<ComputationalStockDetailModel>();
            try
            {
                // Data Annotation
                //if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);  // Data Annotation未通過

                // 庫存詳情
                result = _stockService.Stock_Details(id);

                return Content(JsonConvert.SerializeObject(result), "application/json");
            }
            catch (MyCusResException ex)
            {
                result.AccessState = ResState.Failed;
                result.ErrorMessage = "</br>{ex.Message}";
                return Content(JsonConvert.SerializeObject(result), "application/json");
            }
            catch (Exception)
            {
                result.AccessState = ResState.Failed;
                result.ErrorMessage = "</br>系統異常！";
                return Content(JsonConvert.SerializeObject(result), "application/json");
            }
        }
        [HttpPost]
        public ActionResult GetComputationalStockDetailRecord(FormCollection form)
        {
            JsonResService<JObject> result = new JsonResService<JObject>();
            try
            {
                // Data Annotation
                //if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);  // Data Annotation未通過

                // 庫存變更記錄grid
                result.Datas = _datagridService.GetJsonForGrid_StockChangeRecord(form);
                result.AccessState = ResState.Success;
                return Content(JsonConvert.SerializeObject(result), "application/json");
            }
            catch (MyCusResException ex)
            {
                result.AccessState = ResState.Failed;
                result.ErrorMessage = "</br>{ex.Message}";
                return Content(JsonConvert.SerializeObject(result), "application/json");
            }
            catch (Exception)
            {
                result.AccessState = ResState.Failed;
                result.ErrorMessage = "</br>系統異常！";
                return Content(JsonConvert.SerializeObject(result), "application/json");
            }
        }
        #endregion

        #region 庫存品項 刪除
        public ActionResult Delete(string id)
        {
            ViewBag.id = id;
            return View();
        }
        [HttpPost]
        public ActionResult DeleteComputationalStock(string SISN)
        {
            JsonResService<string> result = new JsonResService<string>();
            try
            {
                // Data Annotation
                //if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);  // Data Annotation未通過

                // 刪除庫存
                result = _stockService.Stock_Delete(SISN);
                return Content(JsonConvert.SerializeObject(result), "application/json");
            }
            catch (MyCusResException ex)
            {
                result.AccessState = ResState.Failed;
                result.ErrorMessage = $"</br>{ex.Message}";
                return Content(JsonConvert.SerializeObject(result), "application/json");
            }
            catch (Exception)
            {
                result.AccessState = ResState.Failed;
                result.ErrorMessage = "</br>系統異常！";
                return Content(JsonConvert.SerializeObject(result), "application/json");
            }
        }
        #endregion

        #region 資產管理 匯出
        public ActionResult ExportToExcel(FormCollection form)
        {
            JObject jo = new JObject();
            DatagridService ds = new DatagridService();
            form.Add("rows", short.MaxValue.ToString());
            jo = ds.GetJsonForGrid_Stock_Management(form);
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("報修管理");

                worksheet.Cells["A1"].Value = "類別";
                worksheet.Cells["B1"].Value = "品項名稱";
                worksheet.Cells["C1"].Value = "狀態";
                worksheet.Cells["D1"].Value = "數量";
                worksheet.Cells["E1"].Value = "單位";
                worksheet.Cells["F1"].Value = "警戒值";

                int row = 2;
                foreach (var item in jo["rows"])
                {
                    worksheet.Cells["A" + row].Value = item["StockType"]?.ToString();
                    worksheet.Cells["B" + row].Value = item["StockName"]?.ToString();
                    worksheet.Cells["C" + row].Value = item["StockStatus"]?.ToString();
                    worksheet.Cells["D" + row].Value = item["StockAmount"]?.ToString();
                    worksheet.Cells["E" + row].Value = item["Unit"]?.ToString();
                    worksheet.Cells["F" + row].Value = item["MinStockAmount"]?.ToString();
                    row++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", $"attachment; filename=庫存管理{DateTime.Now.Ticks}.xlsx");
                Response.BinaryWrite(package.GetAsByteArray());
            }

            return null;
        }
        #endregion

        #region 資產管理 匯出
        public ActionResult DetailExportToExcel(FormCollection form)
        {
            JObject jo = new JObject();
            DatagridService ds = new DatagridService();
            form.Add("rows", short.MaxValue.ToString());
            jo = ds.GetJsonForGrid_StockChangeRecord(form);
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("報修管理");

                worksheet.Cells["A1"].Value = "日期時間";
                worksheet.Cells["B1"].Value = "入庫數量";
                worksheet.Cells["C1"].Value = "出庫數量";
                worksheet.Cells["D1"].Value = "庫存數量";
                worksheet.Cells["E1"].Value = "登記人";
                worksheet.Cells["F1"].Value = "採購單據";
                worksheet.Cells["G1"].Value = "取用人";
                worksheet.Cells["H1"].Value = "備註";

                int row = 2;
                foreach (var item in jo["rows"])
                {
                    worksheet.Cells["A" + row].Value = item["DateTime"]?.ToString();
                    worksheet.Cells["B" + row].Value = item["InboundNum"]?.ToString();
                    worksheet.Cells["C" + row].Value = item["OutboundNum"]?.ToString();
                    worksheet.Cells["D" + row].Value = item["StockNum"]?.ToString();
                    worksheet.Cells["E" + row].Value = item["Registrant"]?.ToString();
                    worksheet.Cells["F" + row].Value = item["Document"]?.ToString();
                    worksheet.Cells["G" + row].Value = item["Taker"]?.ToString();
                    worksheet.Cells["H" + row].Value = item["Memo"]?.ToString();
                    row++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", $"attachment; filename=庫存品項_詳情{DateTime.Now.Ticks}.xlsx");
                Response.BinaryWrite(package.GetAsByteArray());
            }

            return null;
        }
        #endregion
    }
}