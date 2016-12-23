using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ApiGetwayMock
{
    public class Program
    {
        private LoyaltyProgramClient client;

        public static void Main(string[] arg) => new Program().Main();

        public void Main()
        {
            this.client = new LoyaltyProgramClient("localhost:5001");
            WriteLine("Welcome to the API Gateway Mock.");

            var cont = true;
            while (cont)
            {
                WriteLine();
                WriteLine();
                WriteLine("********************");
                WriteLine("Choose one of:");
                WriteLine("q <userid> - to query the Loyalty Program Microservice for a user with id <userid>.");
                WriteLine("r <userid> - to register a user with id <userid> with the Loyalty Program Microservice.");
                WriteLine("u <userid> <interests> - to update a user with new comman separated interests");
                WriteLine("exit - to exit");
                WriteLine("********************");
                var cmd = ReadLine();
                cont = ProcessCommand(cmd);
            }
        }

        private string ReadLine()
        {
            return Console.ReadLine();
        }

        private void WriteLine()
        {
            Console.WriteLine();
        }

        private void WriteLine(string v)
        {
            Console.WriteLine(v);
        }

        private bool ProcessCommand(string cmd)
        {
            if ("exit".Equals(cmd))
                return false;
            if (cmd.StartsWith("q"))
                ProcessUserQuery(cmd);
            else if (cmd.StartsWith("r"))
                ProcessUserRegistration(cmd);
            else if (cmd.StartsWith("u"))
                ProcessUpdateUser(cmd);
            else
                WriteLine("Did not understand command :(");
            return true;
        }

        private void ProcessUserQuery(string cmd)
        {
            int userId;
            if (!int.TryParse(cmd.Substring(1), out userId))
                WriteLine("Please specify user id as an int");
            else
            {
                HttpResponseMessage response = this.client.QueryUser(userId).Result;
                PrettyPrintResponse(response);
            }
        }

        private void ProcessUserRegistration(string cmd)
        {
            var newUser = new LoyaltyProgramUser { Name = cmd.Substring(1).Trim() };
            var response = this.client.RegisterUser(newUser).Result;
            PrettyPrintResponse(response);
        }

        private static async void PrettyPrintResponse(HttpResponseMessage response)
        {
            Console.WriteLine("Status code: " + response?.StatusCode.ToString() ?? "command failed");
            Console.WriteLine("Headers: " + response?.Headers.Aggregate("", (acc, h) => acc + "\n\t" + h.Key + ": " + h.Value) ?? "");
            Console.WriteLine("Body: " + await response?.Content.ReadAsStringAsync() ?? "");
        }

        private async void ProcessUpdateUser(string cmd)
        {
            int userId;
            if (!int.TryParse(cmd.Split(' ').Skip(1).First(), out userId))
                WriteLine("Plaese speciffy user id as an int");
            else
            {
                var response = this.client.QueryUser(userId).Result;
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var user = JsonConvert.DeserializeObject<LoyaltyProgramUser>(await response.Content.ReadAsStringAsync());
                    var newInterests = cmd.Substring(cmd.IndexOf(' ', 2)).Split(',').Select(i => i.Trim());
                    var unionInterests = CombineUnion(user, newInterests);
                    user.Settings =
                      new LoyaltyProgramSettings
                      {
                          Interests = unionInterests.ToArray()
                      };
                    PrettyPrintResponse(this.client.UpdateUser(user).Result);
                }
            }
        }

        private IEnumerable<string> CombineUnion(LoyaltyProgramUser user, IEnumerable<string> newInterests)
        {
            if (user.Settings?.Interests == null)
                return newInterests;

            return user.Settings?.Interests.Union(newInterests);

        }
    }
}
