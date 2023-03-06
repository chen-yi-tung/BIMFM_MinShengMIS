using Microsoft.Ajax.Utilities;
using MinSheng_MIS.Models;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Services
{
    public class SamplePath_DataService
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
            string propertyName = "PSSN";
            string order = "asc";

            //塞來自formdata的資料
            //棟別編號
            string ASN = form["Area"]?.ToString();
            //樓層編號
            string FSN = form["Floor"]?.ToString();

            //巡檢路線標題
            string PathTitle = form["PathTitle"]?.ToString();


            #region 依據查詢字串檢索資料表

            var SourceTable = from x1 in db.PathSample
                              join x2 in db.Floor_Info on x1.FSN equals x2.FSN
                              join x3 in db.AreaInfo on x2.ASN equals x3.ASN
                              select new { x1.PSSN, x1.PathTitle, x1.FSN, x2.ASN, x2.FloorName, x3.Area};

            if (!string.IsNullOrEmpty(ASN)) //查詢棟別編號
            {
                int IntAsn = Convert.ToInt32(ASN);
                SourceTable = SourceTable.Where(x => x.ASN == IntAsn);
            }
            if (!string.IsNullOrEmpty(FSN)) //查詢樓層編號
            {
                SourceTable = SourceTable.Where(x => x.FSN == FSN);
            }
            if (!string.IsNullOrEmpty(PathTitle)) //查詢路徑標題
            {
                SourceTable = SourceTable.Where(x => x.PathTitle.Contains(PathTitle));
            }
            #endregion

            var resulttable = SourceTable.OrderBy(x => x.PSSN).AsQueryable();
            //回傳JSON陣列
            JArray ja = new JArray();
            //記住總筆數
            int total = resulttable.Count();
            //回傳頁數內容處理: 回傳指定的分頁，並且可依據頁數大小設定回傳筆數
            resulttable = resulttable.Skip((page - 1) * rows).Take(rows);

            foreach (var a in resulttable)
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
    }
}