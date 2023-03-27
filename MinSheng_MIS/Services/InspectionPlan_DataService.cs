using MinSheng_MIS.Models;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Services
{
    public class InspectionPlan_DataService
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
            var SourceTable = db.InspectionPlan.AsQueryable();

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

            }
            //設備名稱
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
    }
}