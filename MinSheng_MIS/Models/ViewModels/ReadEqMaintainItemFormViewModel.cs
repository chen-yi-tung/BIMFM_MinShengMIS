using MinSheng_MIS.Surfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Models.ViewModels
{
    public class ReadEqMaintainItemFormViewModel
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();
        private class EqMaintainItemFormInfo
        {
            public string FormItemState { get; set; }//本保養單項目狀態
            public string EMFISN { get; set; }//保養單項目編號
            public string MIName { get; set; }//保養項目
            public string Unit { get; set; }//保養週期單位
            public string Period { get; set; }//週期單位
            public string LastTime { get; set; }//上次保養
            public string Date { get; set; }//最近應保養
            public string ESN { get; set; }//設備屬性
            public List<InspectionPlanList> InspectionPlanList { get; set; }//本保養單相關保養紀錄
        }
        private class InspectionPlanList
        {
            public InspectionPlan InspectionPlan { get; set; }
            public InspectionPlanMaintain InspectionPlanMaintain { get; set; }
            public List<MaintainSupplementaryInfo> MaintainSupplementaryInfo { get; set; }
            public List<MaintainAuditInfo> MaintainAuditInfo { get; set; }
        }
        private class InspectionPlan
        {
            public string IPSN { get; set; }//計畫編號
            public string IPName { get; set; }//計畫名稱
            public string PlanDate { get; set; }//計畫日期
            public string PlanState { get; set; }//計畫執行狀態
            public string Shift { get; set; }//巡檢班別
            public string MyName { get; set; }//巡檢人員
        }
        private class InspectionPlanMaintain
        {
            public string MaintainState { get; set; }//本次保養狀態
            public string MyName { get; set; }//填報人員
            public string MaintainContent { get; set; }//保養備註
            public string MaintainDate { get; set; }//填報時間
            public List<string> ImgPath { get; set; }//保養照片
        }
        private class MaintainSupplementaryInfo
        {
            public string MyName { get; set; }//補件人
            public string SupplementaryDate { get; set; }//補件日期
            public string SupplementContent { get; set; }//補件說明
            public List<string> FilePath { get; set; }//補件檔案
        }
        private class MaintainAuditInfo
        {
            public string MyName { get; set; }//審核者
            public string AuditDate { get; set; }//審核日期
            public string AuditResult { get; set; }//審核結果
            public string AuditMemo { get; set; }//審核意見
            public List<string> ImgPath { get; set; }//審核照片
        }
        public string GetJsonForRead(string EMFISN)
        {
            EqMaintainItemFormInfo eqMaintainItemFormInfo = new EqMaintainItemFormInfo();

            #region 保養項目資料
            var EMFITable = db.EquipmentMaintainFormItem.Where(x => x.EMFISN == EMFISN).ToList();

            foreach(var item in EMFITable)
            {
                //本保養單項目狀態
                var dic = Surface.EquipmentMaintainFormItemState();
                eqMaintainItemFormInfo.FormItemState = dic[item.FormItemState];
                //保養單項目編號
                eqMaintainItemFormInfo.EMFISN = EMFISN;
                //保養項目
                var MISN = db.EquipmentMaintainItem.Find(item.EMISN).MISN;
                eqMaintainItemFormInfo.MIName = db.MaintainItem.Find(MISN).MIName;
                //保養週期單位
                eqMaintainItemFormInfo.Unit = item.Unit;
                //1保養週期
                eqMaintainItemFormInfo.Period = item.Period.ToString();
                //上次保養
                eqMaintainItemFormInfo.LastTime = item.LastTime.ToString("yyyy/MM/dd");
                //最近應保養
                eqMaintainItemFormInfo.Date = item.Date.ToString("yyyy/MM/dd");
                //設備屬性
                eqMaintainItemFormInfo.ESN = db.EquipmentMaintainItem.Find(item.EMISN).ESN;
            }
            #endregion

            string result = JsonConvert.SerializeObject(eqMaintainItemFormInfo);
            return result;
        }
    }
}