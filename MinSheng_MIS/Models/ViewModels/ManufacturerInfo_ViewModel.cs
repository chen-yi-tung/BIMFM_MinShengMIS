using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Models.ViewModels
{
    public class ManufacturerInfo_ViewModel
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();

        #region 廠商管理_詳情
        public string Manufac_Read_GetData(string MFRSN, ref int resultCode)
        {
            try
            {
                var MFR = db.ManufacturerInfo.Find(MFRSN);
                resultCode = 200;
                return JsonConvert.SerializeObject(MFR);
            }
            catch (Exception ex)
            {
                JsonResponseViewModel Jresult = new JsonResponseViewModel()
                { 
                    ResponseCode = 500,
                    ResponseMessage= ex.Message
                };
                resultCode = 500;
                return JsonConvert.SerializeObject(Jresult);
            }
        }
        #endregion

        #region 廠商管理_新增
        public string Manufac_Create(ManufacturerInfo MFR, ref int resultCode)
        { 
            JsonResponseViewModel Jresult = new JsonResponseViewModel();

            try
            {
                var Manufac_LastCount = db.ManufacturerInfo.OrderByDescending(x => x.MFRSN).Select(x => x.MFRSN).FirstOrDefault();

                #region 組新MFRSN
                string newMFRSN = "";
                if (string.IsNullOrEmpty(Manufac_LastCount))
                {
                    newMFRSN = "00001";
                }
                else
                {
                    int Count = Int32.Parse(Manufac_LastCount);
                    string nowCount = (Count + 1).ToString();
                    newMFRSN = nowCount.PadLeft(5, '0');
                }
                #endregion

                #region 新增資料
                ManufacturerInfo ManufacInfo = new ManufacturerInfo()
                {
                    MFRSN = newMFRSN,
                    MFRName = MFR.MFRName,
                    ContactPerson = MFR.ContactPerson,
                    MFRTelNO = MFR.MFRTelNO,
                    MFRMBPhone = MFR.MFRMBPhone,
                    MFRAddress = MFR.MFRAddress,
                    MFREmail = MFR.MFREmail,
                    MFRWeb = MFR.MFRWeb,
                    MFRMainProduct = MFR.MFRMainProduct,
                    Memo = MFR.Memo
                };
                db.ManufacturerInfo.Add(ManufacInfo);
                db.SaveChanges();
                #endregion

                resultCode = 200;
                Jresult.ResponseCode = 200;
                Jresult.ResponseMessage = "新增成功!";
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

        #region 廠商管理_編輯
        public string Manufac_Edit_Update(ManufacturerInfo MFR, ref int resultCode)
        {
            JsonResponseViewModel Jresult = new JsonResponseViewModel();

            try
            {
                #region 更新資料
                var Manufacturer = db.ManufacturerInfo.Find(MFR.MFRSN);
                if (Manufacturer == null)
                {
                    resultCode = 400;
                    Jresult.ResponseCode = 400;
                    Jresult.ResponseMessage = "編輯失敗!";
                    return JsonConvert.SerializeObject(Jresult);
                }
                Manufacturer.MFRName = MFR.MFRName;
                Manufacturer.ContactPerson = MFR.ContactPerson;
                Manufacturer.MFRTelNO = MFR.MFRTelNO;
                Manufacturer.MFRMBPhone = MFR.MFRMBPhone;
                Manufacturer.MFRAddress = MFR.MFRAddress;
                Manufacturer.MFREmail = MFR.MFREmail;
                Manufacturer.MFRWeb = MFR.MFRWeb;
                Manufacturer.MFRMainProduct = MFR.MFRMainProduct;
                db.ManufacturerInfo.AddOrUpdate(Manufacturer);
                db.SaveChanges();
                #endregion

                resultCode = 200;
                Jresult.ResponseCode = 200;
                Jresult.ResponseMessage = "編輯成功!";
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
    }
}