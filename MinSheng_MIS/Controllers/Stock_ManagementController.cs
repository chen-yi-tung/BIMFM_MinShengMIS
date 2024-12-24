using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        public Stock_ManagementController()
        {
            _db = new Bimfm_MinSheng_MISEntities();
            _stockService = new StockService(_db);
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
        #endregion

        #region 新增 出庫填報
        public ActionResult CreateOutbound()
        {
            return View();
        }
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
            JsonResService result = new JsonResService();
            try
            {
                // Data Annotation
                //if (!ModelState.IsValid) return Helper.HandleInvalidModelState(this);  // Data Annotation未通過

                // 庫存詳情
                result.Datas = _stockService.Stock_Details(id);

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