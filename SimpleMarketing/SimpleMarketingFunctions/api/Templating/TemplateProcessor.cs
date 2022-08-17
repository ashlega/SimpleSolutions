using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System.Linq;
using System.Net.Http;
using System.Xml.Linq;
using System.Collections.Generic;
using HandlebarsDotNet;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sql;
using System.Dynamic;

namespace SimpleMarketing.Templating
{
    public class TemplateProcessor
    {
        public TemplateProcessor()
        {
        }

        public void ProcessEmailTemplatesForSqlReader(string subjectSource, string bodySource, SqlDataReader reader, EntityReference scheduler = null, string excludedEmails = null, string testEmail = null)
        {
            var mc = new SimpleMarketing.Mail.MailConnector();
            var dc = new SimpleMarketing.Dataverse.SdkClient();

            var handlebars = Handlebars.Create();
            var bodyTemplate = handlebars.Compile(bodySource);
            var subjectTemplate = handlebars.Compile(subjectSource);
            
            excludedEmails = excludedEmails != null ? excludedEmails.ToUpper() : "";
            
            Dictionary<string, List<Object>> groupedData = new Dictionary<string, List<object>>();
            
            while(reader.Read()){
                string email = null;
                var dynamicObject = new ExpandoObject() as IDictionary<string, Object>;
                //List<string> row = new List<string>();
                for(int i = 0; i<reader.FieldCount; i++){
                    dynamicObject.Add(reader.GetName(i),reader[i] != null ? reader[i].ToString() : "");
                    //row.Add(reader[i] != null ? reader[i].ToString() : "");
                    if(reader.GetName(i) == "email"){
                        email = reader[i].ToString();
                    }
                }

                if(excludedEmails.Contains(email)) continue;

                if(!groupedData.ContainsKey(email)){
                    groupedData[email] = new List<object>();
                }
                List<Object> rows = groupedData[email];
                rows.Add(dynamicObject);
            }
            
            var existingEmails = dc.GetSchedulerEmails(scheduler);
            var existingEmailList = existingEmails.Entities.ToList<Entity>();

            foreach(var key in groupedData.Keys)
            {

                bool emailExists = existingEmailList.Find(e => {
                    EntityCollection toList = (EntityCollection)e["to"];
                    return ((string)(toList[0]["addressused"])).ToUpper() == key.ToUpper();
                }) != null;
                if(!emailExists || testEmail != null){
                    var data = new {
                        rows = groupedData[key]
                    };  
                    var message = bodyTemplate(data);
                    var subject = subjectTemplate(data);
                    dc.CreateEmail(System.Environment.GetEnvironmentVariable("smtpFromEmail"), 
                                        System.Environment.GetEnvironmentVariable("smtpFromName"), 
                                        testEmail != null ? testEmail : key, 
                                        subject,
                                        message, 
                                        scheduler);
                    if (testEmail != null) break;
                }
            }

            

            var updatedScheduler = new Entity("ita_emailscheduler");
            updatedScheduler.Id = scheduler.Id;
            updatedScheduler["statuscode"] = new OptionSetValue(3);
            dc.Client.Update(updatedScheduler);

        }
    }
}