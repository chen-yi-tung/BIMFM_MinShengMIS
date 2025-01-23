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
                #region 若計畫為待執行，則建立InspectionPlan_RFIDOrder、巡檢設備 InspectionPlan_Equipment、InspectionPlan_EquipmentCheckItem、InspectionPlan_EquipmentReportingItem，並將巡檢狀態改為2:執行中
                
                var inspectionPlanTime = _db.InspectionPlan_Time.Find(IPTSN);
                if(inspectionPlanTime.InspectionState == "1")
                {
                    var PlanPathSN = _db.InspectionPlan_Time.Find(IPTSN).PlanPathSN;
                    //新增巡檢設備
                    var esnList = (from x1 in _db.InspectionDefaultOrder
                                  where x1.PlanPathSN == PlanPathSN
                                  orderby x1.DefaultOrder
                                  join x2 in _db.RFID on x1.RFIDInternalCode equals x2.RFIDInternalCode
                                  select  x2.ESN ).Distinct().ToList();
                    foreach(var esn in esnList)
                    {
                        InspectionPlan_Equipment plan_Equipment = new InspectionPlan_Equipment();
                        plan_Equipment.IPESN = IPTSN + esn;
                        plan_Equipment.IPTSN = IPTSN;
                        plan_Equipment.ESN = esn;
                        _db.InspectionPlan_Equipment.AddOrUpdate(plan_Equipment);
                        _db.SaveChanges();

                        //建立巡檢項目
                        var TSN = _db.EquipmentInfo.Find(esn).TSN.ToString();
                        //建立巡檢設備檢查項目
                        var CheckItems = _db.Template_CheckItem.Where(x => x.TSN == TSN).OrderBy(x => x.CISN).ToList();
                        var lastcheckItemId = _db.InspectionPlan_EquipmentCheckItem.Where(x => x.IPESN == plan_Equipment.IPESN).OrderByDescending(x => x.Id).FirstOrDefault()?.Id ?? (plan_Equipment.IPESN + "000");
                        var InspectionPlan_EquipmentCheckItems = new List<InspectionPlan_EquipmentCheckItem>();
                        foreach (var checkItem in CheckItems)
                        {
                            InspectionPlan_EquipmentCheckItem inspectionPlan_EquipmentCheckItem = new InspectionPlan_EquipmentCheckItem();
                            inspectionPlan_EquipmentCheckItem.Id = ComFunc.CreateNextID(plan_Equipment.IPESN+"%{3}", lastcheckItemId);
                            inspectionPlan_EquipmentCheckItem.IPESN = plan_Equipment.IPESN;
                            inspectionPlan_EquipmentCheckItem.CheckItemName = checkItem.CheckItemName;
                            InspectionPlan_EquipmentCheckItems.Add(inspectionPlan_EquipmentCheckItem);
                            lastcheckItemId = inspectionPlan_EquipmentCheckItem.Id;
                        }
                        _db.InspectionPlan_EquipmentCheckItem.AddOrUpdate(InspectionPlan_EquipmentCheckItems.ToArray());
                        _db.SaveChanges();
                        //建立巡檢設備填報項目
                        var ReportingItems = _db.Template_ReportingItem.Where(x => x.TSN == TSN).OrderBy(x => x.RISN).ToList();
                        var lastReportingItemId = _db.InspectionPlan_EquipmentReportingItem.Where(x => x.IPESN == plan_Equipment.IPESN).OrderByDescending(x => x.Id).FirstOrDefault()?.Id ?? (plan_Equipment.IPESN + "000");
                        var InspectionPlan_EquipmentReportingItems = new List<InspectionPlan_EquipmentReportingItem>();
                        foreach (var ReportingItem in ReportingItems)
                        {
                            InspectionPlan_EquipmentReportingItem inspectionPlan_EquipmentReportingItem = new InspectionPlan_EquipmentReportingItem();
                            inspectionPlan_EquipmentReportingItem.Id = ComFunc.CreateNextID(plan_Equipment.IPESN+"%{3}", lastReportingItemId);
                            inspectionPlan_EquipmentReportingItem.IPESN = plan_Equipment.IPESN;
                            inspectionPlan_EquipmentReportingItem.ReportValue = ReportingItem.ReportingItemName;
                            inspectionPlan_EquipmentReportingItem.Unit = ReportingItem.Unit;
                            InspectionPlan_EquipmentReportingItems.Add(inspectionPlan_EquipmentReportingItem);
                            lastReportingItemId = inspectionPlan_EquipmentReportingItem.Id;
                        }
                        _db.InspectionPlan_EquipmentReportingItem.AddOrUpdate(InspectionPlan_EquipmentReportingItems.ToArray());
                        _db.SaveChanges();
                    }
                    //新增巡檢RFID順序
                    var RFIDs = from x1 in _db.InspectionDefaultOrder
                                where x1.PlanPathSN == PlanPathSN
                                orderby x1.DefaultOrder
                                join x2 in _db.RFID on x1.RFIDInternalCode equals x2.RFIDInternalCode
                                select new { x1.RFIDInternalCode, x2.ESN};
                    var InspectionPlan_RFIDOrders = new List<InspectionPlan_RFIDOrder>();
                    var lastInspectionOrder = _db.InspectionPlan_RFIDOrder.Where(x => x.IPTSN == IPTSN).OrderByDescending(x => x.InspectionOrder).FirstOrDefault()?.InspectionOrder ?? (IPTSN + "00000");
                    foreach (var RFID in RFIDs)
                    {
                        InspectionPlan_RFIDOrder inspectionPlan_RFIDOrder = new InspectionPlan_RFIDOrder();
                        inspectionPlan_RFIDOrder.InspectionOrder = ComFunc.CreateNextID(IPTSN+"%{5}", lastInspectionOrder);
                        inspectionPlan_RFIDOrder.IPTSN = IPTSN;
                        inspectionPlan_RFIDOrder.IPESN = IPTSN + RFID.ESN;
                        inspectionPlan_RFIDOrder.RFIDInternalCode = RFID.RFIDInternalCode;
                        inspectionPlan_RFIDOrder.Status = "1";
                        InspectionPlan_RFIDOrders.Add(inspectionPlan_RFIDOrder);
                        lastInspectionOrder = inspectionPlan_RFIDOrder.InspectionOrder;
                    }
                    _db.InspectionPlan_RFIDOrder.AddOrUpdate(InspectionPlan_RFIDOrders.ToArray());
                    _db.SaveChanges();
                    //將巡檢時段計畫與巡檢路線關聯刪除
                    inspectionPlanTime.PlanPathSN = null;
                    //將巡檢時段計畫改為執行中
                    inspectionPlanTime.InspectionState = "2";
                    _db.InspectionPlan_Time.AddOrUpdate(inspectionPlanTime);
                    _db.SaveChanges();
                    //將工單狀態改為執行中
                    var inspectionPlan = _db.InspectionPlan.Find(inspectionPlanTime.IPSN);
                    if(inspectionPlan.PlanState == "1")
                    {
                        inspectionPlan.PlanState = "2";
                        _db.InspectionPlan.AddOrUpdate(inspectionPlan);
                        _db.SaveChanges();
                    }
                }

                #endregion

                #region 資料檢查
                var data = from x1 in _db.InspectionPlan_RFIDOrder
                           where x1.IPTSN == IPTSN
                           join x2 in _db.InspectionPlan_Equipment on x1.IPESN equals x2.IPESN
                           join x3 in _db.EquipmentInfo on x2.ESN equals x3.ESN
                           join x4 in _db.RFID on x1.RFIDInternalCode equals x4.RFIDInternalCode
                           join x5 in _db.Floor_Info on x4.FSN equals x5.FSN
                           join x6 in _db.AreaInfo on x5.ASN equals x6.ASN
                           select new { x1.Status, x3.EName, x3.EState, x3.NO, x3.FSN, x1.RFIDInternalCode, x3.ESN, x5.FloorName, x6.Area, x1.InspectionOrder, RFID_FSN = x4.FSN, RFID_Location_X = x4.Location_X, RFID_Location_Y = x4.Location_Y };

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
                    planRFIDInfo.FSN = plan.FSN;
                    planRFIDInfo.RFIDInternalCode = plan.RFIDInternalCode;
                    planRFIDInfo.ESN = plan.ESN;
                    planRFIDInfo.InspectionOrder = plan.InspectionOrder;
                    planRFIDInfo.RFID_FSN = plan.RFID_FSN;
                    planRFIDInfo.RFID_Location_X = plan.RFID_Location_X;
                    planRFIDInfo.RFID_Location_Y = plan.RFID_Location_Y;
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

        #region APP-巡檢填報
        public JsonResService<string> PlanReportFillIn(string userID, PlanFillInInfo data)
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
                inspectionEquipment.ReportUserName = userID;
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

        #region APP-結束巡檢
        public JsonResService<string> EndInspection(string userID, string IPTSN)
        {
            #region 變數
            JsonResService<string> res = new JsonResService<string>();
            #endregion

            try
            {
                #region 資料檢查
                var data = _db.InspectionPlan_Time.Find(IPTSN);

                if (data == null)
                {
                    res.AccessState = ResState.Failed;
                    res.ErrorMessage = "查無此設備巡檢時段工單";
                    return res;
                }
                #endregion

                #region 結束巡檢
                data.InspectionState = "3";
                 _db.InspectionPlan_Time.AddOrUpdate(data);
                 _db.SaveChanges();
                //to do 刪rfid
                var deleteRFID = _db.InspectionPlan_RFIDOrder.Where(x => x.IPTSN == IPTSN).ToList();
                _db.InspectionPlan_RFIDOrder.RemoveRange(deleteRFID);
                _db.SaveChanges();
                //填報完成檢查是否該工單已執行完成
                var checkAllDone = _db.InspectionPlan_Time.Where(x => x.InspectionState != "3").Count();
                if (checkAllDone == 0) //巡檢時段皆已巡檢完成
                {
                    var inspectionPlan = _db.InspectionPlan.Find(data.IPSN);
                    inspectionPlan.PlanState = "3";
                    _db.InspectionPlan.AddOrUpdate(inspectionPlan);
                    _db.SaveChanges();
                }
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