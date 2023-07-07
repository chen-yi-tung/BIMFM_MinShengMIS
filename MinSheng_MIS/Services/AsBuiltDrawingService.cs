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

namespace MinSheng_MIS.Services
{
    public class AsBuiltDrawingService
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();

        #region 新增竣工圖
        public void AddAsBuiltDrawing(AsBuiltDrawingViewModel info,string ADSN,string FileName)
        {
            var drawing = new AsBuiltDrawing();
            drawing.ADSN = ADSN;
            drawing.ImgPath = FileName;
            drawing.FSN = info.FSN;
            drawing.DSubSystem = info.DSubSystemID;
        }

        #endregion
    }
}