using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System.Linq;
using System.Net.Http.Json;
using System.Net.Http;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Net.Mail;
using System.Net;

namespace SimpleMarketing.Mail
{
    public class MailConnector
    {

        SmtpClient client = new SmtpClient(System.Environment.GetEnvironmentVariable("smtpServer"));      
        public MailConnector()
        {
            client.Credentials = new NetworkCredential(System.Environment.GetEnvironmentVariable("smtpUserName"), System.Environment.GetEnvironmentVariable("smtpPassword"));
            if(System.Environment.GetEnvironmentVariable("smtpPort") != null)
            {
                client.Port = int.Parse(System.Environment.GetEnvironmentVariable("smtpPort"));
            }
            client.EnableSsl = System.Environment.GetEnvironmentVariable("smtpUseSSL") == null ? false : Boolean.Parse(System.Environment.GetEnvironmentVariable("smtpUseSSL"));
            client.UseDefaultCredentials = false;
        }

        public void SendEmail(string from, string fromName, string to, string subject, string body)
        {
            MailAddress fromAddress = new MailAddress(from, fromName);
            MailAddress toAddress = new MailAddress(to);
            MailMessage message = new MailMessage(fromAddress, toAddress);
            message.IsBodyHtml = true;
            message.Body = body;
            message.Subject = subject;
            client.Send(message);
        }
    }
}