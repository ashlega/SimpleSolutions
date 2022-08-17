
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
using System.Configuration;


namespace SimpleMarketing.Dataverse
{
    public class SdkClient
    {
        private static ServiceClient StaticClient {get;set;}
        public ServiceClient Client{ 
            get{
                return StaticClient;
            }
        }


        public SdkClient()
        {
            if(StaticClient == null) StaticClient = new ServiceClient(System.Environment.GetEnvironmentVariable("DataverseSdkConnection"));
        }

        public EntityCollection GetActiveSchedulers(){
            QueryExpression qe = new QueryExpression("ita_emailscheduler");
            qe.Criteria.AddCondition("statuscode", ConditionOperator.Equal, 1);
            FilterExpression fe = new FilterExpression(LogicalOperator.Or);
            fe.AddCondition("ita_starttime", ConditionOperator.LessEqual, DateTime.UtcNow);
            fe.AddCondition("ita_starttime", ConditionOperator.Null);
            qe.Criteria.AddFilter(fe);
           
            qe.ColumnSet = new ColumnSet(true);
            var result = Client.RetrieveMultiple(qe);
            return result;
        }

        public EntityCollection GetScheduledEmails(int topCount){
            QueryExpression qe = new QueryExpression("email");
            qe.TopCount = topCount;//to prevent spam policies from kicking in
            qe.Criteria.AddCondition("ita_schedulingstatus", ConditionOperator.Equal, 556780000);
            qe.ColumnSet = new ColumnSet(true);
            var result = Client.RetrieveMultiple(qe);
            return result;
        }

        public EntityCollection GetSchedulerEmails(EntityReference scheduler)
        {
           
            if(scheduler != null){
                QueryExpression qe = new QueryExpression("email");
                qe.Criteria.AddCondition("ita_scheduler", ConditionOperator.Equal, scheduler.Id);
                qe.ColumnSet = new ColumnSet(true);
                return Client.RetrieveMultiple(qe);
            }
            return null;   
        }
        public void CreateEmail(string from, string fromName, string to, string subject, string body, EntityReference scheduler = null)
        {
            //if(!EmailScheduled(to, scheduler)){
                Entity newEmail = new Entity("email");
                newEmail["subject"] = subject;
                newEmail["description"] = body;
                newEmail["ita_scheduler"] = scheduler;

                Entity toparty = new Entity("activityparty");
                toparty["addressused"] = to;
                //Guid contactid = new Guid();
                //toparty["partyid"] = new EntityReference("contact", contactid);
                newEmail["to"] = new Entity[] { toparty };

                Entity fromparty = new Entity("activityparty");
                fromparty["addressused"] = from;
                //Guid userid = new Guid();
                //fromparty["partyid"] = new EntityReference("systemuser", userid);
                newEmail["from"] = new Entity[] { fromparty };

                newEmail["ita_schedulingstatus"] = new OptionSetValue(556780000);//scheduled

                Guid targetEmailId = Client.Create(newEmail);
            //}
            
        }

        public Entity GetEmailTemplate(Guid id)
        {
            var result = Client.Retrieve("ita_emailtemplate", id, new ColumnSet(true));
            return result;
        }

        
    }

}