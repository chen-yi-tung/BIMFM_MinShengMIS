using MinSheng_MIS.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI.WebControls;
using MinSheng_MIS.Surfaces;
using System.Linq.Dynamic.Core;
using System.Globalization;
using System.Web.Mvc;
using WebGrease;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using OfficeOpenXml.Style;
using Newtonsoft.Json;
using System.Dynamic;

namespace MinSheng_MIS.Services
{
    public class DatagridService
    {
        // 使用 Expression Tree 實現動態排序的方法
        public static IQueryable<T> OrderByField<T>(IQueryable<T> query, string propertyName, bool isAscending)
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

        //--巡檢管理--
        #region 工單管理
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

            #region 塞來自formdata的資料
            //工單編號
            string IPSN = form["IPSN"]?.ToString();
            //工單狀態
            string PlanState = form["PlanState"]?.ToString();
            //工單名稱
            string IPName = form["IPName"]?.ToString();
            //工單日期(起)
            string PlanDateFrom = form["PlanDateFrom"]?.ToString();
            //工單日期(迄)
            string PlanDateTo = form["PlanDateTo"]?.ToString();
            //執行人員
            string Member = form["Member"]?.ToString();
            #endregion

            #region 依據查詢字串檢索資料表
            var SourceTable = db.InspectionPlan
                                .Select(x => new 
                                {   x.IPSN,
                                    x.IPName,
                                    x.PlanDate,
                                    x.PlanState,
                                    x.PlanCreateUserID
                                    ,x.CreateTime,
                                    Member = db.InspectionPlan_Member
                                        .Where(m => db.InspectionPlan_Time
                                            .Where(t => t.IPSN == x.IPSN)
                                            .Select(t => t.IPTSN)
                                            .Contains(m.IPTSN))
                                        .Join(db.AspNetUsers,
                                             m => m.UserID,
                                             u => u.UserName,
                                             (m, u) => u.MyName)
                                        .Distinct()
                                        .ToList()
                                })
                                .AsEnumerable() // 轉換為記憶體操作
                                .Select(x => new
                                {
                                    x.IPSN,
                                    x.IPName,
                                    x.PlanDate,
                                    x.PlanState,
                                    x.PlanCreateUserID,
                                    x.CreateTime,
                                    Member = string.Join("、", x.Member) // 轉換為字串
                                }).AsQueryable();

            //工單編號
            if (!string.IsNullOrEmpty(IPSN))
            {
                SourceTable = SourceTable.Where(x => x.IPSN.Contains(IPSN));
            }
            //工單狀態
            if (!string.IsNullOrEmpty(PlanState))
            {
                SourceTable = SourceTable.Where(x => x.PlanState == PlanState);
            }
            //工單名稱
            if (!string.IsNullOrEmpty(IPName))
            {
                SourceTable = SourceTable.Where(x => x.IPName.Contains(IPName));
            }
            //日期(起)
            if (!string.IsNullOrEmpty(PlanDateFrom))
            {
                var datefrom = DateTime.Parse(PlanDateFrom);
                SourceTable = SourceTable.Where(x => x.PlanDate >= datefrom);
            }
            //日期(迄)
            if (!string.IsNullOrEmpty(PlanDateTo))
            {
                var dateto = DateTime.Parse(PlanDateTo).AddDays(1);
                SourceTable = SourceTable.Where(x => x.PlanDate < dateto);
            }
            //執行人員
            if (!string.IsNullOrEmpty(Member))
            {
                var IPTSNlist = db.InspectionPlan_Member.Where(x => x.UserID == Member).Select(x => x.IPTSN).ToList();
                var planlist = db.InspectionPlan_Time.Where(x => IPTSNlist.Contains(x.IPTSN)).Select(x => x.IPSN).ToList();
                SourceTable = SourceTable.Where(x => planlist.Contains(x.IPSN));
            }
            #endregion

            #region datagrid remoteSort 判斷有無 sort 跟 order
            var sort = form["sort"]?.ToString();
            var order = form["order"]?.ToString();
            if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order)) SourceTable = SourceTable.OrderBy(sort + " " + order); // 使用 System.Linq.Dynamic.Core 套件進行動態排序
            else SourceTable = SourceTable.OrderByDescending(x => x.IPSN);
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
                //工單編號
                if (!string.IsNullOrEmpty(item.IPSN))
                {
                    itemObjects.Add("IPSN", item.IPSN);
                }
                //工單狀態
                if (!string.IsNullOrEmpty(item.PlanState))
                {
                    var dic = Surface.InspectionPlanState();
                    itemObjects.Add("PlanState", dic[item.PlanState]);
                }
                //工單日期
                if (item.PlanDate != DateTime.MinValue && item.PlanDate != null)
                {
                    itemObjects.Add("PlanDate", item.PlanDate.ToString("yyyy-MM-dd"));
                }
                //工單名稱
                if (!string.IsNullOrEmpty(item.IPName))
                {
                    itemObjects.Add("IPName", item.IPName);
                }
                //執行人員
                if (!string.IsNullOrEmpty(item.Member))
                {
                    itemObjects.Add("Member", item.Member);
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

            #region 塞來自formdata的資料
            //巡檢路線名稱
            string PathName = form["PathName"]?.ToString();
            #endregion

            #region 依據查詢字串檢索資料表
            var SourceTable = db.InspectionPathSample.Select(x => new { x.PlanPathSN,x.PathName,x.Frequency,InspectionNum = db.InspectionDefaultOrder.Count(c => c.PlanPathSN == x.PlanPathSN) }).AsQueryable();

            if (!string.IsNullOrEmpty(PathName)) //查詢巡檢路線名稱模糊查詢
            {
                SourceTable = SourceTable.Where(x => x.PathName.Contains(PathName));
            }
            #endregion

            #region datagrid remoteSort 判斷有無 sort 跟 order  
            IValueProvider vp = form.ToValueProvider();
            if (vp.ContainsPrefix("sort") && vp.ContainsPrefix("order"))
            {
                string sort = form["sort"];
                string order = form["order"];
                SourceTable = OrderByField(SourceTable, sort, order == "asc");
            }
            else
            {
                SourceTable = SourceTable.OrderBy(x => x.PathName);
            }
            #endregion

            //回傳JSON陣列
            JArray ja = new JArray();
            //記住總筆數
            int total = SourceTable.Count();
            //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
            SourceTable = SourceTable.Skip((page - 1) * rows).Take(rows);
            var dic_frequency = Surface.InspectionPlanFrequency();
            foreach (var a in SourceTable)
            {
                var itemObjects = new JObject();
                itemObjects.Add("PlanPathSN", a.PlanPathSN);//巡檢路線編號
                itemObjects.Add("PathName", a.PathName);//巡檢路線名稱
                itemObjects.Add("Frequency", dic_frequency[a.Frequency.ToString()]);//巡檢頻率
                itemObjects.Add("InspectionNum", a.InspectionNum);//巡檢數量
                ja.Add(itemObjects);
            }

            JObject jo = new JObject();
            jo.Add("rows", ja);
            jo.Add("total", total);
            return jo;
        }
        #endregion

        #region 每日巡檢時程模板管理
        public JObject GetJsonForGrid_DailyInspectionSample(System.Web.Mvc.FormCollection form)
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
            //巡檢路線名稱
            string TemplateName = form["TemplateName"]?.ToString();
            #endregion

            #region 依據查詢字串檢索資料表
            var SourceTable = db.DailyInspectionSample.Select(x => new { x.DailyTemplateSN,x.TemplateName, InspectionNum = db.DailyInspectionSampleContent.Count(c => c.DailyTemplateSN == x.DailyTemplateSN) }).AsQueryable();

            if (!string.IsNullOrEmpty(TemplateName)) //查詢巡檢路線名稱模糊查詢
            {
                SourceTable = SourceTable.Where(x => x.TemplateName.Contains(TemplateName));
            }
            #endregion

            #region datagrid remoteSort 判斷有無 sort 跟 order  
            IValueProvider vp = form.ToValueProvider();
            if (vp.ContainsPrefix("sort") && vp.ContainsPrefix("order"))
            {
                string sort = form["sort"];
                string order = form["order"];
                SourceTable = OrderByField(SourceTable, sort, order == "asc");
            }
            else
            {
                SourceTable = SourceTable.OrderBy(x => x.TemplateName);
            }
            #endregion

            //回傳JSON陣列
            JArray ja = new JArray();
            //記住總筆數
            int total = SourceTable.Count();
            //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
            SourceTable = SourceTable.Skip((page - 1) * rows).Take(rows);

            foreach (var a in SourceTable)
            {
                var itemObjects = new JObject();
                itemObjects.Add("DailyTemplateSN", a.DailyTemplateSN);//巡檢路線編號
                itemObjects.Add("TemplateName", a.TemplateName);//巡檢模板名稱
                itemObjects.Add("InspectionNum", a.InspectionNum);//巡檢路線數量
                ja.Add(itemObjects);
            }

            JObject jo = new JObject();
            jo.Add("rows", ja);
            jo.Add("total", total);
            return jo;
        }
        #endregion

        //--定期保養管理--
        #region 定期保養管理
        public JObject GetJsonForGrid_MaintainForm(System.Web.Mvc.FormCollection form)
        {
            using (var db = new Bimfm_MinSheng_MISEntities())
            {
                IEnumerable<Equipment_MaintenanceForm> maintenanceForms = db.Equipment_MaintenanceForm.AsNoTracking();

                #region 篩選搜尋的保養單
                // 保養單狀態
                if (!string.IsNullOrEmpty(form["Status"]?.ToString()))
                {
                    maintenanceForms = maintenanceForms.Where(x => x.Status == form["Status"]);
                }
                // 保養單號
                if (!string.IsNullOrEmpty(form["EMFSN"]?.ToString()))
                {
                    maintenanceForms = maintenanceForms.Where(x => x.EMFSN.Contains(form["EMFSN"].ToUpper()));
                }
                // 最近應保養日期(起)
                if (DateTime.TryParse(form["NextMaintainDateFrom"]?.ToString(), out DateTime nextMaintainFrom))
                {
                    maintenanceForms = maintenanceForms.Where(x => x.NextMaintainDate.Date >= nextMaintainFrom.Date);
                }
                // 最近應保養日期(迄)
                if (DateTime.TryParse(form["NextMaintainDateTo"]?.ToString(), out DateTime nextMaintainTo))
                {
                    maintenanceForms = maintenanceForms.Where(e => e.NextMaintainDate.Date <= nextMaintainTo.Date);
                }
                // 實際保養日期(起)
                if (DateTime.TryParse(form["ReportTimeFrom"]?.ToString(), out DateTime reportTimeFrom))
                {
                    maintenanceForms = maintenanceForms.Where(x => x.ReportTime?.Date >= reportTimeFrom.Date);
                }
                // 實際保養日期(迄)
                if (DateTime.TryParse(form["ReportTimeTo"]?.ToString(), out DateTime reportTimeTo))
                {
                    maintenanceForms = maintenanceForms.Where(e => e.ReportTime?.Date <= reportTimeTo.Date);
                }
                // 設備狀態
                if (!string.IsNullOrEmpty(form["EState"]?.ToString()))
                {
                    maintenanceForms = maintenanceForms.Where(x => x.EquipmentInfo.EState == form["EState"]);
                }
                // 棟別 (區域編號)
                if (!string.IsNullOrEmpty(form["ASN"]?.ToString()))
                {
                    maintenanceForms = maintenanceForms.Where(x => x.EquipmentInfo.Floor_Info.ASN == Convert.ToInt32(form["ASN"]));
                }
                // 樓層
                if (!string.IsNullOrEmpty(form["FSN"]?.ToString()))
                {
                    maintenanceForms = maintenanceForms.Where(x => x.EquipmentInfo.FSN == form["FSN"]);
                }
                // 設備名稱
                if (!string.IsNullOrEmpty(form["EName"]?.ToString()))
                {
                    maintenanceForms = maintenanceForms.Where(x => x.EquipmentInfo.EName.Contains(form["EName"]));
                }
                // 設備編號 (NO)
                if (!string.IsNullOrEmpty(form["ESN"]?.ToString()))
                {
                    maintenanceForms = maintenanceForms.Where(x => x.EquipmentInfo.NO.ToUpper().Contains(form["ESN"].ToUpper()));
                }
                // 保養項目
                if (!string.IsNullOrEmpty(form["MaintainName"]?.ToString()))
                {
                    maintenanceForms = maintenanceForms.Where(x => x.MaintainName.Contains(form["MaintainName"]));
                }
                // 保養週期
                if (!string.IsNullOrEmpty(form["Period"]?.ToString()))
                {
                    maintenanceForms = maintenanceForms.Where(x => x.Period == form["Period"]);
                }
                // 執行人員
                if (!string.IsNullOrEmpty(form["Member"]?.ToString()))
                {
                    maintenanceForms = maintenanceForms.Where(x => x.Equipment_MaintenanceFormMember.Any(m => m.Maintainer.Contains(form["Member"])));
                }
                #endregion

                #region 塞回傳資料
                // 回傳JSON陣列
                JArray ja = new JArray();
                // 總筆數
                int total = maintenanceForms.Count();

                foreach (var item in maintenanceForms)
                {
                    var maintainerAccs = item.Equipment_MaintenanceFormMember.Select(m => m.Maintainer);
                    var maintainerList = db.AspNetUsers
                        .Where(x => maintainerAccs.Contains(x.UserName))
                        .Select(x => x.MyName).ToList();

                    JObject itemObject = new JObject();
                    itemObject.Add("Status", Surface.MaintainStatus()[item.Status]); // 保養單狀態
                    itemObject.Add("EMFSN", item.EMFSN); // 保養單號
                    itemObject.Add("NextMaintainDate", item.NextMaintainDate.ToString("yyyy-MM-dd")); // 最近應保養日期
                    itemObject.Add("ReportTime", item.ReportTime?.ToString("yyyy-MM-dd") ?? "-"); // 實際保養日期
                    itemObject.Add("EState", Surface.EState()[item.EquipmentInfo.EState]); // 設備狀態
                    itemObject.Add("Area", item.EquipmentInfo.Floor_Info.AreaInfo.Area); // 棟別 (區域)
                    itemObject.Add("FloorName", item.EquipmentInfo.Floor_Info.FloorName); // 樓層
                    itemObject.Add("EName", item.EquipmentInfo.EName); // 設備名稱
                    itemObject.Add("ESN", item.EquipmentInfo.NO); // 設備編號 (NO)
                    itemObject.Add("MaintainName", item.MaintainName); // 保養項目
                    itemObject.Add("Period", Surface.MaintainPeriod()[item.Period]); // 保養週期
                    itemObject.Add("Maintainer", string.Join("、", maintainerList)); // 執行人員

                    ja.Add(itemObject);
                }
                #endregion

                #region datagrid 排序
                var sort = form["sort"]?.ToString();
                var order = form["order"]?.ToString();
                var list = JsonConvert.DeserializeObject<List<ExpandoObject>>(ja.ToString()).AsQueryable();
                if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order))
                    list = list.OrderBy(x => ((IDictionary<string, object>)x)[sort]); // 使用 System.Linq.Dynamic.Core 套件進行動態排序
                else list = list.OrderByDescending(x => ((IDictionary<string, object>)x)["EMFSN"]);
                #endregion

                #region datagrid 切頁
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
                // 回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
                list = list.Skip((page - 1) * rows).Take(rows);
                #endregion

                JObject jo = new JObject();
                jo.Add("rows", JArray.FromObject(list));
                jo.Add("total", total);
                return jo;
            }
        }
        #endregion

        //--報修管理--
        #region 報修管理
        public JObject RepairManagementDataGrid(FormCollection form)
        {
            using (var db = new Bimfm_MinSheng_MISEntities())
            {
                IQueryable<EquipmentReportForm> equipmentReportFormTable = db.EquipmentReportForm.AsNoTracking();

                //報修單狀態
                if (!string.IsNullOrEmpty(form["ReportState"]?.ToString()))
                {
                    string reportState = form["ReportState"].ToString();
                    equipmentReportFormTable = equipmentReportFormTable.Where(e => e.ReportState == reportState);
                }
                //報修單號
                if (!string.IsNullOrEmpty(form["RSN"]?.ToString()))
                {
                    string rsn = form["RSN"].ToString();
                    equipmentReportFormTable = equipmentReportFormTable.Where(e => e.RSN.Contains(rsn));
                }
                //報修等級
                if (!string.IsNullOrEmpty(form["ReportLevel"]?.ToString()))
                {
                    string reportLevel = form["ReportLevel"].ToString();
                    equipmentReportFormTable = equipmentReportFormTable.Where(e => e.ReportLevel == reportLevel);
                }
                //報修時間(起)
                if (DateTime.TryParse(form["DateFrom"]?.ToString(), out DateTime dateFrom))
                {
                    equipmentReportFormTable = equipmentReportFormTable.Where(e => e.ReportTime >= dateFrom);
                }
                //報修時間(迄)
                if (DateTime.TryParse(form["DateTo"]?.ToString(), out DateTime dateTo))
                {
                    dateTo = dateTo.AddDays(1);
                    equipmentReportFormTable = equipmentReportFormTable.Where(e => e.ReportTime <= dateTo);
                }
                //報修內容
                if (!string.IsNullOrEmpty(form["ReportContent"]?.ToString()))
                {
                    string reportContent = form["ReportContent"].ToString();
                    equipmentReportFormTable = equipmentReportFormTable.Where(e => e.ReportContent.Contains(reportContent));
                }
                //棟別
                if (!string.IsNullOrEmpty(form["ASN"]?.ToString()))
                {
                    if (!int.TryParse(form["ASN"]?.ToString(), out int asn))
                    {
                        equipmentReportFormTable = equipmentReportFormTable.Where(e => e.EquipmentInfo.Floor_Info.ASN == asn);
                    }
                }
                //樓層
                if (!string.IsNullOrEmpty(form["FSN"]?.ToString()))
                {
                    string fsn = form["FSN"].ToString();
                    equipmentReportFormTable = equipmentReportFormTable.Where(e => e.EquipmentInfo.FSN == fsn);
                }
                //設備名稱
                if (!string.IsNullOrEmpty(form["EName"]?.ToString()))
                {
                    string eName = form["EName"].ToString();
                    equipmentReportFormTable = equipmentReportFormTable.Where(e => e.EquipmentInfo.EName.Contains(eName));
                }
                //設備編號
                if (!string.IsNullOrEmpty(form["NO"]?.ToString()))
                {
                    string no = form["NO"].ToString();
                    equipmentReportFormTable = equipmentReportFormTable.Where(e => e.EquipmentInfo.NO.Contains(no));
                }
                //執行人員
                if (!string.IsNullOrEmpty(form["Member"]?.ToString()))
                {
                    string repairUserName = form["Member"].ToString();
                    var userRepairSet = db.Equipment_ReportFormMember.Where(e => e.RepairUserName == repairUserName).Select(e => e.RSN).ToHashSet();
                    equipmentReportFormTable = equipmentReportFormTable.Where(a => userRepairSet.Contains(a.RSN));
                }

                equipmentReportFormTable = equipmentReportFormTable.OrderByDescending(e => e.ReportTime);

                int page = 1;
                if (!string.IsNullOrEmpty(form["page"]?.ToString()))
                {
                    page = int.Parse(form["page"].ToString());
                }
                int rows = 10;
                if (!string.IsNullOrEmpty(form["rows"]?.ToString()))
                {
                    rows = int.Parse(form["rows"]?.ToString());
                }
                if (!string.IsNullOrEmpty(form["sort"]?.ToString()) && !string.IsNullOrEmpty(form["order"]?.ToString()))
                {
                    string sort = form["sort"].ToString();
                    string order = form["order"].ToString();
                    switch (sort)
                    {
                        case "Area":
                            if (order == "asc")
                                equipmentReportFormTable = equipmentReportFormTable.OrderBy(e => e.EquipmentInfo.Floor_Info.AreaInfo.Area);
                            else
                                equipmentReportFormTable = equipmentReportFormTable.OrderByDescending(e => e.EquipmentInfo.Floor_Info.AreaInfo.Area);                            
                            break;
                        case "FloorName":
                            if (order == "asc")
                                equipmentReportFormTable = equipmentReportFormTable.OrderBy(e => e.EquipmentInfo.Floor_Info.FloorName);
                            else
                                equipmentReportFormTable = equipmentReportFormTable.OrderByDescending(e => e.EquipmentInfo.Floor_Info.FloorName);
                            break;
                        case "EName":
                            if (order == "asc")
                                equipmentReportFormTable = equipmentReportFormTable.OrderBy(e => e.EquipmentInfo.EName);
                            else
                                equipmentReportFormTable = equipmentReportFormTable.OrderByDescending(e => e.EquipmentInfo.EName);
                            break;
                        case "NO":
                            if (order == "asc")
                                equipmentReportFormTable = equipmentReportFormTable.OrderBy(e => e.EquipmentInfo.NO);
                            else
                                equipmentReportFormTable = equipmentReportFormTable.OrderByDescending(e => e.EquipmentInfo.NO);
                            break;
                        case "RepairMyName":
                            if (order == "asc")
                                equipmentReportFormTable = equipmentReportFormTable.OrderBy(e => db.Equipment_ReportFormMember.FirstOrDefault(m => m.RSN == e.RSN).RepairUserName);
                            else
                                equipmentReportFormTable = equipmentReportFormTable.OrderByDescending(e => db.Equipment_ReportFormMember.FirstOrDefault(m => m.RSN == e.RSN).RepairUserName);
                            break;
                        default:
                            equipmentReportFormTable = OrderByField(equipmentReportFormTable, sort, order == "asc");
                            break;
                    }
                }

                int total = equipmentReportFormTable.Count();
                List<EquipmentReportForm> equipmentReportFormList = equipmentReportFormTable.Skip((page - 1) * rows).Take(rows).ToList();

                JArray ja = new JArray();
                foreach (var item in equipmentReportFormList)
                {
                    JObject itemObject = new JObject();
                    itemObject.Add("ReportState", Surface.ReportState()[item.ReportState]);
                    itemObject.Add("RSN", item.RSN);
                    itemObject.Add("ReportLevel", Surface.ReportLevel()[item.ReportLevel]);
                    itemObject.Add("ReportTime", item.ReportTime.ToString("yyyy-MM-dd HH:mm"));
                    itemObject.Add("ReportContent", item.ReportContent);
                    itemObject.Add("Area", item.EquipmentInfo.Floor_Info.AreaInfo.Area);
                    itemObject.Add("FloorName", item.EquipmentInfo.Floor_Info.FloorName);
                    itemObject.Add("EName", item.EquipmentInfo.EName);
                    itemObject.Add("NO", item.EquipmentInfo.NO);
                    itemObject.Add("RepairMyName", "");
                    var memberlist = db.Equipment_ReportFormMember.Where(e => e.RSN == item.RSN).ToList();
                    foreach (var member in memberlist)
                    {
                        var myName = db.AspNetUsers.Where(a => a.UserName == member.RepairUserName).Select(a => a.MyName).FirstOrDefault();
                        if (!string.IsNullOrEmpty(itemObject["RepairMyName"]?.ToString()))
                            itemObject["RepairMyName"] = $"{itemObject["RepairMyName"]}、{myName}";
                        else
                            itemObject["RepairMyName"] = myName;
                    }
                    ja.Add(itemObject);
                }

                JObject jo = new JObject
                {
                    { "rows", ja },
                    { "total", total }
                };
                return jo;
            }
        }

        //public JObject GetJsonForGrid_Report_Management(System.Web.Mvc.FormCollection form)
        //{
        //    #region datagrid呼叫時的預設參數有 rows 跟 page
        //    int page = 1;
        //    if (!string.IsNullOrEmpty(form["page"]?.ToString()))
        //    {
        //        page = short.Parse(form["page"].ToString());
        //    }
        //    int rows = 10;
        //    if (!string.IsNullOrEmpty(form["rows"]?.ToString()))
        //    {
        //        rows = short.Parse(form["rows"]?.ToString());
        //    }
        //    #endregion

        //    #region 塞來自formdata的資料
        //    //棟別名稱
        //    string Area = form["Area"]?.ToString();
        //    //棟別編號
        //    string ASN = form["ASN"]?.ToString();
        //    //樓層名稱
        //    string Floor = form["Floor"]?.ToString();
        //    //樓層編號
        //    string FSN = form["FSN"]?.ToString();
        //    //報修單編號
        //    string ReportState = form["ReportState"]?.ToString();
        //    //報修等級
        //    string ReportLevel = form["ReportLevel"]?.ToString();
        //    //報修單號
        //    string RSN = form["RSN"]?.ToString();
        //    //設備編號
        //    string ESN = form["ESN"]?.ToString();
        //    //設備名稱
        //    string EName = form["EName"]?.ToString();
        //    //財產編碼
        //    string PropertyCode = form["PropertyCode"]?.ToString();
        //    //報修說明
        //    string ReportContent = form["ReportContent"]?.ToString();
        //    //報修人員id
        //    string InformantUserID = form["InformantUserID"]?.ToString();
        //    //起始日期
        //    string DateFrom = form["DateFrom"]?.ToString();
        //    //結束日期
        //    string DateTo = form["DateTo"]?.ToString();
        //    //判斷是從哪裡來的請求DataGrid
        //    string SourceReport = form["SourceReport"]?.ToString();
        //    //庫存狀態
        //    string StockState = form["StockState"]?.ToString();
        //    #endregion

        //    #region 依據查詢字串檢索資料表
        //    var SourceTable = from x1 in db.EquipmentReportForm
        //                      join x2 in db.EquipmentInfo on x1.ESN equals x2.ESN
        //                      join x3 in db.AspNetUsers on x1.InformatUserID equals x3.UserName
        //                      join x4 in db.Floor_Info on x2.FSN equals x4.FSN
        //                      select new { x1.ReportState, x1.ReportLevel, x2.Area, x2.Floor, x1.ReportSource, x1.RSN, x1.Date, x2.PropertyCode, x1.ESN, x2.EName, x1.ReportContent, x3.MyName, x3.UserName, x2.EState, x1.StockState, x2.DBID, x2.FSN, x4.ASN };

        //    //若是用於新增巡檢計畫 的 新增維修單需增加狀態判斷
        //    if (SourceReport == "AddReportForm")
        //    {
        //        //增加狀態判斷
        //        SourceTable = SourceTable.Where(x => x.ReportState == "1" || x.ReportState == "5" || x.ReportState == "8" || x.ReportState == "9" || x.ReportState == "10" || x.ReportState == "11");
        //        //設備若停用則不能加入巡檢計畫中
        //        SourceTable = SourceTable.Where(x => x.EState != "3");
        //    }

        //    //Area查詢table方式 以Area至表[設備資訊]查詢出ESN，再以ESN至表[設備報修單]查詢出相關報修單。
        //    if (!string.IsNullOrEmpty(Area))
        //    {
        //        SourceTable = SourceTable.Where(x => x.Area == Area);
        //    }
        //    if (!string.IsNullOrEmpty(Floor))
        //    {
        //        SourceTable = SourceTable.Where(x => x.Floor == Floor);
        //    }
        //    if (!string.IsNullOrEmpty(ReportState))
        //    {
        //        SourceTable = SourceTable.Where(x => x.ReportState == ReportState);
        //    }
        //    if (!string.IsNullOrEmpty(ReportLevel))
        //    {
        //        SourceTable = SourceTable.Where(x => x.ReportLevel == ReportLevel);
        //    }
        //    if (!string.IsNullOrEmpty(RSN))
        //    {
        //        SourceTable = SourceTable.Where(x => x.RSN == RSN);
        //    }
        //    if (!string.IsNullOrEmpty(ESN))
        //    {
        //        SourceTable = SourceTable.Where(x => x.ESN == ESN);
        //    }
        //    if (!string.IsNullOrEmpty(EName))
        //    {
        //        SourceTable = SourceTable.Where(x => x.EName == EName);
        //    }
        //    if (!string.IsNullOrEmpty(PropertyCode))
        //    {
        //        SourceTable = SourceTable.Where(x => x.PropertyCode == PropertyCode);
        //    }
        //    if (!string.IsNullOrEmpty(ReportContent))
        //    {
        //        SourceTable = SourceTable.Where(x => x.ReportContent.Contains(ReportContent));
        //    }
        //    if (!string.IsNullOrEmpty(InformantUserID))
        //    {
        //        SourceTable = SourceTable.Where(x => x.UserName == InformantUserID);
        //    }
        //    if (!string.IsNullOrEmpty(DateFrom))
        //    {
        //        var datefrom = DateTime.Parse(DateFrom);
        //        SourceTable = SourceTable.Where(x => x.Date >= datefrom);
        //    }
        //    if (!string.IsNullOrEmpty(DateTo))
        //    {
        //        var dateto = DateTime.Parse(DateTo).AddDays(1);
        //        SourceTable = SourceTable.Where(x => x.Date <= dateto);
        //    }
        //    if (!string.IsNullOrEmpty(StockState))
        //    {
        //        switch (StockState)
        //        {
        //            case "0":
        //                SourceTable = SourceTable.Where(x => x.StockState == false);
        //                break;
        //            case "1":
        //                SourceTable = SourceTable.Where(x => x.StockState == true);
        //                break;
        //        }
        //    }

        //    //var atable_ESN_list = db.EquipmentInfo.Where(x => x.Area == Area).Select(x=>x.ESN).ToList();
        //    //var atable_SearchTable = db.EquipmentReportForm.Where(x=> atable_ESN_list.Contains(x.ESN));
        //    #endregion

        //    #region datagrid remoteSort 判斷有無 sort 跟 order  
        //    IValueProvider vp = form.ToValueProvider();
        //    if (vp.ContainsPrefix("sort") && vp.ContainsPrefix("order"))
        //    {
        //        string sort = form["sort"];
        //        string order = form["order"];
        //        SourceTable = OrderByField(SourceTable, sort, order == "asc");
        //    }
        //    else
        //    {
        //        SourceTable = SourceTable.OrderByDescending(x => x.Date);
        //    }
        //    #endregion

        //    var resulttable = SourceTable;
        //    //回傳JSON陣列
        //    JArray ja = new JArray();
        //    //記住總筆數
        //    int total = resulttable.Count();
        //    //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
        //    resulttable = resulttable.Skip((page - 1) * rows).Take(rows);


        //    foreach (var a in resulttable)
        //    {
        //        var itemObjects = new JObject();
        //        if (itemObjects["ReportState"] == null)
        //        {
        //            string statsSN = a.ReportState.Trim();
        //            var dic = Surface.EquipmentReportFormState();
        //            //string aaaaa = dic["8"];
        //            itemObjects.Add("ReportState", dic[statsSN]); //報修單狀態
        //            itemObjects.Add("ReportStatenum", statsSN); //報修單狀態編碼
        //        }
        //        if (itemObjects["ReportLevel"] == null)
        //        {
        //            string levelSN = a.ReportLevel.Trim();
        //            var dic = Surface.ReportLevel();
        //            itemObjects.Add("ReportLevel", dic[levelSN]); // 報修單等級
        //        }
        //        if (itemObjects["Area"] == null)
        //            itemObjects.Add("Area", a.Area);    //棟別                           

        //        if (itemObjects["Floor"] == null)
        //            itemObjects.Add("Floor", a.Floor);   //樓層

        //        if (itemObjects["ReportSource"] == null)
        //        {
        //            string sourcesn = a.ReportSource.Trim();
        //            var dic = Surface.ReportSource();   //報修來源
        //            itemObjects.Add("ReportSource", dic[sourcesn]);
        //        }
        //        if (itemObjects["RSN"] == null)
        //            itemObjects.Add("RSN", a.RSN);  //RSN

        //        if (itemObjects["Date"] == null)
        //            itemObjects.Add("Date", a.Date.ToString("yyyy/MM/dd HH:mm:ss"));                                //保養週期

        //        if (itemObjects["PropertyCode"] == null)
        //            itemObjects.Add("PropertyCode", a.PropertyCode);    //財產編碼
        //        if (itemObjects["ESN"] == null)
        //            itemObjects.Add("ESN", a.ESN);    //設備編號
        //        if (itemObjects["EName"] == null)
        //            itemObjects.Add("EName", a.EName);    //設備名稱
        //        if (itemObjects["ReportContent"] == null)
        //            itemObjects.Add("ReportContent", a.ReportContent);    //報修內容
        //        if (itemObjects["MyName"] == null)
        //            itemObjects.Add("MyName", a.MyName);    //報修人員
        //        if (a.StockState) //庫存狀態
        //        {
        //            itemObjects.Add("StockState", "有");
        //        }
        //        else
        //        {
        //            itemObjects.Add("StockState", "無");
        //        }
        //        ja.Add(itemObjects);
        //        if (!string.IsNullOrEmpty(a.EState))
        //        {
        //            var dic = Surface.EState();
        //            itemObjects.Add("EState", dic[a.EState]);
        //        }
        //        if (!string.IsNullOrEmpty(a.DBID.ToString()))
        //        {
        //            itemObjects.Add("DBID", a.DBID);
        //        }
        //        if (!string.IsNullOrEmpty(a.FSN.ToString()))
        //        {
        //            itemObjects.Add("FSN", a.FSN);
        //        }
        //        if (!string.IsNullOrEmpty(a.ASN.ToString()))
        //        {
        //            itemObjects.Add("ASN", a.ASN);
        //        }
        //    }

        //    JObject jo = new JObject();
        //    jo.Add("rows", ja);
        //    jo.Add("total", total);
        //    return jo;
        //}
        #endregion

        //--設備管理--
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
            //設備名稱
            string EName = form["EName"]?.ToString();
            //設備編號
            string NO = form["NO"]?.ToString();
            //設備狀態
            string EState = form["EState"]?.ToString();
            //棟別
            string ASN = form["ASN"]?.ToString();
            //樓層
            string FSN = form["FSN"]?.ToString();
            //設備廠牌
            string Brand = form["Brand"]?.ToString();
            //型號
            string Model = form["Model"]?.ToString();
            //設備廠商
            string Vendor = form["Vendor"]?.ToString();
            #endregion

            #region 依據查詢字串檢索資料表
            var Data = from x1 in db.EquipmentInfo
                       join x2 in db.Floor_Info on x1.FSN equals x2.FSN
                       join x3 in db.AreaInfo on x2.ASN equals x3.ASN
                       select new { x1.ESN, x1.EName, x1.NO, x1.EState, x2.ASN, x3.Area, x1.FSN, x2.FloorName, x1.Brand, x1.Model, x1.Vendor, x1.ContactPhone, x1.OperatingVoltage, x1.OtherInfo, x1.InstallDate, x1.Memo };
            if (!string.IsNullOrEmpty(EName))
            {
                Data = Data.Where(x => x.EName.Contains(EName));
            }

            if (!string.IsNullOrEmpty(NO))
            {
                Data = Data.Where(x => x.NO.Contains(NO));
            }

            if (!string.IsNullOrEmpty(EState))
            {
                Data = Data.Where(x => x.EState == EState);
            }
            if (!string.IsNullOrEmpty(ASN))
            {
                int intasn = Convert.ToInt32(ASN);
                Data = Data.Where(x => x.ASN == intasn);
            }

            if (!string.IsNullOrEmpty(FSN))
            {
                Data = Data.Where(x => x.FSN == FSN);
            }

            if (!string.IsNullOrEmpty(Brand))
            {
                Data = Data.Where(x => x.Brand.Contains(Brand));
            }

            if (!string.IsNullOrEmpty(Model))
            {
                Data = Data.Where(x => x.Model.Contains(Model));
            }

            if (!string.IsNullOrEmpty(Vendor))
            {
                Data = Data.Where(x => x.Vendor.Contains(Vendor));
            }

            #endregion

            #region datagrid remoteSort 判斷有無 sort 跟 order
            IValueProvider vp = form.ToValueProvider();
            if (vp.ContainsPrefix("sort") && vp.ContainsPrefix("order"))
            {
                string sort = form["sort"];
                string order = form["order"];
                Data = OrderByField(Data, sort, order == "asc");
            }
            else
            {
                Data = Data.OrderByDescending(x => x.NO);
            }
            #endregion

            var result = Data;
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
                if (!string.IsNullOrEmpty(item.EName))
                {
                    itemObjects.Add("EName", item.EName);
                }
                if (!string.IsNullOrEmpty(item.NO))
                {
                    itemObjects.Add("NO", item.NO);
                }
                if (!string.IsNullOrEmpty(item.EState))
                {
                    var dic = Surface.EState();
                    itemObjects.Add("EState", dic[item.EState]);
                }
                if (!string.IsNullOrEmpty(item.Area))
                {
                    itemObjects.Add("Area", item.Area);
                }
                if (!string.IsNullOrEmpty(item.FloorName))
                {
                    itemObjects.Add("FloorName", item.FloorName);
                }

                if (!string.IsNullOrEmpty(item.Brand))
                {
                    itemObjects.Add("Brand", item.Brand);
                }
                if (!string.IsNullOrEmpty(item.Model))
                {
                    itemObjects.Add("Model", item.Model);
                }
                if (!string.IsNullOrEmpty(item.Vendor))
                {
                    itemObjects.Add("Vendor", item.Vendor);
                }
                //FilePath
                //查設備操作手冊
                var filename = db.EquipmentOperatingManual.Where(x => x.EName == item.EName && x.Brand == item.Brand && x.Model == item.Model).Select(x => x.FilePath).FirstOrDefault();
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

        #region 一機一卡模板管理
        public JObject GetJsonForGrid_OneDeviceOneCard(System.Web.Mvc.FormCollection form)
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
            //模板名稱
            string SampleName = form["SampleName"]?.ToString();
            #endregion

            #region 依據查詢字串檢索資料表
            var query = db.Template_OneDeviceOneCard.Select(x => new
            {
                x.TSN,
                x.SampleName,
                AddItemsNum = x.Template_AddField.Count,
                MaintainNum = x.Template_MaintainItemSetting.Count,
                InspectNum = x.Template_CheckItem.Count + x.Template_ReportingItem.Count,
                Deletable = !x.EquipmentInfo.Any()
            });
            //模板名稱 (模糊查詢)
            if (!string.IsNullOrEmpty(SampleName))
            {
                query = query.Where(x => x.SampleName.Contains(SampleName));
            }
            #endregion

            #region datagrid remoteSort 判斷有無 sort 跟 order
            var sort = form["sort"]?.ToString();
            var order = form["order"]?.ToString();
            if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order)) query = query.OrderBy(sort + " " + order); // 使用 System.Linq.Dynamic.Core 套件進行動態排序
            else query = query.OrderByDescending(x => x.TSN);
            #endregion

            // 總筆數
            int Total = query.Count();
            // 切頁
            query = query.Skip((page - 1) * rows).Take(rows);

            //回傳JSON陣列
            JArray ja = new JArray();

            if (query != null || Total > 0)
            {
                foreach (var item in query)
                {
                    var itemObject = new JObject
                    {
                        { "TSN", item.TSN },
                        { "SampleName", item.SampleName },
                        { "AddItemsNum", item.AddItemsNum.ToString() },
                        { "MaintainNum", item.MaintainNum.ToString() },
                        { "InspectNum", item.InspectNum.ToString() },
                        { "Deletable", item.Deletable },
                    };

                    ja.Add(itemObject);
                }
            }

            JObject jo = new JObject
            {
                { "rows", ja },
                { "total", Total }
            };

            return jo;
        }
        #endregion

        #region 竣工圖說管理
        public JObject GetJsonForGrid_AsBuiltDrawing(System.Web.Mvc.FormCollection form)
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

            #region datagrid remoteSort 判斷有無 sort 跟 order
            IValueProvider vp = form.ToValueProvider();
            if (vp.ContainsPrefix("sort") && vp.ContainsPrefix("order"))
            {
                string sort = form["sort"];
                string order = form["order"];
                SourceTable = OrderByField(SourceTable, sort, order == "asc");
            }
            else
            {
                SourceTable = SourceTable.OrderByDescending(x => x.ADSN);
            }
            #endregion

            var result = SourceTable;
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
                itemObjects.Add("DSystem", item.DSystem);
                itemObjects.Add("DSubSystem", item.DSubSystem);
                itemObjects.Add("ImgNum", item.ImgNum);
                itemObjects.Add("ImgName", item.ImgName);
                itemObjects.Add("UploadDate", item.UploadDate.ToString("yyyy-MM-dd"));
                itemObjects.Add("ImgVersion", item.ImgVersion);
                itemObjects.Add("ImgPath", "/Files/AsBuiltDrawing" + item.ImgPath);

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

            #region datagrid remoteSort 判斷有無 sort 跟 order
            IValueProvider vp = form.ToValueProvider();
            if (vp.ContainsPrefix("sort") && vp.ContainsPrefix("order"))
            {
                string sort = form["sort"];
                string order = form["order"];
                SourceTable = OrderByField(SourceTable, sort, order == "asc");
            }
            else
            {
                SourceTable = SourceTable.OrderByDescending(x => x.DDSN);
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
                    itemObjects.Add("UploadDate", item.UploadDate.ToString("yyyy-MM-dd"));
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
            ////系統別
            //string System = form["System"]?.ToString();
            ////子系統別
            //string SubSystem = form["SubSystem"]?.ToString();
            //設備名稱
            string EName = form["EName"]?.ToString();
            //廠牌
            string Brand = form["Brand"]?.ToString();
            //型號
            string Model = form["Model"]?.ToString();
            ////日期起
            //string DateStart = form["DateStart"]?.ToString();
            ////日期迄
            //string DateEnd = form["DateEnd"]?.ToString();
            #endregion

            #region 依據查詢字串檢索資料表
            var Data = db.EquipmentOperatingManual.AsQueryable();

            //if (!string.IsNullOrEmpty(System))
            //{
            //    Data = Data.Where(x => x.System == System);
            //}
            //if (!string.IsNullOrEmpty(SubSystem))
            //{
            //    Data = Data.Where(x => x.SubSystem == SubSystem);
            //}
            //if (!string.IsNullOrEmpty(System))
            //{
            //    Data = Data.Where(x => x.System == System);
            //}
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

            #region datagrid remoteSort 判斷有無 sort 跟 order
            IValueProvider vp = form.ToValueProvider();
            if (vp.ContainsPrefix("sort") && vp.ContainsPrefix("order"))
            {
                string sort = form["sort"];
                string order = form["order"];
                Data = OrderByField(Data, sort, order == "asc");
            }
            else
            {
                Data = Data.OrderByDescending(x => x.EOMSN);
            }
            #endregion

            var result = Data;
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
                //itemObjects.Add("System", item.System);
                //itemObjects.Add("SubSystem", item.SubSystem);
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


        //--庫存管理--

        #region 庫存管理
        public JObject GetJsonForGrid_Stock_Management(System.Web.Mvc.FormCollection form)
        {
            //解析查詢字串
            var StockTypeSN = form["StockTypeSN"]?.ToString();//類別
            var StockName = form["StockName"]?.ToString();//品項名稱
            var StockStatus = form["StockStatus"]?.ToString();//狀態
                                                              // DataGrid參數
            var sort = form["sort"]?.ToString();
            var order = form["order"]?.ToString();
            //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
            int page = 1;
            if (!string.IsNullOrEmpty(form["page"]?.ToString())) page = short.Parse(form["page"].ToString());
            int rows = 10;
            if (!string.IsNullOrEmpty(form["rows"]?.ToString())) rows = short.Parse(form["rows"]?.ToString());

            var rpT = db.ComputationalStock.AsQueryable();

            //類別
            if (!string.IsNullOrEmpty(StockTypeSN)) rpT = rpT.Where(x => x.StockTypeSN.ToString() == StockTypeSN);
            //品項名稱 (模糊查詢)
            if (!string.IsNullOrEmpty(StockName)) rpT = rpT.Where(x => x.StockName.Contains(StockName));
            //狀態
            if (!string.IsNullOrEmpty(StockStatus)) rpT = rpT.Where(x => x.StockStatus == StockStatus);

            // 確認 sort 和 order 不為空才進行排序
            if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order)) rpT = rpT.OrderBy(sort + " " + order); // 使用 System.Linq.Dynamic.Core 套件進行動態排序
            else rpT = rpT.OrderBy(x => x.SISN);

            //記住總筆數
            int Total = rpT.Count();
            //切頁
            rpT = rpT.Skip((page - 1) * rows).Take(rows);

            //回傳JSON陣列
            JArray ja = new JArray();

            if (rpT != null || Total > 0)
            {
                var dic_stockStatus = Surface.StockStatus();
                foreach (var item in rpT)
                {
                    var itemObject = new JObject();
                    itemObject.Add("SISN", item.SISN);
                    itemObject.Add("StockType", db.StockType.Find(item.StockTypeSN).StockTypeName.ToString());
                    itemObject.Add("StockName", item.StockName);
                    itemObject.Add("StockStatus", dic_stockStatus[item.StockStatus]);
                    itemObject.Add("StockAmount", item.StockAmount);
                    itemObject.Add("Unit", item.Unit);
                    itemObject.Add("MinStockAmount", item.MinStockAmount);

                    var haverecord = db.StockChangesRecord.Where(x => x.SISN == item.SISN).Count();
                    if (haverecord > 0)
                    {
                        itemObject.Add("CanDelete", false);
                    }
                    else
                    {
                        itemObject.Add("CanDelete", true);
                    }

                    ja.Add(itemObject);
                }
            }

            JObject jo = new JObject
                {
                    { "rows", ja },
                    { "total", Total }
                };

            return jo;
        }
        #endregion

        #region 庫存品項詳情_庫存變更紀錄
        public JObject GetJsonForGrid_StockChangeRecord(System.Web.Mvc.FormCollection form)
        {
            //解析查詢字串
            var SISN = form["SISN"]?.ToString();
            // DataGrid參數
            var sort = form["sort"]?.ToString();
            var order = form["order"]?.ToString();
            //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
            int page = 1;
            if (!string.IsNullOrEmpty(form["page"]?.ToString())) page = short.Parse(form["page"].ToString());
            int rows = 10;
            if (!string.IsNullOrEmpty(form["rows"]?.ToString())) rows = short.Parse(form["rows"]?.ToString());

            var rpT = db.StockChangesRecord.Where(x => x.SISN == SISN).AsQueryable();

            // 確認 sort 和 order 不為空才進行排序
            if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order)) rpT = rpT.OrderBy(sort + " " + order); // 使用 System.Linq.Dynamic.Core 套件進行動態排序
            else rpT = rpT.OrderByDescending(x => x.ChangeTime);

            //記住總筆數
            int Total = rpT.Count();
            //切頁
            rpT = rpT.Skip((page - 1) * rows).Take(rows);

            //回傳JSON陣列
            JArray ja = new JArray();

            if (rpT != null || Total > 0)
            {
                foreach (var item in rpT)
                {
                    var itemObject = new JObject();
                    itemObject.Add("DateTime", item.ChangeTime.ToString("yyyy-MM-dd HH:mm:ss")); //日期時間
                    itemObject.Add("Registrant", db.AspNetUsers.Where(x => x.UserName == item.Registrar).FirstOrDefault()?.MyName.ToString() ?? null); //登記人
                    if (item.ChangeType == "1")//出庫
                    {
                        itemObject.Add("InboundNum", null); //入庫數量
                        itemObject.Add("OutboundNum", item.NumberOfChanges); //出庫數量
                    }
                    else
                    { //入庫
                        itemObject.Add("InboundNum", item.NumberOfChanges); //入庫數量
                        itemObject.Add("OutboundNum", null); //出庫數量
                    }
                    itemObject.Add("Taker", item.Recipient); //取用人
                    itemObject.Add("Document", item.PurchaseOrder != null ? "/Files/PurchaseOrder/" + item.PurchaseOrder : null); //採購單據
                    itemObject.Add("StockNum", item.CurrentInventory); //庫存數量
                    itemObject.Add("Memo", item.Memo); //備註

                    ja.Add(itemObject);
                }

            }
            JObject jo = new JObject
                {
                    { "rows", ja },
                    { "total", Total }
                };

            return jo;

        }
        #endregion

        //--實驗室管理--
        #region 採驗分析流程建立
        public JObject GetJsonForGrid_TestingAndAnalysisWorkflow(System.Web.Mvc.FormCollection form)
        {
            //解析查詢字串
            var ExperimentType = form["ExperimentType"]?.ToString();
            var ExperimentName = form["ExperimentName"]?.ToString();
            // DataGrid參數
            var sort = form["sort"]?.ToString();
            var order = form["order"]?.ToString();
            //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
            int page = 1;
            if (!string.IsNullOrEmpty(form["page"]?.ToString())) page = short.Parse(form["page"].ToString());
            int rows = 10;
            if (!string.IsNullOrEmpty(form["rows"]?.ToString())) rows = short.Parse(form["rows"]?.ToString());

            var rpT = db.TestingAndAnalysisWorkflow.AsQueryable();
            //查詢實驗類型 (模糊查詢)
            if (!string.IsNullOrEmpty(ExperimentType)) rpT = rpT.Where(x => x.ExperimentType.Contains(ExperimentType));
            //查詢實驗名稱 (模糊查詢)
            if (!string.IsNullOrEmpty(ExperimentName)) rpT = rpT.Where(x => x.ExperimentName.Contains(ExperimentName));

            // 確認 sort 和 order 不為空才進行排序
            if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order)) rpT = rpT.OrderBy(sort + " " + order); // 使用 System.Linq.Dynamic.Core 套件進行動態排序
            else rpT = rpT.OrderByDescending(x => x.TAWSN);

            // 記住總筆數
            int Total = rpT.Count();
            // 切頁
            rpT = rpT.Skip((page - 1) * rows).Take(rows);

            //回傳JSON陣列
            JArray ja = new JArray();

            if (rpT != null || Total > 0)
            {
                foreach (var item in rpT)
                {
                    var itemObject = new JObject
                    {
                        { "TAWSN", item.TAWSN },
                        { "ExperimentType", item.ExperimentType },
                        { "ExperimentName", item.ExperimentName }
                    };

                    ja.Add(itemObject);
                }
            }

            JObject jo = new JObject
            {
                { "rows", ja },
                { "total", Total }
            };

            return jo;
        }
        #endregion

        #region 實驗室標籤管理
        public JObject GetJsonForGrid_LaboratoryLabel_Management(System.Web.Mvc.FormCollection form)
        {
            //解析查詢字串
            var ExperimentType = form["ExperimentType"]?.ToString();
            var ExperimentName = form["ExperimentName"]?.ToString();
            var EDateStart = form["EDateStart"]?.ToString();
            var EDateEnd = form["EDateEnd"]?.ToString();
            // DataGrid參數
            var sort = form["sort"]?.ToString();
            var order = form["order"]?.ToString();
            //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
            int page = 1;
            if (!string.IsNullOrEmpty(form["page"]?.ToString())) page = short.Parse(form["page"].ToString());
            int rows = 10;
            if (!string.IsNullOrEmpty(form["rows"]?.ToString())) rows = short.Parse(form["rows"]?.ToString());

            var rpT = from e in db.ExperimentalLabel
                      join t in db.TestingAndAnalysisWorkflow
                      on e.TAWSN equals t.TAWSN
                      select new
                      {
                          e.ELSN,
                          t.ExperimentType,
                          t.ExperimentName,
                          e.EDate
                      };
            //查詢實驗類型 (模糊查詢)
            if (!string.IsNullOrEmpty(ExperimentType)) rpT = rpT.Where(x => x.ExperimentType.Contains(ExperimentType));
            //查詢實驗名稱 (模糊查詢)
            if (!string.IsNullOrEmpty(ExperimentName)) rpT = rpT.Where(x => x.ExperimentName.Contains(ExperimentName));
            //查詢實驗日期(起)
            if (!string.IsNullOrEmpty(EDateStart) && DateTime.Parse(EDateStart) != DateTime.MinValue)
            {
                DateTime start = DateTime.Parse(EDateStart);  // 轉為DateTime
                rpT = rpT.Where(x => x.EDate >= start);
            }
            //查詢實驗日期(迄)
            if (!string.IsNullOrEmpty(EDateEnd) && DateTime.Parse(EDateEnd) != DateTime.MinValue)
            {
                DateTime end = DateTime.Parse(EDateEnd).AddDays(1);
                rpT = rpT.Where(x => x.EDate < end);
            }

            // 確認 sort 和 order 不為空才進行排序
            if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order)) rpT = rpT.OrderBy(sort + " " + order); // 使用 System.Linq.Dynamic.Core 套件進行動態排序
            else rpT = rpT.OrderByDescending(x => x.ELSN);

            // 記住總筆數
            int Total = rpT.Count();
            // 切頁
            rpT = rpT.Skip((page - 1) * rows).Take(rows);

            //回傳JSON陣列
            JArray ja = new JArray();

            if (rpT != null || Total > 0)
            {
                foreach (var item in rpT)
                {
                    var itemObject = new JObject
                    {
                        { "ELSN", item.ELSN },
                        { "ExperimentType", item.ExperimentType },
                        { "ExperimentName", item.ExperimentName },
                        { "EDate", item.EDate.ToString("yyyy-MM-dd") },
                    };

                    ja.Add(itemObject);
                }
            }

            JObject jo = new JObject
            {
                { "rows", ja },
                { "total", Total }
            };

            return jo;
        }
        #endregion

        #region 實驗室維護管理
        public JObject GetJsonForGrid_LaboratoryMaintenance_Management(System.Web.Mvc.FormCollection form)
        {
            //解析查詢字串
            var MType = form["MType"]?.ToString();
            var MTitle = form["MTitle"]?.ToString();
            var MContent = form["MContent"]?.ToString();
            // DataGrid參數
            var sort = form["sort"]?.ToString();
            var order = form["order"]?.ToString();
            //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
            int page = 1;
            if (!string.IsNullOrEmpty(form["page"]?.ToString())) page = short.Parse(form["page"].ToString());
            int rows = 10;
            if (!string.IsNullOrEmpty(form["rows"]?.ToString())) rows = short.Parse(form["rows"]?.ToString());

            var rpT = from m in db.LaboratoryMaintenance
                      join u in db.AspNetUsers on m.UploadUserName equals u.UserName into UserGroup
                      from ug in UserGroup.DefaultIfEmpty()
                      select new
                      {
                          m.LMSN,
                          m.MType,
                          m.MTitle,
                          m.MContent,
                          m.UploadDateTime,
                          UploadUserName = ug != null ? ug.MyName : null
                      };


            //查詢維護類型
            if (!string.IsNullOrEmpty(MType)) rpT = rpT.Where(x => x.MType == MType);
            //查詢標題 (模糊查詢)
            if (!string.IsNullOrEmpty(MTitle)) rpT = rpT.Where(x => x.MTitle.Contains(MTitle));
            //查詢說明 (模糊查詢)
            if (!string.IsNullOrEmpty(MContent)) rpT = rpT.Where(x => x.MContent.Contains(MContent));

            // 確認 sort 和 order 不為空才進行排序
            if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order)) rpT = rpT.OrderBy(sort + " " + order); // 使用 System.Linq.Dynamic.Core 套件進行動態排序
            else rpT = rpT.OrderByDescending(x => x.LMSN);

            // 記住總筆數
            int Total = rpT.Count();
            // 切頁
            rpT = rpT.Skip((page - 1) * rows).Take(rows);

            //回傳JSON陣列
            JArray ja = new JArray();

            if (rpT != null || Total > 0)
            {
                foreach (var item in rpT)
                {
                    var itemObject = new JObject
                    {
                        { "LMSN", item.LMSN },
                        { "MType", item.MType },
                        { "MTitle", item.MTitle },
                        { "MContent", item.MContent },
                        { "UploadUserName", item.UploadUserName },
                        { "UploadDateTime", item.UploadDateTime?.ToString("yyyy-MM-dd HH:mm:ss") },
                    };

                    ja.Add(itemObject);
                }
            }

            JObject jo = new JObject
            {
                { "rows", ja },
                { "total", Total }
            };

            return jo;
        }
        #endregion

        #region 實驗數據管理
        public JObject GetJsonForGrid_ExperimentData_Management(System.Web.Mvc.FormCollection form)
        {
            //解析查詢字串
            var ExperimentType = form["ExperimentType"]?.ToString();
            var ExperimentName = form["ExperimentName"]?.ToString();
            var EDateStart = form["EDateStart"]?.ToString();
            var EDateEnd = form["EDateEnd"]?.ToString();
            // DataGrid參數
            var sort = form["sort"]?.ToString();
            var order = form["order"]?.ToString();
            //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
            int page = 1;
            if (!string.IsNullOrEmpty(form["page"]?.ToString())) page = short.Parse(form["page"].ToString());
            int rows = 10;
            if (!string.IsNullOrEmpty(form["rows"]?.ToString())) rows = short.Parse(form["rows"]?.ToString());

            var rpT = from e in db.ExperimentalDataRecord
                      join t in db.TestingAndAnalysisWorkflow
                      on e.TAWSN equals t.TAWSN
                      select new
                      {
                          e.EDRSN,
                          t.ExperimentType,
                          t.ExperimentName,
                          EDDate = e.EDate
                      };
            //查詢實驗類型 (模糊查詢)
            if (!string.IsNullOrEmpty(ExperimentType)) rpT = rpT.Where(x => x.ExperimentType.Contains(ExperimentType));
            //查詢實驗名稱 (模糊查詢)
            if (!string.IsNullOrEmpty(ExperimentName)) rpT = rpT.Where(x => x.ExperimentName.Contains(ExperimentName));
            //查詢實驗日期(起)
            if (!string.IsNullOrEmpty(EDateStart) && DateTime.Parse(EDateStart) != DateTime.MinValue)
            {
                DateTime start = DateTime.Parse(EDateStart);  // 轉為DateTime
                rpT = rpT.Where(x => x.EDDate >= start);
            }
            //查詢實驗日期(迄)
            if (!string.IsNullOrEmpty(EDateEnd) && DateTime.Parse(EDateEnd) != DateTime.MinValue)
            {
                DateTime end = DateTime.Parse(EDateEnd).AddDays(1);
                rpT = rpT.Where(x => x.EDDate < end);
            }

            // 確認 sort 和 order 不為空才進行排序
            if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order)) rpT = rpT.OrderBy(sort + " " + order); // 使用 System.Linq.Dynamic.Core 套件進行動態排序
            else rpT = rpT.OrderByDescending(x => x.EDRSN);

            // 記住總筆數
            int Total = rpT.Count();
            // 切頁
            rpT = rpT.Skip((page - 1) * rows).Take(rows);

            //回傳JSON陣列
            JArray ja = new JArray();

            if (rpT != null || Total > 0)
            {
                foreach (var item in rpT)
                {
                    var itemObject = new JObject
                    {
                        { "EDRSN", item.EDRSN },
                        { "ExperimentType", item.ExperimentType },
                        { "ExperimentName", item.ExperimentName },
                        { "EDDate", item.EDDate.ToString("yyyy-MM-dd") },
                    };

                    ja.Add(itemObject);
                }
            }

            JObject jo = new JObject
            {
                { "rows", ja },
                { "total", Total }
            };

            return jo;
        }
        #endregion

        //--警示訊息管理--
        #region 警示訊息管理
        public JObject GetJsonForGrid_WarningMessage_Management(System.Web.Mvc.FormCollection form)
        {
            //解析查詢字串
            var WMType = form["WMType"]?.ToString();
            var WMState = form["WMState"]?.ToString();
            var ASN = form["ASN"]?.ToString();
            var Area = form["Area"]?.ToString();
            var FSN = form["FSN"]?.ToString();
            var Floor = form["Floor"]?.ToString();
            var Message = form["Message"]?.ToString();
            var MyName = form["MyName"]?.ToString();
            var DateStart = form["DateStart"]?.ToString();
            var DateEnd = form["DateEnd"]?.ToString();
            // DataGrid參數
            var sort = form["sort"]?.ToString();
            var order = form["order"]?.ToString();
            //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
            int page = 1;
            if (!string.IsNullOrEmpty(form["page"]?.ToString())) page = short.Parse(form["page"].ToString());
            int rows = 10;
            if (!string.IsNullOrEmpty(form["rows"]?.ToString())) rows = short.Parse(form["rows"]?.ToString());

            var rpT = from x1 in db.WarningMessage
                      join x3 in db.Floor_Info
                      on x1.FSN  equals x3.FSN  into floorJoin
                      from x3 in floorJoin.DefaultIfEmpty() //left join
                      join x4 in db.AspNetUsers
                      on x1.UserName equals x4.UserName
                      join x5 in db.AreaInfo
                      on x3.ASN equals x5.ASN into areaJoin
                      from x5 in areaJoin.DefaultIfEmpty()
                      select new
                      {
                          x1.WMSN,
                          x1.WMType,
                          x1.WMState,
                          x1.TimeOfOccurrence,
                          x1.FSN,
                          x1.Message,
                          ASN = x3 != null ? x3.ASN.ToString() : null,
                          FloorName = x3 != null ? x3.FloorName : null,
                          x4.MyName,
                          Area = x5 != null ? x5.Area : null
                      };
            int Totalc = rpT.Count();

            //查詢事件等級
            if (!string.IsNullOrEmpty(WMType))
            {
                rpT = rpT.Where(x => x.WMType == WMType);
            }
            //查詢事件處理狀況
            if (!string.IsNullOrEmpty(WMState))
            {
                rpT = rpT.Where(x => x.WMState == WMState);
            }
            //查詢棟別
            if (!string.IsNullOrEmpty(ASN))
            {
                var numASN = Int32.Parse(ASN);
                rpT = rpT.Where(x => x.ASN == numASN.ToString());
            }
            if (!string.IsNullOrEmpty(Area))
            {
                rpT = rpT.Where(x => x.Area == Area);
            }
            //查詢樓層
            if (!string.IsNullOrEmpty(FSN))
            {
                rpT = rpT.Where(x => x.FSN == FSN);
            }
            if (!string.IsNullOrEmpty(Floor))
            {
                rpT = rpT.Where(x => x.FloorName == Floor);
            }
            //查詢事件內容(模糊查詢)
            if (!string.IsNullOrEmpty(Message))
            {
                rpT = rpT.Where(x => x.Message.Contains(Message));
            }
            //查詢人員姓名(模糊查詢)
            if (!string.IsNullOrEmpty(MyName))
            {
                rpT = rpT.Where(x => x.MyName.Contains(MyName));
            }
            //查詢發生時間(起)
            if (!string.IsNullOrEmpty(DateStart) && DateTime.Parse(DateStart) != DateTime.MinValue)
            {
                DateTime start = DateTime.Parse(DateStart); //轉為DateTime
                rpT = rpT.Where(x => x.TimeOfOccurrence >= start);
            }
            //查詢發生時間(迄)
            if (!string.IsNullOrEmpty(DateEnd) && DateTime.Parse(DateEnd) != DateTime.MinValue)
            {
                DateTime end = DateTime.Parse(DateEnd); //轉為DateTime
                rpT = rpT.Where(x => x.TimeOfOccurrence <= end);
            }

            // 確認 sort 和 order 不為空才進行排序
            if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order)) rpT = rpT.OrderBy(sort + " " + order); // 使用 System.Linq.Dynamic.Core 套件進行動態排序
            else rpT = rpT.OrderByDescending(x => x.WMSN);

            // 記住總筆數
            int Total = rpT.Count();
            // 切頁
            rpT = rpT.Skip((page - 1) * rows).Take(rows);

            //回傳JSON陣列
            JArray ja = new JArray();

            var WMTypedic = Surface.WMType();
            var WMStatedic = Surface.WMState();

            if (rpT != null || Total > 0)
            {
                foreach (var item in rpT)
                {
                    var itemObject = new JObject //DataGrid顯示的資料內容
                    {
                        { "WMSN", item.WMSN },
                        { "WMType", WMTypedic[item.WMType] },
                        { "WMState", WMStatedic[item.WMState] },
                        { "TimeOfOccurrence", item.TimeOfOccurrence.ToString("yyyy-MM-dd HH:mm:ss") },
                        { "Location", item.Area + " " + item.FloorName },
                        { "Message", item.Message },
                        { "MyName", item.MyName },
                    };

                    ja.Add(itemObject);
                }
            }

            JObject jo = new JObject
            {
                { "rows", ja },
                { "total", Total }
            };

            return jo;
        }
        #endregion

        //--文件管理--
        #region 月報管理
        public JObject GetJsonForGrid_MonthlyReport_Management(System.Web.Mvc.FormCollection form)
        {
            #region datagrid呼叫時的預設參數有 rows 跟 page
            int page = 1;
            if (!string.IsNullOrEmpty(form["page"]?.ToString())) page = short.Parse(form["page"].ToString());
            int rows = 10;
            if (!string.IsNullOrEmpty(form["rows"]?.ToString())) rows = short.Parse(form["rows"]?.ToString());
            #endregion

            #region 塞來自formdata的資料
            string reportTitle = form["ReportTitle"]?.ToString();
            string uploadUserName = form["UploadUserName"]?.ToString();

            string dateFrom = form["DateFrom"]?.ToString();
            string dateTo = form["DateTo"]?.ToString();
            #endregion

            #region 依據查詢字串檢索資料表
            var mr = from x in db.MonthlyReport
                     select new { x.ReportTitle, x.UploadUserName, YearMonth = x.Year + "-" + x.Month, x.MRSN, x.ReportContent, x.UploadDateTime };

            if (!string.IsNullOrEmpty(reportTitle)) mr = mr.Where(x => x.ReportTitle.Contains(reportTitle));
            if (!string.IsNullOrEmpty(uploadUserName)) mr = mr.Where(x => x.UploadUserName.Contains(uploadUserName));

            if (!string.IsNullOrEmpty(dateFrom))
            {
                var datestart = DateTime.Parse(dateFrom);
                mr = mr.ToList().Where(x => DateTime.ParseExact(x.YearMonth, "yyyy-MM", CultureInfo.InvariantCulture) >= datestart).AsQueryable();
            }

            if (!string.IsNullOrEmpty(dateTo))
            {
                var dateend = DateTime.Parse(dateTo).AddMonths(1); // Add one month to include records up to the end of the specified month
                mr = mr.ToList().Where(x => DateTime.ParseExact(x.YearMonth, "yyyy-MM", CultureInfo.InvariantCulture) < dateend).AsQueryable();
            }
            #endregion

            #region datagrid remoteSort 判斷有無 sort 跟 order
            IValueProvider vp = form.ToValueProvider();
            if (vp.ContainsPrefix("sort") && vp.ContainsPrefix("order"))
            {
                string sort = form["sort"]?.ToString();
                string order = form["order"]?.ToString();

                if (order == "asc")
                {
                    mr = OrderByField(mr, sort, true);
                }
                else if (order == "desc")
                {
                    mr = OrderByField(mr, sort, false);
                }
            }
            else
            {
                mr = mr.OrderByDescending(x => x.MRSN);
            }
            #endregion

            JArray ja = new JArray();
            int total = mr.Count();
            mr = mr.Skip((page - 1) * rows).Take(rows);
            foreach (var item in mr)
            {
                var itemObjects = new JObject
                    {
                        { "MRSN", item.MRSN },
                        { "ReportTitle", item.ReportTitle },
                        { "UploadUserName", item.UploadUserName },
                        { "ReportContent", item.ReportContent },
                        { "YearMonth", item.YearMonth },
                        { "UploadDateTime", (item.UploadDateTime != DateTime.MinValue && item.UploadDateTime != null) ? item.UploadDateTime.ToString("yyyy-MM-dd") : null }
                    };
                ja.Add(itemObjects);
            }
            JObject jo = new JObject();
            jo.Add("rows", ja);
            jo.Add("total", total);
            return jo;

        }
        #endregion

        #region 會議記錄管理
        public JObject GetJsonForGrid_MeetingMinutes_Management(System.Web.Mvc.FormCollection form)
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
            //會議地點
            string MeetingTopic = form["MeetingTopic"]?.ToString();
            //會議主席
            string Chairperson = form["Chairperson"]?.ToString();
            //會議地點
            string MeetingVenue = form["MeetingVenue"]?.ToString();
            //起始日期
            string DateFrom = form["DateFrom"]?.ToString();
            //結束日期
            string DateTo = form["DateTo"]?.ToString();
            #endregion

            #region 依據查詢字串檢索資料表
            var SourceTable = from x in db.MeetingMinutes select new { x.MMSN, x.MeetingTopic, x.MeetingDate, x.MeetingDateStart, x.MeetingDateEnd, x.MeetingVenue, x.Chairperson, x.Participant, x.ExpectedAttendence, x.ActualAttendence, x.AbsenteeList, x.TakeTheMinutes, x.Agenda, x.MeetingContent };
            if (!string.IsNullOrEmpty(MeetingTopic)) SourceTable = SourceTable.Where(x => x.MeetingTopic.Contains(MeetingTopic));
            if (!string.IsNullOrEmpty(Chairperson)) SourceTable = SourceTable.Where(x => x.Chairperson.Contains(Chairperson));
            if (!string.IsNullOrEmpty(MeetingVenue)) SourceTable = SourceTable.Where(x => x.MeetingVenue.Contains(MeetingVenue));
            //日期(起)
            if (!string.IsNullOrEmpty(DateFrom))
            {
                var datefrom = DateTime.Parse(DateFrom);
                SourceTable = SourceTable.Where(x => x.MeetingDate >= datefrom);
            }
            //日期(迄)
            if (!string.IsNullOrEmpty(DateTo))
            {
                var dateto = DateTime.Parse(DateTo).AddDays(1);
                SourceTable = SourceTable.Where(x => x.MeetingDate < dateto);
            }
            #endregion

            #region datagrid remoteSort 判斷有無 sort 跟 order
            IValueProvider vp = form.ToValueProvider();
            if (vp.ContainsPrefix("sort") && vp.ContainsPrefix("order"))
            {
                string sort = form["sort"];
                string order = form["order"];
                SourceTable = OrderByField(SourceTable, sort, order == "asc");
            }
            else
            {
                SourceTable = SourceTable.OrderByDescending(x => x.MMSN);
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
                JObject itemObject = new JObject();

                if (!string.IsNullOrEmpty(item.MMSN)) itemObject.Add("MMSN", item.MMSN);
                if (!string.IsNullOrEmpty(item.MeetingTopic)) itemObject.Add("MeetingTopic", item.MeetingTopic);

                itemObject.Add("MeetingDate", $"{item.MeetingDate:yyyy-MM-dd} {item.MeetingDateStart:hh:mm}-{item.MeetingDateEnd:hh:mm}");

                if (!string.IsNullOrEmpty(item.MeetingVenue)) itemObject.Add("MeetingVenue", item.MeetingVenue);
                if (!string.IsNullOrEmpty(item.Chairperson)) itemObject.Add("Chairperson", item.Chairperson);
                if (!string.IsNullOrEmpty(item.Participant)) itemObject.Add("Participant", item.Participant);

                itemObject.Add("ExpectedAttendence", item.ExpectedAttendence);
                itemObject.Add("ActualAttendence", item.ActualAttendence);
                itemObject.Add("AbsenteeList", item.AbsenteeList);

                if (!string.IsNullOrEmpty(item.TakeTheMinutes)) itemObject.Add("TakeTheMinutes", item.TakeTheMinutes);
                if (!string.IsNullOrEmpty(item.Agenda)) itemObject.Add("Agenda", item.Agenda);
                if (!string.IsNullOrEmpty(item.MeetingContent)) itemObject.Add("MeetingContent", item.MeetingContent);
                ja.Add(itemObject);
            }

            JObject jo = new JObject
            {
                { "rows", ja },
                { "total", total }
            };
            return jo;
        }
        #endregion


        //--系統管理--
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

            #region datagrid remoteSort 判斷有無 sort 跟 order
            IValueProvider vp = form.ToValueProvider();
            if (vp.ContainsPrefix("sort") && vp.ContainsPrefix("order"))
            {
                string sort = form["sort"];
                string order = form["order"];
                Data = OrderByField(Data, sort, order == "asc");
            }
            else
            {
                Data = Data.OrderByDescending(x => x.UserName);
            }
            #endregion

            var result = Data;
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

            #region 塞來自formdata的資料
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
            #endregion

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

            #region datagrid remoteSort 判斷有無 sort 跟 order
            IValueProvider vp = form.ToValueProvider();
            if (vp.ContainsPrefix("sort") && vp.ContainsPrefix("order"))
            {
                string sort = form["sort"];
                string order = form["order"];
                SourceTable = OrderByField(SourceTable, sort, order == "asc");
            }
            else
            {
                SourceTable = SourceTable.OrderByDescending(x => x.MFRSN);
            }
            #endregion

            var resulttable = SourceTable;
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


    }
}

