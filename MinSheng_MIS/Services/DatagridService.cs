using MinSheng_MIS.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin;

namespace MinSheng_MIS.Services
{
    public class DatagridService
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
        public JObject GetJsonForGrid_ReportManagement(System.Web.Mvc.FormCollection form)
        {
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
            //string propertyName = "MISN";
            //string order = "asc";

            //塞來自formdata的資料
            //string Area = form["Area"]?.ToString();
            //int ASN = short.Parse(form["Area"]?.ToString());
            //string Floor = form["Floor"]?.ToString();
            //int FSN = short.Parse(form["Floor"]?.ToString());

            string ReportState = form["ReportState"]?.ToString();
            string Level = form["Level"]?.ToString();
            string RSN = form["RSN"]?.ToString();
            string ESN = form["ESN"]?.ToString();
            string EName = form["EName"]?.ToString();
            string PropertyCode = form["PropertyCode"]?.ToString();
            string ReportContent = form["ReportContent"]?.ToString();
            string InformantUserID = form["InformantUserID"]?.ToString();
            string DateFrom = form["DateFrom"]?.ToString();
            string DateTo = form["DateTo"]?.ToString();


            #region 依據查詢字串檢索資料表
            //Area查詢table方式 以Area至表[設備資訊]查詢出ESN，再以ESN至表[設備報修單]查詢出相關報修單。
            //var atable_ESN = db.EquipmentInfo.Where(x => x.ESN == ASN).Select(x=>x.E);
            //var atable_SearchTable = db.EquipmentReportForm.Where(x=>x.ESN == atable.)

            #endregion

            //回傳JSON陣列
            JArray ja = new JArray();

            //注意:"{0} {1}"中間必須為一個空格，以讓系統識別此二參數，注意:必須使用OrderBy，不可使用 OrderByDescent
            //table = table.OrderBy(string.Format("{0} {1}", propertyName, order));
            //int total = table.Count();
            int total = 1;
            //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
            #region
            //if (table != null && total > 0)
            //{
            //    foreach (var item in table)
            //    {
            //        var itemObjects = new JObject();
            //        if (itemObjects["State"] == null)
            //            itemObjects.Add("State", "已派工");                                    //保養設備編號

            //        if (itemObjects["Level"] == null)
            //            itemObjects.Add("Level", "最速件");                                //保養項目名稱

            //        if (itemObjects["Area"] == null)
            //            itemObjects.Add("System", "A棟");                                //系統別

            //        if (itemObjects["Floor"] == null)
            //            itemObjects.Add("SubSystem", "1F");                          //子系統別

            //        if (itemObjects["ReportSource"] == null)
            //            itemObjects.Add("EName", "APP");                                  //設備名稱

            //        if (itemObjects["RSN"] == null)
            //            itemObjects.Add("Unit", "R23010601");                                    //保養週期單位

            //        if (itemObjects["Date"] == null)
            //            itemObjects.Add("Period", "2023/1/6 15:00:02");                                //保養週期

            //        if (itemObjects["PropertyCode"] == null)
            //            itemObjects.Add("MaintainItemIsEnable", "無");    //保養項目停啟用狀態
            //        if (itemObjects["ESN"] == null)
            //            itemObjects.Add("MaintainItemIsEnable", "E00001");    //保養項目停啟用狀態
            //        if (itemObjects["EName"] == null)
            //            itemObjects.Add("MaintainItemIsEnable", "日光燈");    //保養項目停啟用狀態
            //        if (itemObjects["ReportContent"] == null)
            //            itemObjects.Add("ReportContent", "故障");    //保養項目停啟用狀態
            //        if (itemObjects["MyName"] == null)
            //            itemObjects.Add("MyName", "王大明");    //保養項目停啟用狀態
            //        ja.Add(itemObjects);
            //    }
            //}
            #endregion
            var itemObjects = new JObject();
            if (itemObjects["State"] == null)
                itemObjects.Add("State", "已派工");                                    //保養設備編號

            if (itemObjects["Level"] == null)
                itemObjects.Add("Level", "最速件");                                //保養項目名稱

            if (itemObjects["Area"] == null)
                itemObjects.Add("System", "A棟");                                //系統別

            if (itemObjects["Floor"] == null)
                itemObjects.Add("SubSystem", "1F");                          //子系統別

            if (itemObjects["ReportSource"] == null)
                itemObjects.Add("EName", "APP");                                  //設備名稱

            if (itemObjects["RSN"] == null)
                itemObjects.Add("Unit", "R23010601");                                    //保養週期單位

            if (itemObjects["Date"] == null)
                itemObjects.Add("Period", "2023/1/6 15:00:02");                                //保養週期

            if (itemObjects["PropertyCode"] == null)
                itemObjects.Add("MaintainItemIsEnable", "無");    //保養項目停啟用狀態
            if (itemObjects["ESN"] == null)
                itemObjects.Add("ESN", "E00001");    //保養項目停啟用狀態
            if (itemObjects["EName"] == null)
                itemObjects.Add("EName", "日光燈");    //保養項目停啟用狀態
            if (itemObjects["ReportContent"] == null)
                itemObjects.Add("ReportContent", "故障");    //保養項目停啟用狀態
            if (itemObjects["MyName"] == null)
                itemObjects.Add("MyName", "王大明");    //保養項目停啟用狀態
            ja.Add(itemObjects);
            JObject jo = new JObject();
            jo.Add("rows", ja);
            jo.Add("total", total);
            return jo;
        }
    }
}