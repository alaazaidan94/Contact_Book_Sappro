using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactBook_Services.AccountServices
{
    public interface IMailService
    {
        public Task<bool> SendEmail(string toEmail, string subject, string body);
    }
}
