using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sql;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;



namespace SimpleMarketing.Functions
{

    
    public static class ProcessScheduledEmails
    {

        public static IActionResult ProcessScheduledEmailsHandler(ILogger log){
            var mc = new Mail.MailConnector();
            var dc = new Dataverse.SdkClient();
            var emails = dc.GetScheduledEmails(150);//To avoid 300 spam filters

            
            foreach(var email in emails.Entities)
            {
                EntityCollection toList = (EntityCollection)email["to"];

                mc.SendEmail(System.Environment.GetEnvironmentVariable("smtpFromEmail"), 
                    System.Environment.GetEnvironmentVariable("smtpFromName"), 
                    (string)(toList[0]["addressused"]),
                    (string)email["subject"],
                    (string)email["description"]
                );

                var updatedEmail = new Entity("email");
                updatedEmail.Id = email.Id;
                updatedEmail["ita_schedulingstatus"] = new OptionSetValue(556780001);
                dc.Client.Update(updatedEmail);
            }

            string responseMessage = "ok";

            return new OkObjectResult(responseMessage);
        }
    }

    public static class ProcessScheduledEmailsHttp
    {
        [FunctionName("ProcessScheduledEmailsHttp")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("ProcessScheduledEmailsHttp function starting...");
            return ProcessScheduledEmails.ProcessScheduledEmailsHandler(log);
            
        }
    }

    public static class ProcessScheduledEmailsTimer
    {

        [FunctionName("ProcessScheduledEmailsTimer")]
        public static void Run([TimerTrigger("0 0 * * * *")]TimerInfo myTimer, ILogger log)
        {
            ProcessScheduledEmails.ProcessScheduledEmailsHandler(log);
        }
    }
}