using MinSheng_MIS.Models;
using MinSheng_MIS.Surfaces;
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
            string ASN = form["ASN"]?.ToString();
            //樓層編號
            string FSN = form["FSN"]?.ToString();
            //保養項目狀態
            string FormItemState = form["FormItemState"]?.ToString();
            //國有財產編碼
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
                              select new {  x1.FormItemState, x6.Area, x5.FloorName, x3.PropertyCode, x3.EName, x1.EMFISN, x4.MIName, x1.Unit, x1.Period, x1.LastTime, x1.Date, x5.ASN, x3.FSN, x2.ESN, x2.MISN, x3.EState, x1.StockState, x3.DBID};

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
            if(!string.IsNullOrEmpty(FSN))
            {
                SourceTable = SourceTable.Where(x => x.FSN == FSN);
            }
            //查詢保養項目狀態
            if(!string.IsNullOrEmpty(FormItemState))
            {
                SourceTable = SourceTable.Where(x => x.FormItemState == FormItemState);
            }
            //查詢國有財產編碼
            if (!string.IsNullOrEmpty(PropertyCode))
            {
                SourceTable = SourceTable.Where(x => x.PropertyCode == PropertyCode);
            }
            //查詢設備編號
            if(!string.IsNullOrEmpty(ESN))
            {
                SourceTable = SourceTable.Where(x => x.ESN == ESN);
            }
            //查詢設備名稱 模糊查詢
            if (!string.IsNullOrEmpty(EName))
            {
                SourceTable = SourceTable.Where(x => x.EName.Contains(EName));
            }
            //查詢保養項目編號
            if(!string.IsNullOrEmpty(MISN))
            {
                SourceTable = SourceTable.Where(x => x.MISN == MISN);
            }
            //查詢保養項目 模糊查詢
            if(!string.IsNullOrEmpty(MIName))
            {
                SourceTable = SourceTable.Where(x => x.MIName.Contains(MIName));
            }
            //查詢日期
            if (!string.IsNullOrEmpty(DateSelect))
            {
                if (!string.IsNullOrEmpty(DateFrom))
                {
                    var datefrom = DateTime.Parse(DateFrom);
                    if(DateSelect == "上次保養日期")
                    {
                        SourceTable = SourceTable.Where(x => x.LastTime >= datefrom);
                    }
                    else if(DateSelect == "最近應保養日期")
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

            foreach(var item in SourceTable)
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
                //國有財產編碼
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
                if(item.LastTime != DateTime.MinValue && item.LastTime != null)
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
    }
}