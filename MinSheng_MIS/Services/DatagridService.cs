using MinSheng_MIS.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        // 使用 Expression Tree 實現動態排序的方法
        static IQueryable<T> OrderByField<T>(IQueryable<T> query, string propertyName, bool isAscending)
        {
            var entityType = typeof(T);
            var property = entityType.GetProperty(propertyName);
            var parameter = Expression.Parameter(entityType, "x");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExp = Expression.Lambda(propertyAccess, parameter);

            string methodName = isAscending ? "OrderBy" : "OrderByDescending";
            var resultExp = Expression.Call(typeof(Queryable), methodName,
                new Type[] { entityType, property.PropertyType },
                query.Expression, Expression.Quote(orderByExp));

            return query.Provider.CreateQuery<T>(resultExp);
        }

        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();

        #region 報修管理
        public JObject GetJsonForGrid_Report_Management(System.Web.Mvc.FormCollection form)
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
            //財產編碼
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
            //庫存狀態
            string StockState = form["StockState"]?.ToString();


            #region 依據查詢字串檢索資料表
            var SourceTable = from x1 in db.EquipmentReportForm
                              join x2 in db.EquipmentInfo on x1.ESN equals x2.ESN
                              join x3 in db.AspNetUsers on x1.InformatUserID equals x3.UserName
                              join x4 in db.Floor_Info on x2.FSN equals x4.FSN
                              select new { x1.ReportState, x1.ReportLevel, x2.Area, x2.Floor, x1.ReportSource, x1.RSN, x1.Date, x2.PropertyCode, x1.ESN, x2.EName, x1.ReportContent, x3.MyName, x3.UserName, x2.EState, x1.StockState, x2.DBID, x2.FSN, x4.ASN };

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
            if (!string.IsNullOrEmpty(StockState))
            {
                switch (StockState)
                {
                    case "0":
                        SourceTable = SourceTable.Where(x => x.StockState == false);
                        break;
                    case "1":
                        SourceTable = SourceTable.Where(x => x.StockState == true);
                        break;
                }
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
                    itemObjects.Add("ReportStatenum", statsSN); //報修單狀態編碼
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
                    itemObjects.Add("PropertyCode", a.PropertyCode);    //財產編碼
                if (itemObjects["ESN"] == null)
                    itemObjects.Add("ESN", a.ESN);    //設備編號
                if (itemObjects["EName"] == null)
                    itemObjects.Add("EName", a.EName);    //設備名稱
                if (itemObjects["ReportContent"] == null)
                    itemObjects.Add("ReportContent", a.ReportContent);    //報修內容
                if (itemObjects["MyName"] == null)
                    itemObjects.Add("MyName", a.MyName);    //報修人員
                if (a.StockState) //庫存狀態
                {
                    itemObjects.Add("StockState", "有");
                }
                else
                {
                    itemObjects.Add("StockState", "無");
                }
                ja.Add(itemObjects);
                if (!string.IsNullOrEmpty(a.EState))
                {
                    var dic = Surface.EState();
                    itemObjects.Add("EState", dic[a.EState]);
                }
                if (!string.IsNullOrEmpty(a.DBID.ToString()))
                {
                    itemObjects.Add("DBID", a.DBID);
                }
                if (!string.IsNullOrEmpty(a.FSN.ToString()))
                {
                    itemObjects.Add("FSN", a.FSN);
                }
                if (!string.IsNullOrEmpty(a.ASN.ToString()))
                {
                    itemObjects.Add("ASN", a.ASN);
                }
            }

            JObject jo = new JObject();
            jo.Add("rows", ja);
            jo.Add("total", total);
            return jo;
        }
        #endregion

        #region 巡檢保養紀錄管理
        public JObject GetJsonForGrid_MaintainRecord_Management(System.Web.Mvc.FormCollection form)
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
            //財產編碼
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
            if (!string.IsNullOrEmpty(PropertyCode)) //財產編碼
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
            foreach (var a in result)
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
                itemObjects.Add("DBID", EquipmentInfo_.DBID);
                ja.Add(itemObjects);
            }
            #endregion

            JObject jo = new JObject();
            jo.Add("rows", ja);
            jo.Add("total", total);
            return jo;
        }
        #endregion

        #region 巡檢維修紀錄管理
        public JObject GetJsonForGrid_RepairRecord_Management(System.Web.Mvc.FormCollection form)
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
            //財產編碼
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
            if (!string.IsNullOrEmpty(PropertyCode)) //財產編碼
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
            foreach (var a in result)
            {
                var InspectionPlan_ = db.InspectionPlan.Where(x => x.IPSN == a.IPSN).FirstOrDefault() == null ? new InspectionPlan() : db.InspectionPlan.Where(x => x.IPSN == a.IPSN).FirstOrDefault();
                var EquipmentReportForm_ = db.EquipmentReportForm.Where(x => x.RSN == a.RSN).FirstOrDefault() == null ? new EquipmentReportForm() : db.EquipmentReportForm.Where(x => x.RSN == a.RSN).FirstOrDefault();
                var EquipmentInfo_ = db.EquipmentInfo.Where(x => x.ESN == EquipmentReportForm_.ESN).FirstOrDefault() == null ? new EquipmentInfo() : db.EquipmentInfo.Where(x => x.ESN == EquipmentReportForm_.ESN).FirstOrDefault();
                var AspNetUsers_Informant = db.AspNetUsers.Where(x => x.UserName == EquipmentReportForm_.InformatUserID).FirstOrDefault() == null ? new AspNetUsers() : db.AspNetUsers.Where(x => x.UserName == EquipmentReportForm_.InformatUserID).FirstOrDefault();
                var AspNetUsers_Repair = db.AspNetUsers.Where(x => x.UserName == a.RepairUserID).FirstOrDefault() == null ? new AspNetUsers() : db.AspNetUsers.Where(x => x.UserName == a.RepairUserID).FirstOrDefault();
                var RepairAuditInfo_ = db.RepairAuditInfo.Where(x => x.IPRSN == a.IPRSN).FirstOrDefault() == null ? new RepairAuditInfo() : db.RepairAuditInfo.Where(x => x.IPRSN == a.IPRSN).FirstOrDefault();
                string id = RepairAuditInfo_.AuditUserID;
                var AspNetUsers_Audit = db.AspNetUsers.Where(x => x.UserName == id).FirstOrDefault() == null ? new AspNetUsers() : db.AspNetUsers.Where(x => x.UserName == id).FirstOrDefault();

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
                itemObjects.Add("AuditDate", RepairAuditInfo_.AuditDate.ToString("yyyy/M/d") == "0001/1/1" ? "" : RepairAuditInfo_.AuditDate.ToString("yyyy/M/d"));
                itemObjects.Add("DBID", EquipmentInfo_.DBID);
                ja.Add(itemObjects);
            }
            #endregion

            JObject jo = new JObject();
            jo.Add("rows", ja);
            jo.Add("total", total);
            return jo;
        }
        #endregion

        #region 設備保養週期管理
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
                              select new { x1.EMISN, x1.IsEnable, x2.Area, x2.Floor, x2.System, x2.SubSystem, x1.ESN, x2.EName, x1.MISN, x3.MIName, x1.Unit, x1.Period, x1.LastTime, x1.NextTime, x2.EState, x2.DBID };

            //設備狀態為3(停用) 不顯示
            SourceTable = SourceTable.Where(x => x.EState != "3");

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
                //DBID
                if (!string.IsNullOrEmpty(a.DBID.ToString()))
                {
                    itemObjects.Add("DBID", a.DBID);
                }
                ja.Add(itemObjects);
            }

            JObject jo = new JObject();
            jo.Add("rows", ja);
            jo.Add("total", total);
            return jo;
        }
        #endregion

        #region 帳號管理
        public JObject GetJsonForGrid_Account_Management(System.Web.Mvc.FormCollection form)
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
            //帳號
            string UserName = form["UserName"]?.ToString();
            //姓名
            string MyName = form["MyName"]?.ToString();
            //權限
            string Authority = form["Authority"]?.ToString();
            //信箱
            string Email = form["Email"]?.ToString();
            //電話
            string PhoneNumber = form["PhoneNumber"]?.ToString();
            //單位
            string Apartment = form["Apartment"]?.ToString();
            //職稱
            string Title = form["Title"]?.ToString();
            #endregion

            #region 依據查詢字串檢索資料表
            var Data = db.AspNetUsers.Where(x => x.IsEnabled == true).AsQueryable();

            if (!string.IsNullOrEmpty(UserName))
            {
                Data = Data.Where(x => x.UserName.Contains(UserName));
            }
            if (!string.IsNullOrEmpty(MyName))
            {
                Data = Data.Where(x => x.MyName.Contains(MyName));
            }
            if (!string.IsNullOrEmpty(Authority))
            {
                Data = Data.Where(x => x.Authority == Authority);
            }
            if (!string.IsNullOrEmpty(Email))
            {
                Data = Data.Where(x => x.Email.Contains(Email));
            }
            if (!string.IsNullOrEmpty(PhoneNumber))
            {
                Data = Data.Where(x => x.PhoneNumber.Contains(PhoneNumber));
            }
            if (!string.IsNullOrEmpty(Apartment))
            {
                Data = Data.Where(x => x.Apartment.Contains(Apartment));
            }
            if (!string.IsNullOrEmpty(Title))
            {
                Data = Data.Where(x => x.Title.Contains(Title));
            }
            #endregion

            //排序資料表
            var result = Data.OrderByDescending(x => x.UserName).AsQueryable();
            //回傳JSON陣列
            JArray ja = new JArray();
            //記住總筆數
            int total = result.Count();
            //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
            result = result.Skip((page - 1) * rows).Take(rows);

            var Dic = Surfaces.Surface.Authority();

            foreach (var item in result)
            {
                var itemObjects = new JObject();
                itemObjects.Add("UserName", item.UserName);
                itemObjects.Add("MyName", item.MyName);
                itemObjects.Add("Authority", Dic[item.Authority]);
                itemObjects.Add("Email", item.Email);
                itemObjects.Add("PhoneNumber", item.PhoneNumber);
                itemObjects.Add("Apartment", item.Apartment);
                itemObjects.Add("Title", item.Title);
                ja.Add(itemObjects);
            }

            JObject jo = new JObject();
            jo.Add("rows", ja);
            jo.Add("total", total);
            return jo;
        }
        #endregion

        #region 巡檢紀錄_設備保養紀錄
        public string GetJsonForGrid_InspectationPlan_Record_EquipMaintain(System.Web.Mvc.FormCollection form)
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

            string IPSN = form["IPSN"].ToString();

            var SourceTable = db.InspectionPlanMaintain.Where(x => x.IPSN == IPSN);

            var resulttable = SourceTable.OrderByDescending(x => x.EMFISN).AsQueryable();
            //回傳JSON陣列
            JArray ja = new JArray();
            //記住總筆數
            int total = resulttable.Count();
            //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
            resulttable = resulttable.Skip((page - 1) * rows).Take(rows);

            foreach (var item in resulttable)
            {
                var itemObjects = new JObject();
                itemObjects.Add("IPMSN", item.IPMSN);
                itemObjects.Add("IPSN", IPSN);
                var dic_IPMS = Surfaces.Surface.InspectionPlanMaintainState();
                itemObjects.Add("MaintainState", dic_IPMS[item.MaintainState]);
                var EMFI = db.EquipmentMaintainFormItem.Find(item.EMFISN);
                var EMI = db.EquipmentMaintainItem.Find(EMFI.EMISN);
                var EI = db.EquipmentInfo.Find(EMI.ESN);
                var MI = db.MaintainItem.Find(EMI.MISN);
                itemObjects.Add("Area", EI.Area);
                itemObjects.Add("Floor", EI.Floor);
                itemObjects.Add("ESN", EI.ESN);
                var dic_EState = Surfaces.Surface.EState();
                itemObjects.Add("EState", dic_EState[EI.EState]);
                itemObjects.Add("EName", EI.EName);
                itemObjects.Add("EMFISN", item.EMFISN);
                itemObjects.Add("MIName", MI.MIName);
                itemObjects.Add("Unit", EMFI.Unit);
                itemObjects.Add("Period", EMFI.Period.ToString());
                itemObjects.Add("LastTime", EMFI.LastTime.ToString("yyyy/MM/dd"));
                itemObjects.Add("NextTime", EMFI.NextTime?.ToString("yyyy/MM/dd"));
                ja.Add(itemObjects);
            }

            JObject jo = new JObject();
            jo.Add("rows", ja);
            jo.Add("total", total);
            string reString = JsonConvert.SerializeObject(jo);
            return reString;
        }
        #endregion

        #region 巡檢紀錄_設備維修紀錄
        public string GetJsonForGrid_InspectationPlan_Record_EquipRepair(System.Web.Mvc.FormCollection form)
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

            string IPSN = form["IPSN"].ToString();

            var SourceTable = db.InspectionPlanRepair.Where(x => x.IPSN == IPSN);

            var resulttable = SourceTable.OrderByDescending(x => x.IPRSN).AsQueryable();
            //回傳JSON陣列
            JArray ja = new JArray();
            //記住總筆數
            int total = resulttable.Count();
            //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
            resulttable = resulttable.Skip((page - 1) * rows).Take(rows);


            foreach (var item in resulttable)
            {
                var itemObjects = new JObject();
                itemObjects.Add("IPRSN", item.IPRSN);
                itemObjects.Add("IPSN", IPSN);
                var dic_IPRS = Surfaces.Surface.InspectionPlanRepairState();
                itemObjects.Add("RepairState", dic_IPRS[item.RepairState]);
                var ERF = db.EquipmentReportForm.Find(item.RSN);
                var EI = db.EquipmentInfo.Find(ERF.ESN);
                var dic_RL = Surfaces.Surface.ReportLevel();
                itemObjects.Add("ReportLevel", dic_RL[ERF.ReportLevel]);
                itemObjects.Add("Area", EI.Area);
                itemObjects.Add("Floor", EI.Floor);
                itemObjects.Add("RSN", item.RSN);
                itemObjects.Add("Date", ERF.Date.ToString("yyyy/MM/dd HH:mm:ss"));
                itemObjects.Add("ESN", ERF.ESN);
                itemObjects.Add("EName", EI.EName);
                var Name = db.AspNetUsers.Where(x => x.UserName == ERF.InformatUserID).Select(x => x.MyName).FirstOrDefault();
                itemObjects.Add("InformantUserID", Name);
                itemObjects.Add("ReportContent", ERF.ReportContent);
                ja.Add(itemObjects);
            }


            JObject jo = new JObject();
            jo.Add("rows", ja);
            jo.Add("total", total);
            string reString = JsonConvert.SerializeObject(jo);
            return reString;
        }
        #endregion

        #region 廠商管理
        public JObject GetJsonForGrid_ManufacturerInfo_Management(System.Web.Mvc.FormCollection form)
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
            string propertyName = "MFRSN";
            string order = "asc";

            //塞來自formdata的資料
            //廠商名稱
            string MFRName = form["MFRName"]?.ToString();
            //聯絡人
            string ContactPerson = form["ContactPerson"]?.ToString();
            //電話
            string MFRTelNO = form["MFRTelNO"]?.ToString();
            //手機
            string MFRMBPhone = form["MFRMBPhone"]?.ToString();
            //主要商品
            string MFRMainProduct = form["MFRMainProduct"]?.ToString();
            //地址
            string MFRAddress = form["MFRAddress"]?.ToString();

            #region 依據查詢字串檢索資料表
            var SourceTable = db.ManufacturerInfo.AsQueryable();

            if (!string.IsNullOrEmpty(MFRName))
            {
                SourceTable = SourceTable.Where(x => x.MFRName.Contains(MFRName));
            }
            if (!string.IsNullOrEmpty(ContactPerson))
            {
                SourceTable = SourceTable.Where(x => x.ContactPerson.Contains(ContactPerson));
            }
            if (!string.IsNullOrEmpty(MFRTelNO))
            {
                SourceTable = SourceTable.Where(x => x.MFRTelNO.Contains(MFRTelNO));
            }
            if (!string.IsNullOrEmpty(MFRMBPhone))
            {
                SourceTable = SourceTable.Where(x => x.MFRMBPhone.Contains(MFRMBPhone));
            }
            if (!string.IsNullOrEmpty(MFRMainProduct))
            {
                SourceTable = SourceTable.Where(x => x.MFRMainProduct.Contains(MFRMainProduct));
            }
            if (!string.IsNullOrEmpty(MFRAddress))
            {
                SourceTable = SourceTable.Where(x => x.MFRAddress.Contains(MFRAddress));
            }

            #endregion
            var resulttable = SourceTable.OrderByDescending(x => x.MFRSN).AsQueryable();
            //回傳JSON陣列
            JArray ja = new JArray();
            //記住總筆數
            int total = resulttable.Count();
            //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
            resulttable = resulttable.Skip((page - 1) * rows).Take(rows);


            foreach (var a in resulttable)
            {
                var itemObjects = new JObject();
                itemObjects.Add("MFRSN", a.MFRSN);

                if (!string.IsNullOrEmpty(a.MFRName))
                {
                    itemObjects.Add("MFRName", a.MFRName);
                }
                if (!string.IsNullOrEmpty(a.ContactPerson))
                {
                    itemObjects.Add("ContactPerson", a.ContactPerson);
                }
                if (!string.IsNullOrEmpty(a.MFRTelNO))
                {
                    itemObjects.Add("MFRTelNO", a.MFRTelNO);
                }
                if (!string.IsNullOrEmpty(a.MFRMBPhone))
                {
                    itemObjects.Add("MFRMBPhone", a.MFRMBPhone);
                }
                if (!string.IsNullOrEmpty(a.MFRMainProduct))
                {
                    itemObjects.Add("MFRMainProduct", a.MFRMainProduct);
                }
                if (!string.IsNullOrEmpty(a.MFREmail))
                {
                    itemObjects.Add("MFREmail", a.MFREmail);
                }
                if (!string.IsNullOrEmpty(a.MFRAddress))
                {
                    itemObjects.Add("MFRAddress", a.MFRAddress);
                }
                if (!string.IsNullOrEmpty(a.MFRWeb))
                {
                    itemObjects.Add("MFRWeb", a.MFRWeb);
                }
                ja.Add(itemObjects);
            }

            JObject jo = new JObject();
            jo.Add("rows", ja);
            jo.Add("total", total);
            return jo;
        }
        #endregion

        #region 定期保養管理
        public JObject GetJsonForGrid_MaintainForm(System.Web.Mvc.FormCollection form)
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
            //string propertyName = "PSSN";
            //string order = "asc";

            //塞來自formdata的資料
            //棟別編號
            string ASN = form["ASN"]?.ToString();
            //樓層編號
            string FSN = form["FSN"]?.ToString();
            //保養項目狀態
            string FormItemState = form["FormItemState"]?.ToString();
            //財產編碼
            string PropertyCode = form["PropertyCode"]?.ToString();
            //設備編號
            string ESN = form["ESN"]?.ToString();
            //設備名稱
            string EName = form["EName"]?.ToString();
            //保養項目編號
            string MISN = form["MISN"]?.ToString();
            //保養項目
            string MIName = form["MIName"]?.ToString();
            //日期項目選擇
            string DateSelect = form["DateSelect"]?.ToString();
            //日期(起)
            string DateFrom = form["DateFrom"]?.ToString();
            //日期(迄)
            string DateTo = form["DateTo"]?.ToString();
            //判斷是從哪裡來的請求DataGrid
            string SourceMaintain = form["SourceMaintain"]?.ToString();
            //庫存狀態
            string StockState = form["StockState"]?.ToString();
            //設備狀態
            string EState = form["EState"]?.ToString();


            #region 依據查詢字串檢索資料表
            var SourceTable = from x1 in db.EquipmentMaintainFormItem
                              join x2 in db.EquipmentMaintainItem on x1.EMISN equals x2.EMISN
                              join x3 in db.EquipmentInfo on x2.ESN equals x3.ESN
                              join x4 in db.MaintainItem on x2.MISN equals x4.MISN
                              join x5 in db.Floor_Info on x3.FSN equals x5.FSN
                              join x6 in db.AreaInfo on x5.ASN equals x6.ASN
                              select new { x1.FormItemState, x6.Area, x5.FloorName, x3.PropertyCode, x3.EName, x1.EMFISN, x4.MIName, x1.Unit, x1.Period, x1.LastTime, x1.Date, x5.ASN, x3.FSN, x2.ESN, x2.MISN, x3.EState, x1.StockState, x3.DBID };

            //若是用於新增巡檢計畫 的 新增保養單項目需增加狀態判斷
            if (SourceMaintain == "AddMaintainForm")
            {
                //增加狀態判斷
                SourceTable = SourceTable.Where(x => x.FormItemState == "1" || x.FormItemState == "5" || x.FormItemState == "8" || x.FormItemState == "9" || x.FormItemState == "10" || x.FormItemState == "11");
                //設備若停用則不能加入巡檢計畫中
                SourceTable = SourceTable.Where(x => x.EState != "3");
            }

            //查詢棟別
            if (!string.IsNullOrEmpty(ASN))
            {
                int IntASN = Convert.ToInt32(ASN);
                SourceTable = SourceTable.Where(x => x.ASN == IntASN);
            }
            //查詢樓層
            if (!string.IsNullOrEmpty(FSN))
            {
                SourceTable = SourceTable.Where(x => x.FSN == FSN);
            }
            //查詢保養項目狀態
            if (!string.IsNullOrEmpty(FormItemState))
            {
                SourceTable = SourceTable.Where(x => x.FormItemState == FormItemState);
            }
            //查詢財產編碼
            if (!string.IsNullOrEmpty(PropertyCode))
            {
                SourceTable = SourceTable.Where(x => x.PropertyCode == PropertyCode);
            }
            //查詢設備編號
            if (!string.IsNullOrEmpty(ESN))
            {
                SourceTable = SourceTable.Where(x => x.ESN == ESN);
            }
            //查詢設備名稱 模糊查詢
            if (!string.IsNullOrEmpty(EName))
            {
                SourceTable = SourceTable.Where(x => x.EName.Contains(EName));
            }
            //查詢保養項目編號
            if (!string.IsNullOrEmpty(MISN))
            {
                SourceTable = SourceTable.Where(x => x.MISN == MISN);
            }
            //查詢保養項目 模糊查詢
            if (!string.IsNullOrEmpty(MIName))
            {
                SourceTable = SourceTable.Where(x => x.MIName.Contains(MIName));
            }
            //查詢日期
            if (!string.IsNullOrEmpty(DateSelect))
            {
                if (!string.IsNullOrEmpty(DateFrom))
                {
                    var datefrom = DateTime.Parse(DateFrom);
                    if (DateSelect == "上次保養日期")
                    {
                        SourceTable = SourceTable.Where(x => x.LastTime >= datefrom);
                    }
                    else if (DateSelect == "最近應保養日期")
                    {
                        SourceTable = SourceTable.Where(x => x.Date >= datefrom);
                    }
                }
                if (!string.IsNullOrEmpty(DateTo))
                {
                    var dateto = DateTime.Parse(DateTo).AddDays(1);
                    if (DateSelect == "上次保養日期")
                    {
                        SourceTable = SourceTable.Where(x => x.LastTime < dateto);
                    }
                    else if (DateSelect == "最近應保養日期")
                    {
                        SourceTable = SourceTable.Where(x => x.Date < dateto);
                    }
                }
            }
            //查詢庫存狀態
            if (!string.IsNullOrEmpty(StockState))
            {
                switch (StockState)
                {
                    case "0":
                        SourceTable = SourceTable.Where(x => x.StockState == false);
                        break;
                    case "1":
                        SourceTable = SourceTable.Where(x => x.StockState == true);
                        break;
                }
            }
            //查詢設備狀態
            if (!string.IsNullOrEmpty(EState))
            {
                SourceTable = SourceTable.Where(x => x.EState == EState);
            }
            #endregion

            SourceTable = SourceTable.OrderByDescending(x => x.Date);

            //回傳JSON陣列
            JArray ja = new JArray();
            //記住總筆數
            int total = SourceTable.Count();
            //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
            SourceTable = SourceTable.Skip((page - 1) * rows).Take(rows);

            foreach (var item in SourceTable)
            {
                var itemObjects = new JObject();
                //保養項目狀態
                if (!string.IsNullOrEmpty(item.FormItemState))
                {
                    string formitemstate = item.FormItemState.Trim();
                    var dic = Surface.EquipmentMaintainFormItemState();
                    itemObjects.Add("FormItemState", dic[formitemstate]);
                }
                //設備狀態
                if (!string.IsNullOrEmpty(item.EState))
                {
                    var dic = Surface.EState();
                    itemObjects.Add("EState", dic[item.EState]);
                }
                //棟別
                if (!string.IsNullOrEmpty(item.Area))
                {
                    itemObjects.Add("Area", item.Area);
                }
                //樓層
                if (!string.IsNullOrEmpty(item.FloorName))
                {
                    itemObjects.Add("Floor", item.FloorName);
                }
                //財產編碼
                if (!string.IsNullOrEmpty(item.PropertyCode))
                {
                    itemObjects.Add("PropertyCode", item.PropertyCode);
                }
                //設備名稱
                if (!string.IsNullOrEmpty(item.EName))
                {
                    itemObjects.Add("EName", item.EName);
                }
                //保養單項目編號
                if (!string.IsNullOrEmpty(item.EMFISN))
                {
                    itemObjects.Add("EMFISN", item.EMFISN);
                }
                //保養項目
                if (!string.IsNullOrEmpty(item.MIName))
                {
                    itemObjects.Add("MIName", item.MIName);
                }
                //保養週期單位
                if (!string.IsNullOrEmpty(item.Unit))
                {
                    itemObjects.Add("Unit", item.Unit);
                }
                //保養週期
                if (!string.IsNullOrEmpty(item.Period.ToString()))
                {
                    itemObjects.Add("Period", item.Period.ToString());
                }
                //上次保養日期
                if (item.LastTime != DateTime.MinValue && item.LastTime != null)
                {
                    itemObjects.Add("LastTime", item.LastTime.ToString("yyyy/MM/dd"));
                }
                //最近應保養日期
                if (item.Date != DateTime.MinValue && item.Date != null)
                {
                    itemObjects.Add("Date", item.Date.ToString("yyyy/MM/dd"));
                }
                //庫存狀態
                if (item.StockState)
                {
                    itemObjects.Add("StockState", "有");
                }
                else
                {
                    itemObjects.Add("StockState", "無");
                }
                //設備編號
                if (!string.IsNullOrEmpty(item.ESN))
                {
                    itemObjects.Add("ESN", item.ESN);
                }
                //保養項目狀態編碼
                if (!string.IsNullOrEmpty(item.FormItemState))
                {
                    itemObjects.Add("FormItemStatenum", item.FormItemState);
                }
                //DBID
                if (!string.IsNullOrEmpty(item.DBID.ToString()))
                {
                    itemObjects.Add("DBID", item.DBID);
                }
                //ASN
                if (!string.IsNullOrEmpty(item.ASN.ToString()))
                {
                    itemObjects.Add("ASN", item.ASN);
                }
                //FSN
                if (!string.IsNullOrEmpty(item.FSN.ToString()))
                {
                    itemObjects.Add("FSN", item.FSN);
                }

                ja.Add(itemObjects);
            }

            JObject jo = new JObject();
            jo.Add("rows", ja);
            jo.Add("total", total);
            return jo;
        }
        #endregion

        #region 保養項目管理
        public JObject GetJsonForGrid_MaintainItem(System.Web.Mvc.FormCollection form)
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
            //系統別
            string System = form["System"]?.ToString();
            //子系統別
            string SubSystem = form["SubSystem"]?.ToString();
            //設備名稱
            string EName = form["EName"]?.ToString();
            //保養項目名稱
            string MIName = form["MIName"]?.ToString();
            //保養週期單位
            string Unit = form["Unit"]?.ToString();
            //保養週期
            string Period = form["Period"]?.ToString();
            #endregion

            #region 依據查詢字串檢索資料表
            var SourceTable = from x1 in db.MaintainItem
                              select new { x1.MISN, x1.System, x1.SubSystem, x1.EName, x1.MIName, x1.Unit, x1.Period, x1.MaintainItemIsEnable };
            
            SourceTable = SourceTable.Where(x => x.MaintainItemIsEnable == "1");
            if (!string.IsNullOrEmpty(System))
            {
                SourceTable = SourceTable.Where(x => x.System == System);
            }
            if (!string.IsNullOrEmpty(SubSystem))
            {
                SourceTable = SourceTable.Where(x => x.SubSystem == SubSystem);
            }
            if (!string.IsNullOrEmpty(EName))
            {
                SourceTable = SourceTable.Where(x => x.EName.Contains(EName));
            }
            if (!string.IsNullOrEmpty(MIName))
            {
                SourceTable = SourceTable.Where(x => x.MIName.Contains(MIName));
            }
            if (!string.IsNullOrEmpty(Unit))
            {
                SourceTable = SourceTable.Where(x => x.Unit.Contains(Unit));
            }
            if (!string.IsNullOrEmpty(Period))
            {
                bool conversionSuccessful = int.TryParse(Period, out int IntPeriod);
                if (conversionSuccessful)
                {
                    SourceTable = SourceTable.Where(x => x.Period == IntPeriod);
                }
            }
            #endregion

            #region datagrid remoteSort 判斷有無 sort 跟 order
            System.Web.Mvc.IValueProvider vp = form.ToValueProvider();
            if (vp.ContainsPrefix("sort") && vp.ContainsPrefix("order"))
            {
                string sort = form["sort"];
                string order = form["order"];

                if (order == "asc")
                {
                    SourceTable = OrderByField(SourceTable, sort, true);
                }
                else if (order == "desc")
                {
                    SourceTable = OrderByField(SourceTable, sort, false);
                }
            }
            else
            {
                SourceTable = SourceTable.OrderByDescending(x => x.MISN);
            }
            #endregion

            //回傳JSON陣列
            JArray ja = new JArray();
            //記住總筆數
            int total = SourceTable.Count();
            //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
            SourceTable = SourceTable.Skip((page - 1) * rows).Take(rows);

            foreach (var item in SourceTable)
            {
                var itemObjects = new JObject();
                if (!string.IsNullOrEmpty(item.MISN))
                {
                    itemObjects.Add("MISN", item.MISN);
                }
                if (!string.IsNullOrEmpty(item.System))
                {
                    itemObjects.Add("System", item.System);
                }
                if (!string.IsNullOrEmpty(item.SubSystem))
                {
                    itemObjects.Add("SubSystem", item.SubSystem);
                }
                if (!string.IsNullOrEmpty(item.EName))
                {
                    itemObjects.Add("EName", item.EName);
                }
                if (!string.IsNullOrEmpty(item.MIName))
                {
                    itemObjects.Add("MIName", item.MIName);
                }
                if (!string.IsNullOrEmpty(item.Unit))
                {
                    itemObjects.Add("Unit", item.Unit);
                }

                itemObjects.Add("Period", item.Period);

                ja.Add(itemObjects);
            }

            JObject jo = new JObject();
            jo.Add("rows", ja);
            jo.Add("total", total);
            return jo;
        }
        #endregion

        #region 巡檢計畫管理
        public JObject GetJsonForGrid_InspectionPlan(System.Web.Mvc.FormCollection form)
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
            //string propertyName = "PSSN";
            //string order = "asc";

            //塞來自formdata的資料
            //巡檢狀態
            string PlanState = form["PlanState"]?.ToString();
            //計畫編號
            string IPSN = form["IPSN"]?.ToString();
            //巡檢計畫名稱
            string IPName = form["IPName"]?.ToString();
            //巡檢班別
            string Shift = form["Shift"]?.ToString();
            //巡檢人員
            string UserID = form["UserID"]?.ToString();
            //設備編號
            string ESN = form["ESN"]?.ToString();
            //設備名稱
            string EName = form["EName"]?.ToString();
            //日期(起)
            string DateFrom = form["DateFrom"]?.ToString();
            //日期(迄)
            string DateTo = form["DateTo"]?.ToString();


            #region 依據查詢字串檢索資料表
            var SourceTable = db.InspectionPlan.Where(x => x.PlanState != "5").AsQueryable();

            //巡檢狀態
            if (!string.IsNullOrEmpty(PlanState))
            {
                SourceTable = SourceTable.Where(x => x.PlanState == PlanState);
            }
            //計畫編號
            if (!string.IsNullOrEmpty(IPSN))
            {
                SourceTable = SourceTable.Where(x => x.IPSN == IPSN);
            }
            //巡檢計畫名稱
            if (!string.IsNullOrEmpty(IPName))
            {
                SourceTable = SourceTable.Where(x => x.IPName.Contains(IPName));
            }
            //巡檢班別
            if (!string.IsNullOrEmpty(Shift))
            {
                SourceTable = SourceTable.Where(x => x.Shift == Shift);
            }
            //巡檢人員
            if (!string.IsNullOrEmpty(UserID))
            {
                var planlist = db.InspectionPlanMember.Where(x => x.UserID == UserID).Select(x => x.IPSN).ToList();
                SourceTable = SourceTable.Where(x => planlist.Contains(x.IPSN));
            }
            //設備編號
            if (!string.IsNullOrEmpty(ESN))
            {
                var RepairSourceTable = from x1 in db.InspectionPlanRepair
                                        join x2 in db.EquipmentReportForm on x1.RSN equals x2.RSN
                                        where x2.ESN == ESN
                                        select x1.IPSN;
                var MaintainSourceTable = from x1 in db.InspectionPlanMaintain
                                          join x2 in db.EquipmentMaintainFormItem on x1.EMFISN equals x2.EMFISN
                                          join x3 in db.EquipmentMaintainItem on x2.EMISN equals x3.EMISN
                                          where x3.ESN == ESN
                                          select x1.IPSN;
                var IPSNlist = RepairSourceTable.Union(MaintainSourceTable);
                SourceTable = SourceTable.Where(x => IPSNlist.Contains(x.IPSN));
            }
            //設備名稱
            if (!string.IsNullOrEmpty(EName))
            {
                var RepairSourceTable = from x1 in db.InspectionPlanRepair
                                        join x2 in db.EquipmentReportForm on x1.RSN equals x2.RSN
                                        join x3 in db.EquipmentInfo on x2.ESN equals x3.ESN
                                        where x3.EName.Contains(EName)
                                        select x1.IPSN;
                var MaintainSourceTable = from x1 in db.InspectionPlanMaintain
                                          join x2 in db.EquipmentMaintainFormItem on x1.EMFISN equals x2.EMFISN
                                          join x3 in db.EquipmentMaintainItem on x2.EMISN equals x3.EMISN
                                          join x4 in db.EquipmentInfo on x3.ESN equals x4.ESN
                                          where x4.EName.Contains(EName)
                                          select x1.IPSN;
                var IPSNlist = RepairSourceTable.Union(MaintainSourceTable);
                SourceTable = SourceTable.Where(x => IPSNlist.Contains(x.IPSN));
            }
            //日期(起)
            if (!string.IsNullOrEmpty(DateFrom))
            {
                var datefrom = DateTime.Parse(DateFrom);
                SourceTable = SourceTable.Where(x => x.PlanDate >= datefrom);
            }
            //日期(迄)
            if (!string.IsNullOrEmpty(DateTo))
            {
                var dateto = DateTime.Parse(DateTo).AddDays(1);
                SourceTable = SourceTable.Where(x => x.PlanDate < dateto);
            }
            #endregion

            SourceTable = SourceTable.OrderByDescending(x => x.IPSN);

            //回傳JSON陣列
            JArray ja = new JArray();
            //記住總筆數
            int total = SourceTable.Count();
            //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
            SourceTable = SourceTable.Skip((page - 1) * rows).Take(rows);

            foreach (var item in SourceTable)
            {
                var itemObjects = new JObject();
                //巡檢計畫狀態
                if (!string.IsNullOrEmpty(item.PlanState))
                {
                    var dic = Surface.InspectionPlanState();
                    itemObjects.Add("PlanState", dic[item.PlanState]);
                }
                //計畫編號
                if (!string.IsNullOrEmpty(item.IPSN))
                {
                    itemObjects.Add("IPSN", item.IPSN);
                }
                //計畫名稱
                if (!string.IsNullOrEmpty(item.IPName))
                {
                    itemObjects.Add("IPName", item.IPName);
                }
                //計畫日期
                if (item.PlanDate != DateTime.MinValue && item.PlanDate != null)
                {
                    itemObjects.Add("PlanDate", item.PlanDate.ToString("yyyy/MM/dd"));
                }
                //巡檢班別
                if (!string.IsNullOrEmpty(item.Shift))
                {
                    var dic = Surface.Shift();
                    itemObjects.Add("Shift", dic[item.Shift]);
                }
                //巡檢人員
                var IPUseridlist = db.InspectionPlanMember.Where(x => x.IPSN == item.IPSN).Select(x => x.UserID).ToList();
                var INSPNameList = "";
                int a = 0;
                foreach (var id in IPUseridlist)
                {
                    var myname = db.AspNetUsers.Where(x => x.UserName == id).Select(x => x.MyName).FirstOrDefault();
                    if (myname != null)
                    {
                        if (a == 0)
                            INSPNameList += myname;
                        else
                            INSPNameList += $"、{myname}";
                    }
                    a++;
                }
                a = 0;
                itemObjects.Add("MyName", INSPNameList);
                //維修數量
                if (!string.IsNullOrEmpty(item.RepairAmount.ToString()))
                {
                    itemObjects.Add("RepairAmount", item.RepairAmount);
                }
                //定期保養
                if (!string.IsNullOrEmpty(item.MaintainAmount.ToString()))
                {
                    itemObjects.Add("MaintainAmount", item.MaintainAmount);
                }

                ja.Add(itemObjects);
            }

            JObject jo = new JObject();
            jo.Add("rows", ja);
            jo.Add("total", total);
            return jo;
        }
        #endregion

        #region 巡檢路線模板管理
        public JObject GetJsonForGrid_SamplePath(System.Web.Mvc.FormCollection form)
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
            //棟別編號
            string ASN = form["ASN"]?.ToString();
            //樓層編號
            string FSN = form["FSN"]?.ToString();
            //巡檢路線標題
            string PathTitle = form["PathTitle"]?.ToString();

            #region 依據查詢字串檢索資料表
            var SourceTable = from x1 in db.PathSample
                              join x2 in db.Floor_Info on x1.FSN equals x2.FSN
                              join x3 in db.AreaInfo on x2.ASN equals x3.ASN
                              select new { x1.PSSN, x1.PathTitle, x1.FSN, x2.ASN, x2.FloorName, x3.Area };

            if (!string.IsNullOrEmpty(ASN)) //查詢棟別編號
            {
                int IntASN = 0;
                bool conversionSuccessful = int.TryParse(ASN, out IntASN);
                if (conversionSuccessful)
                {
                    SourceTable = SourceTable.Where(x => x.ASN == IntASN);
                }
            }
            if (!string.IsNullOrEmpty(FSN)) //查詢樓層編號
            {
                SourceTable = SourceTable.Where(x => x.FSN == FSN);
            }
            if (!string.IsNullOrEmpty(PathTitle)) //查詢路徑標題模糊查詢
            {
                SourceTable = SourceTable.Where(x => x.PathTitle.Contains(PathTitle));
            }
            #endregion

            SourceTable = SourceTable.OrderBy(x => x.PSSN);

            //回傳JSON陣列
            JArray ja = new JArray();
            //記住總筆數
            int total = SourceTable.Count();
            //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
            SourceTable = SourceTable.Skip((page - 1) * rows).Take(rows);

            foreach (var a in SourceTable)
            {
                var itemObjects = new JObject();
                if (itemObjects["PSSN"] == null)
                {
                    itemObjects.Add("PSSN", a.PSSN);//路線模板編號
                }
                if (itemObjects["PathTitle"] == null)
                {
                    itemObjects.Add("PathTitle", a.PathTitle);//路線標題
                }
                if (itemObjects["Area"] == null)
                    itemObjects.Add("Area", a.Area);//棟別                  

                if (itemObjects["Floor"] == null)
                    itemObjects.Add("Floor", a.FloorName);//樓層

                ja.Add(itemObjects);
            }

            JObject jo = new JObject();
            jo.Add("rows", ja);
            jo.Add("total", total);
            return jo;
        }
        #endregion

        #region 資產管理
        public JObject GetJsonForGrid_EquipmentInfo(System.Web.Mvc.FormCollection form)
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
            //設備狀態
            string EState = form["EState"]?.ToString();
            //棟別
            string ASN = form["ASN"]?.ToString();
            string Area = form["Area"]?.ToString();
            //樓層
            string FSN = form["FSN"]?.ToString();
            string Floor = form["Floor"]?.ToString();
            //空間名稱
            string RoomName = form["RoomName"]?.ToString();
            //系統別
            string System = form["System"]?.ToString();
            //子系統別
            string SubSystem = form["SubSystem"]?.ToString();
            //設備編號
            string ESN = form["ESN"]?.ToString();
            //設備名稱
            string EName = form["EName"]?.ToString();
            //廠牌
            string Brand = form["Brand"]?.ToString();
            //型號
            string Model = form["Model"]?.ToString();
            //財產編碼
            string PropertyCode = form["PropertyCode"]?.ToString();
            #endregion

            #region 依據查詢字串檢索資料表
            var Data = from x1 in db.EquipmentInfo
                       join x2 in db.Floor_Info on x1.FSN equals x2.FSN
                       select new { x1.EState, x2.ASN, x1.Area, x1.FSN, x1.Floor, x1.RoomName, x1.System, x1.SubSystem, x1.ESN, x1.EName, x1.Brand, x1.Model, x1.PropertyCode };

            if (!string.IsNullOrEmpty(EState))
            {
                Data = Data.Where(x => x.EState == EState);
            }
            if (!string.IsNullOrEmpty(ASN))
            {
                int intasn = Convert.ToInt32(ASN);
                Data = Data.Where(x => x.ASN == intasn);
            }
            if (!string.IsNullOrEmpty(Area))
            {
                Data = Data.Where(x => x.Area == Area);
            }
            if (!string.IsNullOrEmpty(FSN))
            {
                Data = Data.Where(x => x.FSN == FSN);
            }
            if (!string.IsNullOrEmpty(Floor))
            {
                Data = Data.Where(x => x.Floor == Floor);
            }
            if (!string.IsNullOrEmpty(RoomName))
            {
                Data = Data.Where(x => x.RoomName.Contains(RoomName));
            }
            if (!string.IsNullOrEmpty(System))
            {
                Data = Data.Where(x => x.System == System);
            }
            if (!string.IsNullOrEmpty(SubSystem))
            {
                Data = Data.Where(x => x.SubSystem == SubSystem);
            }
            if (!string.IsNullOrEmpty(ESN))
            {
                Data = Data.Where(x => x.ESN == ESN);
            }
            if (!string.IsNullOrEmpty(EName))
            {
                Data = Data.Where(x => x.EName.Contains(EName));
            }
            if (!string.IsNullOrEmpty(Brand))
            {
                Data = Data.Where(x => x.Brand.Contains(Brand));
            }
            if (!string.IsNullOrEmpty(Model))
            {
                Data = Data.Where(x => x.Model.Contains(Model));
            }
            if (!string.IsNullOrEmpty(PropertyCode))
            {
                Data = Data.Where(x => x.PropertyCode == PropertyCode);
            }
            #endregion

            var result = Data.OrderByDescending(x => x.ESN).AsQueryable();
            //回傳JSON陣列
            JArray ja = new JArray();
            //記住總筆數
            int total = result.Count();
            //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
            result = result.Skip((page - 1) * rows).Take(rows);

            var Dic = Surfaces.Surface.Authority();

            foreach (var item in result)
            {
                var itemObjects = new JObject();

                itemObjects.Add("ESN", item.ESN);
                if (!string.IsNullOrEmpty(item.EState))
                {
                    var dic = Surface.EState();
                    itemObjects.Add("EState", dic[item.EState]);
                }
                if (!string.IsNullOrEmpty(item.Area))
                {
                    itemObjects.Add("Area", item.Area);
                }
                if (!string.IsNullOrEmpty(item.Floor))
                {
                    itemObjects.Add("Floor", item.Floor);
                }
                if (!string.IsNullOrEmpty(item.RoomName))
                {
                    itemObjects.Add("RoomName", item.RoomName);
                }
                if (!string.IsNullOrEmpty(item.System))
                {
                    itemObjects.Add("System", item.System);
                }
                if (!string.IsNullOrEmpty(item.SubSystem))
                {
                    itemObjects.Add("SubSystem", item.SubSystem);
                }
                if (!string.IsNullOrEmpty(item.EName))
                {
                    itemObjects.Add("EName", item.EName);
                }
                if (!string.IsNullOrEmpty(item.Brand))
                {
                    itemObjects.Add("Brand", item.Brand);
                }
                if (!string.IsNullOrEmpty(item.Model))
                {
                    itemObjects.Add("Model", item.Model);
                }
                if (!string.IsNullOrEmpty(item.PropertyCode))
                {
                    itemObjects.Add("PropertyCode", item.PropertyCode);
                }
                //FilePath
                //查設備操作手冊
                var filename = db.EquipmentOperatingManual.Where(x => x.System == item.System && x.SubSystem == item.SubSystem && x.EName == item.EName && x.Brand == item.Brand && x.Model == item.Model).Select(x => x.FilePath).FirstOrDefault();
                if (!string.IsNullOrEmpty(filename))
                {
                    itemObjects.Add("FilePath", "/Files/EquipmentOperatingManual" + filename);
                }
                ja.Add(itemObjects);
            }

            JObject jo = new JObject();
            jo.Add("rows", ja);
            jo.Add("total", total);
            return jo;
        }
        #endregion

        #region 設備操作手冊管理
        public JObject GetJsonForGrid_EquipmentOperatingManual(System.Web.Mvc.FormCollection form)
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
            //系統別
            string System = form["System"]?.ToString();
            //子系統別
            string SubSystem = form["SubSystem"]?.ToString();
            //設備名稱
            string EName = form["EName"]?.ToString();
            //廠牌
            string Brand = form["Brand"]?.ToString();
            //型號
            string Model = form["Model"]?.ToString();
            //日期起
            string DateStart = form["DateStart"]?.ToString();
            //日期迄
            string DateEnd = form["DateEnd"]?.ToString();
            #endregion

            #region 依據查詢字串檢索資料表
            var Data = db.EquipmentOperatingManual.AsQueryable();

            if (!string.IsNullOrEmpty(System))
            {
                Data = Data.Where(x => x.System == System);
            }
            if (!string.IsNullOrEmpty(SubSystem))
            {
                Data = Data.Where(x => x.SubSystem == SubSystem);
            }
            if (!string.IsNullOrEmpty(System))
            {
                Data = Data.Where(x => x.System == System);
            }
            if (!string.IsNullOrEmpty(EName))
            {
                Data = Data.Where(x => x.EName.Contains(EName));
            }
            if (!string.IsNullOrEmpty(Brand))
            {
                Data = Data.Where(x => x.Brand.Contains(Brand));
            }
            if (!string.IsNullOrEmpty(Model))
            {
                Data = Data.Where(x => x.Model.Contains(Model));
            }
            /*
            //日期(起)
            if (!string.IsNullOrEmpty(DateStart))
            {
                var datefrom = DateTime.Parse(DateStart);
                Data = Data.Where(x => x.PlanDate >= datefrom);
            }
            //日期(迄)
            if (!string.IsNullOrEmpty(DateEnd))
            {
                var dateto = DateTime.Parse(DateEnd).AddDays(1);
                Data = Data.Where(x => x.PlanDate < dateto);
            }
            */
            #endregion

            var result = Data.OrderByDescending(x => x.EOMSN).AsQueryable();
            //回傳JSON陣列
            JArray ja = new JArray();
            //記住總筆數
            int total = result.Count();
            //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
            result = result.Skip((page - 1) * rows).Take(rows);

            var Dic = Surfaces.Surface.Authority();

            foreach (var item in result)
            {
                var itemObjects = new JObject();

                itemObjects.Add("EOMSN", item.EOMSN);
                itemObjects.Add("FilePath", "/Files/EquipmentOperatingManual" + item.FilePath);
                itemObjects.Add("System", item.System);
                itemObjects.Add("SubSystem", item.SubSystem);
                itemObjects.Add("EName", item.EName);
                if (!string.IsNullOrEmpty(item.Brand))
                {
                    itemObjects.Add("Brand", item.Brand);
                }
                if (!string.IsNullOrEmpty(item.Model))
                {
                    itemObjects.Add("Model", item.Model);
                }

                ja.Add(itemObjects);
            }

            JObject jo = new JObject();
            jo.Add("rows", ja);
            jo.Add("total", total);
            return jo;
        }
        #endregion

        #region 設計圖說管理
        public JObject GetJsonForGrid_DesignDiagrams(System.Web.Mvc.FormCollection form)
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
            //string propertyName = "DDSN";
            //string order = "asc";

            #region 塞來自formdata的資料
            //圖名
            string ImgName = form["ImgName"]?.ToString();
            //圖說種類
            string ImgType = form["ImgType"]?.ToString();
            //上傳日期(起)
            string DateStart = form["DateStart"]?.ToString();
            //上傳日期(迄)
            string DateEnd = form["DateEnd"]?.ToString();
            #endregion


            #region 依據查詢字串檢索資料表
            var SourceTable = db.DesignDiagrams.AsQueryable();

            //圖名
            if (!string.IsNullOrEmpty(ImgName))
            {
                SourceTable = SourceTable.Where(x => x.ImgName.Contains(ImgName));
            }
            //圖說種類
            if (!string.IsNullOrEmpty(ImgType))
            {
                SourceTable = SourceTable.Where(x => x.ImgType == ImgType);
            }
            //上傳日期(起)
            if (!string.IsNullOrEmpty(DateStart))
            {
                var datestart = DateTime.Parse(DateStart);
                SourceTable = SourceTable.Where(x => x.UploadDate >= datestart);
            }
            //上傳日期(迄)
            if (!string.IsNullOrEmpty(DateEnd))
            {
                var dateend = DateTime.Parse(DateEnd).AddDays(1);
                SourceTable = SourceTable.Where(x => x.UploadDate < dateend);
            }
            #endregion

            SourceTable = SourceTable.OrderByDescending(x => x.DDSN);

            //回傳JSON陣列
            JArray ja = new JArray();
            //記住總筆數
            int total = SourceTable.Count();
            //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
            SourceTable = SourceTable.Skip((page - 1) * rows).Take(rows);

            foreach (var item in SourceTable)
            {
                var itemObjects = new JObject();
                itemObjects.Add("DDSN", item.DDSN);
                itemObjects.Add("ImgPath", "/Files/DesignDiagrams" + item.ImgPath);
                itemObjects.Add("ImgName", item.ImgName);
                //圖說種類
                if (!string.IsNullOrEmpty(item.ImgType))
                {
                    var dic = Surface.ImgType();
                    itemObjects.Add("ImgType", dic[item.ImgType]);
                }
                //上傳日期
                if (item.UploadDate != DateTime.MinValue && item.UploadDate != null)
                {
                    itemObjects.Add("UploadDate", item.UploadDate.ToString("yyyy/MM/dd"));
                }

                ja.Add(itemObjects);
            }

            JObject jo = new JObject();
            jo.Add("rows", ja);
            jo.Add("total", total);
            return jo;
        }
        #endregion

        #region 竣工圖說管理
        public String GetJsonForGrid_AsBuiltDrawing(System.Web.Mvc.FormCollection form)
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
            //系統別
            string DSystemID = form["DSystemID"]?.ToString();
            //子系統別
            string DSubSystemID = form["DSubSystemID"]?.ToString();
            //圖號
            string ImgNum = form["ImgNum"]?.ToString();
            //圖名
            string ImgName = form["ImgName"]?.ToString();
            //版本
            string ImgVersion = form["ImgVersion"]?.ToString();
            //日期起
            string DateStart = form["DateFrom"]?.ToString();
            //日期迄
            string DateEnd = form["DateTo"]?.ToString();
            #endregion

            #region 依據查詢字串檢索資料表
            var SourceTable = from x1 in db.AsBuiltDrawing
                              join x2 in db.Floor_Info on x1.FSN equals x2.FSN
                              join x3 in db.DrawingSubSystemManagement on x1.DSubSystemID equals x3.DSubSystemID
                              join x4 in db.AreaInfo on x2.ASN equals x4.ASN
                              join x5 in db.DrawingSystemManagement on x3.DSystemID equals x5.DSystemID
                              select new { x1.ADSN, x1.ImgPath, x2.ASN, x1.FSN, x3.DSystemID, x1.DSubSystemID, x1.ImgNum, x1.ImgName, x1.ImgVersion, x1.UploadDate, x4.Area, x2.FloorName, x3.DSubSystem, x5.DSystem };


            if (!string.IsNullOrEmpty(ASN))
            {
                var asn = Convert.ToInt32(ASN);
                SourceTable = SourceTable.Where(x => x.ASN == asn);
            }
            if (!string.IsNullOrEmpty(FSN))
            {
                SourceTable = SourceTable.Where(x => x.FSN == FSN);
            }
            if (!string.IsNullOrEmpty(DSystemID))
            {
                var dsystemid = Convert.ToInt32(DSystemID);
                SourceTable = SourceTable.Where(x => x.DSystemID == dsystemid);
            }
            if (!string.IsNullOrEmpty(DSubSystemID))
            {
                SourceTable = SourceTable.Where(x => x.DSubSystemID == DSubSystemID);
            }
            if (!string.IsNullOrEmpty(ImgNum))
            {
                SourceTable = SourceTable.Where(x => x.ImgNum.Contains(ImgNum));
            }
            if (!string.IsNullOrEmpty(ImgName))
            {
                SourceTable = SourceTable.Where(x => x.ImgName.Contains(ImgName));
            }
            if (!string.IsNullOrEmpty(ImgVersion))
            {
                SourceTable = SourceTable.Where(x => x.ImgVersion.Contains(ImgVersion));
            }
            //日期(起)
            if (!string.IsNullOrEmpty(DateStart))
            {
                var datefrom = DateTime.Parse(DateStart);
                SourceTable = SourceTable.Where(x => x.UploadDate >= datefrom);
            }
            //日期(迄)
            if (!string.IsNullOrEmpty(DateEnd))
            {
                var dateto = DateTime.Parse(DateEnd).AddDays(1);
                SourceTable = SourceTable.Where(x => x.UploadDate < dateto);
            }

            #endregion

            var result = SourceTable.OrderByDescending(x => x.ADSN).AsQueryable();
            //回傳JSON陣列
            JArray ja = new JArray();
            //記住總筆數
            int total = result.Count();
            //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
            result = result.Skip((page - 1) * rows).Take(rows);

            foreach (var item in result)
            {
                var itemObjects = new JObject();

                itemObjects.Add("ADSN", item.ADSN);
                itemObjects.Add("Area", item.Area);
                itemObjects.Add("Floor", item.FloorName);
                itemObjects.Add("System", item.DSystem);
                itemObjects.Add("SubSystem", item.DSubSystem);
                itemObjects.Add("ImgNum", item.ImgNum);
                itemObjects.Add("ImgName", item.ImgName);
                itemObjects.Add("UploadDate", item.UploadDate.ToString("yyyy/MM/dd"));
                itemObjects.Add("ImgVersion", item.ImgVersion);
                itemObjects.Add("ImgPath", "/Files/AsBuiltDrawing" + item.ImgPath);

                ja.Add(itemObjects);
            }

            JObject jo = new JObject();
            jo.Add("rows", ja);
            jo.Add("total", total);

            string reString = JsonConvert.SerializeObject(jo);
            return reString;
        }
        #endregion
    }
}