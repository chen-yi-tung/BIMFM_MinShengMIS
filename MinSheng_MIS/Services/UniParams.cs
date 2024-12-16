using MinSheng_MIS.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Services
{
    public class UniParams
    {
        #region 保養週期
        public static class MaintainPeriod
        {
            public enum Period
            {
                /// <summary>
                /// 每日
                /// </summary>
                [EnumLabel("每日")]
                Daily = 1,
                /// <summary>
                /// 每月
                /// </summary>
                [EnumLabel("每月")]
                Monthly = 2,
                /// <summary>
                /// 每季
                /// </summary>
                [EnumLabel("每季")]
                Quarterly = 3,
                /// <summary>
                /// 每年
                /// </summary>
                [EnumLabel("每年")]
                Yearly = 4
            }

            public static readonly Dictionary<string, string> Surface = new Dictionary<string, string>
            {
                { Period.Daily.ToString(), "每日" },
                { Period.Monthly.ToString(), "每月" },
                { Period.Quarterly.ToString(), "每季" },
                { Period.Yearly.ToString(), "每年" }
            };

            public static DateTime GetNextMaintainDate(string period)
            {
                if (!Enum.TryParse<Period>(period, out var parsedPeriod))
                    throw new ArgumentException($"Invalid period value: {period}");

                switch (parsedPeriod)
                {
                    case Period.Daily:
                        return DateTime.Now.AddDays(1);
                    case Period.Monthly:
                        return DateTime.Now.AddMonths(1);
                    case Period.Quarterly:
                        return DateTime.Now.AddMonths(3);
                    case Period.Yearly:
                        return DateTime.Now.AddYears(1);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(parsedPeriod), $"Unhandled period value: {parsedPeriod}");
                }
            }
        }
        #endregion

        #region 設備狀態
        public enum EState
        {
            /// <summary>
            /// 正常
            /// </summary>
            [EnumLabel("正常")]
            Normal = 1,
            /// <summary>
            /// 報修中
            /// </summary>
            [EnumLabel("報修中")]
            Repair = 2
        }
        #endregion
    }
}