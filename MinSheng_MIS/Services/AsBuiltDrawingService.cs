using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.Http.Results;
using System.Web.Mvc;
using System.Web.Services.Protocols;
using System.Data.Entity.Migrations;
using Microsoft.Owin.Security;

namespace MinSheng_MIS.Services
{
    public class AsBuiltDrawingService
    {
        private readonly Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();

        #region 新增竣工圖
        public void AddAsBuiltDrawing(AsBuiltDrawingViewModel info, string ADSN, string FileName)
        {
            var drawing = new AsBuiltDrawing
            {
                ADSN = ADSN,
                ImgPath = "/" + FileName,
                FSN = info.FSN,
                DSubSystemID = info.DSubSystemID,
                ImgNum = info.ImgNum,
                ImgName = info.ImgName,
                ImgVersion = info.ImgVersion,
                UploadDate = DateTime.Now,
                UploadUser = HttpContext.Current.User.Identity.Name
            };

            db.AsBuiltDrawing.AddOrUpdate(drawing);
            db.SaveChanges();
        }
        #endregion

        #region 編輯竣工圖
        public void EditAsBuiltDrawing(AsBuiltDrawingViewModel info, string FileName)
        {
            var drawing = db.AsBuiltDrawing.Find(info.ADSN);
            if (!string.IsNullOrEmpty(FileName))
            {
                drawing.ImgPath = "/" + FileName;
            }
            drawing.ImgNum = info.ImgNum;
            drawing.ImgName = info.ImgName;
            drawing.ImgVersion = info.ImgVersion;
            drawing.UploadDate = DateTime.Now;
            drawing.UploadUser = HttpContext.Current.User.Identity.Name;

            db.AsBuiltDrawing.AddOrUpdate(drawing);
            db.SaveChanges();
        }
        #endregion
    }
}