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
            string Floor = form["Floor"]?.ToString();
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
            var SourceTable = from x1 in db.InspectionPlanMaintain
                              join x2 in db.InspectionPlan on x1.IPSN equals x2.IPSN
                              join x3 in db.EquipmentMaintainFormItem on x1.EMFISN equals x3.EMFISN
                              join x4 in db.EquipmentMaintainItem on x3.EMISN equals x4.EMISN
                              join x5 in db.EquipmentInfo on x4.ESN equals x5.ESN
                              join x6 in db.MaintainItem on x4.MISN equals x6.MISN
                              join x7 in db.AspNetUsers on x1.MaintainUserID equals x7.UserName
                              join x8 in db.MaintainAuditInfo on x1.IPMSN equals x8.IPMSN //這邊是要用IPMAN再去查Users的表
                              //join x9 in db.AspNetUsers on x8.AuditUserID equals x9.UserName
                              select new
                              {
                                  x1.IPMSN,
                                  x1.IPSN,
                                  x2.IPName,
                                  x2.PlanDate,
                                  x1.MaintainState,
                                  x5.Area,
                                  x5.Floor,
                                  x5.PropertyCode,
                                  x4.ESN,
                                  x5.EName,
                                  x1.EMFISN,
                                  x6.MIName,
                                  x3.Unit,
                                  x3.Period,
                                  x3.LastTime,
                                  x3.Date,
                                  x3.NextTime,
                                  MaintainUserID = x7.MyName,
                                  //AuditUserID = x9.MyName,
                                  x8.AuditDate,
                                  MaintainID = x8.IPMSN
                              };

            var sourcequery = SourceTable.AsQueryable();
            foreach (var item in sourcequery)
            {
                
            }
            
            var cc = SourceTable.ToList();

            //Area查詢table方式 以Area至表[設備資訊]查詢出ESN，再以ESN至表[設備報修單]查詢出相關報修單。

            //查詢參數如下:
            //棟別**樓層**巡檢計畫編號**巡檢計畫名稱**報修單狀態**維修等級
            //報修單號**設備編號**設備名稱**國有財產編號**報修說明**報修人員
            //施工人員**審核人員**日期項目選擇**日期(起)**日期(迄)
            if (!string.IsNullOrEmpty(Area)) //棟別
            {
                SourceTable = SourceTable.Where(x => x.Area == Area);
            }
            if (!string.IsNullOrEmpty(Floor)) //樓層
            {
                SourceTable = SourceTable.Where(x => x.Floor == Floor);
            }
            if (!string.IsNullOrEmpty(IPSN)) //巡檢計畫編號
            {
                SourceTable = SourceTable.Where(x => x.IPSN.Contains(IPSN));
            }
            if (!string.IsNullOrEmpty(IPName)) //巡檢計畫名稱
            {
                SourceTable = SourceTable.Where(x => x.IPName.Contains(IPName));
            }
            if (!string.IsNullOrEmpty(MaintainState)) //保養單狀態
            {
                SourceTable = SourceTable.Where(x => x.MaintainState == MaintainState);
            }
            if (!string.IsNullOrEmpty(PropertyCode)) //國有財產編碼
            {
                SourceTable = SourceTable.Where(x => x.PropertyCode.Contains(PropertyCode));
            }
            if (!string.IsNullOrEmpty(ESN)) //設備編號
            {
                SourceTable = SourceTable.Where(x => x.ESN == ESN);
            }
            if (!string.IsNullOrEmpty(EName)) //設備名稱
            {
                SourceTable = SourceTable.Where(x => x.EName == EName);
            }
            if (!string.IsNullOrEmpty(EMFISN)) //保養項目編號
            {
                SourceTable = SourceTable.Where(x => x.EMFISN.Contains(EMFISN));
            }
            if (!string.IsNullOrEmpty(MIName)) //保養項目
            {
                SourceTable = SourceTable.Where(x => x.MIName.Contains(MIName));
            }
            if (!string.IsNullOrEmpty(MaintainUserID)) //保養人員
            {
                SourceTable = SourceTable.Where(x => x.MaintainUserID == MaintainUserID);
            }
            if (!string.IsNullOrEmpty(AuditUserID)) //審核人員
            {
                SourceTable = SourceTable.Where(x => x.AuditUserID == AuditUserID);
            }

            if (!string.IsNullOrEmpty(DateSelect)) //日期項目選擇
            {
                if (!string.IsNullOrEmpty(DateFrom)) //日期(起)
                {
                    var datefrom = DateTime.Parse(DateFrom);
                    switch (DateSelect)
                    {
                        case "1": SourceTable = SourceTable.Where(x => x.PlanDate >= datefrom); break;
                        case "2": SourceTable = SourceTable.Where(x => x.LastTime >= datefrom); break;
                        case "3": SourceTable = SourceTable.Where(x => x.Date >= datefrom); break;
                        case "4": SourceTable = SourceTable.Where(x => x.NextTime >= datefrom); break;
                        case "5": SourceTable = SourceTable.Where(x => x.AuditDate >= datefrom); break;
                    }
                }
                if (!string.IsNullOrEmpty(DateTo)) //日期(迄)
                {
                    var dateto = DateTime.Parse(DateTo).AddDays(1);
                    switch (DateSelect)
                    {
                        case "1": SourceTable = SourceTable.Where(x => x.PlanDate <= dateto); break;
                        case "2": SourceTable = SourceTable.Where(x => x.LastTime <= dateto); break;
                        case "3": SourceTable = SourceTable.Where(x => x.Date <= dateto); break;
                        case "4": SourceTable = SourceTable.Where(x => x.NextTime <= dateto); break;
                        case "5": SourceTable = SourceTable.Where(x => x.AuditDate <= dateto); break;
                    }
                }
            }
            #endregion

            //排序資料表
            var resulttable = SourceTable.OrderByDescending(x => x.IPMSN).AsQueryable();
            //回傳JSON陣列
            JArray ja = new JArray();
            //記住總筆數
            int total = resulttable.Count();
            //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
            resulttable = resulttable.Skip((page - 1) * rows).Take(rows);

            //建Json格式資料表回傳給前端
            foreach (var a in resulttable)
            {
                var itemObjects = new JObject();
                itemObjects.Add("IPMSN", a.IPMSN);
                itemObjects.Add("IPSN", a.IPSN);
                itemObjects.Add("IPName", a.IPName);
                itemObjects.Add("PlanDate", a.PlanDate);
                var dic = Surface.EquipmentReportFormState();
                itemObjects.Add("MaintainState", dic[a.MaintainState.Trim()]); //這個要再用 Surface 做中文轉譯!!
                itemObjects.Add("Area", a.Area);
                itemObjects.Add("Floor", a.Floor);
                itemObjects.Add("PropertyCode", a.PropertyCode);
                itemObjects.Add("ESN", a.ESN);
                itemObjects.Add("EName", a.EName);
                itemObjects.Add("EMFISN", a.EMFISN);
                itemObjects.Add("MIName", a.MIName);
                itemObjects.Add("Unit", a.Unit);
                itemObjects.Add("Period", a.Period);
                itemObjects.Add("LastTime", a.LastTime);
                itemObjects.Add("Date", a.Date);
                itemObjects.Add("NextTime", a.NextTime);
                itemObjects.Add("MaintainUserID", a.MaintainUserID);
                itemObjects.Add("AuditUserID", a.AuditUserID);
                itemObjects.Add("AuditDate", a.AuditDate);

                ja.Add(itemObjects);
            }

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
            //string propertyName = "Date";
            //string order = "asc";

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
                              select new { x1.EMISN, x1.IsEnable, x2.Area, x2.Floor, x2.System, x2.SubSystem, x1.ESN, x2.EName, x1.MISN, x3.MIName, x1.Unit, x1.Period, x1.LastTime, x1.NextTime };

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
                    itemObjects.Add("LastTime", a.LastTime?.ToString("yyyy/M/d"));    //上次保養日期
                if (itemObjects["NextTime"] == null)
                    itemObjects.Add("NextTime", a.NextTime?.ToString("yyyy/M/d"));    //最近應保養日期
                ja.Add(itemObjects);
            }

            JObject jo = new JObject();
            jo.Add("rows", ja);
            jo.Add("total", total);
            return jo;
        }

    }
}