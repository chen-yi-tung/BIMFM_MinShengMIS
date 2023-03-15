using MinSheng_MIS.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.WebPages;

namespace MinSheng_MIS.Services
{
    public class EquipmentMaintainForm_DataService
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
            //棟別編號
            string ASN = form["Area"]?.ToString();
            //樓層編號
            string FSN = form["Floor"]?.ToString();
            //保養項目狀態
            string FormItemState = form["FormItemState"]?.ToString();
            //國有財產編碼
            string PropertyCode = form["PropertyCode"]?.ToString();
            //設備編號
            string ESN = form["ESN"]?.ToString();
            //設備名稱
            string EName = form["EName"]?.ToString();
            //保養項目編號
            string ENaEMFISNme = form["EMFISN"]?.ToString();
            //保養項目
            string MIName = form["MIName"]?.ToString();
            //日期項目選擇
            string DateSelect = form["DateSelect"]?.ToString();
            //日期(起)
            string DateFrom = form["DateFrom"]?.ToString();
            //日期(迄)
            string DateTo = form["DateTo"]?.ToString();


            #region 依據查詢字串檢索資料表
            var SourceTable = from x1 in db.EquipmentMaintainFormItem
                              join x2 in db.EquipmentMaintainItem on x1.EMISN equals x2.EMISN
                              join x3 in db.EquipmentInfo on x2.ESN equals x3.ESN
                              join x4 in db.MaintainItem on x2.MISN equals x4.MISN
                              join x5 in db.Floor_Info on x3.FSN equals x5.FSN
                              join x6 in db.AreaInfo on x5.ASN equals x6.ASN
                              select new {  x1.FormItemState, x6.Area, x5.FloorName, x3.PropertyCode, x3.EName, x1.EMFISN, x4.MIName, x1.Unit, x1.Period, x1.LastTime, x1.Date};
            #endregion

            //回傳JSON陣列
            JArray ja = new JArray();

            JObject jo = new JObject();
            jo.Add("rows", ja);
            return jo;
        }
    }
}