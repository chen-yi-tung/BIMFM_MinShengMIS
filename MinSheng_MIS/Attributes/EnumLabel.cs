using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace MinSheng_MIS.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class EnumLabelAttribute : Attribute
    {
        public string Label { get; }

        public EnumLabelAttribute(string label)
        {
            Label = label;
        }
    }

    public static class EnumExtensions
    {
        public static string GetLabel(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field.GetCustomAttribute<EnumLabelAttribute>();
            return attribute?.Label ?? value.ToString(); // 若無標籤，回傳 enum 的名稱
        }
    }
}