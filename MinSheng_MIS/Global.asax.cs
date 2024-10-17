using MinSheng_MIS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MinSheng_MIS
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            #region 檢查是否需產生設備保養單項目
            //Check_EquipmentFormItem c = new Check_EquipmentFormItem();
            //c.CheckEquipmentFormItem();
            #endregion

            //#region 檢查巡檢計畫是否過期還沒開始巡檢
            //Check_InspectionPlan checkplan = new Check_InspectionPlan();
            //checkplan.CheckInspectionPlan();
            //#endregion

            System.Timers.Timer Wtimer = new System.Timers.Timer(3600000);//執行任務的週期 //小時3600000 1000
            Wtimer.Elapsed += new System.Timers.ElapsedEventHandler(Wtimer_Elapsed);
            Wtimer.Enabled = true;
            Wtimer.AutoReset = true;
        }
        async void Wtimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            #region 檢查是否需產生設備保養單項目
            //Check_EquipmentFormItem c = new Check_EquipmentFormItem();
            //c.CheckEquipmentFormItem();
            #endregion
            //#region 檢查巡檢計畫是否過期還沒開始巡檢
            //Check_InspectionPlan checkplan = new Check_InspectionPlan();
            //checkplan.CheckInspectionPlan();
            //#endregion

        }
    }
}
