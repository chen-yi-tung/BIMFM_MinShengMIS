using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Services;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using static MinSheng_MIS.Models.ViewModels.InspectionPlan_ManagementViewModel;
using System.Threading.Tasks;

namespace MinSheng_MIS.Controllers.API
{
    public class WarningMessage_ManagementController : ApiController
    {
        public class AddWarningMessageController : ApiController
        {
            private readonly Bimfm_MinSheng_MISEntities _db;
            private readonly WarningMessageService _warningMessageService;

            public AddWarningMessageController()
            {
                _db = new Bimfm_MinSheng_MISEntities();
                _warningMessageService = new WarningMessageService(_db);
            }
            public async Task<JsonResService<string>> PostAsync(WarningMessageCreateModel info)
            {
                JsonResService<string> result = new JsonResService<string>();
                try
                {
                    string userID = HttpContext.Current.User.Identity.Name;
                    if (string.IsNullOrEmpty(userID))
                    {
                        result.AccessState = ResState.Failed;
                        result.ErrorMessage = "無登入者資料";
                    }
                    else
                    {
                        info.UserName = userID;
                        await _warningMessageService.AddWarningMessageAsync(info);
                        result.AccessState = ResState.Success;
                    }
                }
                catch (Exception ex)
                {
                    result.AccessState = ResState.Failed;
                    result.ErrorMessage = ex.Message;
                }
                return result;
            }
        }
    }
}
