using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                result.ErrorMessage = "</br>系統異常!";
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
                #region 採購單
                //檢查檔案格式todo
                string extension = Path.GetExtension(data.PurchaseOrder.FileName).ToLower();
                if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".pdf")
                {
                    result.AccessState = ResState.Failed;
                    result.ErrorMessage = "圖片僅接受jpg、jpeg、png、pdf!";
                    return Content(JsonConvert.SerializeObject(result), "application/json");
                }
                string Folder = Server.MapPath("~/Files/PurchaseOrder");
                if (!Directory.Exists(Folder))
                {
                    System.IO.Directory.CreateDirectory(Folder);
                }

                var lastSARSN = _db.StockChangesRecord.OrderByDescending(x => x.SARSN).FirstOrDefault()?.SARSN ?? (DateTime.Today.ToString("yyyyMMddHHmm") + "000");
                var SARSN = ComFunc.CreateNextID("!{yyMMddHHmm}%{3}", lastSARSN);
                string FolderPath = Server.MapPath("~/Files/PurchaseOrder");
                string Filename = SARSN + Path.GetExtension(data.PurchaseOrder.FileName);
                System.IO.Directory.CreateDirectory(FolderPath);
                string filefullpath = Path.Combine(FolderPath, Filename);
                data.PurchaseOrder.SaveAs(filefullpath);
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
                result.ErrorMessage = "</br>系統異常!";
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
                result.ErrorMessage = "</br>系統異常!";
                return Content(JsonConvert.SerializeObject(result), "application/json");
            }
        }
        #endregion
        #endregion


        #region 編輯 庫存品項
        public ActionResult Edit()
        {
            return View();
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
                result.ErrorMessage = "</br>系統異常!";
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
                result.ErrorMessage = "</br>系統異常!";
                return Content(JsonConvert.SerializeObject(result), "application/json");
            }
        }
        #endregion

        #region 庫存品項 刪除
        public ActionResult Delete()
        {
            return View();
        }
        #endregion
    }
}