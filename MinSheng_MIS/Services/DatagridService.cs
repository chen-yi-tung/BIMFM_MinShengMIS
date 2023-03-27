using MinSheng_MIS.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin;
using System.Drawing.Printing;
using System.Web.UI.WebControls;
using MinSheng_MIS.Surfaces;
using System.Security.Cryptography.X509Certificates;

namespace MinSheng_MIS.Services
{
    public class DatagridService
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
        public JObject GetJsonForGrid_Management(System.Web.Mvc.FormCollection form)
        {
            #region datagrid呼叫時的預設參數有 rows 跟 page
            int page = 1;
            if (!string.IsNullOrEmpty(form["page"]?.ToString()))
            {
                page = short.Parse(form["page"].ToString());
            }
            int rows = 10;
            if (!string.IsNullOrEmpty(form["rows"]?.ToString()))
            {
                rows = short.Parse(form["rows"]?.ToString());
            }
            #endregion
            string propertyName = "Date";
            string order = "asc";

            //塞來自formdata的資料
            //棟別名稱
            string Area = form["Area"]?.ToString();
            //棟別編號
            string ASN = form["ASN"]?.ToString();
            //樓層名稱
            string Floor = form["Floor"]?.ToString();
            //樓層編號
            string FSN = form["FSN"]?.ToString();
            //報修單編號
            string ReportState = form["ReportState"]?.ToString();
            //報修等級
            string ReportLevel = form["ReportLevel"]?.ToString();
            //報修單號
            string RSN = form["RSN"]?.ToString();
            //設備編號
            string ESN = form["ESN"]?.ToString();
            //設備名稱
            string EName = form["EName"]?.ToString();
            //國有財產編碼
            string PropertyCode = form["PropertyCode"]?.ToString();
            //報修說明
            string ReportContent = form["ReportContent"]?.ToString();
            //報修人員id
            string InformantUserID = form["InformantUserID"]?.ToString();
            //起始日期
            string DateFrom = form["DateFrom"]?.ToString();
            //結束日期
            string DateTo = form["DateTo"]?.ToString();
            //判斷是從哪裡來的請求DataGrid
            string SourceReport = form["SourceReport"]?.ToString();


            #region 依據查詢字串檢索資料表

            var SourceTable = from x1 in db.EquipmentReportForm
                              join x2 in db.EquipmentInfo on x1.ESN equals x2.ESN
                              join x3 in db.AspNetUsers on x1.InformatUserID equals x3.UserName
                              select new { x1.ReportState, x1.ReportLevel, x2.Area, x2.Floor, x1.ReportSource, x1.RSN, x1.Date, x2.PropertyCode, x1.ESN, x2.EName, x1.ReportContent, x3.MyName, x3.UserName, x2.EState, x1.StockState };

            //若是用於新增巡檢計畫 的 新增維修單需增加狀態判斷
            if (SourceReport == "AddReportForm")
            {
                //增加狀態判斷
                SourceTable = SourceTable.Where(x => x.ReportState == "1" || x.ReportState == "5" || x.ReportState == "8" || x.ReportState == "9" || x.ReportState == "10" || x.ReportState == "11");
                //設備若停用則不能加入巡檢計畫中
                SourceTable = SourceTable.Where(x => x.EState != "3");
            }

            //Area查詢table方式 以Area至表[設備資訊]查詢出ESN，再以ESN至表[設備報修單]查詢出相關報修單。
            if (!string.IsNullOrEmpty(Area))
            {
                SourceTable = SourceTable.Where(x => x.Area == Area);
            }
            if (!string.IsNullOrEmpty(Floor))
            {
                SourceTable = SourceTable.Where(x => x.Floor == Floor);
            }
            if (!string.IsNullOrEmpty(ReportState))
            {
                SourceTable = SourceTable.Where(x => x.ReportState == ReportState);
            }
            if (!string.IsNullOrEmpty(ReportLevel))
            {
                SourceTable = SourceTable.Where(x => x.ReportLevel == ReportLevel);
            }
            if (!string.IsNullOrEmpty(RSN))
            {
                SourceTable = SourceTable.Where(x => x.RSN == RSN);
            }
            if (!string.IsNullOrEmpty(ESN))
            {
                SourceTable = SourceTable.Where(x => x.ESN == ESN);
            }
            if (!string.IsNullOrEmpty(EName))
            {
                SourceTable = SourceTable.Where(x => x.EName == EName);
            }
            if (!string.IsNullOrEmpty(PropertyCode))
            {
                SourceTable = SourceTable.Where(x => x.PropertyCode == PropertyCode);
            }
            if (!string.IsNullOrEmpty(ReportContent))
            {
                SourceTable = SourceTable.Where(x => x.ReportContent.Contains(ReportContent));
            }
            if (!string.IsNullOrEmpty(InformantUserID))
            {
                SourceTable = SourceTable.Where(x => x.UserName == InformantUserID);
            }
            if (!string.IsNullOrEmpty(DateFrom))
            {
                var datefrom = DateTime.Parse(DateFrom);
                SourceTable = SourceTable.Where(x => x.Date >= datefrom);
            }
            if (!string.IsNullOrEmpty(DateTo))
            {
                var dateto = DateTime.Parse(DateTo).AddDays(1);
                SourceTable = SourceTable.Where(x => x.Date <= dateto);
            }
            //var atable_ESN_list = db.EquipmentInfo.Where(x => x.Area == Area).Select(x=>x.ESN).ToList();
            //var atable_SearchTable = db.EquipmentReportForm.Where(x=> atable_ESN_list.Contains(x.ESN));
            #endregion
            var resulttable = SourceTable.OrderByDescending(x => x.Date).AsQueryable();
            //回傳JSON陣列
            JArray ja = new JArray();
            //記住總筆數
            int total = resulttable.Count();
            //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
            resulttable = resulttable.Skip((page - 1) * rows).Take(rows);


            foreach (var a in resulttable)
            {
                var itemObjects = new JObject();
                if (itemObjects["ReportState"] == null)
                {
                    string statsSN = a.ReportState.Trim();
                    var dic = Surface.EquipmentReportFormState();
                    //string aaaaa = dic["8"];
                    itemObjects.Add("ReportState", dic[statsSN]); //報修單狀態
                }
                if (itemObjects["ReportLevel"] == null)
                {
                    string levelSN = a.ReportLevel.Trim();
                    var dic = Surface.ReportLevel();
                    itemObjects.Add("ReportLevel", dic[levelSN]); // 報修單等級
                }
                if (itemObjects["Area"] == null)
                    itemObjects.Add("Area", a.Area);    //棟別                           

                if (itemObjects["Floor"] == null)
                    itemObjects.Add("Floor", a.Floor);   //樓層

                if (itemObjects["ReportSource"] == null)
                {
                    string sourcesn = a.ReportSource.Trim();
                    var dic = Surface.ReportSource();   //報修來源
                    itemObjects.Add("ReportSource", dic[sourcesn]);
                }
                if (itemObjects["RSN"] == null)
                    itemObjects.Add("RSN", a.RSN);  //RSN

                if (itemObjects["Date"] == null)
                    itemObjects.Add("Date", a.Date.ToString("yyyy/MM/dd HH:mm:ss"));                                //保養週期

                if (itemObjects["PropertyCode"] == null)
                    itemObjects.Add("PropertyCode", a.PropertyCode);    //國有財產編碼
                if (itemObjects["ESN"] == null)
                    itemObjects.Add("ESN", a.ESN);    //設備編號
                if (itemObjects["EName"] == null)
                    itemObjects.Add("EName", a.EName);    //設備名稱
                if (itemObjects["ReportContent"] == null)
                    itemObjects.Add("ReportContent", a.ReportContent);    //報修內容
                if (itemObjects["MyName"] == null)
                    itemObjects.Add("MyName", a.MyName);    //報修人員
                if(a.StockState) //庫存狀態
                {
                    itemObjects.Add("StockState", "是");
                }
                else
                {
                    itemObjects.Add("StockState", "無");
                }
                ja.Add(itemObjects);
            }

            JObject jo = new JObject();
            jo.Add("rows", ja);
            jo.Add("total", total);
            return jo;
        }

        public JObject GetJsonForGrid_MaintainRecord_Management(System.Web.Mvc.FormCollection form) //巡檢保養紀錄管理
        {
            #region datagrid呼叫時的預設參數有 rows 跟 page
            int page = 1;
            if (!string.IsNullOrEmpty(form["page"]?.ToString()))
            {
                page = short.Parse(form["page"].ToString());
            }
            int rows = 10;
            if (!string.IsNullOrEmpty(form["rows"]?.ToString()))
            {
                rows = short.Parse(form["rows"]?.ToString());
            }
            #endregion

            #region 塞來自formdata的資料
            //棟別名稱
            string Area = form["Area"]?.ToString();
            //棟別編號
            string ASN = form["ASN"]?.ToString();
            //樓層名稱
            string Floor = form["Floor"]?.ToString();
            //樓層編號
            string FSN = form["FSN"]?.ToString();
            //巡檢計畫編號
            string IPSN = form["IPSN"]?.ToString();
            //巡檢計畫名稱
            string IPName = form["IPName"]?.ToString();
            //保養單狀態
            string MaintainState = form["MaintainState"]?.ToString();
            //國有財產編碼
            string PropertyCode = form["PropertyCode"]?.ToString();
            //設備編號
            string ESN = form["ESN"]?.ToString();
            //設備名稱
            string EName = form["EName"]?.ToString();
            //保養項目編號
            string EMFISN = form["EMFISN"]?.ToString();
            //保養項目
            string MIName = form["MIName"]?.ToString();
            //保養人員
            string MaintainUserID = form["MaintainUserID"]?.ToString();
            //審核人員
            string AuditUserID = form["AuditUserID"]?.ToString();
            //日期項目選擇   //有五個選項--> 計畫日期 && 上次保養日期 && 本次保養日期 && 下次保養日期 && 審核日期     前端的Value還沒決定
            string DateSelect = form["DateSelect"]?.ToString();
            //日期(起)
            string DateFrom = form["DateFrom"]?.ToString();
            //日期(迄)
            string DateTo = form["DateTo"]?.ToString();
            #endregion

            #region 依據查詢字串檢索資料表

            var DataSource = from x1 in db.InspectionPlanMaintain select x1;

            var IPMSNlist = db.InspectionPlanMaintain.Select(x => x.IPMSN).ToList();
            if (!string.IsNullOrEmpty(ASN)) //棟別編號
            {
                int a = Int16.Parse(ASN);
                var FSNlist = db.Floor_Info.Where(x => x.ASN == a).Select(x => x.FSN).ToList();
                var ESNlist = db.EquipmentInfo.Where(x => FSNlist.Contains(x.FSN)).Select(x => x.ESN).ToList();
                var EMISNlist = db.EquipmentMaintainItem.Where(x => ESNlist.Contains(x.ESN)).Select(x => x.EMISN).ToList();
                var EMFISNlist = db.EquipmentMaintainFormItem.Where(x => EMISNlist.Contains(x.EMISN)).Select(x => x.EMFISN).ToList();
                var iPMSNlist = db.InspectionPlanMaintain.Where(x => EMFISNlist.Contains(x.EMFISN)).Select(x => x.IPMSN).ToList();
                var templist = new List<string>();
                foreach (var item in IPMSNlist)
                {
                    if (iPMSNlist.Contains(item))
                        templist.Add(item);
                }
                IPMSNlist = templist;
            }
            if (!string.IsNullOrEmpty(FSN)) //樓層編號
            {
                var ESNlist = db.EquipmentInfo.Where(x => x.FSN == FSN).Select(x => x.ESN).ToList();
                var EMISNlist = db.EquipmentMaintainItem.Where(x => ESNlist.Contains(x.ESN)).Select(x => x.EMISN).ToList();
                var EMFISNlist = db.EquipmentMaintainFormItem.Where(x => EMISNlist.Contains(x.EMISN)).Select(x => x.EMFISN).ToList();
                var iPMSNlist = db.InspectionPlanMaintain.Where(x => EMFISNlist.Contains(x.EMFISN)).Select(x => x.IPMSN).ToList();
                var templist = new List<string>();
                foreach (var item in IPMSNlist)
                {
                    if (iPMSNlist.Contains(item))
                        templist.Add(item);
                }
                IPMSNlist = templist;
            }
            if (!string.IsNullOrEmpty(IPName)) //巡檢計畫名稱
            {
                var IPSNlist = db.InspectionPlan.Where(x => x.IPName.Contains(IPName)).Select(x => x.IPSN).ToList();
                var iPMSNlist = db.InspectionPlanMaintain.Where(x => IPSNlist.Contains(x.IPSN)).Select(x => x.IPMSN).ToList();
                var templist = new List<string>();
                foreach (var item in IPMSNlist)
                {
                    if (iPMSNlist.Contains(item))
                        templist.Add(item);
                }
                IPMSNlist = templist;
            }
            if (!string.IsNullOrEmpty(PropertyCode)) //國有財產編碼
            {
                var ESNlist = db.EquipmentInfo.Where(x => x.PropertyCode == PropertyCode).Select(x => x.ESN).ToList();
                var EMISNlist = db.EquipmentMaintainItem.Where(x => ESNlist.Contains(x.ESN)).Select(x => x.EMISN).ToList();
                var EMFISNlist = db.EquipmentMaintainFormItem.Where(x => EMISNlist.Contains(x.EMISN)).Select(x => x.EMFISN).ToList();
                var iPMSNlist = db.InspectionPlanMaintain.Where(x => EMFISNlist.Contains(x.EMFISN)).Select(x => x.IPMSN).ToList();
                var templist = new List<string>();
                foreach (var item in IPMSNlist)
                {
                    if (iPMSNlist.Contains(item))
                        templist.Add(item);
                }
                IPMSNlist = templist;
            }
            if (!string.IsNullOrEmpty(ESN)) //設備編號
            {
                var EMISNlist = db.EquipmentMaintainItem.Where(x => x.ESN == ESN).Select(x => x.EMISN).ToList();
                var EMFISNlist = db.EquipmentMaintainFormItem.Where(x => EMISNlist.Contains(x.EMISN)).Select(x => x.EMFISN).ToList();
                var iPMSNlist = db.InspectionPlanMaintain.Where(x => EMFISNlist.Contains(x.EMFISN)).Select(x => x.IPMSN).ToList();
                var templist = new List<string>();
                foreach (var item in IPMSNlist)
                {
                    if (iPMSNlist.Contains(item))
                        templist.Add(item);
                }
                IPMSNlist = templist;
            }
            if (!string.IsNullOrEmpty(EName)) //設備名稱
            {
                var ESNlist = db.EquipmentInfo.Where(x => x.EName.Contains(EName)).Select(x => x.ESN).ToList();
                var EMISNlist = db.EquipmentMaintainItem.Where(x => ESNlist.Contains(x.ESN)).Select(x => x.EMISN).ToList();
                var EMFISNlist = db.EquipmentMaintainFormItem.Where(x => EMISNlist.Contains(x.EMISN)).Select(x => x.EMFISN).ToList();
                var iPMSNlist = db.InspectionPlanMaintain.Where(x => EMFISNlist.Contains(x.EMFISN)).Select(x => x.IPMSN).ToList();
                var templist = new List<string>();
                foreach (var item in IPMSNlist)
                {
                    if (iPMSNlist.Contains(item))
                        templist.Add(item);
                }
                IPMSNlist = templist;
            }
            if (!string.IsNullOrEmpty(MIName)) //保養項目
            {
                var MISNlist = db.MaintainItem.Where(x => x.MIName.Contains(MIName)).Select(x => x.MISN).ToList();
                var EMISNlist = db.EquipmentMaintainItem.Where(x => MISNlist.Contains(x.MISN)).Select(x => x.EMISN).ToList();
                var EMFISNlist = db.EquipmentMaintainFormItem.Where(x => EMISNlist.Contains(x.EMISN)).Select(x => x.EMFISN).ToList();
                var iPMSNlist = db.InspectionPlanMaintain.Where(x => EMFISNlist.Contains(x.EMFISN)).Select(x => x.IPMSN).ToList();
                var templist = new List<string>();
                foreach (var item in IPMSNlist)
                {
                    if (iPMSNlist.Contains(item))
                        templist.Add(item);
                }
                IPMSNlist = templist;
            }
            if (!string.IsNullOrEmpty(AuditUserID)) //審核人員
            {
                var iPMSNlist = db.MaintainAuditInfo.Where(x => x.AuditUserID == AuditUserID).Select(x => x.IPMSN).ToList();
                var templist = new List<string>();
                foreach (var item in IPMSNlist)
                {
                    if (iPMSNlist.Contains(item))
                        templist.Add(item);
                }
                IPMSNlist = templist;
            }
            if (!string.IsNullOrEmpty(DateSelect)) //日期項目選擇
            {
                if (!string.IsNullOrEmpty(DateFrom)) //日期(起)
                {
                    var datefrom = DateTime.Parse(DateFrom);
                    switch (DateSelect)
                    {
                        case "1":
                            var IPSNlist = db.InspectionPlan.Where(x => x.PlanDate >= datefrom).Select(x => x.IPSN).ToList();
                            var iPMSNlist = db.InspectionPlanMaintain.Where(x => IPSNlist.Contains(x.IPSN)).Select(x => x.IPMSN).ToList();
                            var templist = new List<string>();
                            foreach (var item in IPMSNlist)
                            {
                                if (iPMSNlist.Contains(item))
                                    templist.Add(item);
                            }
                            IPMSNlist = templist;
                            break;
                        case "2":
                            var EMFISNlist = db.EquipmentMaintainFormItem.Where(x => x.LastTime >= datefrom).Select(x => x.EMFISN).ToList();
                            var iPMSNlist2 = db.InspectionPlanMaintain.Where(x => EMFISNlist.Contains(x.EMFISN)).Select(x => x.IPMSN).ToList();
                            var templist2 = new List<string>();
                            foreach (var item in IPMSNlist)
                            {
                                if (iPMSNlist2.Contains(item))
                                    templist2.Add(item);
                            }
                            IPMSNlist = templist2;
                            break;
                        case "3":
                            var EMFISNlist3 = db.EquipmentMaintainFormItem.Where(x => x.Date >= datefrom).Select(x => x.EMFISN).ToList();
                            var iPMSNlist3 = db.InspectionPlanMaintain.Where(x => EMFISNlist3.Contains(x.EMFISN)).Select(x => x.IPMSN).ToList();
                            var templist3 = new List<string>();
                            foreach (var item in IPMSNlist)
                            {
                                if (iPMSNlist3.Contains(item))
                                    templist3.Add(item);
                            }
                            IPMSNlist = templist3;
                            break;
                        case "4":
                            var EMFISNlist4 = db.EquipmentMaintainFormItem.Where(x => x.NextTime >= datefrom).Select(x => x.EMFISN).ToList();
                            var iPMSNlist4 = db.InspectionPlanMaintain.Where(x => EMFISNlist4.Contains(x.EMFISN)).Select(x => x.IPMSN).ToList();
                            var templist4 = new List<string>();
                            foreach (var item in IPMSNlist)
                            {
                                if (iPMSNlist4.Contains(item))
                                    templist4.Add(item);
                            }
                            IPMSNlist = templist4;
                            break;
                        case "5":
                            var iPMSNlist5 = db.MaintainAuditInfo.Where(x => x.AuditDate >= datefrom).Select(x => x.IPMSN).ToList();
                            var templist5 = new List<string>();
                            foreach (var item in IPMSNlist)
                            {
                                if (iPMSNlist5.Contains(item))
                                    templist5.Add(item);
                            }
                            IPMSNlist = templist5;
                            break;
                    }
                }
                if (!string.IsNullOrEmpty(DateTo)) //日期(迄)
                {
                    var dateto = DateTime.Parse(DateTo).AddDays(1);
                    switch (DateSelect)
                    {
                        case "1":
                            var IPSNlist = db.InspectionPlan.Where(x => x.PlanDate < dateto).Select(x => x.IPSN).ToList();
                            var iPMSNlist = db.InspectionPlanMaintain.Where(x => IPSNlist.Contains(x.IPSN)).Select(x => x.IPMSN).ToList();
                            var templist = new List<string>();
                            foreach (var item in IPMSNlist)
                            {
                                if (iPMSNlist.Contains(item))
                                    templist.Add(item);
                            }
                            IPMSNlist = templist;
                            break;
                        case "2":
                            var EMFISNlist = db.EquipmentMaintainFormItem.Where(x => x.LastTime < dateto).Select(x => x.EMFISN).ToList();
                            var iPMSNlist2 = db.InspectionPlanMaintain.Where(x => EMFISNlist.Contains(x.EMFISN)).Select(x => x.IPMSN).ToList();
                            var templist2 = new List<string>();
                            foreach (var item in IPMSNlist)
                            {
                                if (iPMSNlist2.Contains(item))
                                    templist2.Add(item);
                            }
                            IPMSNlist = templist2;
                            break;
                        case "3":
                            var EMFISNlist3 = db.EquipmentMaintainFormItem.Where(x => x.Date < dateto).Select(x => x.EMFISN).ToList();
                            var iPMSNlist3 = db.InspectionPlanMaintain.Where(x => EMFISNlist3.Contains(x.EMFISN)).Select(x => x.IPMSN).ToList();
                            var templist3 = new List<string>();
                            foreach (var item in IPMSNlist)
                            {
                                if (iPMSNlist3.Contains(item))
                                    templist3.Add(item);
                            }
                            IPMSNlist = templist3;
                            break;
                        case "4":
                            var EMFISNlist4 = db.EquipmentMaintainFormItem.Where(x => x.NextTime < dateto).Select(x => x.EMFISN).ToList();
                            var iPMSNlist4 = db.InspectionPlanMaintain.Where(x => EMFISNlist4.Contains(x.EMFISN)).Select(x => x.IPMSN).ToList();
                            var templist4 = new List<string>();
                            foreach (var item in IPMSNlist)
                            {
                                if (iPMSNlist4.Contains(item))
                                    templist4.Add(item);
                            }
                            IPMSNlist = templist4;
                            break;
                        case "5":
                            var iPMSNlist5 = db.MaintainAuditInfo.Where(x => x.AuditDate < dateto).Select(x => x.IPMSN).ToList();
                            var templist5 = new List<string>();
                            foreach (var item in IPMSNlist)
                            {
                                if (iPMSNlist5.Contains(item))
                                    templist5.Add(item);
                            }
                            IPMSNlist = templist5;
                            break;
                    }
                }
            }

            /////////  where 剩下的條件
            DataSource = DataSource.Where(x => IPMSNlist.Contains(x.IPMSN));

            if (!string.IsNullOrEmpty(IPSN)) //巡檢計畫編號
            {
                DataSource = DataSource.Where(x => x.IPSN == IPSN);
            }
            if (!string.IsNullOrEmpty(MaintainState)) //保養單狀態   
            {
                DataSource = DataSource.Where(x => x.MaintainState == MaintainState);
            }
            if (!string.IsNullOrEmpty(EMFISN)) //保養項目編號 
            {
                DataSource = DataSource.Where(x => x.EMFISN == EMFISN);
            }
            if (!string.IsNullOrEmpty(MaintainUserID)) //保養人員 
            {
                DataSource = DataSource.Where(x => x.MaintainUserID == MaintainUserID);
            }
            #endregion

            //排序資料表
            var result = DataSource.OrderByDescending(x => x.IPMSN).AsQueryable();
            //回傳JSON陣列
            JArray ja = new JArray();
            //記住總筆數
            int total = result.Count();
            //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
            result = result.Skip((page - 1) * rows).Take(rows);

            #region 塞資料
            //建Json格式資料表回傳給前端
            foreach (var a in DataSource)
            {
                var InspectionPlan_ = db.InspectionPlan.Where(x => x.IPSN == a.IPSN).FirstOrDefault() == null ? new InspectionPlan() : db.InspectionPlan.Where(x => x.IPSN == a.IPSN).FirstOrDefault();
                var EquipmentMaintainFormItem_ = db.EquipmentMaintainFormItem.Where(x => x.EMFISN == a.EMFISN).FirstOrDefault() == null ? new EquipmentMaintainFormItem() : db.EquipmentMaintainFormItem.Where(x => x.EMFISN == a.EMFISN).FirstOrDefault();
                var EquipmentMaintainItem_ = db.EquipmentMaintainItem.Where(x => x.EMISN == EquipmentMaintainFormItem_.EMISN).FirstOrDefault() == null ? new EquipmentMaintainItem() : db.EquipmentMaintainItem.Where(x => x.EMISN == EquipmentMaintainFormItem_.EMISN).FirstOrDefault();
                var MaintainItem_ = db.MaintainItem.Where(x => x.MISN == EquipmentMaintainItem_.MISN).FirstOrDefault() == null ? new MaintainItem() : db.MaintainItem.Where(x => x.MISN == EquipmentMaintainItem_.MISN).FirstOrDefault();
                var EquipmentInfo_ = db.EquipmentInfo.Where(x => x.ESN == EquipmentMaintainItem_.ESN).FirstOrDefault() == null ? new EquipmentInfo() : db.EquipmentInfo.Where(x => x.ESN == EquipmentMaintainItem_.ESN).FirstOrDefault();
                var AspNetUsers_MaintainID_ = db.AspNetUsers.Where(x => x.UserName == a.MaintainUserID).FirstOrDefault() == null ? new AspNetUsers() : db.AspNetUsers.Where(x => x.UserName == a.MaintainUserID).FirstOrDefault();
                var MaintainAuditInfo_ = db.MaintainAuditInfo.Where(x => x.IPMSN == a.IPMSN).FirstOrDefault() == null ? new MaintainAuditInfo() : db.MaintainAuditInfo.Where(x => x.IPMSN == a.IPMSN).FirstOrDefault();
                var AspNetUsers_AuditID_ = db.AspNetUsers.Where(x => x.UserName == MaintainAuditInfo_.AuditUserID).FirstOrDefault() == null ? new AspNetUsers() : db.AspNetUsers.Where(x => x.UserName == MaintainAuditInfo_.AuditUserID).FirstOrDefault();

                var itemObjects = new JObject();
                itemObjects.Add("IPMSN", a.IPMSN);
                itemObjects.Add("IPSN", a.IPSN);
                itemObjects.Add("IPName", InspectionPlan_.IPName);
                itemObjects.Add("PlanDate", InspectionPlan_.PlanDate.ToString("yyyy/M/d"));

                var dic = Surface.InspectionPlanMaintainState();
                itemObjects.Add("MaintainState", dic[a.MaintainState.Trim()]); //這個要再用 Surface 做中文轉譯!!

                itemObjects.Add("Area", EquipmentInfo_.Area);
                itemObjects.Add("Floor", EquipmentInfo_.Floor);
                itemObjects.Add("PropertyCode", EquipmentInfo_.PropertyCode);
                itemObjects.Add("EName", EquipmentInfo_.EName);
                itemObjects.Add("ESN", EquipmentMaintainItem_.ESN);
                itemObjects.Add("EMFISN", a.EMFISN);
                itemObjects.Add("MIName", MaintainItem_.MIName);
                itemObjects.Add("Unit", EquipmentMaintainFormItem_.Unit);
                itemObjects.Add("Period", EquipmentMaintainFormItem_.Period);
                itemObjects.Add("LastTime", EquipmentMaintainFormItem_.LastTime.ToString("yyyy/M/d"));
                itemObjects.Add("Date", EquipmentMaintainFormItem_.Date.ToString("yyyy/M/d"));
                itemObjects.Add("NextTime", EquipmentMaintainFormItem_.NextTime?.ToString("yyyy/M/d"));
                itemObjects.Add("MaintainUserName", AspNetUsers_MaintainID_.MyName);
                itemObjects.Add("AuditUserName", AspNetUsers_AuditID_.MyName);
                itemObjects.Add("AuditDate", MaintainAuditInfo_.AuditDate.ToString("yyyy/M/d"));
                ja.Add(itemObjects);
            }
            #endregion

            JObject jo = new JObject();
            jo.Add("rows", ja);
            jo.Add("total", total);
            return jo;
        }


        public JObject GetJsonForGrid_RepairRecord_Management(System.Web.Mvc.FormCollection form) //巡檢維修紀錄管理
        {
            #region datagrid呼叫時的預設參數有 rows 跟 page
            int page = 1;
            if (!string.IsNullOrEmpty(form["page"]?.ToString()))
            {
                page = short.Parse(form["page"].ToString());
            }
            int rows = 10;
            if (!string.IsNullOrEmpty(form["rows"]?.ToString()))
            {
                rows = short.Parse(form["rows"]?.ToString());
            }
            #endregion

            #region 塞來自formdata的資料
            //棟別名稱
            string Area = form["Area"]?.ToString();
            //棟別編號
            string ASN = form["ASN"]?.ToString();
            //樓層名稱
            string Floor = form["Floor"]?.ToString();
            //樓層編號
            string FSN = form["FSN"]?.ToString();
            //巡檢計畫編號
            string IPSN = form["IPSN"]?.ToString();
            //巡檢計畫名稱
            string IPName = form["IPName"]?.ToString();
            //維修單狀態
            string RepairState = form["RepairState"]?.ToString();
            //報修等級
            string ReportLevel = form["ReportLevel"]?.ToString();
            //報修單號
            string RSN = form["RSN"]?.ToString();
            //設備編號
            string ESN = form["ESN"]?.ToString();
            //設備名稱
            string EName = form["EName"]?.ToString();
            //國有財產編碼
            string PropertyCode = form["PropertyCode"]?.ToString();
            //報修說明
            string ReportContent = form["ReportContent"]?.ToString();
            //報修人員
            string InformantUserID = form["InformantUserID"]?.ToString();
            //施工人員
            string RepairUserID = form["RepairUserID"]?.ToString();
            //審核人員
            string AuditUserID = form["AuditUserID"]?.ToString();
            //日期項目選擇   //有五個選項--> 計畫日期 && 上次保養日期 && 本次保養日期 && 下次保養日期 && 審核日期     前端的Value還沒決定
            string DateSelect = form["DateSelect"]?.ToString();
            //日期(起)
            string DateFrom = form["DateFrom"]?.ToString();
            //日期(迄)
            string DateTo = form["DateTo"]?.ToString();
            #endregion

            #region 依據查詢字串檢索資料表

            var DataSource = from x1 in db.InspectionPlanRepair select x1;

            var RSNlist = db.InspectionPlanRepair.Select(x => x.RSN).ToList();
            var IPSNlist = db.InspectionPlanRepair.Select(x => x.IPSN).ToList();
            var IPRSNlist = db.InspectionPlanRepair.Select(x => x.IPRSN).ToList();

            if (!string.IsNullOrEmpty(ASN)) //棟別
            {
                int a = Int16.Parse(ASN);
                var FSNlist = db.Floor_Info.Where(x => x.ASN == a).Select(x => x.FSN).ToList();
                var ESNlist = db.EquipmentInfo.Where(x => FSNlist.Contains(x.FSN)).Select(x => x.ESN).ToList();
                var rSNlist = db.EquipmentReportForm.Where(x => ESNlist.Contains(x.ESN)).Select(x => x.RSN).ToList();
                var templist = new List<string>();
                foreach (var item in RSNlist)
                {
                    if (rSNlist.Contains(item))
                        templist.Add(item);
                }
                RSNlist = templist;
            }
            if (!string.IsNullOrEmpty(FSN)) //樓層
            {
                var ESNlist = db.EquipmentInfo.Where(x => x.FSN == FSN).Select(x => x.ESN).ToList();
                var rSNlist = db.EquipmentReportForm.Where(x => ESNlist.Contains(x.ESN)).Select(x => x.RSN).ToList();
                var templist = new List<string>();
                foreach (var item in RSNlist)
                {
                    if (rSNlist.Contains(item))
                        templist.Add(item);
                }
                RSNlist = templist;
            }
            if (!string.IsNullOrEmpty(IPName)) //巡檢計畫名稱
            {
                var iPSNlist = db.InspectionPlan.Where(x => x.IPName == IPName).Select(x => x.IPSN).ToList();
                var templist = new List<string>();
                foreach (var item in IPSNlist)
                {
                    if (iPSNlist.Contains(item))
                        templist.Add(item);
                }
                IPSNlist = templist;
            }
            if (!string.IsNullOrEmpty(ReportLevel)) //報修等級
            {
                var rSNlist = db.EquipmentReportForm.Where(x => x.ReportLevel == ReportLevel).Select(x => x.RSN).ToList();
                var templist = new List<string>();
                foreach (var item in RSNlist)
                {
                    if (rSNlist.Contains(item))
                        templist.Add(item);
                }
                RSNlist = templist;
            }
            if (!string.IsNullOrEmpty(ESN)) //設備編號
            {
                var rSNlist = db.EquipmentReportForm.Where(x => x.ESN == ESN).Select(x => x.RSN).ToList();
                var templist = new List<string>();
                foreach (var item in RSNlist)
                {
                    if (rSNlist.Contains(item))
                        templist.Add(item);
                }
                RSNlist = templist;
            }
            if (!string.IsNullOrEmpty(EName)) //設備名稱
            {
                var ESNlist = db.EquipmentInfo.Where(x => x.EName == EName).Select(x => x.ESN).ToList();
                var rSNlist = db.EquipmentReportForm.Where(x => ESNlist.Contains(x.ESN)).Select(x => x.RSN).ToList();
                var templist = new List<string>();
                foreach (var item in RSNlist)
                {
                    if (rSNlist.Contains(item))
                        templist.Add(item);
                }
                RSNlist = templist;
            }
            if (!string.IsNullOrEmpty(PropertyCode)) //國有財產編碼
            {
                var ESNlist = db.EquipmentInfo.Where(x => x.PropertyCode == PropertyCode).Select(x => x.ESN).ToList();
                var rSNlist = db.EquipmentReportForm.Where(x => ESNlist.Contains(x.ESN)).Select(x => x.RSN).ToList();
                var templist = new List<string>();
                foreach (var item in RSNlist)
                {
                    if (rSNlist.Contains(item))
                        templist.Add(item);
                }
                RSNlist = templist;
            }
            if (!string.IsNullOrEmpty(ReportContent)) //報修說明
            {
                var rSNlist = db.EquipmentReportForm.Where(x => x.ReportContent.Contains(ReportContent)).Select(x => x.RSN).ToList();
                var templist = new List<string>();
                foreach (var item in RSNlist)
                {
                    if (rSNlist.Contains(item))
                        templist.Add(item);
                }
                RSNlist = templist;
            }
            if (!string.IsNullOrEmpty(InformantUserID)) //報修人員
            {
                var rSNlist = db.EquipmentReportForm.Where(x => x.InformatUserID == InformantUserID).Select(x => x.RSN).ToList();
                var templist = new List<string>();
                foreach (var item in RSNlist)
                {
                    if (rSNlist.Contains(item))
                        templist.Add(item);
                }
                RSNlist = templist;
            }
            if (!string.IsNullOrEmpty(AuditUserID)) //審核人員
            {
                var iPRSNlist = db.RepairAuditInfo.Where(x => x.AuditUserID == AuditUserID).Select(x => x.IPRSN).ToList();
                var templist = new List<string>();
                foreach (var item in IPRSNlist)
                {
                    if (iPRSNlist.Contains(item))
                        templist.Add(item);
                }
                IPRSNlist = templist;
            }

            if (!string.IsNullOrEmpty(DateSelect)) //日期項目選擇
            {
                if (!string.IsNullOrEmpty(DateFrom)) //日期(起)
                {
                    var datefrom = DateTime.Parse(DateFrom);
                    switch (DateSelect)
                    {
                        case "1":
                            var iPSNlist = db.InspectionPlan.Where(x => x.PlanDate >= datefrom).Select(x => x.IPSN).ToList();
                            var templist = new List<string>();
                            foreach (var item in IPSNlist)
                            {
                                if (iPSNlist.Contains(item))
                                    templist.Add(item);
                            }
                            IPSNlist = templist;
                            break;
                        case "2":
                            var rSNlist = db.EquipmentReportForm.Where(x => x.Date >= datefrom).Select(x => x.RSN).ToList();
                            var templist2 = new List<string>();
                            foreach (var item in RSNlist)
                            {
                                if (rSNlist.Contains(item))
                                    templist2.Add(item);
                            }
                            RSNlist = templist2;
                            break;
                        case "3":
                            var iPRSNlist = db.RepairAuditInfo.Where(x => x.AuditDate >= datefrom).Select(x => x.IPRSN).ToList();
                            var templist3 = new List<string>();
                            foreach (var item in IPRSNlist)
                            {
                                if (iPRSNlist.Contains(item))
                                    templist3.Add(item);
                            }
                            IPRSNlist = templist3;
                            break;

                    }
                }
                if (!string.IsNullOrEmpty(DateTo)) //日期(迄)
                {
                    var dateto = DateTime.Parse(DateTo).AddDays(1);
                    switch (DateSelect)
                    {
                        case "1":
                            var iPSNlist = db.InspectionPlan.Where(x => x.PlanDate < dateto).Select(x => x.IPSN).ToList();
                            var templist = new List<string>();
                            foreach (var item in IPSNlist)
                            {
                                if (iPSNlist.Contains(item))
                                    templist.Add(item);
                            }
                            IPSNlist = templist;
                            break;
                        case "2":
                            var rSNlist = db.EquipmentReportForm.Where(x => x.Date < dateto).Select(x => x.RSN).ToList();
                            var templist2 = new List<string>();
                            foreach (var item in RSNlist)
                            {
                                if (rSNlist.Contains(item))
                                    templist2.Add(item);
                            }
                            RSNlist = templist2;
                            break;
                        case "3":
                            var iPRSNlist = db.RepairAuditInfo.Where(x => x.AuditDate < dateto).Select(x => x.IPRSN).ToList();
                            var templist3 = new List<string>();
                            foreach (var item in IPRSNlist)
                            {
                                if (iPRSNlist.Contains(item))
                                    templist3.Add(item);
                            }
                            IPRSNlist = templist3;
                            break;
                    }
                }
            }

            /////////  where 剩下的條件
            DataSource = DataSource.Where(x => RSNlist.Contains(x.RSN)).Where(x => IPSNlist.Contains(x.IPSN)).Where(x => IPRSNlist.Contains(x.IPRSN)).Where(x => x.RepairState != "1");

            if (!string.IsNullOrEmpty(IPSN)) //巡檢計畫編號
            {
                DataSource = DataSource.Where(x => x.IPSN == IPSN);
            }
            if (!string.IsNullOrEmpty(RepairState)) //維修單狀態   
            {
                DataSource = DataSource.Where(x => x.RepairState == RepairState);
            }
            if (!string.IsNullOrEmpty(RSN)) //報修單號
            {
                DataSource = DataSource.Where(x => x.RSN == RSN);
            }
            if (!string.IsNullOrEmpty(RepairUserID)) //施工人員 
            {
                DataSource = DataSource.Where(x => x.RepairUserID == RepairUserID);
            }
            #endregion

            //排序資料表
            var result = DataSource.OrderByDescending(x => x.IPRSN).AsQueryable();
            //回傳JSON陣列
            JArray ja = new JArray();
            //記住總筆數
            int total = result.Count();
            //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
            result = result.Skip((page - 1) * rows).Take(rows);

            #region 塞資料
            //建Json格式資料表回傳給前端
            foreach (var a in DataSource)
            {
                var InspectionPlan_ = db.InspectionPlan.Where(x => x.IPSN == a.IPSN).FirstOrDefault() == null ? new InspectionPlan() : db.InspectionPlan.Where(x => x.IPSN == a.IPSN).FirstOrDefault();
                var EquipmentReportForm_ = db.EquipmentReportForm.Where(x => x.RSN == a.RSN).FirstOrDefault() == null ? new EquipmentReportForm() : db.EquipmentReportForm.Where(x => x.RSN == a.RSN).FirstOrDefault();
                var EquipmentInfo_ = db.EquipmentInfo.Where(x => x.ESN == EquipmentReportForm_.ESN).FirstOrDefault() == null ? new EquipmentInfo() : db.EquipmentInfo.Where(x => x.ESN == EquipmentReportForm_.ESN).FirstOrDefault();
                var AspNetUsers_Informant = db.AspNetUsers.Where(x => x.UserName == EquipmentReportForm_.InformatUserID).FirstOrDefault() == null ? new AspNetUsers(): db.AspNetUsers.Where(x => x.UserName == EquipmentReportForm_.InformatUserID).FirstOrDefault();
                var AspNetUsers_Repair = db.AspNetUsers.Where(x => x.UserName == a.RepairUserID).FirstOrDefault() == null? new AspNetUsers(): db.AspNetUsers.Where(x => x.UserName == a.RepairUserID).FirstOrDefault();
                var RepairAuditInfo_ = db.RepairAuditInfo.Where(x => x.IPRSN == a.IPRSN).FirstOrDefault() == null? new RepairAuditInfo(): db.RepairAuditInfo.Where(x => x.IPRSN == a.IPRSN).FirstOrDefault();
                string id = RepairAuditInfo_.AuditUserID;
                var AspNetUsers_Audit = db.AspNetUsers.Where(x => x.UserName == id).FirstOrDefault() == null? new AspNetUsers(): db.AspNetUsers.Where(x => x.UserName == id).FirstOrDefault();

                var itemObjects = new JObject();
                itemObjects.Add("IPRSN", a.IPRSN);
                itemObjects.Add("IPSN", a.IPSN);
                itemObjects.Add("IPName", InspectionPlan_.IPName);
                itemObjects.Add("PlanDate", InspectionPlan_.PlanDate.ToString("yyyy/MM/dd"));

                var dic = Surface.InspectionPlanRepairState();
                itemObjects.Add("RepairState", dic[a.RepairState.Trim()]); //這個要再用 Surface 做中文轉譯!!

                var dicLevel = Surface.ReportLevel();
                itemObjects.Add("ReportLevel", dicLevel[EquipmentReportForm_.ReportLevel.Trim()]);
                itemObjects.Add("Area", EquipmentInfo_.Area);
                itemObjects.Add("Floor", EquipmentInfo_.Floor);
                itemObjects.Add("RSN", a.RSN);
                itemObjects.Add("Date", EquipmentReportForm_.Date.ToString("yyyy/MM/dd HH:mm:ss"));
                itemObjects.Add("PropertyCode", EquipmentInfo_.PropertyCode);
                itemObjects.Add("ESN", EquipmentReportForm_.ESN);
                itemObjects.Add("EName", EquipmentInfo_.EName);
                itemObjects.Add("ReportContent", EquipmentReportForm_.ReportContent);
                itemObjects.Add("InformantUserID", AspNetUsers_Informant.MyName);
                itemObjects.Add("RepairUserID", AspNetUsers_Repair.MyName);
                itemObjects.Add("AuditUserID", AspNetUsers_Audit.MyName);
                itemObjects.Add("AuditDate", RepairAuditInfo_.AuditDate.ToString("yyyy/M/d") == "0001/1/1"? "": RepairAuditInfo_.AuditDate.ToString("yyyy/M/d"));

                ja.Add(itemObjects);
            }
            #endregion

            JObject jo = new JObject();
            jo.Add("rows", ja);
            jo.Add("total", total);
            return jo;
        }


        public JObject GetJsonForGrid_EquipmentMaintainPeriod_Management(System.Web.Mvc.FormCollection form)
        {
            #region datagrid呼叫時的預設參數有 rows 跟 page
            int page = 1;
            if (!string.IsNullOrEmpty(form["page"]?.ToString()))
            {
                page = short.Parse(form["page"].ToString());
            }
            int rows = 10;
            if (!string.IsNullOrEmpty(form["rows"]?.ToString()))
            {
                rows = short.Parse(form["rows"]?.ToString());
            }
            #endregion


            //塞來自formdata的資料
            //棟別名稱
            string Area = form["Area"]?.ToString();
            //棟別編號
            string ASN = form["ASN"]?.ToString();
            //樓層名稱
            string Floor = form["Floor"]?.ToString();
            //樓層編號
            string FSN = form["FSN"]?.ToString();
            //系統別
            string System = form["System"]?.ToString();
            //子系統別
            string SubSystem = form["SubSystem"]?.ToString();
            //設備編號
            string ESN = form["ESN"]?.ToString();
            //設備名稱
            string EName = form["EName"]?.ToString();
            //啟用狀態
            string IsEnable = form["IsEnable"]?.ToString();


            #region 依據查詢字串檢索資料表

            var SourceTable = from x1 in db.EquipmentMaintainItem
                              join x2 in db.EquipmentInfo on x1.ESN equals x2.ESN
                              join x3 in db.MaintainItem on x1.MISN equals x3.MISN
                              select new { x1.EMISN, x1.IsEnable, x2.Area, x2.Floor, x2.System, x2.SubSystem, x1.ESN, x2.EName, x1.MISN, x3.MIName, x1.Unit, x1.Period, x1.LastTime, x1.NextTime, x2.EState };

            //Area查詢table方式 以Area至表[設備資訊]查詢出ESN，再以ESN至表[設備報修單]查詢出相關報修單。
            if (!string.IsNullOrEmpty(Area))
            {
                SourceTable = SourceTable.Where(x => x.Area == Area);
            }
            if (!string.IsNullOrEmpty(Floor))
            {
                SourceTable = SourceTable.Where(x => x.Floor == Floor);
            }
            if (!string.IsNullOrEmpty(System))
            {
                SourceTable = SourceTable.Where(x => x.System == System);
            }
            if (!string.IsNullOrEmpty(SubSystem))
            {
                SourceTable = SourceTable.Where(x => x.SubSystem == SubSystem);
            }
            if (!string.IsNullOrEmpty(ESN))
            {
                SourceTable = SourceTable.Where(x => x.ESN == ESN);
            }
            if (!string.IsNullOrEmpty(EName))
            {
                SourceTable = SourceTable.Where(x => x.EName.Contains(EName));
            }
            if (!string.IsNullOrEmpty(IsEnable))
            {
                SourceTable = SourceTable.Where(x => x.IsEnable == IsEnable);
            }
            SourceTable = SourceTable.Where(x => x.IsEnable != "2");
            //var atable_ESN_list = db.EquipmentInfo.Where(x => x.Area == Area).Select(x=>x.ESN).ToList();
            //var atable_SearchTable = db.EquipmentReportForm.Where(x=> atable_ESN_list.Contains(x.ESN));
            #endregion
            var resulttable = SourceTable.OrderBy(x => x.EName).AsQueryable();
            //回傳JSON陣列
            JArray ja = new JArray();
            //記住總筆數
            int total = resulttable.Count();
            //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
            resulttable = resulttable.Skip((page - 1) * rows).Take(rows);


            foreach (var a in resulttable)
            {
                var itemObjects = new JObject();
                if (itemObjects["EMISN"] == null)
                {
                    //string aaaaa = dic["8"];
                    itemObjects.Add("EMISN", a.EMISN);
                }
                if (itemObjects["IsEnable"] == null)
                {
                    //var dic = Surface.MaintainItemIsEnable();
                    //string aaaaa = dic["8"];
                    int Enabled = Int16.Parse(a.IsEnable);
                    itemObjects.Add("IsEnable", Enabled); //啟用狀態
                }
                if (itemObjects["Area"] == null)
                    itemObjects.Add("Area", a.Area);    //棟別                           

                if (itemObjects["Floor"] == null)
                    itemObjects.Add("Floor", a.Floor);   //樓層

                if (itemObjects["System"] == null)
                {
                    itemObjects.Add("System", a.System); //系統別
                }
                if (itemObjects["SubSystem"] == null)
                {
                    itemObjects.Add("SubSystem", a.SubSystem); //子系統別
                }
                if (itemObjects["ESN"] == null)
                    itemObjects.Add("ESN", a.ESN);    //設備編號
                if (itemObjects["EState"] == null)
                {
                    var dic = Surface.EState();
                    itemObjects.Add("EState", dic[a.EState]);    //設備狀態
                }
                if (itemObjects["EName"] == null)
                    itemObjects.Add("EName", a.EName);    //設備名稱
                if (itemObjects["MISN"] == null)
                    itemObjects.Add("MISN", a.MISN);    //保養項目編號
                if (itemObjects["MIName"] == null)
                    itemObjects.Add("MIName", a.MIName);    //項目名稱
                if (itemObjects["Unit"] == null)
                    itemObjects.Add("Unit", a.Unit);    //保養週期單位
                if (itemObjects["Period"] == null)
                    itemObjects.Add("Period", a.Period);    //保養週期
                if (itemObjects["LastTime"] == null)
                    itemObjects.Add("LastTime", a.LastTime?.ToString("yyyy/MM/dd"));    //上次保養日期
                if (itemObjects["NextTime"] == null)
                    itemObjects.Add("NextTime", a.NextTime?.ToString("yyyy/MM/dd"));    //最近應保養日期
                ja.Add(itemObjects);
            }

            JObject jo = new JObject();
            jo.Add("rows", ja);
            jo.Add("total", total);
            return jo;
        }

    }
}