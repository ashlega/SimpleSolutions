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
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sql;
using System.Configuration;


namespace SimpleMarketing.Dataverse
{
    public class SqlClient
    {
        private static SqlConnection staticConnection {get;set;}
        public SqlConnection Connection{ 
            get{
                return staticConnection;
            }
        }


        public SqlClient()
        {
            if(staticConnection == null){
                staticConnection = new SqlConnection(System.Environment.GetEnvironmentVariable("DataverseSqlConnection"));
                staticConnection.Open();
            }
        }

        
    }

}