using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Http.Results;

namespace MinSheng_MIS.Models.ViewModels
{
    public class DesignDiagramsViewModel
    {
        public string DDSN { get; set; }
        public string ImgName { get; set; }
        public string ImgType { get; set; }
        public HttpPostedFileBase DesignDiagrams { get; set; }

    }
}