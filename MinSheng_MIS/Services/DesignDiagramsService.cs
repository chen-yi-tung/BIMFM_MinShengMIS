using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Services
{
    public class DesignDiagramsService
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
        public void AddDesignDiagrams(DesignDiagramsViewModel ddvm, string newDDSN, string Filename)
        {
            #region 新增設計圖說

            var dditem = new DesignDiagrams();
            dditem.DDSN = newDDSN;
            dditem.ImgName = ddvm.ImgName;
            dditem.ImgType = ddvm.ImgType;
            dditem.UploadDate = DateTime.Now.Date;
            dditem.UploadUser = HttpContext.Current.User.Identity.Name;
            dditem.ImgPath = "/" + Filename;

            db.DesignDiagrams.AddOrUpdate(dditem);
            db.SaveChanges();
            #endregion
        }
        public void EditDesignDiagrams(DesignDiagramsViewModel ddvm, string DDSN, string Filename)
        {
            #region 編輯設計圖說

            var dditem = db.DesignDiagrams.Find(DDSN);
            dditem.ImgName = ddvm.ImgName;
            dditem.ImgType = ddvm.ImgType;
            dditem.UploadDate = DateTime.Now.Date;
            dditem.UploadUser = HttpContext.Current.User.Identity.Name;
            if (!string.IsNullOrEmpty(Filename))
            {
                dditem.ImgPath = "/" + Filename;
            }

            db.DesignDiagrams.AddOrUpdate(dditem);
            db.SaveChanges();
            #endregion
        }
    }
}