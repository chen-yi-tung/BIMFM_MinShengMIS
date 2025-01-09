using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Surfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Web;

namespace MinSheng_MIS.Services
{
    public class RFIDService
    {
        private readonly Bimfm_MinSheng_MISEntities _db;
        private readonly IPAddress _clientIP;
        private readonly string _clientIP_IP;
        private readonly int _clientIP_Port;

        public RFIDService(Bimfm_MinSheng_MISEntities db)
        {
            _db = db;
            _clientIP = IPAddress.Parse(HttpContext.Current.Request.UserHostAddress);
            //_clientIP_IP = _clientIP.MapToIPv4().ToString(); //本機測試改成自己的IP
            _clientIP_IP = "192.168.0.147";
            _clientIP_Port = 5000;
        }

        #region 查詢設備的所有RFID
        public IEnumerable<RFID> GetRFIDsByEsn(string esn)
        {
            return _db.RFID.Where(x => x.ESN == esn).AsEnumerable();
        }
        #endregion

        #region 查詢符合Dto的RFID資訊
        public IQueryable<T> GetRFIDQueryByDto<T>(Expression<Func<RFID, bool>> filter = null)
            where T : new()
        {
            // 基本查詢
            var query = _db.RFID.AsQueryable();

            // 如果有過濾條件，應用過濾
            if (filter != null)
                query = query.Where(filter);

            // 映射到目標類型
            return query.Select(Helper.MapDatabaseToQuery<RFID, T>());
        }
        #endregion

        #region 新增設備RFID
        public async Task AddEquipRFIDAsync(EquipRFID data, string esn)
        {
            // 資料驗證
            await RFIDDataAnnotationAsync(data);
            if (string.IsNullOrEmpty(esn))
                throw new ArgumentNullException($"{nameof(esn)}不可為null！");
            //if (!await _db.EquipmentInfo.AnyAsync(x => x.ESN == esn))
            //    throw new ArgumentException($"{nameof(esn)}不存在！");

            // 新增/更新RFID
            var rfid = new RFID
            {
                RFIDInternalCode = data.InternalCode,
                SARSN = null,
                ESN = esn,
                RFIDExternalCode = data.ExternalCode,
                FSN = data.FSN,
                Location_X = data.Location_X,
                Location_Y = data.Location_Y,
                Memo = data.Memo
            };

            _db.RFID.AddOrUpdate(rfid);
        }
        #endregion

        #region 獲取設備RFID
        public async Task<T> GetRfidAsync<T>(string internalCode) where T : class, new()
        {
            var rfid = await _db.RFID.FindAsync(internalCode)
                ?? throw new MyCusResException("查無資料！");

            T dest = rfid.ToDto<RFID, T>();

            if (dest is IRFIDInfoDetail info)
            {
                info.ASN = rfid.Floor_Info.ASN.ToString();
                info.AreaName = rfid.Floor_Info.AreaInfo.Area;
                info.FloorName = rfid.Floor_Info.FloorName;
                info.RFIDViewName = rfid.Floor_Info.ViewName;

                dest = (T)info;
            }

            return dest;
        }
        #endregion

        //-----資料驗證
        #region RFID資料驗證
        public async Task RFIDDataAnnotationAsync(IRFIDInfo data)
        {
            var floorSNList = await _db.Floor_Info.Select(x => x.FSN).ToListAsync(); // 取得所有樓層SN列表

            // 不可重複: RFID內碼
            if (await _db.RFID.AnyAsync(x => x.RFIDInternalCode == data.InternalCode))
                throw new MyCusResException("RFID內碼已存在！");
            // 不可重複: RFID外碼
            //if (await _db.RFID.AnyAsync(x => x.RFIDExternalCode == data.ExternalCode))
            //    throw new MyCusResException("RFID外碼已存在！");
            // 關聯性PK是否存在: 樓層
            if (!floorSNList.Contains(data.FSN))
                throw new MyCusResException("樓層不存在！");

            #region 批次驗證(X)
            //// 不可重複: RFID內碼
            //if (await _db.RFID.Where(x => data.RFIDList.Any(r => r.InternalCode == x.RFIDInternalCode)).AnyAsync())
            //    throw new MyCusResException("RFID內碼已存在！");
            //// 不可重複: RFID外碼
            //if (await _db.RFID.Where(x => data.RFIDList.Any(r => r.ExternalCode == x.RFIDExternalCode)).AnyAsync())
            //    throw new MyCusResException("RFID外碼已存在！");
            //// 關聯性PK是否存在: 樓層
            //if (!floorSNList.All(floor => data.RFIDList.Select(x => x.FSN).AsEnumerable().Contains(floor)))
            //    throw new MyCusResException("樓層不存在！");
            #endregion
        }
        #endregion

        #region 檢查RFID內碼是否重複
        public async Task CheckRFIDInternalCode(string RFIDInternalCode)
        {

            // 不可重複: RFID內碼
            if (await _db.RFID.AnyAsync(x => x.RFIDInternalCode == RFIDInternalCode))
                throw new MyCusResException("RFID內碼已存在！");
        }
        #endregion

        //------掃描
        #region 掃描RFID
        public async Task<JsonResService<string>> CheckRFID()
        {
            #region 變數
            JsonResService<string> res = new JsonResService<string>();
            JsonObject data = new JsonObject();
            #endregion

            try
            {
                if (string.IsNullOrEmpty(_clientIP.ToString()))
                {
                    //return Json(new { RFIDInternalCode = (string)null, ErrorMessage = "1" });
                    throw new MyCusResException("找不到設備IP位址");
                }

                // Combine all steps into a single command to the local server
                string command = "GET";

                // Send the combined command to the local server
                string response = SendCommandToLocalServer(command);

                // Parse the response
                if (string.IsNullOrEmpty(response))
                {
                    //return Json(new { RFIDInternalCode = (string)null, ErrorMessage = "1" });
                    throw new MyCusResException("掃描不到RFID");
                }

                try
                {
                    // Deserialize the JSON response
                    var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);

                    if (jsonResponse != null && jsonResponse.ContainsKey("InternalCode") && jsonResponse.ContainsKey("ErrorMessage"))
                    {
                        res.ErrorMessage = jsonResponse["ErrorMessage"]?.ToString();
                        res.Datas = jsonResponse["InternalCode"]?.ToString();
                        // Check if RFIDInternalCode is empty and ErrorMessage is null
                        if (string.IsNullOrEmpty(res.Datas) && string.IsNullOrEmpty(res.ErrorMessage))
                        {
                            //return Json(new { RFIDInternalCode = (string)null, ErrorMessage = "No EPC found." });
                            throw new MyCusResException("掃描不到RFID");
                        }
                        else if (!string.IsNullOrEmpty(res.Datas) && string.IsNullOrEmpty(res.ErrorMessage))
                        {
                            //檢查RFID是否重複
                            await CheckRFIDInternalCode(res.Datas);
                            return res;
                        }
                        else
                        {
                            throw new MyCusResException(res.ErrorMessage);
                        }
                    }
                    else
                    {
                        //return Json(new { RFIDInternalCode = (string)null, ErrorMessage = "Unexpected response format from the local server." });
                        throw new MyCusResException("掃描格式錯誤");
                    }
                }
                catch (JsonReaderException ex)
                {
                    //return Json(new { RFIDInternalCode = (string)null, ErrorMessage = $"Error parsing server response: {ex.Message}" });
                    throw new MyCusResException($"{ex.Message}");
                }
            }
            catch (Exception ex)
            {
                //return Json(new { RFIDInternalCode = (string)null, ErrorMessage = ex.Message });
                throw new MyCusResException($"{ex.Message}");
            }

            res.AccessState = ResState.Success;
            return res;
        }
        private string SendCommandToLocalServer(string command)
        {
            try
            {
                Debug.WriteLine($"Attempting to connect to {_clientIP_IP}:{_clientIP_Port}");
                using (var client = new TcpClient(_clientIP_IP, _clientIP_Port))
                using (var stream = client.GetStream())
                using (var writer = new StreamWriter(stream) { AutoFlush = true })
                using (var reader = new StreamReader(stream))
                {
                    // Send command to the local server
                    writer.WriteLine(command);

                    // Read and return the response from the local server
                    string response = reader.ReadLine();
                    //Debug.WriteLine($"Received response: {response}");
                    return response;
                }
            }
            catch (Exception ex)
            {
                //Debug.WriteLine($"Error communicating with the local server: {ex.Message}");
                //throw new Exception("Failed to communicate with the local server.", ex);
                throw new MyCusResException($"Error communicating with the local server: {ex.Message}");
            }
        }
        #endregion
    }
}