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
            var SourceTable = db.InspectionPlan.AsQueryable();

            //工單編號
            if (!string.IsNullOrEmpty(PlanState))
            {
                SourceTable = SourceTable.Where(x => x.PlanState.Contains(PlanState));
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
                    itemObjects.Add("PlanDate", item.PlanDate.ToString("yyyy/MM/dd"));
                }
                //工單名稱
                if (!string.IsNullOrEmpty(item.IPName))
                {
                    itemObjects.Add("IPName", item.IPName);
                }
                //執行人員
                var IPTSNlist = db.InspectionPlan_Time.Where(x => x.IPSN == item.IPSN).Select(x => x.IPTSN).ToList();
                var IPUseridlist = db.InspectionPlan_Member.Where(x => IPTSNlist.Contains(x.IPTSN)).Select(x => x.UserID).Distinct().ToList();
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
                itemObjects.Add("Member", INSPNameList);

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
            var SourceTable = db.InspectionPathSample.AsQueryable();

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
                itemObjects.Add("InspectionNum", db.InspectionDefaultOrder.Where(x => x.PlanPathSN == a.PlanPathSN).Count());//巡檢數量
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
            var SourceTable = db.DailyInspectionSample.AsQueryable();

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
                itemObjects.Add("InspectionNum", db.DailyInspectionSampleContent.Where(x => x.DailyTemplateSN == a.DailyTemplateSN).Count());//巡檢路線數量
                ja.Add(itemObjects);
            }

            JObject jo = new JObject();
            jo.Add("rows", ja);
            jo.Add("total", total);
            return jo;
        }
        #endregion

        #region 巡檢紀錄_設備保養紀錄
        //public string GetJsonForGrid_InspectationPlan_Record_EquipMaintain(System.Web.Mvc.FormCollection form)
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

        //    string IPSN = form["IPSN"].ToString();

        //    var SourceTable = db.InspectionPlanMaintain.Where(x => x.IPSN == IPSN);

        //    var resulttable = SourceTable.OrderByDescending(x => x.EMFISN).AsQueryable();
        //    //回傳JSON陣列
        //    JArray ja = new JArray();
        //    //記住總筆數
        //    int total = resulttable.Count();
        //    //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
        //    resulttable = resulttable.Skip((page - 1) * rows).Take(rows);

        //    foreach (var item in resulttable)
        //    {
        //        var itemObjects = new JObject();
        //        itemObjects.Add("IPMSN", item.IPMSN);
        //        itemObjects.Add("IPSN", IPSN);
        //        var dic_IPMS = Surfaces.Surface.InspectionPlanMaintainState();
        //        itemObjects.Add("MaintainState", dic_IPMS[item.MaintainState]);
        //        var EMFI = db.EquipmentMaintainFormItem.Find(item.EMFISN);
        //        var EMI = db.EquipmentMaintainItem.Find(EMFI.EMISN);
        //        var EI = db.EquipmentInfo.Find(EMI.ESN);
        //        var MI = db.MaintainItem.Find(EMI.MISN);
        //        itemObjects.Add("Area", EI.Area);
        //        itemObjects.Add("Floor", EI.Floor);
        //        itemObjects.Add("ESN", EI.ESN);
        //        var dic_EState = Surfaces.Surface.EState();
        //        itemObjects.Add("EState", dic_EState[EI.EState]);
        //        itemObjects.Add("EName", EI.EName);
        //        itemObjects.Add("EMFISN", item.EMFISN);
        //        itemObjects.Add("MIName", MI.MIName);
        //        itemObjects.Add("Unit", EMFI.Unit);
        //        itemObjects.Add("Period", EMFI.Period.ToString());
        //        itemObjects.Add("LastTime", EMFI.LastTime.ToString("yyyy/MM/dd"));
        //        itemObjects.Add("NextTime", EMFI.NextTime?.ToString("yyyy/MM/dd"));
        //        ja.Add(itemObjects);
        //    }

        //    JObject jo = new JObject();
        //    jo.Add("rows", ja);
        //    jo.Add("total", total);
        //    string reString = JsonConvert.SerializeObject(jo);
        //    return reString;
        //}
        #endregion

        #region 巡檢紀錄_設備維修紀錄
        //public string GetJsonForGrid_InspectationPlan_Record_EquipRepair(System.Web.Mvc.FormCollection form)
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

        //    string IPSN = form["IPSN"].ToString();

        //    var SourceTable = db.InspectionPlanRepair.Where(x => x.IPSN == IPSN);

        //    var resulttable = SourceTable.OrderByDescending(x => x.IPRSN).AsQueryable();
        //    //回傳JSON陣列
        //    JArray ja = new JArray();
        //    //記住總筆數
        //    int total = resulttable.Count();
        //    //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
        //    resulttable = resulttable.Skip((page - 1) * rows).Take(rows);


        //    foreach (var item in resulttable)
        //    {
        //        var itemObjects = new JObject();
        //        itemObjects.Add("IPRSN", item.IPRSN);
        //        itemObjects.Add("IPSN", IPSN);
        //        var dic_IPRS = Surfaces.Surface.InspectionPlanRepairState();
        //        itemObjects.Add("RepairState", dic_IPRS[item.RepairState]);
        //        var ERF = db.EquipmentReportForm.Find(item.RSN);
        //        var EI = db.EquipmentInfo.Find(ERF.ESN);
        //        var dic_RL = Surfaces.Surface.ReportLevel();
        //        itemObjects.Add("ReportLevel", dic_RL[ERF.ReportLevel]);
        //        itemObjects.Add("Area", EI.Area);
        //        itemObjects.Add("Floor", EI.Floor);
        //        itemObjects.Add("RSN", item.RSN);
        //        itemObjects.Add("Date", ERF.Date.ToString("yyyy/MM/dd HH:mm:ss"));
        //        itemObjects.Add("ESN", ERF.ESN);
        //        itemObjects.Add("EName", EI.EName);
        //        var Name = db.AspNetUsers.Where(x => x.UserName == ERF.InformatUserID).Select(x => x.MyName).FirstOrDefault();
        //        itemObjects.Add("InformantUserID", Name);
        //        itemObjects.Add("ReportContent", ERF.ReportContent);
        //        ja.Add(itemObjects);
        //    }


        //    JObject jo = new JObject();
        //    jo.Add("rows", ja);
        //    jo.Add("total", total);
        //    string reString = JsonConvert.SerializeObject(jo);
        //    return reString;
        //}
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
                if (!string.IsNullOrEmpty(form["Maintainer"]?.ToString()))
                {
                    maintenanceForms = maintenanceForms.Where(x => x.Equipment_MaintenanceFormMember.Any(m => m.Maintainer.Contains(form["Maintainer"])));
                }
                #endregion

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

                #region 塞回傳資料
                maintenanceForms = maintenanceForms.OrderByDescending(x => x.NextMaintainDate);

                // 回傳JSON陣列
                JArray ja = new JArray();
                // 總筆數
                int total = maintenanceForms.Count();
                // 回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
                maintenanceForms = maintenanceForms.Skip((page - 1) * rows).Take(rows);

                foreach (var item in maintenanceForms)
                {
                    JObject itemObject = new JObject();
                    itemObject.Add("Status", Surface.MaintainStatus()[item.Status]); // 保養單狀態
                    itemObject.Add("EMFSN", item.EMFSN); // 保養單號
                    itemObject.Add("NextMaintainDate", item.NextMaintainDate.ToString("yyyy/MM/dd")); // 最近應保養日期
                    itemObject.Add("ReportTime", item.ReportTime?.ToString("yyyy/MM/dd") ?? ""); // 實際保養日期
                    itemObject.Add("EState", Surface.EState()[item.EquipmentInfo.EState]); // 設備狀態
                    itemObject.Add("ASN", item.EquipmentInfo.Floor_Info.AreaInfo.Area); // 棟別 (區域)
                    itemObject.Add("FSN", item.EquipmentInfo.Floor_Info.FloorName); // 樓層
                    itemObject.Add("EName", item.EquipmentInfo.EName); // 設備名稱
                    itemObject.Add("NO", item.EquipmentInfo.NO); // 設備編號 (NO)
                    itemObject.Add("MaintainName", item.MaintainName); // 保養項目
                    itemObject.Add("Period", Surface.MaintainPeriod()[item.Period]); // 保養週期
                    itemObject.Add("Maintainer", string.Join("、", item.Equipment_MaintenanceFormMember.Select(x => x.Maintainer))); // 執行人員

                    ja.Add(itemObject);
                }
                #endregion

                JObject jo = new JObject();
                jo.Add("rows", ja);
                jo.Add("total", total);
                return jo;
            }
        }
        #endregion

        #region 保養項目管理
        //public JObject GetJsonForGrid_MaintainItem(System.Web.Mvc.FormCollection form)
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
        //    //系統別
        //    string System = form["System"]?.ToString();
        //    //子系統別
        //    string SubSystem = form["SubSystem"]?.ToString();
        //    //設備名稱
        //    string EName = form["EName"]?.ToString();
        //    //保養項目名稱
        //    string MIName = form["MIName"]?.ToString();
        //    //保養週期單位
        //    string Unit = form["Unit"]?.ToString();
        //    //保養週期
        //    string Period = form["Period"]?.ToString();
        //    #endregion

        //    #region 依據查詢字串檢索資料表
        //    var SourceTable = from x1 in db.MaintainItem
        //                      select new { x1.MISN, x1.System, x1.SubSystem, x1.EName, x1.MIName, x1.Unit, x1.Period, x1.MaintainItemIsEnable };

        //    SourceTable = SourceTable.Where(x => x.MaintainItemIsEnable == "1");
        //    if (!string.IsNullOrEmpty(System))
        //    {
        //        SourceTable = SourceTable.Where(x => x.System == System);
        //    }
        //    if (!string.IsNullOrEmpty(SubSystem))
        //    {
        //        SourceTable = SourceTable.Where(x => x.SubSystem == SubSystem);
        //    }
        //    if (!string.IsNullOrEmpty(EName))
        //    {
        //        SourceTable = SourceTable.Where(x => x.EName.Contains(EName));
        //    }
        //    if (!string.IsNullOrEmpty(MIName))
        //    {
        //        SourceTable = SourceTable.Where(x => x.MIName.Contains(MIName));
        //    }
        //    if (!string.IsNullOrEmpty(Unit))
        //    {
        //        SourceTable = SourceTable.Where(x => x.Unit.Contains(Unit));
        //    }
        //    if (!string.IsNullOrEmpty(Period))
        //    {
        //        bool conversionSuccessful = int.TryParse(Period, out int IntPeriod);
        //        if (conversionSuccessful)
        //        {
        //            SourceTable = SourceTable.Where(x => x.Period == IntPeriod);
        //        }
        //    }
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
        //        SourceTable = SourceTable.OrderByDescending(x => x.MISN);
        //    }
        //    #endregion

        //    //回傳JSON陣列
        //    JArray ja = new JArray();
        //    //記住總筆數
        //    int total = SourceTable.Count();
        //    //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
        //    SourceTable = SourceTable.Skip((page - 1) * rows).Take(rows);

        //    foreach (var item in SourceTable)
        //    {
        //        var itemObjects = new JObject();
        //        if (!string.IsNullOrEmpty(item.MISN))
        //        {
        //            itemObjects.Add("MISN", item.MISN);
        //        }
        //        if (!string.IsNullOrEmpty(item.System))
        //        {
        //            itemObjects.Add("System", item.System);
        //        }
        //        if (!string.IsNullOrEmpty(item.SubSystem))
        //        {
        //            itemObjects.Add("SubSystem", item.SubSystem);
        //        }
        //        if (!string.IsNullOrEmpty(item.EName))
        //        {
        //            itemObjects.Add("EName", item.EName);
        //        }
        //        if (!string.IsNullOrEmpty(item.MIName))
        //        {
        //            itemObjects.Add("MIName", item.MIName);
        //        }
        //        if (!string.IsNullOrEmpty(item.Unit))
        //        {
        //            itemObjects.Add("Unit", item.Unit);
        //        }

        //        itemObjects.Add("Period", item.Period);

        //        ja.Add(itemObjects);
        //    }

        //    JObject jo = new JObject();
        //    jo.Add("rows", ja);
        //    jo.Add("total", total);
        //    return jo;
        //}
        #endregion

        #region 設備保養週期管理
        //public JObject GetJsonForGrid_EquipmentMaintainPeriod_Management(System.Web.Mvc.FormCollection form)
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


        //    //塞來自formdata的資料
        //    //棟別名稱
        //    string Area = form["Area"]?.ToString();
        //    //棟別編號
        //    string ASN = form["ASN"]?.ToString();
        //    //樓層名稱
        //    string Floor = form["Floor"]?.ToString();
        //    //樓層編號
        //    string FSN = form["FSN"]?.ToString();
        //    //系統別
        //    string System = form["System"]?.ToString();
        //    //子系統別
        //    string SubSystem = form["SubSystem"]?.ToString();
        //    //設備編號
        //    string ESN = form["ESN"]?.ToString();
        //    //設備名稱
        //    string EName = form["EName"]?.ToString();
        //    //啟用狀態
        //    string IsEnable = form["IsEnable"]?.ToString();


        //    #region 依據查詢字串檢索資料表

        //    var SourceTable = from x1 in db.EquipmentMaintainItem
        //                      join x2 in db.EquipmentInfo on x1.ESN equals x2.ESN
        //                      join x3 in db.MaintainItem on x1.MISN equals x3.MISN
        //                      select new { x1.EMISN, x1.IsEnable, x2.Area, x2.Floor, x2.System, x2.SubSystem, x1.ESN, x2.EName, x1.MISN, x3.MIName, x1.Unit, x1.Period, x1.LastTime, x1.NextTime, x2.EState, x2.DBID };

        //    //設備狀態為3(停用) 不顯示
        //    SourceTable = SourceTable.Where(x => x.EState != "3");

        //    //Area查詢table方式 以Area至表[設備資訊]查詢出ESN，再以ESN至表[設備報修單]查詢出相關報修單。
        //    if (!string.IsNullOrEmpty(Area))
        //    {
        //        SourceTable = SourceTable.Where(x => x.Area == Area);
        //    }
        //    if (!string.IsNullOrEmpty(Floor))
        //    {
        //        SourceTable = SourceTable.Where(x => x.Floor == Floor);
        //    }
        //    if (!string.IsNullOrEmpty(System))
        //    {
        //        SourceTable = SourceTable.Where(x => x.System == System);
        //    }
        //    if (!string.IsNullOrEmpty(SubSystem))
        //    {
        //        SourceTable = SourceTable.Where(x => x.SubSystem == SubSystem);
        //    }
        //    if (!string.IsNullOrEmpty(ESN))
        //    {
        //        SourceTable = SourceTable.Where(x => x.ESN == ESN);
        //    }
        //    if (!string.IsNullOrEmpty(EName))
        //    {
        //        SourceTable = SourceTable.Where(x => x.EName.Contains(EName));
        //    }
        //    if (!string.IsNullOrEmpty(IsEnable))
        //    {
        //        SourceTable = SourceTable.Where(x => x.IsEnable == IsEnable);
        //    }
        //    SourceTable = SourceTable.Where(x => x.IsEnable != "2");
        //    //var atable_ESN_list = db.EquipmentInfo.Where(x => x.Area == Area).Select(x=>x.ESN).ToList();
        //    //var atable_SearchTable = db.EquipmentReportForm.Where(x=> atable_ESN_list.Contains(x.ESN));
        //    #endregion
        //    var resulttable = SourceTable.OrderBy(x => x.EName).AsQueryable();
        //    //回傳JSON陣列
        //    JArray ja = new JArray();
        //    //記住總筆數
        //    int total = resulttable.Count();
        //    //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
        //    resulttable = resulttable.Skip((page - 1) * rows).Take(rows);


        //    foreach (var a in resulttable)
        //    {
        //        var itemObjects = new JObject();
        //        if (itemObjects["EMISN"] == null)
        //        {
        //            //string aaaaa = dic["8"];
        //            itemObjects.Add("EMISN", a.EMISN);
        //        }
        //        if (itemObjects["IsEnable"] == null)
        //        {
        //            //var dic = Surface.MaintainItemIsEnable();
        //            //string aaaaa = dic["8"];
        //            int Enabled = Int16.Parse(a.IsEnable);
        //            itemObjects.Add("IsEnable", Enabled); //啟用狀態
        //        }
        //        if (itemObjects["Area"] == null)
        //            itemObjects.Add("Area", a.Area);    //棟別                           

        //        if (itemObjects["Floor"] == null)
        //            itemObjects.Add("Floor", a.Floor);   //樓層

        //        if (itemObjects["System"] == null)
        //        {
        //            itemObjects.Add("System", a.System); //系統別
        //        }
        //        if (itemObjects["SubSystem"] == null)
        //        {
        //            itemObjects.Add("SubSystem", a.SubSystem); //子系統別
        //        }
        //        if (itemObjects["ESN"] == null)
        //            itemObjects.Add("ESN", a.ESN);    //設備編號
        //        if (itemObjects["EState"] == null)
        //        {
        //            var dic = Surface.EState();
        //            itemObjects.Add("EState", dic[a.EState]);    //設備狀態
        //        }
        //        if (itemObjects["EName"] == null)
        //            itemObjects.Add("EName", a.EName);    //設備名稱
        //        if (itemObjects["MISN"] == null)
        //            itemObjects.Add("MISN", a.MISN);    //保養項目編號
        //        if (itemObjects["MIName"] == null)
        //            itemObjects.Add("MIName", a.MIName);    //項目名稱
        //        if (itemObjects["Unit"] == null)
        //            itemObjects.Add("Unit", a.Unit);    //保養週期單位
        //        if (itemObjects["Period"] == null)
        //            itemObjects.Add("Period", a.Period);    //保養週期
        //        if (itemObjects["LastTime"] == null)
        //            itemObjects.Add("LastTime", a.LastTime?.ToString("yyyy/MM/dd"));    //上次保養日期
        //        if (itemObjects["NextTime"] == null)
        //            itemObjects.Add("NextTime", a.NextTime?.ToString("yyyy/MM/dd"));    //最近應保養日期
        //        //DBID
        //        if (!string.IsNullOrEmpty(a.DBID.ToString()))
        //        {
        //            itemObjects.Add("DBID", a.DBID);
        //        }
        //        ja.Add(itemObjects);
        //    }

        //    JObject jo = new JObject();
        //    jo.Add("rows", ja);
        //    jo.Add("total", total);
        //    return jo;
        //}
        #endregion

        #region 巡檢保養紀錄管理
        //public JObject GetJsonForGrid_MaintainRecord_Management(System.Web.Mvc.FormCollection form)
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
        //    //巡檢計畫編號
        //    string IPSN = form["IPSN"]?.ToString();
        //    //巡檢計畫名稱
        //    string IPName = form["IPName"]?.ToString();
        //    //保養單狀態
        //    string MaintainState = form["MaintainState"]?.ToString();
        //    //財產編碼
        //    string PropertyCode = form["PropertyCode"]?.ToString();
        //    //設備編號
        //    string ESN = form["ESN"]?.ToString();
        //    //設備名稱
        //    string EName = form["EName"]?.ToString();
        //    //保養項目編號
        //    string EMFISN = form["EMFISN"]?.ToString();
        //    //保養項目
        //    string MIName = form["MIName"]?.ToString();
        //    //保養人員
        //    string MaintainUserID = form["MaintainUserID"]?.ToString();
        //    //審核人員
        //    string AuditUserID = form["AuditUserID"]?.ToString();
        //    //日期項目選擇   //有五個選項--> 計畫日期 && 上次保養日期 && 本次保養日期 && 下次保養日期 && 審核日期     前端的Value還沒決定
        //    string DateSelect = form["DateSelect"]?.ToString();
        //    //日期(起)
        //    string DateFrom = form["DateFrom"]?.ToString();
        //    //日期(迄)
        //    string DateTo = form["DateTo"]?.ToString();
        //    #endregion

        //    #region 依據查詢字串檢索資料表

        //    var DataSource = from x1 in db.InspectionPlanMaintain select x1;

        //    var IPMSNlist = db.InspectionPlanMaintain.Select(x => x.IPMSN).ToList();
        //    if (!string.IsNullOrEmpty(ASN)) //棟別編號
        //    {
        //        int a = Int16.Parse(ASN);
        //        var FSNlist = db.Floor_Info.Where(x => x.ASN == a).Select(x => x.FSN).ToList();
        //        var ESNlist = db.EquipmentInfo.Where(x => FSNlist.Contains(x.FSN)).Select(x => x.ESN).ToList();
        //        var EMISNlist = db.EquipmentMaintainItem.Where(x => ESNlist.Contains(x.ESN)).Select(x => x.EMISN).ToList();
        //        var EMFISNlist = db.EquipmentMaintainFormItem.Where(x => EMISNlist.Contains(x.EMISN)).Select(x => x.EMFISN).ToList();
        //        var iPMSNlist = db.InspectionPlanMaintain.Where(x => EMFISNlist.Contains(x.EMFISN)).Select(x => x.IPMSN).ToList();
        //        var templist = new List<string>();
        //        foreach (var item in IPMSNlist)
        //        {
        //            if (iPMSNlist.Contains(item))
        //                templist.Add(item);
        //        }
        //        IPMSNlist = templist;
        //    }
        //    if (!string.IsNullOrEmpty(FSN)) //樓層編號
        //    {
        //        var ESNlist = db.EquipmentInfo.Where(x => x.FSN == FSN).Select(x => x.ESN).ToList();
        //        var EMISNlist = db.EquipmentMaintainItem.Where(x => ESNlist.Contains(x.ESN)).Select(x => x.EMISN).ToList();
        //        var EMFISNlist = db.EquipmentMaintainFormItem.Where(x => EMISNlist.Contains(x.EMISN)).Select(x => x.EMFISN).ToList();
        //        var iPMSNlist = db.InspectionPlanMaintain.Where(x => EMFISNlist.Contains(x.EMFISN)).Select(x => x.IPMSN).ToList();
        //        var templist = new List<string>();
        //        foreach (var item in IPMSNlist)
        //        {
        //            if (iPMSNlist.Contains(item))
        //                templist.Add(item);
        //        }
        //        IPMSNlist = templist;
        //    }
        //    if (!string.IsNullOrEmpty(IPName)) //巡檢計畫名稱
        //    {
        //        var IPSNlist = db.InspectionPlan.Where(x => x.IPName.Contains(IPName)).Select(x => x.IPSN).ToList();
        //        var iPMSNlist = db.InspectionPlanMaintain.Where(x => IPSNlist.Contains(x.IPSN)).Select(x => x.IPMSN).ToList();
        //        var templist = new List<string>();
        //        foreach (var item in IPMSNlist)
        //        {
        //            if (iPMSNlist.Contains(item))
        //                templist.Add(item);
        //        }
        //        IPMSNlist = templist;
        //    }
        //    if (!string.IsNullOrEmpty(PropertyCode)) //財產編碼
        //    {
        //        var ESNlist = db.EquipmentInfo.Where(x => x.PropertyCode == PropertyCode).Select(x => x.ESN).ToList();
        //        var EMISNlist = db.EquipmentMaintainItem.Where(x => ESNlist.Contains(x.ESN)).Select(x => x.EMISN).ToList();
        //        var EMFISNlist = db.EquipmentMaintainFormItem.Where(x => EMISNlist.Contains(x.EMISN)).Select(x => x.EMFISN).ToList();
        //        var iPMSNlist = db.InspectionPlanMaintain.Where(x => EMFISNlist.Contains(x.EMFISN)).Select(x => x.IPMSN).ToList();
        //        var templist = new List<string>();
        //        foreach (var item in IPMSNlist)
        //        {
        //            if (iPMSNlist.Contains(item))
        //                templist.Add(item);
        //        }
        //        IPMSNlist = templist;
        //    }
        //    if (!string.IsNullOrEmpty(ESN)) //設備編號
        //    {
        //        var EMISNlist = db.EquipmentMaintainItem.Where(x => x.ESN == ESN).Select(x => x.EMISN).ToList();
        //        var EMFISNlist = db.EquipmentMaintainFormItem.Where(x => EMISNlist.Contains(x.EMISN)).Select(x => x.EMFISN).ToList();
        //        var iPMSNlist = db.InspectionPlanMaintain.Where(x => EMFISNlist.Contains(x.EMFISN)).Select(x => x.IPMSN).ToList();
        //        var templist = new List<string>();
        //        foreach (var item in IPMSNlist)
        //        {
        //            if (iPMSNlist.Contains(item))
        //                templist.Add(item);
        //        }
        //        IPMSNlist = templist;
        //    }
        //    if (!string.IsNullOrEmpty(EName)) //設備名稱
        //    {
        //        var ESNlist = db.EquipmentInfo.Where(x => x.EName.Contains(EName)).Select(x => x.ESN).ToList();
        //        var EMISNlist = db.EquipmentMaintainItem.Where(x => ESNlist.Contains(x.ESN)).Select(x => x.EMISN).ToList();
        //        var EMFISNlist = db.EquipmentMaintainFormItem.Where(x => EMISNlist.Contains(x.EMISN)).Select(x => x.EMFISN).ToList();
        //        var iPMSNlist = db.InspectionPlanMaintain.Where(x => EMFISNlist.Contains(x.EMFISN)).Select(x => x.IPMSN).ToList();
        //        var templist = new List<string>();
        //        foreach (var item in IPMSNlist)
        //        {
        //            if (iPMSNlist.Contains(item))
        //                templist.Add(item);
        //        }
        //        IPMSNlist = templist;
        //    }
        //    if (!string.IsNullOrEmpty(MIName)) //保養項目
        //    {
        //        var MISNlist = db.MaintainItem.Where(x => x.MIName.Contains(MIName)).Select(x => x.MISN).ToList();
        //        var EMISNlist = db.EquipmentMaintainItem.Where(x => MISNlist.Contains(x.MISN)).Select(x => x.EMISN).ToList();
        //        var EMFISNlist = db.EquipmentMaintainFormItem.Where(x => EMISNlist.Contains(x.EMISN)).Select(x => x.EMFISN).ToList();
        //        var iPMSNlist = db.InspectionPlanMaintain.Where(x => EMFISNlist.Contains(x.EMFISN)).Select(x => x.IPMSN).ToList();
        //        var templist = new List<string>();
        //        foreach (var item in IPMSNlist)
        //        {
        //            if (iPMSNlist.Contains(item))
        //                templist.Add(item);
        //        }
        //        IPMSNlist = templist;
        //    }
        //    if (!string.IsNullOrEmpty(AuditUserID)) //審核人員
        //    {
        //        var iPMSNlist = db.MaintainAuditInfo.Where(x => x.AuditUserID == AuditUserID).Select(x => x.IPMSN).ToList();
        //        var templist = new List<string>();
        //        foreach (var item in IPMSNlist)
        //        {
        //            if (iPMSNlist.Contains(item))
        //                templist.Add(item);
        //        }
        //        IPMSNlist = templist;
        //    }
        //    if (!string.IsNullOrEmpty(DateSelect)) //日期項目選擇
        //    {
        //        if (!string.IsNullOrEmpty(DateFrom)) //日期(起)
        //        {
        //            var datefrom = DateTime.Parse(DateFrom);
        //            switch (DateSelect)
        //            {
        //                case "1":
        //                    var IPSNlist = db.InspectionPlan.Where(x => x.PlanDate >= datefrom).Select(x => x.IPSN).ToList();
        //                    var iPMSNlist = db.InspectionPlanMaintain.Where(x => IPSNlist.Contains(x.IPSN)).Select(x => x.IPMSN).ToList();
        //                    var templist = new List<string>();
        //                    foreach (var item in IPMSNlist)
        //                    {
        //                        if (iPMSNlist.Contains(item))
        //                            templist.Add(item);
        //                    }
        //                    IPMSNlist = templist;
        //                    break;
        //                case "2":
        //                    var EMFISNlist = db.EquipmentMaintainFormItem.Where(x => x.LastTime >= datefrom).Select(x => x.EMFISN).ToList();
        //                    var iPMSNlist2 = db.InspectionPlanMaintain.Where(x => EMFISNlist.Contains(x.EMFISN)).Select(x => x.IPMSN).ToList();
        //                    var templist2 = new List<string>();
        //                    foreach (var item in IPMSNlist)
        //                    {
        //                        if (iPMSNlist2.Contains(item))
        //                            templist2.Add(item);
        //                    }
        //                    IPMSNlist = templist2;
        //                    break;
        //                case "3":
        //                    var EMFISNlist3 = db.EquipmentMaintainFormItem.Where(x => x.Date >= datefrom).Select(x => x.EMFISN).ToList();
        //                    var iPMSNlist3 = db.InspectionPlanMaintain.Where(x => EMFISNlist3.Contains(x.EMFISN)).Select(x => x.IPMSN).ToList();
        //                    var templist3 = new List<string>();
        //                    foreach (var item in IPMSNlist)
        //                    {
        //                        if (iPMSNlist3.Contains(item))
        //                            templist3.Add(item);
        //                    }
        //                    IPMSNlist = templist3;
        //                    break;
        //                case "4":
        //                    var EMFISNlist4 = db.EquipmentMaintainFormItem.Where(x => x.NextTime >= datefrom).Select(x => x.EMFISN).ToList();
        //                    var iPMSNlist4 = db.InspectionPlanMaintain.Where(x => EMFISNlist4.Contains(x.EMFISN)).Select(x => x.IPMSN).ToList();
        //                    var templist4 = new List<string>();
        //                    foreach (var item in IPMSNlist)
        //                    {
        //                        if (iPMSNlist4.Contains(item))
        //                            templist4.Add(item);
        //                    }
        //                    IPMSNlist = templist4;
        //                    break;
        //                case "5":
        //                    var iPMSNlist5 = db.MaintainAuditInfo.Where(x => x.AuditDate >= datefrom).Select(x => x.IPMSN).ToList();
        //                    var templist5 = new List<string>();
        //                    foreach (var item in IPMSNlist)
        //                    {
        //                        if (iPMSNlist5.Contains(item))
        //                            templist5.Add(item);
        //                    }
        //                    IPMSNlist = templist5;
        //                    break;
        //            }
        //        }
        //        if (!string.IsNullOrEmpty(DateTo)) //日期(迄)
        //        {
        //            var dateto = DateTime.Parse(DateTo).AddDays(1);
        //            switch (DateSelect)
        //            {
        //                case "1":
        //                    var IPSNlist = db.InspectionPlan.Where(x => x.PlanDate < dateto).Select(x => x.IPSN).ToList();
        //                    var iPMSNlist = db.InspectionPlanMaintain.Where(x => IPSNlist.Contains(x.IPSN)).Select(x => x.IPMSN).ToList();
        //                    var templist = new List<string>();
        //                    foreach (var item in IPMSNlist)
        //                    {
        //                        if (iPMSNlist.Contains(item))
        //                            templist.Add(item);
        //                    }
        //                    IPMSNlist = templist;
        //                    break;
        //                case "2":
        //                    var EMFISNlist = db.EquipmentMaintainFormItem.Where(x => x.LastTime < dateto).Select(x => x.EMFISN).ToList();
        //                    var iPMSNlist2 = db.InspectionPlanMaintain.Where(x => EMFISNlist.Contains(x.EMFISN)).Select(x => x.IPMSN).ToList();
        //                    var templist2 = new List<string>();
        //                    foreach (var item in IPMSNlist)
        //                    {
        //                        if (iPMSNlist2.Contains(item))
        //                            templist2.Add(item);
        //                    }
        //                    IPMSNlist = templist2;
        //                    break;
        //                case "3":
        //                    var EMFISNlist3 = db.EquipmentMaintainFormItem.Where(x => x.Date < dateto).Select(x => x.EMFISN).ToList();
        //                    var iPMSNlist3 = db.InspectionPlanMaintain.Where(x => EMFISNlist3.Contains(x.EMFISN)).Select(x => x.IPMSN).ToList();
        //                    var templist3 = new List<string>();
        //                    foreach (var item in IPMSNlist)
        //                    {
        //                        if (iPMSNlist3.Contains(item))
        //                            templist3.Add(item);
        //                    }
        //                    IPMSNlist = templist3;
        //                    break;
        //                case "4":
        //                    var EMFISNlist4 = db.EquipmentMaintainFormItem.Where(x => x.NextTime < dateto).Select(x => x.EMFISN).ToList();
        //                    var iPMSNlist4 = db.InspectionPlanMaintain.Where(x => EMFISNlist4.Contains(x.EMFISN)).Select(x => x.IPMSN).ToList();
        //                    var templist4 = new List<string>();
        //                    foreach (var item in IPMSNlist)
        //                    {
        //                        if (iPMSNlist4.Contains(item))
        //                            templist4.Add(item);
        //                    }
        //                    IPMSNlist = templist4;
        //                    break;
        //                case "5":
        //                    var iPMSNlist5 = db.MaintainAuditInfo.Where(x => x.AuditDate < dateto).Select(x => x.IPMSN).ToList();
        //                    var templist5 = new List<string>();
        //                    foreach (var item in IPMSNlist)
        //                    {
        //                        if (iPMSNlist5.Contains(item))
        //                            templist5.Add(item);
        //                    }
        //                    IPMSNlist = templist5;
        //                    break;
        //            }
        //        }
        //    }

        //    /////////  where 剩下的條件
        //    DataSource = DataSource.Where(x => IPMSNlist.Contains(x.IPMSN));

        //    if (!string.IsNullOrEmpty(IPSN)) //巡檢計畫編號
        //    {
        //        DataSource = DataSource.Where(x => x.IPSN == IPSN);
        //    }
        //    if (!string.IsNullOrEmpty(MaintainState)) //保養單狀態   
        //    {
        //        DataSource = DataSource.Where(x => x.MaintainState == MaintainState);
        //    }
        //    if (!string.IsNullOrEmpty(EMFISN)) //保養項目編號 
        //    {
        //        DataSource = DataSource.Where(x => x.EMFISN == EMFISN);
        //    }
        //    if (!string.IsNullOrEmpty(MaintainUserID)) //保養人員 
        //    {
        //        DataSource = DataSource.Where(x => x.MaintainUserID == MaintainUserID);
        //    }
        //    #endregion

        //    //排序資料表
        //    var result = DataSource.OrderByDescending(x => x.IPMSN).AsQueryable();
        //    //回傳JSON陣列
        //    JArray ja = new JArray();
        //    //記住總筆數
        //    int total = result.Count();
        //    //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
        //    result = result.Skip((page - 1) * rows).Take(rows);

        //    #region 塞資料
        //    //建Json格式資料表回傳給前端
        //    foreach (var a in result)
        //    {
        //        var InspectionPlan_ = db.InspectionPlan.Where(x => x.IPSN == a.IPSN).FirstOrDefault() == null ? new InspectionPlan() : db.InspectionPlan.Where(x => x.IPSN == a.IPSN).FirstOrDefault();
        //        var EquipmentMaintainFormItem_ = db.EquipmentMaintainFormItem.Where(x => x.EMFISN == a.EMFISN).FirstOrDefault() == null ? new EquipmentMaintainFormItem() : db.EquipmentMaintainFormItem.Where(x => x.EMFISN == a.EMFISN).FirstOrDefault();
        //        var EquipmentMaintainItem_ = db.EquipmentMaintainItem.Where(x => x.EMISN == EquipmentMaintainFormItem_.EMISN).FirstOrDefault() == null ? new EquipmentMaintainItem() : db.EquipmentMaintainItem.Where(x => x.EMISN == EquipmentMaintainFormItem_.EMISN).FirstOrDefault();
        //        var MaintainItem_ = db.MaintainItem.Where(x => x.MISN == EquipmentMaintainItem_.MISN).FirstOrDefault() == null ? new MaintainItem() : db.MaintainItem.Where(x => x.MISN == EquipmentMaintainItem_.MISN).FirstOrDefault();
        //        var EquipmentInfo_ = db.EquipmentInfo.Where(x => x.ESN == EquipmentMaintainItem_.ESN).FirstOrDefault() == null ? new EquipmentInfo() : db.EquipmentInfo.Where(x => x.ESN == EquipmentMaintainItem_.ESN).FirstOrDefault();
        //        var AspNetUsers_MaintainID_ = db.AspNetUsers.Where(x => x.UserName == a.MaintainUserID).FirstOrDefault() == null ? new AspNetUsers() : db.AspNetUsers.Where(x => x.UserName == a.MaintainUserID).FirstOrDefault();
        //        var MaintainAuditInfo_ = db.MaintainAuditInfo.Where(x => x.IPMSN == a.IPMSN).FirstOrDefault() == null ? new MaintainAuditInfo() : db.MaintainAuditInfo.Where(x => x.IPMSN == a.IPMSN).FirstOrDefault();
        //        var AspNetUsers_AuditID_ = db.AspNetUsers.Where(x => x.UserName == MaintainAuditInfo_.AuditUserID).FirstOrDefault() == null ? new AspNetUsers() : db.AspNetUsers.Where(x => x.UserName == MaintainAuditInfo_.AuditUserID).FirstOrDefault();

        //        var itemObjects = new JObject();
        //        itemObjects.Add("IPMSN", a.IPMSN);
        //        itemObjects.Add("IPSN", a.IPSN);
        //        itemObjects.Add("IPName", InspectionPlan_.IPName);
        //        itemObjects.Add("PlanDate", InspectionPlan_.PlanDate.ToString("yyyy/M/d"));

        //        var dic = Surface.InspectionPlanMaintainState();
        //        itemObjects.Add("MaintainState", dic[a.MaintainState.Trim()]); //這個要再用 Surface 做中文轉譯!!

        //        itemObjects.Add("Area", EquipmentInfo_.Area);
        //        itemObjects.Add("Floor", EquipmentInfo_.Floor);
        //        itemObjects.Add("PropertyCode", EquipmentInfo_.PropertyCode);
        //        itemObjects.Add("EName", EquipmentInfo_.EName);
        //        itemObjects.Add("ESN", EquipmentMaintainItem_.ESN);
        //        itemObjects.Add("EMFISN", a.EMFISN);
        //        itemObjects.Add("MIName", MaintainItem_.MIName);
        //        itemObjects.Add("Unit", EquipmentMaintainFormItem_.Unit);
        //        itemObjects.Add("Period", EquipmentMaintainFormItem_.Period);
        //        itemObjects.Add("LastTime", EquipmentMaintainFormItem_.LastTime.ToString("yyyy/M/d"));
        //        itemObjects.Add("Date", EquipmentMaintainFormItem_.Date.ToString("yyyy/M/d"));
        //        itemObjects.Add("NextTime", EquipmentMaintainFormItem_.NextTime?.ToString("yyyy/M/d"));
        //        itemObjects.Add("MaintainUserName", AspNetUsers_MaintainID_.MyName);
        //        itemObjects.Add("AuditUserName", AspNetUsers_AuditID_.MyName);
        //        itemObjects.Add("AuditDate", MaintainAuditInfo_.AuditDate.ToString("yyyy/M/d"));
        //        itemObjects.Add("DBID", EquipmentInfo_.DBID);
        //        ja.Add(itemObjects);
        //    }
        //    #endregion

        //    JObject jo = new JObject();
        //    jo.Add("rows", ja);
        //    jo.Add("total", total);
        //    return jo;
        //}
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
                if (!string.IsNullOrEmpty(form["RepairUserName"]?.ToString()))
                {
                    string repairUserName = form["RepairUserName"].ToString();
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

                int total = equipmentReportFormTable.Count();
                List<EquipmentReportForm> equipmentReportFormList = equipmentReportFormTable.Skip((page - 1) * rows).Take(rows).ToList();

                JArray ja = new JArray();
                foreach (var item in equipmentReportFormList)
                {
                    JObject itemObject = new JObject();
                    itemObject.Add("ReportState", Surface.ReportState()[item.ReportState]);
                    itemObject.Add("RSN", item.RSN);
                    itemObject.Add("ReportLevel", Surface.ReportLevel()[item.ReportLevel]);
                    itemObject.Add("ReportTime", item.ReportTime.ToString("yyyy/MM/dd HH:mm"));
                    itemObject.Add("ReportContent", item.ReportContent);
                    itemObject.Add("ASN", item.EquipmentInfo.Floor_Info.ASN);
                    itemObject.Add("FSN", item.EquipmentInfo.FSN);
                    itemObject.Add("EName", item.EquipmentInfo.EName);
                    itemObject.Add("NO", item.EquipmentInfo.NO);
                    itemObject.Add("RepairUserName", "");
                    var memberlist = db.Equipment_ReportFormMember.Where(e => e.RSN == item.RSN).ToList();
                    foreach (var member in memberlist)
                    {
                        if (!string.IsNullOrEmpty(itemObject["RepairUserName"]?.ToString()))
                            itemObject["RepairUserName"] = $"{itemObject["RepairUserName"]}、{member.RepairUserName}";
                        else
                            itemObject["RepairUserName"] = member.RepairUserName;
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

        #region 巡檢維修紀錄管理
        //public JObject GetJsonForGrid_RepairRecord_Management(System.Web.Mvc.FormCollection form)
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
        //    //巡檢計畫編號
        //    string IPSN = form["IPSN"]?.ToString();
        //    //巡檢計畫名稱
        //    string IPName = form["IPName"]?.ToString();
        //    //維修單狀態
        //    string RepairState = form["RepairState"]?.ToString();
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
        //    //報修人員
        //    string InformantUserID = form["InformantUserID"]?.ToString();
        //    //施工人員
        //    string RepairUserID = form["RepairUserID"]?.ToString();
        //    //審核人員
        //    string AuditUserID = form["AuditUserID"]?.ToString();
        //    //日期項目選擇   //有五個選項--> 計畫日期 && 上次保養日期 && 本次保養日期 && 下次保養日期 && 審核日期     前端的Value還沒決定
        //    string DateSelect = form["DateSelect"]?.ToString();
        //    //日期(起)
        //    string DateFrom = form["DateFrom"]?.ToString();
        //    //日期(迄)
        //    string DateTo = form["DateTo"]?.ToString();
        //    #endregion

        //    #region 依據查詢字串檢索資料表

        //    var DataSource = from x1 in db.InspectionPlanRepair select x1;

        //    var RSNlist = db.InspectionPlanRepair.Select(x => x.RSN).ToList();
        //    var IPSNlist = db.InspectionPlanRepair.Select(x => x.IPSN).ToList();
        //    var IPRSNlist = db.InspectionPlanRepair.Select(x => x.IPRSN).ToList();

        //    if (!string.IsNullOrEmpty(ASN)) //棟別
        //    {
        //        int a = Int16.Parse(ASN);
        //        var FSNlist = db.Floor_Info.Where(x => x.ASN == a).Select(x => x.FSN).ToList();
        //        var ESNlist = db.EquipmentInfo.Where(x => FSNlist.Contains(x.FSN)).Select(x => x.ESN).ToList();
        //        var rSNlist = db.EquipmentReportForm.Where(x => ESNlist.Contains(x.ESN)).Select(x => x.RSN).ToList();
        //        var templist = new List<string>();
        //        foreach (var item in RSNlist)
        //        {
        //            if (rSNlist.Contains(item))
        //                templist.Add(item);
        //        }
        //        RSNlist = templist;
        //    }
        //    if (!string.IsNullOrEmpty(FSN)) //樓層
        //    {
        //        var ESNlist = db.EquipmentInfo.Where(x => x.FSN == FSN).Select(x => x.ESN).ToList();
        //        var rSNlist = db.EquipmentReportForm.Where(x => ESNlist.Contains(x.ESN)).Select(x => x.RSN).ToList();
        //        var templist = new List<string>();
        //        foreach (var item in RSNlist)
        //        {
        //            if (rSNlist.Contains(item))
        //                templist.Add(item);
        //        }
        //        RSNlist = templist;
        //    }
        //    if (!string.IsNullOrEmpty(IPName)) //巡檢計畫名稱
        //    {
        //        var iPSNlist = db.InspectionPlan.Where(x => x.IPName == IPName).Select(x => x.IPSN).ToList();
        //        var templist = new List<string>();
        //        foreach (var item in IPSNlist)
        //        {
        //            if (iPSNlist.Contains(item))
        //                templist.Add(item);
        //        }
        //        IPSNlist = templist;
        //    }
        //    if (!string.IsNullOrEmpty(ReportLevel)) //報修等級
        //    {
        //        var rSNlist = db.EquipmentReportForm.Where(x => x.ReportLevel == ReportLevel).Select(x => x.RSN).ToList();
        //        var templist = new List<string>();
        //        foreach (var item in RSNlist)
        //        {
        //            if (rSNlist.Contains(item))
        //                templist.Add(item);
        //        }
        //        RSNlist = templist;
        //    }
        //    if (!string.IsNullOrEmpty(ESN)) //設備編號
        //    {
        //        var rSNlist = db.EquipmentReportForm.Where(x => x.ESN == ESN).Select(x => x.RSN).ToList();
        //        var templist = new List<string>();
        //        foreach (var item in RSNlist)
        //        {
        //            if (rSNlist.Contains(item))
        //                templist.Add(item);
        //        }
        //        RSNlist = templist;
        //    }
        //    if (!string.IsNullOrEmpty(EName)) //設備名稱
        //    {
        //        var ESNlist = db.EquipmentInfo.Where(x => x.EName == EName).Select(x => x.ESN).ToList();
        //        var rSNlist = db.EquipmentReportForm.Where(x => ESNlist.Contains(x.ESN)).Select(x => x.RSN).ToList();
        //        var templist = new List<string>();
        //        foreach (var item in RSNlist)
        //        {
        //            if (rSNlist.Contains(item))
        //                templist.Add(item);
        //        }
        //        RSNlist = templist;
        //    }
        //    if (!string.IsNullOrEmpty(PropertyCode)) //財產編碼
        //    {
        //        var ESNlist = db.EquipmentInfo.Where(x => x.PropertyCode == PropertyCode).Select(x => x.ESN).ToList();
        //        var rSNlist = db.EquipmentReportForm.Where(x => ESNlist.Contains(x.ESN)).Select(x => x.RSN).ToList();
        //        var templist = new List<string>();
        //        foreach (var item in RSNlist)
        //        {
        //            if (rSNlist.Contains(item))
        //                templist.Add(item);
        //        }
        //        RSNlist = templist;
        //    }
        //    if (!string.IsNullOrEmpty(ReportContent)) //報修說明
        //    {
        //        var rSNlist = db.EquipmentReportForm.Where(x => x.ReportContent.Contains(ReportContent)).Select(x => x.RSN).ToList();
        //        var templist = new List<string>();
        //        foreach (var item in RSNlist)
        //        {
        //            if (rSNlist.Contains(item))
        //                templist.Add(item);
        //        }
        //        RSNlist = templist;
        //    }
        //    if (!string.IsNullOrEmpty(InformantUserID)) //報修人員
        //    {
        //        var rSNlist = db.EquipmentReportForm.Where(x => x.InformatUserID == InformantUserID).Select(x => x.RSN).ToList();
        //        var templist = new List<string>();
        //        foreach (var item in RSNlist)
        //        {
        //            if (rSNlist.Contains(item))
        //                templist.Add(item);
        //        }
        //        RSNlist = templist;
        //    }
        //    if (!string.IsNullOrEmpty(AuditUserID)) //審核人員
        //    {
        //        var iPRSNlist = db.RepairAuditInfo.Where(x => x.AuditUserID == AuditUserID).Select(x => x.IPRSN).ToList();
        //        var templist = new List<string>();
        //        foreach (var item in IPRSNlist)
        //        {
        //            if (iPRSNlist.Contains(item))
        //                templist.Add(item);
        //        }
        //        IPRSNlist = templist;
        //    }

        //    if (!string.IsNullOrEmpty(DateSelect)) //日期項目選擇
        //    {
        //        if (!string.IsNullOrEmpty(DateFrom)) //日期(起)
        //        {
        //            var datefrom = DateTime.Parse(DateFrom);
        //            switch (DateSelect)
        //            {
        //                case "1":
        //                    var iPSNlist = db.InspectionPlan.Where(x => x.PlanDate >= datefrom).Select(x => x.IPSN).ToList();
        //                    var templist = new List<string>();
        //                    foreach (var item in IPSNlist)
        //                    {
        //                        if (iPSNlist.Contains(item))
        //                            templist.Add(item);
        //                    }
        //                    IPSNlist = templist;
        //                    break;
        //                case "2":
        //                    var rSNlist = db.EquipmentReportForm.Where(x => x.Date >= datefrom).Select(x => x.RSN).ToList();
        //                    var templist2 = new List<string>();
        //                    foreach (var item in RSNlist)
        //                    {
        //                        if (rSNlist.Contains(item))
        //                            templist2.Add(item);
        //                    }
        //                    RSNlist = templist2;
        //                    break;
        //                case "3":
        //                    var iPRSNlist = db.RepairAuditInfo.Where(x => x.AuditDate >= datefrom).Select(x => x.IPRSN).ToList();
        //                    var templist3 = new List<string>();
        //                    foreach (var item in IPRSNlist)
        //                    {
        //                        if (iPRSNlist.Contains(item))
        //                            templist3.Add(item);
        //                    }
        //                    IPRSNlist = templist3;
        //                    break;

        //            }
        //        }
        //        if (!string.IsNullOrEmpty(DateTo)) //日期(迄)
        //        {
        //            var dateto = DateTime.Parse(DateTo).AddDays(1);
        //            switch (DateSelect)
        //            {
        //                case "1":
        //                    var iPSNlist = db.InspectionPlan.Where(x => x.PlanDate < dateto).Select(x => x.IPSN).ToList();
        //                    var templist = new List<string>();
        //                    foreach (var item in IPSNlist)
        //                    {
        //                        if (iPSNlist.Contains(item))
        //                            templist.Add(item);
        //                    }
        //                    IPSNlist = templist;
        //                    break;
        //                case "2":
        //                    var rSNlist = db.EquipmentReportForm.Where(x => x.Date < dateto).Select(x => x.RSN).ToList();
        //                    var templist2 = new List<string>();
        //                    foreach (var item in RSNlist)
        //                    {
        //                        if (rSNlist.Contains(item))
        //                            templist2.Add(item);
        //                    }
        //                    RSNlist = templist2;
        //                    break;
        //                case "3":
        //                    var iPRSNlist = db.RepairAuditInfo.Where(x => x.AuditDate < dateto).Select(x => x.IPRSN).ToList();
        //                    var templist3 = new List<string>();
        //                    foreach (var item in IPRSNlist)
        //                    {
        //                        if (iPRSNlist.Contains(item))
        //                            templist3.Add(item);
        //                    }
        //                    IPRSNlist = templist3;
        //                    break;
        //            }
        //        }
        //    }

        //    /////////  where 剩下的條件
        //    DataSource = DataSource.Where(x => RSNlist.Contains(x.RSN)).Where(x => IPSNlist.Contains(x.IPSN)).Where(x => IPRSNlist.Contains(x.IPRSN)).Where(x => x.RepairState != "1");

        //    if (!string.IsNullOrEmpty(IPSN)) //巡檢計畫編號
        //    {
        //        DataSource = DataSource.Where(x => x.IPSN == IPSN);
        //    }
        //    if (!string.IsNullOrEmpty(RepairState)) //維修單狀態   
        //    {
        //        DataSource = DataSource.Where(x => x.RepairState == RepairState);
        //    }
        //    if (!string.IsNullOrEmpty(RSN)) //報修單號
        //    {
        //        DataSource = DataSource.Where(x => x.RSN == RSN);
        //    }
        //    if (!string.IsNullOrEmpty(RepairUserID)) //施工人員 
        //    {
        //        DataSource = DataSource.Where(x => x.RepairUserID == RepairUserID);
        //    }
        //    #endregion

        //    var result = DataSource.OrderByDescending(x => x.IPRSN).AsQueryable();
        //    //回傳JSON陣列
        //    JArray ja = new JArray();
        //    //記住總筆數
        //    int total = result.Count();
        //    //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
        //    result = result.Skip((page - 1) * rows).Take(rows);

        //    #region 塞資料
        //    //建Json格式資料表回傳給前端
        //    foreach (var a in result)
        //    {
        //        var InspectionPlan_ = db.InspectionPlan.Where(x => x.IPSN == a.IPSN).FirstOrDefault() == null ? new InspectionPlan() : db.InspectionPlan.Where(x => x.IPSN == a.IPSN).FirstOrDefault();
        //        var EquipmentReportForm_ = db.EquipmentReportForm.Where(x => x.RSN == a.RSN).FirstOrDefault() == null ? new EquipmentReportForm() : db.EquipmentReportForm.Where(x => x.RSN == a.RSN).FirstOrDefault();
        //        var EquipmentInfo_ = db.EquipmentInfo.Where(x => x.ESN == EquipmentReportForm_.ESN).FirstOrDefault() == null ? new EquipmentInfo() : db.EquipmentInfo.Where(x => x.ESN == EquipmentReportForm_.ESN).FirstOrDefault();
        //        var AspNetUsers_Informant = db.AspNetUsers.Where(x => x.UserName == EquipmentReportForm_.InformatUserID).FirstOrDefault() == null ? new AspNetUsers() : db.AspNetUsers.Where(x => x.UserName == EquipmentReportForm_.InformatUserID).FirstOrDefault();
        //        var AspNetUsers_Repair = db.AspNetUsers.Where(x => x.UserName == a.RepairUserID).FirstOrDefault() == null ? new AspNetUsers() : db.AspNetUsers.Where(x => x.UserName == a.RepairUserID).FirstOrDefault();
        //        var RepairAuditInfo_ = db.RepairAuditInfo.Where(x => x.IPRSN == a.IPRSN).FirstOrDefault() == null ? new RepairAuditInfo() : db.RepairAuditInfo.Where(x => x.IPRSN == a.IPRSN).FirstOrDefault();
        //        string id = RepairAuditInfo_.AuditUserID;
        //        var AspNetUsers_Audit = db.AspNetUsers.Where(x => x.UserName == id).FirstOrDefault() == null ? new AspNetUsers() : db.AspNetUsers.Where(x => x.UserName == id).FirstOrDefault();

        //        var itemObjects = new JObject();
        //        itemObjects.Add("IPRSN", a.IPRSN);
        //        itemObjects.Add("IPSN", a.IPSN);
        //        itemObjects.Add("IPName", InspectionPlan_.IPName);
        //        itemObjects.Add("PlanDate", InspectionPlan_.PlanDate.ToString("yyyy/MM/dd"));

        //        var dic = Surface.InspectionPlanRepairState();
        //        itemObjects.Add("RepairState", dic[a.RepairState.Trim()]); //這個要再用 Surface 做中文轉譯!!

        //        var dicLevel = Surface.ReportLevel();
        //        itemObjects.Add("ReportLevel", dicLevel[EquipmentReportForm_.ReportLevel.Trim()]);
        //        itemObjects.Add("Area", EquipmentInfo_.Area);
        //        itemObjects.Add("Floor", EquipmentInfo_.Floor);
        //        itemObjects.Add("RSN", a.RSN);
        //        itemObjects.Add("Date", EquipmentReportForm_.Date.ToString("yyyy/MM/dd HH:mm:ss"));
        //        itemObjects.Add("PropertyCode", EquipmentInfo_.PropertyCode);
        //        itemObjects.Add("ESN", EquipmentReportForm_.ESN);
        //        itemObjects.Add("EName", EquipmentInfo_.EName);
        //        itemObjects.Add("ReportContent", EquipmentReportForm_.ReportContent);
        //        itemObjects.Add("InformantUserID", AspNetUsers_Informant.MyName);
        //        itemObjects.Add("RepairUserID", AspNetUsers_Repair.MyName);
        //        itemObjects.Add("AuditUserID", AspNetUsers_Audit.MyName);
        //        itemObjects.Add("AuditDate", RepairAuditInfo_.AuditDate.ToString("yyyy/M/d") == "0001/1/1" ? "" : RepairAuditInfo_.AuditDate.ToString("yyyy/M/d"));
        //        itemObjects.Add("DBID", EquipmentInfo_.DBID);
        //        ja.Add(itemObjects);
        //    }
        //    #endregion

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
                    itemObjects.Add("Floor", item.FloorName);
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
                InspectNum = x.Template_CheckItem.Count + x.Template_ReportingItem.Count
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
        #region 請購管理
        //public JObject GetJsonForGrid_PurchaseRequisition_Management(System.Web.Mvc.FormCollection form)
        //{
        //    //解析查詢字串
        //    var PRState = form["PRState"]?.ToString();
        //    var PRN = form["PRN"]?.ToString();
        //    var PRUserName = form["PRUserName"]?.ToString();
        //    var PRDept = form["PRDept"]?.ToString();
        //    var DateFrom = form["DateFrom"]?.ToString();
        //    var DateTo = form["DateTo"]?.ToString();
        //    // DataGrid參數
        //    var sort = form["sort"]?.ToString();
        //    var order = form["order"]?.ToString();
        //    //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
        //    int page = 1;
        //    if (!string.IsNullOrEmpty(form["page"]?.ToString())) page = short.Parse(form["page"].ToString());
        //    int rows = 10;
        //    if (!string.IsNullOrEmpty(form["rows"]?.ToString())) rows = short.Parse(form["rows"]?.ToString());

        //    var rpT = from p in db.PurchaseRequisition
        //              join u in db.AspNetUsers on p.PRUserName equals u.UserName into UserGroup
        //              from ug in UserGroup.DefaultIfEmpty()
        //              select new
        //              {
        //                  p.PRState,
        //                  p.PRN,
        //                  p.PRDate,
        //                  p.PRDept,
        //                  PRUserName = ug != null ? ug.MyName : null
        //              };

        //    //查詢請購單狀態
        //    if (!string.IsNullOrEmpty(PRState)) rpT = rpT.Where(x => x.PRState == PRState);
        //    //查詢單號
        //    if (!string.IsNullOrEmpty(PRN)) rpT = rpT.Where(x => x.PRN == PRN);
        //    //查詢請購人
        //    if (!string.IsNullOrEmpty(PRUserName)) rpT = rpT.Where(x => x.PRUserName == PRUserName);
        //    //查詢請購部門 (模糊查詢)
        //    if (!string.IsNullOrEmpty(PRDept)) rpT = rpT.Where(x => x.PRDept.Contains(PRDept));
        //    //查詢申請日期(起)
        //    if (!string.IsNullOrEmpty(DateFrom) && DateTime.Parse(DateFrom) != DateTime.MinValue)
        //    {
        //        DateTime start = DateTime.Parse(DateFrom);  // 轉為DateTime
        //        rpT = rpT.Where(x => x.PRDate >= start);
        //    }
        //    //查詢申請日期(迄)
        //    if (!string.IsNullOrEmpty(DateTo) && DateTime.Parse(DateTo) != DateTime.MinValue)
        //    {
        //        DateTime end = DateTime.Parse(DateTo).AddDays(1);
        //        rpT = rpT.Where(x => x.PRDate < end);
        //    }

        //    // 確認 sort 和 order 不為空才進行排序
        //    if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order)) rpT = rpT.OrderBy(sort + " " + order); // 使用 System.Linq.Dynamic.Core 套件進行動態排序
        //    else rpT = rpT.OrderByDescending(x => x.PRN);

        //    //記住總筆數
        //    int Total = rpT.Count();
        //    //切頁
        //    rpT = rpT.Skip((page - 1) * rows).Take(rows);

        //    //回傳JSON陣列
        //    JArray ja = new JArray();

        //    if (rpT != null || Total > 0)
        //    {
        //        var StateDics = Surface.PRState();
        //        foreach (var item in rpT)
        //        {
        //            var itemObject = new JObject
        //            {
        //                { "PRState", StateDics[item.PRState] },
        //                { "PRN", item.PRN },
        //                { "PRDate", item.PRDate.ToString("yyyy/MM/dd") },
        //                { "PRUserName", item.PRUserName },
        //                { "PRDept", item.PRDept },
        //            };

        //            ja.Add(itemObject);
        //        }
        //    }

        //    JObject jo = new JObject
        //    {
        //        { "rows", ja },
        //        { "total", Total }
        //    };

        //    return jo;
        //}
        #endregion

        #region 入庫管理
        //public JObject GetJsonForGrid_StockIn_Management(System.Web.Mvc.FormCollection form)
        //{
        //    //解析查詢字串
        //    var SIRSN = form["SIRSN"]?.ToString();
        //    var StockType = form["StockType"]?.ToString();
        //    var StockName = form["StockName"]?.ToString();
        //    var StockInMyName = form["StockInMyName"]?.ToString();
        //    var DateStart = form["DateStart"]?.ToString();
        //    var DateEnd = form["DateEnd"]?.ToString();
        //    // DataGrid參數
        //    var sort = form["sort"]?.ToString();
        //    var order = form["order"]?.ToString();
        //    //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
        //    int page = 1;
        //    if (!string.IsNullOrEmpty(form["page"]?.ToString())) page = short.Parse(form["page"].ToString());
        //    int rows = 10;
        //    if (!string.IsNullOrEmpty(form["rows"]?.ToString())) rows = short.Parse(form["rows"]?.ToString());

        //    var rpT = (from s in db.Stock
        //               join r in db.StockInRecord on s.SIRSN equals r.SIRSN into RecordGroup
        //               from rg in RecordGroup.DefaultIfEmpty() // 使用 DefaultIfEmpty 進行左外部連接
        //               join u in db.AspNetUsers on rg.StockInUserName equals u.UserName into UserGroup
        //               from ug in UserGroup.DefaultIfEmpty() // 使用 DefaultIfEmpty 進行左外部連接
        //               join cs in db.ComputationalStock on s.SISN equals cs.SISN into ComputationalStockGroup
        //               from csg in ComputationalStockGroup.DefaultIfEmpty() // 使用 DefaultIfEmpty 進行左外部連接
        //               group new
        //               {
        //                   rg.SIRSN,
        //                   csg.StockType,
        //                   csg.StockName,
        //                   s.Amount,
        //                   csg.Unit,
        //                   rg.StockInDateTime,
        //                   StockInMyName = ug != null ? ug.MyName : null
        //               } by rg.SIRSN into result
        //               let first = result.FirstOrDefault()
        //               select new
        //               {
        //                   SIRSN = result.Key,
        //                   TotalAmount = result.Sum(item => item.Amount), // 加總數量
        //                   first.StockType,
        //                   first.StockName,
        //                   first.Unit,
        //                   first.StockInDateTime,
        //                   first.StockInMyName
        //               });

        //    //查詢入庫編號 (模糊查詢)
        //    if (!string.IsNullOrEmpty(SIRSN)) rpT = rpT.Where(x => x.SIRSN.Contains(SIRSN));
        //    //查詢品項
        //    if (!string.IsNullOrEmpty(StockType)) rpT = rpT.Where(x => x.StockType == StockType);
        //    //查詢品名 (模糊查詢)
        //    if (!string.IsNullOrEmpty(StockName)) rpT = rpT.Where(x => x.StockName.Contains(StockName));
        //    //查詢入庫人員 (模糊查詢)
        //    if (!string.IsNullOrEmpty(StockInMyName)) rpT = rpT.Where(x => x.StockInMyName.Contains(StockInMyName));
        //    //查詢入庫日期(起)
        //    if (!string.IsNullOrEmpty(DateStart) && DateTime.Parse(DateStart) != DateTime.MinValue)
        //    {
        //        DateTime start = DateTime.Parse(DateStart);  // 轉為DateTime
        //        rpT = rpT.Where(x => x.StockInDateTime >= start);
        //    }
        //    //查詢入庫日期(迄)
        //    if (!string.IsNullOrEmpty(DateEnd) && DateTime.Parse(DateEnd) != DateTime.MinValue)
        //    {
        //        DateTime end = DateTime.Parse(DateEnd).AddDays(1);
        //        rpT = rpT.Where(x => x.StockInDateTime < end);
        //    }

        //    // 確認 sort 和 order 不為空才進行排序
        //    if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order)) rpT = rpT.OrderBy(sort + " " + order); // 使用 System.Linq.Dynamic.Core 套件進行動態排序
        //    else rpT = rpT.OrderByDescending(x => x.SIRSN);

        //    //記住總筆數
        //    int Total = rpT.Count();
        //    //切頁
        //    rpT = rpT.Skip((page - 1) * rows).Take(rows);

        //    //回傳JSON陣列
        //    JArray ja = new JArray();

        //    if (rpT != null || Total > 0)
        //    {
        //        var TypeDics = Surface.StockType();
        //        var UnitDics = Surface.Unit();
        //        foreach (var item in rpT)
        //        {
        //            var itemObject = new JObject
        //            {
        //                { "SIRSN", item.SIRSN },
        //                { "StockType", item.StockType != null ? TypeDics[item.StockType] : null },
        //                { "StockName", item.StockName },
        //                { "TotalAmount", item.TotalAmount },
        //                { "Unit", item.Unit != null ? UnitDics[item.Unit] : null },
        //                { "StockInDateTime", item.StockInDateTime.ToString("yyyy/MM/dd HH:mm:ss") },
        //                { "StockInMyName", item.StockInMyName },
        //            };

        //            ja.Add(itemObject);
        //        }
        //    }

        //    JObject jo = new JObject
        //    {
        //        { "rows", ja },
        //        { "total", Total }
        //    };

        //    return jo;
        //}
        #endregion

        #region 領用申請管理
        //public JObject GetJsonForGrid_StoresRequisition_Management(System.Web.Mvc.FormCollection form)
        //{
        //    //解析查詢字串
        //    var SRState = form["SRState"]?.ToString();
        //    var SRSN = form["SRSN"]?.ToString();
        //    var SRMyName = form["SRMyName"]?.ToString();
        //    var SRDept = form["SRDept"]?.ToString();
        //    var AuditMyName = form["AuditMyName"]?.ToString();
        //    var AuditContent = form["AuditContent"]?.ToString();
        //    var DateType = form["DateType"]?.ToString();
        //    var DateStart = form["DateStart"]?.ToString();
        //    var DateEnd = form["DateEnd"]?.ToString();
        //    // DataGrid參數
        //    var sort = form["sort"]?.ToString();
        //    var order = form["order"]?.ToString();
        //    //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
        //    int page = 1;
        //    if (!string.IsNullOrEmpty(form["page"]?.ToString())) page = short.Parse(form["page"].ToString());
        //    int rows = 10;
        //    if (!string.IsNullOrEmpty(form["rows"]?.ToString())) rows = short.Parse(form["rows"]?.ToString());

        //    var rpT = (from sr in db.StoresRequisition
        //               join u in db.AspNetUsers on sr.SRUserName equals u.UserName into SRUserGroup
        //               join u in db.AspNetUsers on sr.AuditUserID equals u.UserName into AuditUserGroup
        //               from SR_ug in SRUserGroup.DefaultIfEmpty() // 使用 DefaultIfEmpty 進行左外部連接
        //               from Audit_ug in AuditUserGroup.DefaultIfEmpty() // 使用 DefaultIfEmpty 進行左外部連接
        //               select new
        //               {
        //                   sr.SRSN,
        //                   sr.SRState,
        //                   sr.SRDateTime,
        //                   SRMyName = SR_ug != null ? SR_ug.MyName : null,
        //                   sr.SRDept,
        //                   sr.AuditDate,
        //                   AuditMyName = Audit_ug != null ? Audit_ug.MyName : null,
        //                   sr.AuditContent
        //               });

        //    //查詢狀態
        //    if (!string.IsNullOrEmpty(SRState)) rpT = rpT.Where(x => x.SRState == SRState);
        //    //查詢單號 (模糊查詢)
        //    if (!string.IsNullOrEmpty(SRSN)) rpT = rpT.Where(x => x.SRSN.Contains(SRSN));
        //    //查詢申請人員 (模糊查詢)
        //    if (!string.IsNullOrEmpty(SRMyName)) rpT = rpT.Where(x => x.SRMyName.Contains(SRMyName));
        //    //查詢申請部門 (模糊查詢)
        //    if (!string.IsNullOrEmpty(SRDept)) rpT = rpT.Where(x => x.SRDept.Contains(SRDept));
        //    //查詢審核人員 (模糊查詢)
        //    if (!string.IsNullOrEmpty(AuditMyName)) rpT = rpT.Where(x => x.AuditMyName.Contains(AuditMyName));
        //    //查詢審核意見 (模糊查詢)
        //    if (!string.IsNullOrEmpty(AuditContent)) rpT = rpT.Where(x => x.AuditContent.Contains(AuditContent));
        //    //查詢領用申請/審核日期
        //    if (!string.IsNullOrEmpty(DateType))
        //    {
        //        //日期(起)
        //        if (!string.IsNullOrEmpty(DateStart) && DateTime.Parse(DateStart) != DateTime.MinValue)
        //        {
        //            DateTime start = DateTime.Parse(DateStart);  // 轉為DateTime
        //            rpT = rpT.Where($"{DateType} >= @0", start);
        //        }
        //        //日期(迄)
        //        if (!string.IsNullOrEmpty(DateEnd) && DateTime.Parse(DateEnd) != DateTime.MinValue)
        //        {
        //            DateTime end = DateTime.Parse(DateEnd).AddDays(1);
        //            rpT = rpT.Where($"{DateType} < @0", end);
        //        }
        //    }

        //    // 確認 sort 和 order 不為空才進行排序
        //    if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order)) rpT = rpT.OrderBy(sort + " " + order); // 使用 System.Linq.Dynamic.Core 套件進行動態排序
        //    else rpT = rpT.OrderByDescending(x => x.SRSN);

        //    //記住總筆數
        //    int Total = rpT.Count();
        //    //切頁
        //    rpT = rpT.Skip((page - 1) * rows).Take(rows);

        //    //回傳JSON陣列
        //    JArray ja = new JArray();

        //    if (rpT != null || Total > 0)
        //    {
        //        var StateDics = Surface.SRState();
        //        var UnitDics = Surface.Unit();
        //        foreach (var item in rpT)
        //        {
        //            var itemObject = new JObject
        //            {
        //                { "SRSN", item.SRSN },
        //                { "SRState", item.SRState },
        //                { "SRStateName", StateDics[item.SRState] },
        //                { "SRDateTime", item.SRDateTime.ToString("yyyy/MM/dd") },
        //                { "SRMyName", item.SRMyName },
        //                { "SRDept", item.SRDept },
        //                { "AuditMyName", item.AuditMyName },
        //                { "AuditDate", item.AuditDate?.ToString("yyyy/MM/dd") },
        //                { "AuditContent", item.AuditContent }
        //            };

        //            ja.Add(itemObject);
        //        }
        //    }

        //    JObject jo = new JObject
        //    {
        //        { "rows", ja },
        //        { "total", Total }
        //    };

        //    return jo;
        //}
        #endregion

        #region 出庫管理
        //public JObject GetJsonForGrid_StockOut_Management(System.Web.Mvc.FormCollection form)
        //{
        //    //解析查詢字串
        //    var SORSN = form["SORSN"]?.ToString();
        //    var SRSN = form["SRSN"]?.ToString();
        //    var StockOutMyName = form["StockOutMyName"]?.ToString();
        //    var ReceiverMyName = form["ReceiverMyName"]?.ToString();
        //    var StockOutContent = form["StockOutContent"]?.ToString();
        //    var StockOutDateStart = form["StockOutDateStart"]?.ToString();
        //    var StockOutDateEnd = form["StockOutDateEnd"]?.ToString();
        //    // DataGrid參數
        //    var sort = form["sort"]?.ToString();
        //    var order = form["order"]?.ToString();
        //    //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
        //    int page = 1;
        //    if (!string.IsNullOrEmpty(form["page"]?.ToString())) page = short.Parse(form["page"].ToString());
        //    int rows = 10;
        //    if (!string.IsNullOrEmpty(form["rows"]?.ToString())) rows = short.Parse(form["rows"]?.ToString());

        //    var rpT = (from so in db.StockOutRecord
        //               join u in db.AspNetUsers on so.StockOutUserName equals u.UserName into SOUserGroup
        //               join u in db.AspNetUsers on so.ReceiverUserName equals u.UserName into RUserGroup
        //               from SO_ug in SOUserGroup.DefaultIfEmpty() // 使用 DefaultIfEmpty 進行左外部連接
        //               from R_ug in RUserGroup.DefaultIfEmpty() // 使用 DefaultIfEmpty 進行左外部連接
        //               select new
        //               {
        //                   so.SORSN,
        //                   so.SRSN,
        //                   StockOutMyName = SO_ug != null ? SO_ug.MyName : null,
        //                   ReceiverMyName = R_ug != null ? R_ug.MyName : null,
        //                   so.StockOutContent,
        //                   so.StockOutDateTime
        //               });

        //    //查詢出庫單號 (模糊查詢)
        //    if (!string.IsNullOrEmpty(SORSN)) rpT = rpT.Where(x => x.SORSN.Contains(SORSN));
        //    //查詢領用單號 (模糊查詢)
        //    if (!string.IsNullOrEmpty(SRSN)) rpT = rpT.Where(x => x.SRSN.Contains(SRSN));
        //    //查詢出庫人員 (模糊查詢)
        //    if (!string.IsNullOrEmpty(StockOutMyName)) rpT = rpT.Where(x => x.StockOutMyName.Contains(StockOutMyName));
        //    //查詢領取人員 (模糊查詢)
        //    if (!string.IsNullOrEmpty(ReceiverMyName)) rpT = rpT.Where(x => x.ReceiverMyName.Contains(ReceiverMyName));
        //    //查詢說明 (模糊查詢)
        //    if (!string.IsNullOrEmpty(StockOutContent)) rpT = rpT.Where(x => x.StockOutContent.Contains(StockOutContent));
        //    //查詢入庫日期(起)
        //    if (!string.IsNullOrEmpty(StockOutDateStart) && DateTime.Parse(StockOutDateStart) != DateTime.MinValue)
        //    {
        //        DateTime start = DateTime.Parse(StockOutDateStart);  // 轉為DateTime
        //        rpT = rpT.Where(x => x.StockOutDateTime >= start);
        //    }
        //    //查詢入庫日期(迄)
        //    if (!string.IsNullOrEmpty(StockOutDateEnd) && DateTime.Parse(StockOutDateEnd) != DateTime.MinValue)
        //    {
        //        DateTime end = DateTime.Parse(StockOutDateEnd).AddDays(1);
        //        rpT = rpT.Where(x => x.StockOutDateTime < end);
        //    }

        //    // 確認 sort 和 order 不為空才進行排序
        //    if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order)) rpT = rpT.OrderBy(sort + " " + order); // 使用 System.Linq.Dynamic.Core 套件進行動態排序
        //    else rpT = rpT.OrderByDescending(x => x.SORSN);

        //    //記住總筆數
        //    int Total = rpT.Count();
        //    //切頁
        //    rpT = rpT.Skip((page - 1) * rows).Take(rows);

        //    //回傳JSON陣列
        //    JArray ja = new JArray();

        //    if (rpT != null || Total > 0)
        //    {
        //        foreach (var item in rpT)
        //        {
        //            var itemObject = new JObject
        //            {
        //                { "SORSN", item.SORSN },
        //                { "SRSN", item.SRSN },
        //                { "StockOutMyName", item.StockOutMyName },
        //                { "ReceiverMyName", item.ReceiverMyName },
        //                { "StockOutDateTime", item.StockOutDateTime?.ToString("yyyy/MM/dd HH:mm:ss") },
        //                { "StockOutContent", item.StockOutContent },
        //            };

        //            ja.Add(itemObject);
        //        }
        //    }

        //    JObject jo = new JObject
        //    {
        //        { "rows", ja },
        //        { "total", Total }
        //    };

        //    return jo;
        //}
        #endregion

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
                    if(haverecord > 0)
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
                var dic_stocktype = Surface.StockType();
                foreach (var item in rpT)
                {
                    var itemObject = new JObject();
                    itemObject.Add("DateTime", item.ChangeTime.ToString("yyyy/MM/dd HH:mm:ss")); //日期時間
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
                        { "EDate", item.EDate.ToString("yyyy/MM/dd") },
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
                        { "UploadDateTime", item.UploadDateTime?.ToString("yyyy/MM/dd HH:mm:ss") },
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
                        { "EDDate", item.EDDate.ToString("yyyy/MM/dd") },
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
        //public JObject GetJsonForGrid_WarningMessage_Management(System.Web.Mvc.FormCollection form)
        //{
        //    //解析查詢字串
        //    var WMType = form["WMType"]?.ToString();
        //    var WMState = form["WMState"]?.ToString();
        //    var ASN = form["ASN"]?.ToString();
        //    var Area = form["Area"]?.ToString();
        //    var FSN = form["FSN"]?.ToString();
        //    var Floor = form["Floor"]?.ToString();
        //    var Message = form["Message"]?.ToString();
        //    var IPSN = form["IPSN"]?.ToString();
        //    var MyName = form["MyName"]?.ToString();
        //    var DateStart = form["DateStart"]?.ToString();
        //    var DateEnd = form["DateEnd"]?.ToString();
        //    // DataGrid參數
        //    var sort = form["sort"]?.ToString();
        //    var order = form["order"]?.ToString();
        //    //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
        //    int page = 1;
        //    if (!string.IsNullOrEmpty(form["page"]?.ToString())) page = short.Parse(form["page"].ToString());
        //    int rows = 10;
        //    if (!string.IsNullOrEmpty(form["rows"]?.ToString())) rows = short.Parse(form["rows"]?.ToString());

        //    var rpT = from x1 in db.WarningMessage
        //              join x2 in db.InspectionPlanMember
        //              on x1.PMSN equals x2.PMSN
        //              join x3 in db.Floor_Info
        //              on x1.FSN equals x3.FSN
        //              join x4 in db.AspNetUsers
        //              on x2.UserID equals x4.UserName
        //              join x5 in db.AreaInfo
        //              on x3.ASN equals x5.ASN
        //              select new
        //              {
        //                  x1.WMSN,
        //                  x1.WMType,
        //                  x1.WMState,
        //                  x1.TimeOfOccurrence,
        //                  x1.FSN,
        //                  x1.Message,
        //                  x1.PMSN,
        //                  x2.IPSN,
        //                  x3.ASN,
        //                  x3.FloorName,
        //                  x4.MyName,
        //                  x5.Area
        //              };
        //    //查詢事件等級
        //    if (!string.IsNullOrEmpty(WMType))
        //    {
        //        rpT = rpT.Where(x => x.WMType == WMType);
        //    }
        //    //查詢事件處理狀況
        //    if (!string.IsNullOrEmpty(WMState))
        //    {
        //        rpT = rpT.Where(x => x.WMState == WMState);
        //    }
        //    //查詢棟別
        //    if (!string.IsNullOrEmpty(ASN))
        //    {
        //        var numASN = Int32.Parse(ASN);
        //        rpT = rpT.Where(x => x.ASN == numASN);
        //    }
        //    if (!string.IsNullOrEmpty(Area))
        //    {
        //        rpT = rpT.Where(x => x.Area == Area);
        //    }
        //    //查詢樓層
        //    if (!string.IsNullOrEmpty(FSN))
        //    {
        //        rpT = rpT.Where(x => x.FSN == FSN);
        //    }
        //    if (!string.IsNullOrEmpty(Floor))
        //    {
        //        rpT = rpT.Where(x => x.FloorName == Floor);
        //    }
        //    //查詢事件內容(模糊查詢)
        //    if (!string.IsNullOrEmpty(Message))
        //    {
        //        rpT = rpT.Where(x => x.Message.Contains(Message));
        //    }
        //    //查詢巡檢計畫編號(模糊查詢)
        //    if (!string.IsNullOrEmpty(IPSN))
        //    {
        //        rpT = rpT.Where(x => x.IPSN.Contains(IPSN));
        //    }
        //    //查詢人員姓名(模糊查詢)
        //    if(!string.IsNullOrEmpty(MyName))
        //    {
        //        rpT = rpT.Where(x => x.MyName.Contains(MyName));
        //    }
        //    //查詢發生時間(起)
        //    if (!string.IsNullOrEmpty(DateStart) && DateTime.Parse(DateStart) != DateTime.MinValue)
        //    {
        //        DateTime start = DateTime.Parse(DateStart); //轉為DateTime
        //        rpT = rpT.Where(x => x.TimeOfOccurrence >= start);
        //    }
        //    //查詢發生時間(迄)
        //    if (!string.IsNullOrEmpty(DateEnd) && DateTime.Parse(DateEnd) != DateTime.MinValue)
        //    {
        //        DateTime end = DateTime.Parse(DateEnd); //轉為DateTime
        //        rpT = rpT.Where(x => x.TimeOfOccurrence <= end);
        //    }

        //    // 確認 sort 和 order 不為空才進行排序
        //    if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order)) rpT = rpT.OrderBy(sort + " " + order); // 使用 System.Linq.Dynamic.Core 套件進行動態排序
        //    else rpT = rpT.OrderByDescending(x => x.WMSN);

        //    // 記住總筆數
        //    int Total = rpT.Count();
        //    // 切頁
        //    rpT = rpT.Skip((page - 1) * rows).Take(rows);

        //    //回傳JSON陣列
        //    JArray ja = new JArray();

        //    var WMTypedic = Surface.WMType();
        //    var WMStatedic = Surface.WMState();

        //    if (rpT != null || Total > 0)
        //    {
        //        foreach (var item in rpT)
        //        {
        //            var itemObject = new JObject //DataGrid顯示的資料內容
        //            {
        //                { "WMSN", item.WMSN },
        //                { "PMSN", item.PMSN },
        //                { "WMType", WMTypedic[item.WMType] },
        //                { "WMState", WMStatedic[item.WMState] },
        //                { "TimeOfOccurrence", item.TimeOfOccurrence.ToString("yyyy/MM/dd HH:mm:ss") },
        //                { "Location", item.Area + " " + item.FloorName },
        //                { "Message", item.Message },
        //                { "IPSN", item.IPSN },
        //                { "MyName", item.MyName },
        //            };

        //            ja.Add(itemObject);
        //        }
        //    }

        //    JObject jo = new JObject
        //    {
        //        { "rows", ja },
        //        { "total", Total }
        //    };

        //    return jo;
        //}
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
                        { "UploadDateTime", (item.UploadDateTime != DateTime.MinValue && item.UploadDateTime != null) ? item.UploadDateTime.ToString("yyyy/MM/dd") : null }
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

                itemObject.Add("MeetingDate", $"{item.MeetingDate:yyyy/MM/dd} {item.MeetingDateStart:hh:mm}-{item.MeetingDateEnd:hh:mm}");

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

