using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.Xrm.Tooling.Connector;
using System.IO;
using Microsoft.Crm.Sdk.Messages;
using System.Reflection;
using Microsoft.Xrm.Sdk.Messages;
using System.Diagnostics;

namespace SolutionImport
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            Console.WriteLine("Please check the app.config to add credentials and organization url");
            string connectionString = ConfigurationManager.ConnectionStrings["CrmDefaultConnection"].ToString();
            var serviceClient = new CrmServiceClient(connectionString);
            serviceClient.OrganizationServiceProxy.Timeout = new TimeSpan(1, 30, 0);
            if (!serviceClient.IsReady) throw new Exception("Cannot connect to organization!, Please check the app.config to add credentials and organization url");

            Console.WriteLine("Connected to {0}", serviceClient.ConnectedOrgFriendlyName);
            Console.WriteLine("Do you want to import the solution Synchronously or Asynchronously ?\n 1 - Sync \n 2 - Async");
            string input = Console.ReadLine();
            double d;
            if (Double.TryParse(input, out d))
            {
                if (d == 1)//Synchronous
                {
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();
                    var importSolutionResponse = (ImportSolutionResponse)serviceClient.Execute(ImportSolution(serviceClient));
                    var est = importSolutionResponse.Results;
                    stopWatch.Stop();
                    Console.WriteLine("RunTime " + stopWatch.Elapsed);
                    Console.ReadKey();
                }
                else if (d == 2)//Asynchronous 
                {
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();
                    ExecuteAsyncRequest request = new ExecuteAsyncRequest();
                    request.Request = ImportSolution(serviceClient);
                    ExecuteAsyncResponse response = (ExecuteAsyncResponse)serviceClient.Execute(request);
                    stopWatch.Stop();
                    Console.WriteLine("RunTime " + stopWatch.Elapsed);
                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine("Invalid option - bye");
                    Console.ReadKey();
                }
            }
        }
        private static ImportSolutionRequest ImportSolution(CrmServiceClient client)
        {
            string solutionName = "Test_1_0_0_0.zip";
            Guid importID = Guid.NewGuid();
            string folderPath = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
            var solutionBytes = File.ReadAllBytes(Path.Combine(folderPath , solutionName));
            var importSolutionRequest = new ImportSolutionRequest
            {
                CustomizationFile = solutionBytes,
                PublishWorkflows = true,
                ConvertToManaged = false,
                OverwriteUnmanagedCustomizations = true,
                ImportJobId = importID,
                HoldingSolution = false,
                SkipProductUpdateDependencies = true
            };
            return importSolutionRequest;
        }
    }
}
