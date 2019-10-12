using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace ThuisOefeningWeek3
{
    public static class getRegistrationsFunction
    {
        [FunctionName("getRegistrationsFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "registrations")] HttpRequest req,
            ILogger log)
        {
            try
            {
                //connectionstring ophalen uit de local.settings
                string constr = Environment.GetEnvironmentVariable("CONNECTIONSTRING");

                //lege lijst met Registration objecten aanmaken
                List<Registration> regs = new List<Registration>();

                //we zullen gebruikmaken van een connectie, als deze niet meer gebruikt wordt zal deze "gedropt" worden
                using (SqlConnection con = new SqlConnection())
                {
                    //de connectionstring doorgeven aan de connection
                    con.ConnectionString = constr;

                    //wachten tot de connection geopend is
                    await con.OpenAsync();

                    //we zullen een sqlcommando gebruiken, als dit niet meer gebruikt wordt zal dit "gedropt" worden
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        //connection waarop het commando zal worden uitgevoerd doorgeven
                        cmd.Connection = con;
                        cmd.CommandText = "select * from tblRegistraties";
                        SqlDataReader reader = await cmd.ExecuteReaderAsync();
                        while(reader.Read())
                        {
                            Registration r = new Registration();
                            r.RegistrationId = reader["RegistrationId"].ToString();
                            r.LastName = reader["LastName"].ToString();
                            r.FirstName = reader["FirstName"].ToString();
                            r.Email = reader["Email"].ToString();
                            r.ZipCode = reader["Zipcode"].ToString();
                            r.Age = Convert.ToInt32(reader["Age"]);
                            r.IsFirstTimer = Convert.ToBoolean(reader["isFirstTimer"]);
                            regs.Add(r);
                        }
                    }
                }
                //De nieuwe lijst teruggeven
                return new OkObjectResult(regs);
            }
            catch (Exception ex)
            {
                log.LogError(ex + "     ---->getRegistrations");
                return new StatusCodeResult(500);
            }
        }
    }
}
