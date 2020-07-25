using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaskManagment.Models
{
    public class MailServerInfo
    {
      public string UserName { get; set; }
      public string Password { get; set; }
      public string Port { get; set; }
      public string SmtpUrl { get; set; }
    }
}