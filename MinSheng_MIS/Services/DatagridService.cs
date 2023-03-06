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


            #region 依據查詢字串檢索資料表

            var SourceTable = from x1 in db.EquipmentReportForm
                              join x2 in db.EquipmentInfo on x1.ESN equals x2.ESN
                              join x3 in db.AspNetUsers on x1.InformatUserID equals x3.UserName
                              select new { x1.ReportState, x1.ReportLevel, x2.Area, x2.Floor, x1.ReportSource, x1.RSN, x1.Date, x2.PropertyCode, x1.ESN, x2.EName, x1.ReportContent, x3.MyName ,x3.UserName};

            //Area查詢table方式 以Area至表[設備資訊]查詢出ESN，再以ESN至表[設備報修單]查詢出相關報修單。
            if (!string.IsNullOrEmpty(Area)) {
                SourceTable = SourceTable.Where(x=>x.Area == Area);
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
            var resulttable = SourceTable.OrderBy(x => x.Date).AsQueryable();
            //回傳JSON陣列
            JArray ja = new JArray();
            //記住總筆數
            int total = resulttable.Count();
            //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
            resulttable = resulttable.Skip((page - 1) * rows).Take(rows);


            foreach (var a in resulttable) {
                var itemObjects = new JObject();
                if (itemObjects["ReportState"] == null) 
                {
                    string statsSN = a.ReportState.Trim();
                    var dic = Surface.EquipmentReportFormState();
                    //string aaaaa = dic["8"];
                    itemObjects.Add("ReportState", dic[statsSN]); //報修單狀態
                }
                if (itemObjects["ReportLevel"] == null) {
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
                    itemObjects.Add("Date", a.Date?.ToString("yyyy/M/d HH:mm:ss"));                                //保養週期

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
                ja.Add(itemObjects);
            }

            JObject jo = new JObject();
            jo.Add("rows", ja);
            jo.Add("total", total);
            return jo;
        }

    }
}