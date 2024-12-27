using Microsoft.Ajax.Utilities;
using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;

namespace MinSheng_MIS.Controllers.API
{
    #region 定期保養單 列表
    public class MaintainListController : ApiController
    {
        private readonly Bimfm_MinSheng_MISEntities _db;
        private readonly Maintain_ManagementService _maintainService;

        public MaintainListController()
        {
            _db = new Bimfm_MinSheng_MISEntities();
            _maintainService = new Maintain_ManagementService(_db);
        }
        public JsonResService<MaintainManagementApp_ListViewModel> Get(string Status = null)
        {
            JsonResService<MaintainManagementApp_ListViewModel> result = new JsonResService<MaintainManagementApp_ListViewModel>();
            try
            {
                result = _maintainService.MaintainManagementApp_List(Status);
            }
            catch (MyCusResException ex)
            {
                result.AccessState = ResState.Failed;
                result.ErrorMessage = ex.Message;
            }
            catch (Exception)
            {
                result.AccessState = ResState.Failed;
                result.ErrorMessage = "系統異常!";
            }
            return result;
        }
    }
    #endregion

    #region 定期保養單 填報詳情
    public class MaintainDetailController : ApiController
    {
        private readonly Bimfm_MinSheng_MISEntities _db;
        private readonly Maintain_ManagementService _maintainService;

        public MaintainDetailController()
        {
            _db = new Bimfm_MinSheng_MISEntities();
            _maintainService = new Maintain_ManagementService(_db);
        }
        public JsonResService<MaintainManagementApp_Detail> Get(string EMFSN)
        {
            JsonResService<MaintainManagementApp_Detail> result = new JsonResService<MaintainManagementApp_Detail>();
            try
            {
                result = _maintainService.MaintainManagementApp_Detail(EMFSN);
            }
            catch (MyCusResException ex)
            {
                result.AccessState = ResState.Failed;
                result.ErrorMessage = ex.Message;
            }
            catch (Exception)
            {
                result.AccessState = ResState.Failed;
                result.ErrorMessage = "系統異常!";
            }
            return result;
        }
    }
    #endregion

    #region 定期保養單 填報
    public class MaintainReportController : ApiController
    {
        private readonly Bimfm_MinSheng_MISEntities _db;
        private readonly Maintain_ManagementService _maintainService;

        public MaintainReportController()
        {
            _db = new Bimfm_MinSheng_MISEntities();
            _maintainService = new Maintain_ManagementService(_db);
        }
        public JsonResService<string> Post(MaintainManagementApp_Report datas)
        {
            JsonResService<string> result = new JsonResService<string>();
            try
            {
                string userName = HttpContext.Current.User.Identity.Name;
                result = _maintainService.MaintainManagementApp_Report(datas, userName);
            }
            catch (MyCusResException ex)
            {
                result.AccessState = ResState.Failed;
                result.ErrorMessage = ex.Message;
            }
            catch (Exception)
            {
                result.AccessState = ResState.Failed;
                result.ErrorMessage = "系統異常!";
            }
            return result;
        }
    }
    #endregion
}