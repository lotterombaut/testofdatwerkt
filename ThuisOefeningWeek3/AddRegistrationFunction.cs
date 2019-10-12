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

namespace ThuisOefeningWeek3
{
    public static class AddRegistrationFunction
    {
        [FunctionName("AddRegistration")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "registrations")] HttpRequest req,
            ILogger log)
        {
            try
            {
                //connectionstring ophalen uit de local.settings
                string constr = Environment.GetEnvironmentVariable("CONNECTIONSTRING");

                //body van request inlezen, dit bevat de json voor onze registratie
                string json = await new StreamReader(req.Body).ReadToEndAsync();

                //json in een nieuw registratie object steken
                Registration reg = JsonConvert.DeserializeObject<Registration>(json);

                //registratieid genereren en toevoegen
                reg.RegistrationId = Guid.NewGuid().ToString();

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

                        //Het effectieve commando (@xxx voor parameterisering)
                        cmd.CommandText = "insert into tblRegistraties values (@RegistrationId, @LastName, @FirstName, @Email, @Zipcode, @Age, @isFirstTimer)";

                        //parameters gaan toevoegen
                        cmd.Parameters.AddWithValue("@RegistrationId", reg.RegistrationId);
                        cmd.Parameters.AddWithValue("@LastName", reg.LastName);
                        cmd.Parameters.AddWithValue("@FirstName", reg.FirstName);
                        cmd.Parameters.AddWithValue("@Email", reg.Email);
                        cmd.Parameters.AddWithValue("@Zipcode", reg.ZipCode);
                        cmd.Parameters.AddWithValue("@Age", reg.Age);
                        cmd.Parameters.AddWithValue("@isFirstTimer", reg.IsFirstTimer);

                        //commando uitvoeren
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                //het nieuw aangemaakte registratieobject teruggeven
                return new OkObjectResult(reg);
            }
            catch(Exception ex)
            {
                log.LogError(ex + "     ---->AddRegistration");
                return new StatusCodeResult(500);
            }
        }
    }
}
