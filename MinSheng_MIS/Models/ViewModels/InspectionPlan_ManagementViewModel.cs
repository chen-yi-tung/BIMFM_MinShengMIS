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
	public class InspectionPlan_ManagementViewModel
	{
		Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();

		#region APP-巡檢工單列表
		public class PlanInfo
		{
			public string InspectionState { get; set; }
			public string IPTSN { get; set; }
			public string InspectionTime { get; set; }
			public List<string> Member { get; set; }
		}
		#endregion

		#region APP-巡檢RFID列表
		public class PlanRFIDInfo
		{
			public string Status { get; set; } //巡檢狀態
			public string EName { get; set; } //設備名稱
			public string EState { get; set; }//設備狀態
			public string NO { get; set; } //設備編號

			public string Location { get; set; } //地點
            public string FSN { get; set; } //地點
            public string RFIDInternalCode { get; set; } //RFID編碼
			public string ESN { get; set; } //取設備內容用
			public string InspectionOrder { get; set; } //填報用
            public string RFID_FSN { get; set; } //地點
            public decimal? RFID_Location_X { get; set; } //地點
            public decimal? RFID_Location_Y { get; set; } //地點
        }
		#endregion

		#region APP-巡檢填報
		public class PlanFillInInfo
		{
			public string InspectionOrder { get; set; }

			public List<EquipmentCheckItem> EquipmentCheckItems { get; set; }
			public List<EquipmentReportingItem> EquipmentReportingItems { get; set; }
		}
		public class EquipmentCheckItem
		{
			public string Id { get; set; } //編號
            public string CheckItemName { get; set; } //檢查項目名稱
            public string CheckResult { get; set; } //檢查結果
        }
		public class EquipmentReportingItem
		{
			public string Id { get; set; } //編號
            public string ReportValue { get; set; } //填報項目名稱
            public string ReportContent { get; set; } //填報內容
            public string Unit { get; set; } //單位
        }
        #endregion
    }
}