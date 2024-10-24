using Microsoft.Ajax.Utilities;
using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
//using static MinSheng_MIS.Models.ViewModels.PathSampleViewModel;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Mvc;
using WebGrease.Css.Extensions;
using static MinSheng_MIS.Models.ViewModels.PathSampleViewModel;
using PathSample = MinSheng_MIS.Models.PathSample;

namespace MinSheng_MIS.Controllers
{
	public class SamplePath_ManagementController : Controller
	{
		Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
	}
}