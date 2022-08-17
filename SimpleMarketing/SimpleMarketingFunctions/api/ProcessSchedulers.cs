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
    public static class ProcessSchedulers
    {
        public static IActionResult ProcessSchedulersHandler(ILogger log)
        {
            Dataverse.SqlClient sc = new Dataverse.SqlClient();
                        
            SimpleMarketing.Templating.TemplateProcessor tp = new Templating.TemplateProcessor();
            
            var dc = new Dataverse.SdkClient();
            var schedulers = dc.GetActiveSchedulers();
            foreach(var scheduler in schedulers.Entities)
            {
                string testEmail = null;
                if(scheduler.Contains("ita_testemail")) testEmail = (string)scheduler["ita_testemail"];

                var template = dc.GetEmailTemplate(((EntityReference)scheduler["ita_emailtemplate"]).Id);
                string subject = (string)template["ita_subject"];
                string body = (string)template["ita_body"];
                string query = (string)template["ita_defaultquery"];

                string excludedEmails = (scheduler.Contains("ita_excludedemails") ? (string)scheduler["ita_excludedemails"] : "").ToUpper();
                
                SqlCommand command = new SqlCommand(query, sc.Connection);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                
                    tp.ProcessEmailTemplatesForSqlReader(subject, body, reader, scheduler.ToEntityReference(), excludedEmails, testEmail);
                }
            }

            string responseMessage = "ok";

            return new OkObjectResult(responseMessage);
        }
    }

    public static class ProcessSchedulersHttp
    {

        [FunctionName("ProcessSchedulersHttp")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("ProcessSchedulersHttp function starting...");
            return ProcessSchedulers.ProcessSchedulersHandler(null);
        }
    }

    public static class ProcessSchedulersTimer
    {

        [FunctionName("ProcessSchedulers")]
        public static void Run([TimerTrigger("0 30 * * * *")]TimerInfo myTimer, ILogger log)
        {
            ProcessSchedulers.ProcessSchedulersHandler(log);
        }
    }


    
}