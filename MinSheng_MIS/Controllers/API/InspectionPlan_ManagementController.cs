using MinSheng_MIS.Models;
using MinSheng_MIS.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using static MinSheng_MIS.Models.ViewModels.InspectionPlan_ManagementViewModel;

namespace MinSheng_MIS.Controllers.API
{
    public class GetInspectionListController : ApiController
    {
        private readonly Bimfm_MinSheng_MISEntities _db;
        private readonly InspectionPlan_ManagementService _inspectionPlanService;

        public GetInspectionListController()
        {
            _db = new Bimfm_MinSheng_MISEntities();
            _inspectionPlanService = new InspectionPlan_ManagementService(_db);
        }
        public JsonResService<List<PlanInfo>> Get(string userID,DateTime searchdate)
        {
            JsonResService<List<PlanInfo>> result = new JsonResService<List<PlanInfo>>();
            try
            {
                result = _inspectionPlanService.GetPlanList(userID,searchdate);
            }
            catch (Exception ex)
            {
                result.AccessState = ResState.Failed;
                result.ErrorMessage = ex.Message;
            }
            return result;
        }
    }
    public class GetInspectionRFIDListController : ApiController
    {
        private readonly Bimfm_MinSheng_MISEntities _db;
        private readonly InspectionPlan_ManagementService _inspectionPlanService;

        public GetInspectionRFIDListController()
        {
            _db = new Bimfm_MinSheng_MISEntities();
            _inspectionPlanService = new InspectionPlan_ManagementService(_db);
        }
        public JsonResService<List<PlanRFIDInfo>> Get(string IPTSN)
        {
            JsonResService<List<PlanRFIDInfo>> result = new JsonResService<List<PlanRFIDInfo>>();
            try
            {
                result = _inspectionPlanService.GetPlanRFIDList(IPTSN);
            }
            catch (Exception ex)
            {
                result.AccessState = ResState.Failed;
                result.ErrorMessage = ex.Message;
            }
            return result;
        }
    }
    public class InspectionrReportController : ApiController
    {
        private readonly Bimfm_MinSheng_MISEntities _db;
        private readonly InspectionPlan_ManagementService _inspectionPlanService;

        public InspectionrReportController()
        {
            _db = new Bimfm_MinSheng_MISEntities();
            _inspectionPlanService = new InspectionPlan_ManagementService(_db);
        }
        public JsonResService<PlanFillInInfo> Get(string InspectionOrder)
        {
            JsonResService<PlanFillInInfo> result = new JsonResService<PlanFillInInfo>();
            try
            {
                result = _inspectionPlanService.GetPlanReportContent(InspectionOrder);
            }
            catch (Exception ex)
            {
                result.AccessState = ResState.Failed;
                result.ErrorMessage = ex.Message;
            }
            return result;
        }
        public JsonResService<string> Post(PlanFillInInfo data)
        {
            JsonResService<string> result = new JsonResService<string>();
            try
            {
                result = _inspectionPlanService.PlanReportFillIn(data);
            }
            catch (Exception ex)
            {
                result.AccessState = ResState.Failed;
                result.ErrorMessage = ex.Message;
            }
            return result;
        }
    }
}
