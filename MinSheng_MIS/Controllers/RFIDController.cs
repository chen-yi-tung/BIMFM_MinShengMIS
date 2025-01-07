using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Models;
using MinSheng_MIS.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;

namespace MinSheng_MIS.Controllers
{
    public class RFIDController : Controller

    {
        private readonly Bimfm_MinSheng_MISEntities _db;
        private readonly RFIDService _rfidService;
        private readonly DatagridService _datagridService;

        public RFIDController()
        {
            _db = new Bimfm_MinSheng_MISEntities();
            _rfidService = new RFIDService(_db);
        }
        #region 檢查RFID內碼是否有使用過
        [HttpGet]
        public async Task<ActionResult> CheckIsRFIDInternalCodeDuplicate(string id)
        {
            JsonResService<string> result = new JsonResService<string>();
            try
            {
                await _rfidService.CheckRFIDInternalCode(id);
                return Content(JsonConvert.SerializeObject(new JsonResService<string>
                {
                    AccessState = ResState.Success,
                    ErrorMessage = null,
                    Datas = null,
                }), "application/json");
            }
            catch (MyCusResException ex)
            {
                return Helper.HandleMyCusResException(this, ex);
            }
            catch (Exception)
            {
                return Helper.HandleException(this);
            }
        }
        #endregion
    }
}