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

		#region 巡檢計畫-詳情 DataGrid
		public string InspectationPlan_Read_Data(string IPSN)
		{
			#region 參數
			var dic_InPlanState = Surfaces.Surface.InspectionPlanState();
			var dic_Shift = Surfaces.Surface.Shift();
			var dic_EMFIState = Surfaces.Surface.EquipmentMaintainFormItemState();
			var dic_EState = Surfaces.Surface.EState();
			var dic_ERFState = Surfaces.Surface.EquipmentReportFormState();
			var dic_RL = Surfaces.Surface.ReportLevel();
			List<string> AllMaintainItem = new List<string>();
			List<string> AllRepairItem = new List<string>();
			#endregion

			#region 巡檢計畫
			var IP_SourceTable = db.InspectionPlan.Find(IPSN);
			var IPM_UID = db.InspectionPlanMember.Where(x => x.IPSN == IPSN).Select(x => x.UserID);
			List<string> IP_NameList = new List<string>();
			foreach (var item in IPM_UID)
			{
				var IP_Name = db.AspNetUsers.Where(x => x.UserName == item).Select(x => x.MyName).FirstOrDefault();
				IP_NameList.Add(IP_Name);
			}
			var IP_MaintainName = db.AspNetUsers.Where(x => x.UserName == IP_SourceTable.MaintainUserID).Select(x => x.MyName).FirstOrDefault();
			var IP_RepairName = db.AspNetUsers.Where(x => x.UserName == IP_SourceTable.RepairUserID).Select(x => x.MyName).FirstOrDefault();
			#endregion

			#region 定期保養設備
			JArray ME = new JArray();

			var IPM_SourceTable = db.InspectionPlanMaintain.Where(x => x.IPSN == IPSN);

			foreach (var IPM_item in IPM_SourceTable)
			{
				var EMFI_SourceTable = db.EquipmentMaintainFormItem.Find(IPM_item.EMFISN);
				var EMI_SourceTable = db.EquipmentMaintainItem.Find(EMFI_SourceTable.EMISN);
				var EI_SourceTable = db.EquipmentInfo.Find(EMI_SourceTable.ESN);
				var MI_SourceTable = db.MaintainItem.Find(EMI_SourceTable.MISN);
				JObject ME_Row = new JObject()
				{
					{ "StockState", EMFI_SourceTable.StockState? "有":"無" },
					{ "FormItemState", dic_EMFIState[EMFI_SourceTable.FormItemState] },
					{ "EMFISN", IPM_item.EMFISN },
					{ "Period", EMFI_SourceTable.Period },
					{ "Unit", EMFI_SourceTable.Unit },
					{ "LastTime", EMFI_SourceTable.LastTime.ToString("yyyy/MM/dd") },
					{ "Date", EMFI_SourceTable.Date.ToString("yyyy/MM/dd") },
					{ "EState", dic_EState[EI_SourceTable.EState] },
					{ "Area", EI_SourceTable.Area },
					{ "Floor", EI_SourceTable.Floor },
					{ "ESN", EMI_SourceTable.ESN },
					{ "EName", EI_SourceTable.EName },
					{ "MIName", MI_SourceTable.MIName }
				};
				AllMaintainItem.Add(EMI_SourceTable.ESN);
				ME.Add(ME_Row);
			}
			#endregion

			#region 維修設備
			JArray RE = new JArray();

			var IPR_SourceTable = db.InspectionPlanRepair.Where(x => x.IPSN == IPSN);

			foreach (var IPR_item in IPR_SourceTable)
			{
				var ERF_SourceTable = db.EquipmentReportForm.Find(IPR_item.RSN);
				var EI_SourceTable = db.EquipmentInfo.Find(ERF_SourceTable.ESN);
				var ERF_Name = db.AspNetUsers.Where(x => x.UserName == ERF_SourceTable.InformatUserID).Select(x => x.MyName).FirstOrDefault();
				JObject RE_Row = new JObject()
				{
					{ "StockState", ERF_SourceTable.StockState?"有":"無" },
					{ "ReportState", dic_ERFState[ERF_SourceTable.ReportState] },
					{ "ESN", ERF_SourceTable.ESN },
					{ "RSN", IPR_item.RSN },
					{ "ReportLevel", dic_RL[ERF_SourceTable.ReportLevel] },
					{ "Date", ERF_SourceTable.Date.ToString("yyyy/MM/dd HH:mm:ss") },
					{ "ReportContent", ERF_SourceTable.ReportContent },
					{ "MyName", ERF_Name },
					{ "EState", dic_EState[EI_SourceTable.EState] },
					{ "Area", EI_SourceTable.Area },
					{ "Floor", EI_SourceTable.Floor },
					{ "EName", EI_SourceTable.EName }
				};
				AllRepairItem.Add(ERF_SourceTable.ESN);
				RE.Add(RE_Row);
			}
			#endregion

			#region 巡檢路線規劃
			JArray IPP = new JArray();

			var IPP_SourceTable = db.InspectionPlanPath.Where(x => x.IPSN == IPSN);

			foreach (var IPP_item in IPP_SourceTable)
			{
				JArray PathSampleOrder_ja = new JArray(); //路徑
				JArray PathSampleRecord_ja = new JArray(); //座標
				JArray MaintainEquipment_ja = new JArray(); //沒用到
				JArray RepairEquipment_ja = new JArray(); //沒用到
				JArray BothEquipment_ja = new JArray(); //沒用到

				#region 路徑標題
				var FI_SourceTable = db.Floor_Info.Find(IPP_item.FSN);
				var AI_SourceTable = db.AreaInfo.Find(FI_SourceTable.ASN);
				var EI_FloorEquip = db.EquipmentInfo.Where(x => x.FSN == IPP_item.FSN).Where(x => x.System == "BT");

				#region 樓層設備 
				JArray Beacon_ja = new JArray();

				foreach (var EI_item in EI_FloorEquip)
				{
					JObject Beacon_jo = new JObject()
					{
						{"dbId", EI_item.DBID },
						{"deviceType", EI_item.EName },
						{"deviceName", EI_item.ESN }
					};
					Beacon_ja.Add(Beacon_jo);
				}
				#endregion

				JObject PathSample_jo = new JObject()
				{
					{"PSSN", IPP_item.PSN },
					{"Area", AI_SourceTable.Area },
					{"Floor", FI_SourceTable.FloorName },
					{"ASN", FI_SourceTable.ASN },
					{"FSN", IPP_item.FSN },
					{"PathTitle", IPP_item.PathTitle },
					{"BIMPath", FI_SourceTable.BIMPath },
					{"Beacon",Beacon_ja }
				}; //路徑資訊
				#endregion

				#region 路線順序
				var IPFP_SourceTable = db.InspectionPlanFloorPath.Where(x => x.PSN == IPP_item.PSN).OrderBy(x => x.FPSN);

				foreach (var IPFP_item in IPFP_SourceTable)
				{
					PathSampleOrder_ja.Add(IPFP_item.DeviceID.ToString());
				}
				#endregion

				#region 路線呈現
				var DIPP_SourceTable = db.DrawInspectionPlanPath.Where(x => x.PSN == IPP_item.PSN);

				foreach (var DIPP_item in DIPP_SourceTable)
				{
					JObject XY_Path = new JObject()
					{
						{ "LocationX", DIPP_item.LocationX },
						{ "LocationY", DIPP_item.LocationY }
					};
					PathSampleRecord_ja.Add(XY_Path);
				}
				#endregion

				#region 保養設備
				foreach (string MainItem in AllMaintainItem)
				{
					MaintainEquipment_ja.Add(MainItem);
				}
				#endregion

				#region 維修設備
				foreach (string RepairItem in AllRepairItem)
				{
					RepairEquipment_ja.Add(RepairItem);
				}
				#endregion

				#region 保養+維修
				var BothItem = AllMaintainItem.Intersect(AllRepairItem);
				foreach (string Both_item in BothItem)
				{
					BothEquipment_ja.Add(Both_item);
				}
				#endregion

				JObject IPP_Row = new JObject()
				{
					{ "PathSample", PathSample_jo },
					{ "PathSampleOrder", PathSampleOrder_ja },
					{ "PathSampleRecord", PathSampleRecord_ja },
					{ "MaintainEquipment", MaintainEquipment_ja },
					{ "RepairEquipment", RepairEquipment_ja },
					{ "BothEquipment", BothEquipment_ja }
				};
				IPP.Add(IPP_Row);
			}
			#endregion

			JObject Main_jo = new JObject
			{
				{ "IPSN", IPSN },
				{ "IPName", IP_SourceTable.IPName },
				{ "PlanCreateUserID", "" },
				{ "PlanDate", IP_SourceTable.PlanDate.ToString("yyyy/MM/dd") },
				{ "PlanState", dic_InPlanState[IP_SourceTable.PlanState] },
				{ "Shift", dic_Shift[IP_SourceTable.Shift] },
				{ "UserID", string.Join("、",IP_NameList) },
				{ "MaintainUserID", IP_MaintainName },
				{ "RepairUserID", IP_RepairName },
				{ "MaintainEquipment", ME },
				{ "RepairEquipment", RE },
				{ "InspectionPlanPaths", IPP }
			};

			string reString = JsonConvert.SerializeObject(Main_jo);
			return reString;
		}
		#endregion

		#region 巡檢計畫-編輯 DataGrid
		public string InspectationPlan_Edit_Data(string IPSN)
		{
			#region 變數宣告
			var dic_InPlanState = Surfaces.Surface.InspectionPlanState();
			var dic_Shift = Surfaces.Surface.Shift();
			var dic_EMFIState = Surfaces.Surface.EquipmentMaintainFormItemState();
			var dic_EState = Surfaces.Surface.EState();
			var dic_ERFState = Surfaces.Surface.EquipmentReportFormState();
			var dic_RL = Surfaces.Surface.ReportLevel();
			List<string> AllMaintainItem = new List<string>();
			List<string> AllRepairItem = new List<string>();
			#endregion

			#region 巡檢計畫
			var IP_SourceTable = db.InspectionPlan.Find(IPSN);
			//string IP_MaintainID = db.AspNetUsers.Where(x => x.UserName == IP_SourceTable.MaintainUserID).Select(x => x.MyName).FirstOrDefault();
			//string IP_RepairID = db.AspNetUsers.Where(x => x.UserName == IP_SourceTable.RepairUserID).Select(x => x.MyName).FirstOrDefault();
			//string IP_PlanCreateID = db.AspNetUsers.Where(x => x.UserName == IP_SourceTable.PlanCreateUserID).Select(x => x.MyName).FirstOrDefault();
			var IPM_SourceTable = db.InspectionPlanMember.Where(x => x.IPSN == IPSN);
			JArray InsName_ja = new JArray();

			foreach (var IPM_item in IPM_SourceTable)
			{
				InsName_ja.Add(IPM_item.UserID);
			}
			#endregion

			#region 定期保養項目
			var IPMaintain_SourceTable = db.InspectionPlanMaintain.Where(x => x.IPSN == IPSN);
			JArray ME_ja = new JArray();

			foreach (var IPMaintain_item in IPMaintain_SourceTable)
			{
				var EMFI_SourceTable = db.EquipmentMaintainFormItem.Find(IPMaintain_item.EMFISN);
				var EMI_SourceTable = db.EquipmentMaintainItem.Find(EMFI_SourceTable.EMISN);
				var EI_SourceTable = db.EquipmentInfo.Find(EMI_SourceTable.ESN);
				var MI_SourceTable = db.MaintainItem.Find(EMI_SourceTable.MISN);
				JObject ME_jo = new JObject()
				{
					{"StockState", EMFI_SourceTable.StockState? "有":"無"},
					{"FormItemState", dic_EMFIState[EMFI_SourceTable.FormItemState] },
					{"EMFISN", IPMaintain_item.EMFISN},
					{"Period", EMFI_SourceTable.Period},
					{"Unit", EMFI_SourceTable.Unit},
					{"LastTime", EMFI_SourceTable.LastTime.ToString("yyyy/MM/dd")},
					{"Date", EMFI_SourceTable.Date.ToString("yyyy/MM/dd")},
					{"EState", dic_EState[EI_SourceTable.EState]},
					{"Area", EI_SourceTable.Area},
					{"Floor", EI_SourceTable.Floor},
					{"ESN", EMI_SourceTable.ESN},
					{"EName", EI_SourceTable.EName},
					{"MIName", MI_SourceTable.MIName}
				};
				AllMaintainItem.Add(EMI_SourceTable.ESN);
				ME_ja.Add(ME_jo);
			}

			#endregion

			#region 維修設備
			var IPR_SourceTable = db.InspectionPlanRepair.Where(x => x.IPSN == IPSN);
			JArray RE_ja = new JArray();
			foreach (var IPR_item in IPR_SourceTable)
			{
				var ERF_SourceTable = db.EquipmentReportForm.Find(IPR_item.RSN);
				var EI_SourceTable = db.EquipmentInfo.Find(ERF_SourceTable.ESN);
				var ERF_Name = db.AspNetUsers.Where(x => x.UserName == ERF_SourceTable.InformatUserID).Select(x => x.MyName).FirstOrDefault();
				JObject RE_Row = new JObject()
				{
					{ "StockState", ERF_SourceTable.StockState?"有":"無" },
					{ "ReportState", dic_ERFState[ERF_SourceTable.ReportState] },
					{ "ESN", ERF_SourceTable.ESN },
					{ "RSN", IPR_item.RSN },
					{ "ReportLevel", dic_RL[ERF_SourceTable.ReportLevel] },
					{ "Date", ERF_SourceTable.Date.ToString("yyyy/MM/dd HH:mm:ss") },
					{ "ReportContent", ERF_SourceTable.ReportContent },
					{ "MyName", ERF_Name },
					{ "EState", dic_EState[EI_SourceTable.EState] },
					{ "Area", EI_SourceTable.Area },
					{ "Floor", EI_SourceTable.Floor },
					{ "EName", EI_SourceTable.EName }
				};
				AllRepairItem.Add(ERF_SourceTable.ESN);
				RE_ja.Add(RE_Row);
			}
			#endregion

			#region 巡檢路線規劃
			JArray IP_ja = new JArray();

			var IPP_SourceTable = db.InspectionPlanPath.Where(x => x.IPSN == IPSN);

			foreach (var IPP_item in IPP_SourceTable)
			{
				JArray PathSampleOrder_ja = new JArray(); //路徑
				JArray PathSampleRecord_ja = new JArray(); //座標
				JArray MaintainEquipment_ja = new JArray(); //沒用到
				JArray RepairEquipment_ja = new JArray(); //沒用到
				JArray BothEquipment_ja = new JArray(); //沒用到

				#region 路徑標題
				var FI_SourceTable = db.Floor_Info.Find(IPP_item.FSN);
				var AI_SourceTable = db.AreaInfo.Find(FI_SourceTable.ASN);
				var EI_FloorEquip = db.EquipmentInfo.Where(x => x.FSN == IPP_item.FSN).Where(x => x.System == "BT");

				#region 樓層設備 
				JArray Beacon_ja = new JArray();

				foreach (var EI_item in EI_FloorEquip)
				{
					JObject Beacon_jo = new JObject()
					{
						{"dbId", EI_item.DBID },
						{"deviceType", EI_item.EName },
						{"deviceName", EI_item.ESN }
					};
					Beacon_ja.Add(Beacon_jo);
				}
				#endregion


				JObject PathSample_jo = new JObject()
				{
					{"PSSN", IPP_item.PSN },
					{"Area", AI_SourceTable.Area },
					{"Floor", FI_SourceTable.FloorName },
					{"ASN", FI_SourceTable.ASN },
					{"FSN", IPP_item.FSN },
					{"PathTitle", IPP_item.PathTitle },
					{"BIMPath", FI_SourceTable.BIMPath },
					{"BeaconPath", FI_SourceTable.BeaconPath },
					{"Beacon",Beacon_ja }
				}; //路徑資訊
				#endregion

				#region 路線順序
				var IPFP_SourceTable = db.InspectionPlanFloorPath.Where(x => x.PSN == IPP_item.PSN).OrderBy(x => x.FPSN);

				foreach (var IPFP_item in IPFP_SourceTable)
				{
					PathSampleOrder_ja.Add(IPFP_item.DeviceID.ToString());
				}
				#endregion

				#region 路線呈現
				var DIPP_SourceTable = db.DrawInspectionPlanPath.Where(x => x.PSN == IPP_item.PSN);

				foreach (var DIPP_item in DIPP_SourceTable)
				{
					JObject XY_Path = new JObject()
					{
						{ "LocationX", DIPP_item.LocationX },
						{ "LocationY", DIPP_item.LocationY }
					};
					PathSampleRecord_ja.Add(XY_Path);
				}
				#endregion

				#region 保養設備
				foreach (string MainItem in AllMaintainItem)
				{
					MaintainEquipment_ja.Add(MainItem);
				}
				#endregion

				#region 維修設備
				foreach (string RepairItem in AllRepairItem)
				{
					RepairEquipment_ja.Add(RepairItem);
				}
				#endregion

				#region 保養+維修
				var BothItem = AllMaintainItem.Intersect(AllRepairItem);
				foreach (string Both_item in BothItem)
				{
					BothEquipment_ja.Add(Both_item);
				}
				#endregion

				JObject IPP_Row = new JObject()
				{
					{ "PathSample", PathSample_jo },
					{ "PathSampleOrder", PathSampleOrder_ja },
					{ "PathSampleRecord", PathSampleRecord_ja },
					{ "MaintainEquipment", MaintainEquipment_ja },
					{ "RepairEquipment", RepairEquipment_ja },
					{ "BothEquipment", BothEquipment_ja }
				};
				IP_ja.Add(IPP_Row);
			}
			#endregion

			JObject Main_jo = new JObject
			{
				{"IPSN", IPSN},
				{"IPName", IP_SourceTable.IPName },
				{"PlanCreateUserID", IP_SourceTable.PlanCreateUserID },
				{"PlanDate", IP_SourceTable.PlanDate.ToString("yyyy-MM-dd") },
				{"PlanState", dic_InPlanState[IP_SourceTable.PlanState] },
				{"Shift", IP_SourceTable.Shift },
				{"UserID", InsName_ja },
				{"MaintainUserID", IP_SourceTable.MaintainUserID },
				{"RepairUserID", IP_SourceTable.RepairUserID },
				{"MaintainAmount", IP_SourceTable.MaintainAmount },
				{"RepairAmount", IP_SourceTable.RepairAmount },
				{"MaintainEquipment", ME_ja },
				{"RepairEquipment", RE_ja },
				{"InspectionPlanPaths", IP_ja }
			};

			string reString = JsonConvert.SerializeObject(Main_jo);
			return reString;
		}
		#endregion

		#region 巡檢計畫-編輯 update
		public string InspectationPlan_Edit_Update(System.Web.Mvc.FormCollection form, ref int resultCode)
		{
			/*  前端回傳格式
            IPName: IPName,
            PlanCreateUserID: PlanCreateUserID,
            PlanDate: PlanDate,
            Shift: Shift,
            UserID: UserID,
            MaintainUserID: MaintainUserID,
            RepairUserID: RepairUserID,
            MaintainEquipment: MaintainEquipment,
            RepairEquipment: RepairEquipment,
            InspectionPlanPaths: InspectionPlanPaths
            */

			JsonResponseViewModel Jresult = new JsonResponseViewModel();

			try
			{
				#region 變數宣告
				string ipsn = form["IPSN"].ToString();
				JArray NameArray = (JArray)form["UserID"];
				JArray EMFISN_ja = (JArray)form["MaintainEquipment"];
				JArray RSN_ja = (JArray)form["RepairEquipment"];
				JArray InsPlanPaths = (JArray)form["InspectionPlanPaths"];
				#endregion

				#region 主表更新 InspectionPlan
				var IP_SourceTable = db.InspectionPlan.Find(ipsn);
				if (IP_SourceTable == null)
				{
					resultCode = 400;
					Jresult.ResponseCode = 400;
					Jresult.ResponseMessage = "更新失敗!";
					return JsonConvert.SerializeObject(Jresult);
				}
				IP_SourceTable.IPName = form["IPName"].ToString();
				IP_SourceTable.Shift = form["Shift"].ToString();
				IP_SourceTable.MaintainUserID = form["MaintainUserID"].ToString();
				IP_SourceTable.RepairUserID = form["RepairUserID"].ToString();
				IP_SourceTable.MaintainAmount = EMFISN_ja.Count();
				IP_SourceTable.RepairAmount = RSN_ja.Count();
				IP_SourceTable.PlanState = "1";
				db.InspectionPlan.AddOrUpdate(IP_SourceTable);
				db.SaveChanges();
				#endregion

				#region 巡檢計畫人員名單 Inspection Plan Member
				var DelInsPlanMember = db.InspectionPlanMember.Where(x => x.IPSN == ipsn);
				if (DelInsPlanMember != null)
				{
					db.InspectionPlanMember.RemoveRange(DelInsPlanMember);//先刪除全部成員
					db.SaveChanges();
				}

				for (int i = 0; i < NameArray.Count(); i++)
				{
					Models.InspectionPlanMember IPM = new Models.InspectionPlanMember()
					{
						PMSN = ipsn + "_" + (i + 1).ToString(),
						IPSN = ipsn,
						UserID = NameArray[i].ToString(),
						WatchID = ""
					};
					db.InspectionPlanMember.Add(IPM);
					db.SaveChanges();
				}
				#endregion

				#region 定期保養項目
				var IPM_Del = db.InspectionPlanMaintain.Where(x => x.IPSN == ipsn);
				if (IPM_Del != null)
				{
					db.InspectionPlanMaintain.RemoveRange(IPM_Del);
					db.SaveChanges();
				}

				for (int i = 0; i < EMFISN_ja.Count; i++)
				{
					string EMFISN = EMFISN_ja[i].ToString();

					#region 組IPMSN
					int IPMSN_Count = i;
					string New_IPMSN = "";
					IPMSN_Count++;
					if (IPMSN_Count < 10)
					{
						New_IPMSN = ipsn + "_M0" + IPMSN_Count.ToString();
					}
					else
					{
						New_IPMSN = ipsn + "_M" + IPMSN_Count.ToString();
					}
					#endregion

					InspectionPlanMaintain IPM = new InspectionPlanMaintain()
					{
						IPMSN = New_IPMSN,
						IPSN = ipsn,
						EMFISN = EMFISN,
						MaintainState = "1"
					};
					db.InspectionPlanMaintain.Add(IPM);

					var EMFI_SourceTable = db.EquipmentMaintainFormItem.Find(EMFISN);
					EMFI_SourceTable.FormItemState = "2";
					db.EquipmentMaintainFormItem.AddOrUpdate(EMFI_SourceTable);
					db.SaveChanges();
				}
				#endregion

				#region 維修設備
				var InsPlanRepair_Del = db.InspectionPlanRepair.Where(x => x.IPSN == ipsn);
				if (InsPlanRepair_Del != null)
				{
					db.InspectionPlanRepair.RemoveRange(InsPlanRepair_Del);
					db.SaveChanges();
				}

				for (int i = 0; i < RSN_ja.Count; i++)
				{
					string RSN = RSN_ja[i].ToString();

					#region 組IPRSN
					int IPRSN_Count = i;
					string New_IPRSN = "";
					IPRSN_Count++;
					if (IPRSN_Count < 10)
					{
						New_IPRSN = ipsn + "_R0" + IPRSN_Count.ToString();
					}
					else
					{
						New_IPRSN = ipsn + "_R" + IPRSN_Count.ToString();
					}
					#endregion

					InspectionPlanRepair IPR = new InspectionPlanRepair()
					{
						IPRSN = New_IPRSN,
						IPSN = ipsn,
						RSN = RSN,
						RepairState = "1"
					};
					db.InspectionPlanRepair.Add(IPR);

					var ERF_SourceTable = db.EquipmentReportForm.Find(RSN);
					ERF_SourceTable.ReportState = "2";
					db.EquipmentReportForm.AddOrUpdate(ERF_SourceTable);

					db.SaveChanges();
				}
				#endregion

				#region 巡檢路線規劃
				var InsPlanPath_Del = db.InspectionPlanPath.Where(x => x.IPSN == ipsn);
				var InsPlanFloorPath_Del = db.InspectionPlanFloorPath.Where(x => x.PSN.Contains(ipsn));

				foreach (var item in InsPlanPath_Del)
				{
					var DrawPath_Del = db.DrawInspectionPlanPath.Where(x => x.PSN == item.PSN);
					if (DrawPath_Del != null)
					{
						db.DrawInspectionPlanPath.RemoveRange(DrawPath_Del);
						db.SaveChanges();
					}
				}

				if (InsPlanPath_Del != null)
				{
					db.InspectionPlanPath.RemoveRange(InsPlanPath_Del);
					db.SaveChanges();
				}
				if (InsPlanFloorPath_Del != null)
				{
					db.InspectionPlanFloorPath.RemoveRange(InsPlanFloorPath_Del);
					db.SaveChanges();
				}

				int PSN_Count = 1;
				foreach (var item in InsPlanPaths)
				{
					#region 組PSN
					string New_PSN = "";
					if (PSN_Count < 10)
					{
						New_PSN = ipsn + "_0" + PSN_Count.ToString();
					}
					else
					{
						New_PSN = ipsn + "_" + PSN_Count.ToString();
					}
					PSN_Count++;
					#endregion

					#region 巡檢計畫路徑 InspectionPlanPath
					JObject PathSample = (JObject)item["PathSample"];
					InspectionPlanPath IPP = new InspectionPlanPath()
					{
						PSN = New_PSN,
						IPSN = ipsn,
						PathTitle = PathSample["PathTitle"].ToString(),
						FSN = PathSample["FSN"].ToString()
					};
					db.InspectionPlanPath.Add(IPP);
					db.SaveChanges();
					#endregion

					#region  巡檢計畫單樓層路徑 InspectionPlanFloorPath
					JArray Device_ja = (JArray)item["PathSampleOrder"];
					int FPSN_Count = 1;
					foreach (string ESN in Device_ja)
					{
						#region 組FPSN
						string New_FPSN = "";
						if (FPSN_Count < 10)
						{
							New_FPSN = New_PSN + "_0" + FPSN_Count.ToString();
						}
						else
						{
							New_FPSN = New_PSN + "_" + FPSN_Count.ToString();
						}
						FPSN_Count++;
						#endregion

						InspectionPlanFloorPath IPFP = new InspectionPlanFloorPath()
						{
							FPSN = New_FPSN,
							PSN = New_PSN,
							DeviceID = ESN //待調整
						};
						db.InspectionPlanFloorPath.Add(IPFP);
						db.SaveChanges();
					}
					#endregion

					#region 巡檢路徑 DrawInspectionPlanPath
					JArray XYPath_ja = (JArray)item["PathSampleRecord"];
					int DrawPath_Count = 0;
					foreach (JObject XYitem in XYPath_ja)
					{
						#region 組ISN
						DrawPath_Count++;
						string New_ISN = "";
						if (DrawPath_Count < 10)
						{
							New_ISN = New_PSN + "_0" + DrawPath_Count.ToString();
						}
						else
						{
							New_ISN = New_PSN + "_" + DrawPath_Count.ToString();
						}
						#endregion

						DrawInspectionPlanPath DIPP = new DrawInspectionPlanPath()
						{
							ISN = New_ISN,
							PSN = New_PSN,
							LocationX = decimal.Parse(XYitem["LocationX"].ToString()),
							LocationY = decimal.Parse(XYitem["LocationY"].ToString())
						};
						db.DrawInspectionPlanPath.Add(DIPP);
						db.SaveChanges();
					}
					#endregion
				}
				#endregion

				resultCode = 200;
				Jresult.ResponseCode = 200;
				Jresult.ResponseMessage = "更新成功!";
				return JsonConvert.SerializeObject(Jresult);
			}
			catch (Exception ex)
			{
				resultCode = 500;
				Jresult.ResponseCode = 500;
				Jresult.ResponseMessage = ex.Message;
				return JsonConvert.SerializeObject(Jresult);
			}

		}
		#endregion

		#region 巡檢計畫-紀錄 格式
		public class Root
		{
			public InspectionPlan InspectionPlan { get; set; }
			public InspectionPlanRecord InspectionPlanRecord { get; set; }
			public List<InspectionPlanMember> InspectionPlanMember { get; set; }
			public List<EquipmentMaintainRecord> EquipmentMaintainRecordList { get; set; }
			public List<EquipmentRepairRecord> EquipmentRepairRecordList { get; set; }
		}

		public class EquipmentMaintainRecord
		{
			public string IPMSN { get; set; }
			public string IPSN { get; set; }
			public string MaintainState { get; set; }
			public string Area { get; set; }
			public string Floor { get; set; }
			public string ESN { get; set; }
			public string EState { get; set; }
			public string EName { get; set; }
			public string EMFISN { get; set; }
			public string MIName { get; set; }
			public string Period { get; set; }
			public string Unit { get; set; }
			public string LastTime { get; set; }
			public string NextTime { get; set; }
		}

		public class EquipmentRepairRecord
		{
			public string IPRSN { get; set; }
			public string IPSN { get; set; }
			public string RepairState { get; set; }
			public string Area { get; set; }
			public string Floor { get; set; }
			public string ESN { get; set; }
			public string EName { get; set; }
			public string RSN { get; set; }
			public string ReportLevel { get; set; }
			public string Date { get; set; }
			public string InformantUserID { get; set; }
			public string ReportContent { get; set; }
		}

		public class InspectionPlan
		{
			public string IPSN { get; set; }
			public string IPName { get; set; }
			public string PlanDate { get; set; }
			public string PlanState { get; set; }
			public string Shift { get; set; }
			public string MyName { get; set; }
		}

		public class InspectionPlanMember
		{
			public string MyName { get; set; }
			public string WatchID { get; set; }
			public string PMSN { get; set; }
		}

		public class InspectionPlanRecord
		{
			public string PlanState { get; set; }
			public string MyName { get; set; }
			public string DateOfFilling { get; set; }
			public string InspectionRecord { get; set; }
			public List<string> ImgPath { get; set; }
		}
		#endregion

		#region 巡檢計畫-紀錄 DataGrid
		public string GetJsonForRecord(string IPSN)
		{
			try
			{
				#region 巡檢計畫資訊 
				InspectionPlan inspectionPlan = new InspectionPlan();
				var InsPlan = db.InspectionPlan.Find(IPSN);
				var InsPlanMember = db.InspectionPlanMember.Where(x => x.IPSN == IPSN).Select(x => x.UserID);
				List<string> NameList = new List<string>();
				string MyNameString = string.Empty;
				foreach (var item in InsPlanMember)
				{
					var MyName = db.AspNetUsers.Where(x => x.UserName == item).Select(x => x.MyName).FirstOrDefault();
					NameList.Add(MyName);
				}
				MyNameString = string.Join("、", NameList);

				inspectionPlan.IPSN = IPSN;
				inspectionPlan.IPName = InsPlan.IPName;
				inspectionPlan.PlanDate = InsPlan.PlanDate.ToString("yyyy/MM/dd");
				var dic_IPS = Surfaces.Surface.InspectionPlanState();
				inspectionPlan.PlanState = dic_IPS[InsPlan.PlanState];
				var dic_Shift = Surfaces.Surface.Shift();
				inspectionPlan.Shift = dic_Shift[InsPlan.Shift];
				inspectionPlan.MyName = MyNameString;
				#endregion

				#region 巡檢完工填報
				InspectionPlanRecord inspectionPlanRecord = new InspectionPlanRecord();
				inspectionPlanRecord.PlanState = dic_IPS[InsPlan.PlanState];
				string InfoName = db.AspNetUsers.Where(x => x.UserName == InsPlan.InformatUserID).Select(x => x.MyName).FirstOrDefault();
				inspectionPlanRecord.MyName = InfoName;
				inspectionPlanRecord.DateOfFilling = InsPlan.DateOfFilling?.ToString("yyyy/MM/dd HH:mm:ss");
				inspectionPlanRecord.InspectionRecord = InsPlan.InspectionRecord;
				List<string> ImgList = new List<string>();
				var pathList = db.CompletionReportImage.Where(x => x.IPSN == IPSN).Select(x => x.ImgPath);
				foreach (var item in pathList)
				{
					ImgList.Add(item);
				}
				inspectionPlanRecord.ImgPath = ImgList;
				#endregion

				#region 巡檢軌跡紀錄
				List<InspectionPlanMember> ListIP = new List<InspectionPlanMember>();
				var IPM = db.InspectionPlanMember.Where(x => x.IPSN == IPSN);
				foreach (var item in IPM)
				{
					InspectionPlanMember inspectionPlanMember = new InspectionPlanMember();
					var ipmName = db.AspNetUsers.Where(x => x.UserName == item.UserID).Select(x => x.MyName).FirstOrDefault();
					inspectionPlanMember.MyName = ipmName;
					inspectionPlanMember.WatchID = item.WatchID;
					inspectionPlanMember.PMSN = item.PMSN;
					ListIP.Add(inspectionPlanMember);
				}
				#endregion

				#region 設備保養紀錄
				List<EquipmentMaintainRecord> ListEMR = new List<EquipmentMaintainRecord>();
				var EMR = db.InspectionPlanMaintain.Where(x => x.IPSN == IPSN);
				foreach (var item in EMR)
				{
					EquipmentMaintainRecord equipmentMaintainRecord = new EquipmentMaintainRecord();
					equipmentMaintainRecord.IPMSN = item.IPMSN;
					equipmentMaintainRecord.IPSN = IPSN;
					var dic_IPMS = Surfaces.Surface.InspectionPlanMaintainState();
					equipmentMaintainRecord.MaintainState = dic_IPMS[item.MaintainState];
					var EMFI = db.EquipmentMaintainFormItem.Find(item.EMFISN);
					var EMI = db.EquipmentMaintainItem.Find(EMFI.EMISN);
					var EI = db.EquipmentInfo.Find(EMI.ESN);
					var MI = db.MaintainItem.Find(EMI.MISN);
					equipmentMaintainRecord.Area = EI.Area;
					equipmentMaintainRecord.Floor = EI.Floor;
					equipmentMaintainRecord.ESN = EI.ESN;
					var dic_EState = Surfaces.Surface.EState();
					equipmentMaintainRecord.EState = dic_EState[EI.EState];
					equipmentMaintainRecord.EName = EI.EName;
					equipmentMaintainRecord.EMFISN = item.EMFISN;
					equipmentMaintainRecord.MIName = MI.MIName;
					equipmentMaintainRecord.Unit = EMFI.Unit;
					equipmentMaintainRecord.Period = EMFI.Period.ToString();
					equipmentMaintainRecord.LastTime = EMFI.LastTime.ToString("yyyy/MM/dd");
					equipmentMaintainRecord.NextTime = EMFI.NextTime?.ToString("yyyy/MM/dd");
					ListEMR.Add(equipmentMaintainRecord);
				}
				#endregion

				#region 設備維修紀錄
				List<EquipmentRepairRecord> ListERR = new List<EquipmentRepairRecord>();
				var IPR = db.InspectionPlanRepair.Where(x => x.IPSN == IPSN);
				foreach (var item in IPR)
				{
					EquipmentRepairRecord equipmentRepairRecord = new EquipmentRepairRecord();
					equipmentRepairRecord.IPRSN = item.IPRSN;
					equipmentRepairRecord.IPSN = IPSN;
					var dic_IPRS = Surfaces.Surface.InspectionPlanRepairState();
					equipmentRepairRecord.RepairState = dic_IPRS[item.RepairState];
					var ERF = db.EquipmentReportForm.Find(item.RSN);
					var EI = db.EquipmentInfo.Find(ERF.ESN);
					var dic_RL = Surfaces.Surface.ReportLevel();
					equipmentRepairRecord.ReportLevel = dic_RL[ERF.ReportLevel];
					equipmentRepairRecord.Area = EI.Area;
					equipmentRepairRecord.Floor = EI.Floor;
					equipmentRepairRecord.RSN = item.RSN;
					equipmentRepairRecord.Date = ERF.Date.ToString("yyyy/MM/dd HH:mm:ss");
					equipmentRepairRecord.ESN = ERF.ESN;
					equipmentRepairRecord.EName = EI.EName;
					var Name = db.AspNetUsers.Where(x => x.UserName == ERF.InformatUserID).Select(x => x.MyName).FirstOrDefault();
					equipmentRepairRecord.InformantUserID = Name;
					equipmentRepairRecord.ReportContent = ERF.ReportContent;
					ListERR.Add(equipmentRepairRecord);
				}
				#endregion

				Root root = new Root();
				root.InspectionPlan = inspectionPlan;
				root.InspectionPlanRecord = inspectionPlanRecord;
				root.InspectionPlanMember = ListIP;
				root.EquipmentMaintainRecordList = ListEMR;
				root.EquipmentRepairRecordList = ListERR;

				string result = JsonConvert.SerializeObject(root);
				return result;
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
		}
		#endregion
	}
}