using MinSheng_MIS.Attributes;
using System.Web.Mvc;

namespace MinSheng_MIS
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new MinShengAuthorizeAttribute());
        }
    }
}
