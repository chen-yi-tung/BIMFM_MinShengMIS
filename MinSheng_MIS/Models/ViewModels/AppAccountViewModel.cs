using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Models.ViewModels
{
    public class AppLoginViewModel
    {
        public string Account { get; set; }
        public string Password { get; set; }
    }

    public class AppChangePasswordViewModel
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string CheckPassword { get; set; }

    }
}