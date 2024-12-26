using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using static MinSheng_MIS.Models.ViewModels.InspectionPlan_ManagementViewModel;

namespace MinSheng_MIS.Services
{
    public class InspectionPlan_ManagementService
    {
        private readonly Bimfm_MinSheng_MISEntities _db;

        public InspectionPlan_ManagementService(Bimfm_MinSheng_MISEntities db)
        {
            _db = db;
        }

        #region APP-巡檢工單列表
        public JsonResService<List<PlanInfo>> GetPlanList(string userID,DateTime searchdate)
        {
            #region 變數
            JsonResService<List<PlanInfo>> res = new JsonResService<List<PlanInfo>>();
            var dic_inspectionState = Surface.InspectionState();
            #endregion

            try
            {
                #region 資料檢查
                var data = from x1 in _db.InspectionPlan_Member
                           where x1.UserID == userID
                           join x2 in _db.InspectionPlan_Time on x1.IPTSN equals x2.IPTSN
                           join x3 in _db.InspectionPlan on x2.IPSN equals x3.IPSN
                           where x3.PlanDate == searchdate
                           select new { x3.PlanDate, x2.IPTSN, x2.StartTime, x2.EndTime, x2.InspectionState };
                if (data == null)
                {
                    res.AccessState = ResState.Failed;
                    res.ErrorMessage = "查無此日期之巡檢計畫";
                    return res;
                }
                #endregion

                #region 資料
                List<PlanInfo> datas = new List<PlanInfo>();
                foreach (var plan in data)
                {
                    PlanInfo planInfo = new PlanInfo();
                    planInfo.InspectionState = dic_inspectionState[plan.InspectionState];
                    planInfo.IPTSN = plan.IPTSN;
                    planInfo.InspectionTime = plan.PlanDate.ToString("yyyy/MM/dd") + " " + plan.StartTime + "-" + plan.EndTime;
                    List<string> members = new List<string>();
                    var memberlist = from x1 in _db.InspectionPlan_Member
                                  where x1.IPTSN == plan.IPTSN
                                  join x2 in _db.AspNetUsers on x1.UserID equals x2.UserName
                                  select new { x2.MyName};
                    foreach(var member in memberlist)
                    {
                        members.Add(member.MyName);
                    }
                    planInfo.Member= members;
                    datas.Add(planInfo);
                }
                res.Datas = datas;
                #endregion

                res.AccessState = ResState.Success;
                return res;
            }
            catch (Exception ex)
            {
                res.AccessState = ResState.Failed;
                res.ErrorMessage = ex.Message;
                throw;
            }
        }
        #endregion

        #region APP-巡檢設備列表
        public JsonResService<List<PlanRFIDInfo>> GetPlanRFIDList(string IPTSN)
        {
            #region 變數
            JsonResService<List<PlanRFIDInfo>> res = new JsonResService<List<PlanRFIDInfo>>();
            var dic_eState = Surface.EState();
            var dic_status = Surface.Status();
            #endregion

            try
            {
                #region 資料檢查
                var data = from x1 in _db.InspectionPlan_RFIDOrder
                           where x1.IPTSN == IPTSN
                           join x2 in _db.InspectionPlan_Equipment on x1.IPESN equals x2.IPESN
                           join x3 in _db.EquipmentInfo on x2.ESN equals x3.ESN
                           join x4 in _db.RFID on x1.RFIDInternalCode equals x4.RFIDInternalCode
                           join x5 in _db.Floor_Info on x4.FSN equals x5.FSN
                           join x6 in _db.AreaInfo on x5.ASN equals x6.ASN
                           select new { x1.Status,x3.EName,x3.EState,x3.NO,x3.FSN, x1.RFIDInternalCode,x3.ESN, x5.FloorName, x6.Area, x1.InspectionOrder};

                if (data == null)
                {
                    res.AccessState = ResState.Failed;
                    res.ErrorMessage = "查無此巡檢時段之巡檢設備列表";
                    return res;
                }
                #endregion

                #region 資料
                List<PlanRFIDInfo> datas = new List<PlanRFIDInfo>();
                foreach (var plan in data)
                {
                    PlanRFIDInfo planRFIDInfo = new PlanRFIDInfo();
                    planRFIDInfo.Status = dic_status[plan.Status];
                    planRFIDInfo.EName = plan.EName;
                    planRFIDInfo.EState = dic_eState[plan.EState];
                    planRFIDInfo.NO = plan.NO;
                    planRFIDInfo.Location = plan.Area + " " + plan.FloorName;
                    planRFIDInfo.RFIDInternalCode = plan.RFIDInternalCode;
                    planRFIDInfo.ESN = plan.ESN;
                    planRFIDInfo.InspectionOrder = plan.InspectionOrder;
                    datas.Add(planRFIDInfo);
                }
                res.Datas = datas;
                #endregion

                res.AccessState = ResState.Success;
                return res;
            }
            catch (Exception ex)
            {
                res.AccessState = ResState.Failed;
                res.ErrorMessage = ex.Message;
                throw;
            }
        }
        #endregion

        #region APP-取得巡檢填報內容
        public JsonResService<PlanFillInInfo> GetPlanReportContent(string InspectionOrder)
        {
            #region 變數
            JsonResService<PlanFillInInfo> res = new JsonResService<PlanFillInInfo>();
            #endregion

            try
            {
                #region 資料檢查
                var data = _db.InspectionPlan_RFIDOrder.Find(InspectionOrder);

                if (data == null)
                {
                    res.AccessState = ResState.Failed;
                    res.ErrorMessage = "查無此設備巡檢填報內容";
                    return res;
                }
                #endregion

                #region 資料
                PlanFillInInfo datas = new PlanFillInInfo();
                datas.InspectionOrder = data.InspectionOrder;
                var equipmentCheckItems = _db.InspectionPlan_EquipmentCheckItem.Where(x => x.IPESN == data.IPESN).OrderBy(x => x.Id).ToList();
                List<EquipmentCheckItem> checkItems = new List<EquipmentCheckItem>();
                foreach (var item in equipmentCheckItems)
                {
                    EquipmentCheckItem checkItem = new EquipmentCheckItem();
                    checkItem.Id = item.Id;
                    checkItem.CheckItemName = item.CheckItemName;
                    checkItem.CheckResult = item.CheckResult;
                    checkItems.Add(checkItem);
                }
                datas.EquipmentCheckItems = checkItems;
                var equipmentReportingItems = _db.InspectionPlan_EquipmentReportingItem.Where(x => x.IPESN == data.IPESN).OrderBy(x => x.Id).ToList();
                List<EquipmentReportingItem> reportItems = new List<EquipmentReportingItem>();
                foreach (var item in equipmentReportingItems)
                {
                    EquipmentReportingItem reportItem = new EquipmentReportingItem();
                    reportItem.Id = item.Id;
                    reportItem.ReportValue = item.ReportValue;
                    reportItem.ReportContent = item.ReportContent;
                    reportItem.Unit = item.Unit;
                    reportItems.Add(reportItem);
                }
                datas.EquipmentReportingItems = reportItems;
                res.Datas = datas;
                #endregion

                res.AccessState = ResState.Success;
                return res;
            }
            catch (Exception ex)
            {
                res.AccessState = ResState.Failed;
                res.ErrorMessage = ex.Message;
                throw;
            }
        }
        #endregion

        #region APP-取得巡檢填報
        public JsonResService<string> PlanReportFillIn(PlanFillInInfo data)
        {
            #region 變數
            JsonResService<string> res = new JsonResService<string>();
            #endregion

            try
            {
                #region 資料檢查
                var checkRFIDOrder = _db.InspectionPlan_RFIDOrder.Find(data.InspectionOrder);

                if (checkRFIDOrder == null)
                {
                    res.AccessState = ResState.Failed;
                    res.ErrorMessage = "查無此設備巡檢填報內容";
                    return res;
                }
                #endregion
                var inspectionEquipment = _db.InspectionPlan_Equipment.Find(checkRFIDOrder.IPESN);
                #region 填報
                //巡檢設備
                inspectionEquipment.ReportUserName = data.ReportUserName;
                inspectionEquipment.FillinTime = DateTime.Now;
                _db.InspectionPlan_Equipment.AddOrUpdate(inspectionEquipment);
                _db.SaveChanges();
                //巡檢設備檢查項目
                foreach (var item in data.EquipmentCheckItems)
                {
                    var checkItem = _db.InspectionPlan_EquipmentCheckItem.Find(item.Id);
                    checkItem.CheckResult = item.CheckResult;
                    _db.InspectionPlan_EquipmentCheckItem.AddOrUpdate(checkItem);
                    _db.SaveChanges();
                }
                //巡檢設備填報項目
                foreach (var item in data.EquipmentReportingItems)
                {
                    var reportItem = _db.InspectionPlan_EquipmentReportingItem.Find(item.Id);
                    reportItem.ReportContent = item.ReportContent;
                    _db.InspectionPlan_EquipmentReportingItem.AddOrUpdate(reportItem);
                    _db.SaveChanges();
                }
                //巡檢RFID更改狀態為完成
                checkRFIDOrder.Status = "2";
                _db.InspectionPlan_RFIDOrder.AddOrUpdate(checkRFIDOrder);
                _db.SaveChanges();
                #endregion

                res.AccessState = ResState.Success;
                return res;
            }
            catch (Exception ex)
            {
                res.AccessState = ResState.Failed;
                res.ErrorMessage = ex.Message;
                throw;
            }
        }
        #endregion
    }
}