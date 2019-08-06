using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace TMAWebService
{
    public class NotifyWS
    {
        private string emailAddr = "";
        public void SendMessage(string quoteID, string quoteDesc)
        {
            emailAddr = ConfigurationSettings.AppSettings["WS_notify_email"];
            string from = "admin@mdl.com";
            MailMessage msg = new MailMessage(from, emailAddr);
       
            msg.Subject = "WS Quote Scoped";
            string body = "New quote scoping for " + quoteID + ", " + quoteDesc;
            msg.Body = body;
            SmtpClient smtp = new SmtpClient();
            //smtp.EnableSsl = true;
            int port = smtp.Port;
            string host = smtp.Host;
            try
            {
                smtp.Send(msg);
            }
            catch (Exception e) { string err3 = e.Message; }
            //get configured addressee for email notification
            //1. create MailMessage
            //2. assign properties
            //3. create smtpClient class, get SMTP server config from web.config
            //4. send MailMessage using SmtpClient.send()

        }
    }
}