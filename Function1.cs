using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure;
using Azure.Communication.Email;

namespace FunctionAppHome
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string connectionString = Environment.GetEnvironmentVariable("COMMUNICATION_SERVICES_CONNECTION_STRING");
            EmailClient emailClient = new EmailClient(connectionString);

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            name = name ?? "The Dude";

            var subject = "Welcome to Azure Communication Service Email APIs.";
            var htmlContent = "<html><body><h1>Quick send email test</h1><br/><h4>This email message is sent from Azure Communication Service Email.</h4><p>This mail was sent using .NET SDK!!</p></body></html>";
            var sender = Environment.GetEnvironmentVariable("SENDER_ADDRESS");
            var recipient = Environment.GetEnvironmentVariable("RECIPIENT_ADDRESS");

            try
            {
                Console.WriteLine("Sending email...");
                EmailSendOperation emailSendOperation = await emailClient.SendAsync(
                    Azure.WaitUntil.Completed,
                    sender,
                    recipient,
                    subject,
                    htmlContent);
                EmailSendResult statusMonitor = emailSendOperation.Value;
                Console.WriteLine($"Email Sent. Status = {emailSendOperation.Value.Status}");

                string operationId = emailSendOperation.Id;
                Console.WriteLine($"Email operation id = {operationId}");
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"Email send operation failed with error code: {ex.ErrorCode}, message: {ex.Message}");
            }

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}
