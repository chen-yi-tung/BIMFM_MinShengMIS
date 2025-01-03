using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MinSheng_MIS.Models.ViewModels
{
    public class BeaconPosition : IBeaconPosition
    {
        public double? X
        {
            get => (double?)Location_X;
            set => Location_X = value.HasValue ? (decimal?)value.Value : null;
        } // 將 X 映射到 Location_X
        public decimal? Location_X { get; set; } // 這裡儲存實際的 RFID X軸

        public double? Y
        {
            get => (double?)Location_Y;
            set => Location_Y = value.HasValue ? (decimal?)value.Value : null;
        } // 將 Y 映射到 Location_Y
        public decimal? Location_Y { get; set; } // 這裡儲存實際的 RFID Y軸
        public string FSN { get; set; } // 所在樓層
        public double Distance { get; set; }
    }

    public interface IBeaconPosition
    {
        double? X { get; set; } // X軸
        double? Y { get; set; } // Y軸
        string FSN { get; set; } // Z軸(樓層)
        double Distance { get; set; } // 與目標物距離
    }

    public class VitalsAndPosViewModel : IVitals
    {
        [Required]
        public int? HeartRhythm { get; set; }
        [Required]
        public IEnumerable<Beacon> Beacons { get; set; }
        [Required]
        public DateTime? Timestamp { get; set; }
    }

    public interface IVitals
    {
        int? HeartRhythm { get; set; }
    }

    public class Beacon : IBeacon
    {
        public string MacAddress { get; set; } // 媒體存取控制位置(唯一)
        public string UUID { get; set; } // (每個都一樣)
        public string Major { get; set; } // (每個都一樣)
        [Required]
        public string Minor { get; set; } // (1~200)(唯一)
        public int RSSI { get; set; } // 信號強度
        [Required]
        public double Distance { get; set; } // 目標和Beacon的距離
    }

    public interface IBeacon
    {
        string MacAddress { get; set; } // 媒體存取控制位置(唯一)
        string UUID { get; set; } // 通用唯一辨識碼(每個都一樣)
        string Major { get; set; } // (每個都一樣)
        string Minor { get; set; } // (1~200)(唯一)
        int RSSI { get; set; } // 目標接收到Beacon的信號強度
        double Distance { get; set; } // 目標和Beacon的距離
    }
}