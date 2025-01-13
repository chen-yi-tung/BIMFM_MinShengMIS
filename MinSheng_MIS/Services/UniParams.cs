using MinSheng_MIS.Attributes;
using System;

namespace MinSheng_MIS.Services
{
    public class UniParams
    {
        #region 保養週期
        public enum MaintainPeriod
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
            if (!Enum.TryParse<MaintainPeriod>(period, out var parsedPeriod))
                throw new ArgumentException($"Invalid period value: {period}");

            switch (parsedPeriod)
            {
                case MaintainPeriod.Daily:
                    return DateTime.Now.AddDays(1);
                case MaintainPeriod.Monthly:
                    return DateTime.Now.AddMonths(1);
                case MaintainPeriod.Quarterly:
                    return DateTime.Now.AddMonths(3);
                case MaintainPeriod.Yearly:
                    return DateTime.Now.AddYears(1);
                default:
                    throw new ArgumentOutOfRangeException(nameof(parsedPeriod), $"Unhandled period value: {parsedPeriod}");
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
        public enum MaintenanceFormStatus
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
        #endregion

        #region 報修單狀態
        public enum ReportFormStatus
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
        #endregion

        #region 工單狀態
        public enum InspectionPlanState
        {
            /// <summary>
            /// 待執行
            /// </summary>
            [EnumLabel("待執行")]
            ToDo = 1,
            /// <summary>
            /// 執行中
            /// </summary>
            [EnumLabel("執行中")]
            InProgress = 2,
            /// <summary>
            /// 完成
            /// </summary>
            [EnumLabel("完成")]
            Done = 3,
        }
        #endregion

        #region 巡檢狀態
        public enum InspectionState
        {
            /// <summary>
            /// 待執行
            /// </summary>
            [EnumLabel("待執行")]
            ToDo = 1,
            /// <summary>
            /// 執行中
            /// </summary>
            [EnumLabel("執行中")]
            InProgress = 2,
            /// <summary>
            /// 完成
            /// </summary>
            [EnumLabel("完成")]
            Done = 3,
        }
        #endregion

        #region 檢查結果
        public enum CheckResult
        {
            /// <summary>
            /// 正常
            /// </summary>
            [EnumLabel("正常")]
            Normal = 1,
            /// <summary>
            /// 異常
            /// </summary>
            [EnumLabel("異常")]
            Defective = 2
        }
        #endregion

        /// <summary>
        /// 將字串視為列舉值轉為對應的列舉成員
        /// </summary>
        /// <typeparam name="T">列舉類型</typeparam>
        /// <param name="str">字串</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">str無法解析</exception>
        public static T ConvertStringToEnum<T>(string str) where T : struct, Enum
        {
            if (!Enum.TryParse<T>(str, out var result))
                throw new ArgumentException($"Invalid status value: {nameof(str)}");

            return result;
        }

        /// <summary>
        /// 比較字串轉為列舉值後是否與列舉成員相同
        /// </summary>
        /// <typeparam name="T">列舉類型</typeparam>
        /// <param name="str">字串</param>
        /// <param name="status">列舉成員</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">str為空值</exception>
        public static bool IsEnumEqualToStr<T>(string str, T status) where T : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(str))
                throw new ArgumentException($"Cannot be null or empty: {nameof(str)}");

            var strState = ConvertStringToEnum<T>(str);

            return strState.Equals(status);
        }
    }
}