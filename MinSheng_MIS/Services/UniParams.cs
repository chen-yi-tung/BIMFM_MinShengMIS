using MinSheng_MIS.Attributes;
using System;

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

        #region 保養單狀態
        public class MaintenanceFormStatus
        {
            public enum Status
            {
                /// <summary>
                /// 待派工
                /// </summary>
                [EnumLabel("待派工")]
                ToAssign = 1,
                /// <summary>
                /// 待執行
                /// </summary>
                [EnumLabel("待執行")]
                ToDo = 2,
                /// <summary>
                /// 待審核
                /// </summary>
                [EnumLabel("待審核")]
                ToAduit = 3,
                /// <summary>
                /// 審核通過
                /// </summary>
                [EnumLabel("審核通過")]
                Approved = 4,
                /// <summary>
                /// 審核未過
                /// </summary>
                [EnumLabel("審核未過")]
                NotApproved = 5
            }

            public static Status ConvertStringToEnum(string str)
            {
                if (!Enum.TryParse<Status>(str, out var result))
                    throw new ArgumentException($"Invalid status value: {str}");

                return result;
            }

            public static bool IsStatusEqualToStr(string str, Status status)
            {
                var strState = ConvertStringToEnum(str);

                return strState == status;
            }
        }
        #endregion
    }
}