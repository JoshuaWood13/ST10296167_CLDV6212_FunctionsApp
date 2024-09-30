using Azure;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace ST10296167_CLDV6212_FunctionsApp.Functions
{
    public static class StoreCustomerProfileFunction
    {
//------------------------------------------------------------------------------------------------------------------------------------------//
        // This function stores a created user profile in an Azure Table
        [Function("StoreCustomerProfile")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("StoreCustomerProfile");
            logger.LogInformation("Processing a request to store a customer profile.");

            // Read body from HTTP request
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            // Log update
            logger.LogInformation($"Received data: {requestBody}");

            // Splitting the body into key-value pairs
            var keyValuePairs = requestBody.Trim('{', '}').Split(',');

            var profile = new CustomerProfile
            {
                PartitionKey = "CustomerProfile",
                RowKey = Guid.NewGuid().ToString(), 
            };

            // Populate the profile properties from the key-value pairs
            foreach (var pair in keyValuePairs)
            {
                var keyValue = pair.Split(':');
                var key = keyValue[0].Trim().Trim('"');
                var value = keyValue[1].Trim().Trim('"');

                // Assign data values
                switch (key)
                {
                    case "FirstName":
                        profile.FirstName = value;
                        break;
                    case "LastName":
                        profile.LastName = value;
                        break;
                    case "Email":
                        profile.Email = value;
                        break;
                    case "PhoneNumber":
                        profile.PhoneNumber = value;
                        break;
                }
            }

            // Initialize the Azure Table Storage client using connection string
            var tableServiceClient = new TableServiceClient("REMOVED FOR SECURITY REASONS");
            // Find or create the necessary table in Azure portal
            var customerTableClient = tableServiceClient.GetTableClient("CustomerProfileTable");
            await customerTableClient.CreateIfNotExistsAsync();

            // Add data to the table
            await customerTableClient.AddEntityAsync(profile);

            // Return a success message
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("Customer profile received successfully!");
            return response;
        }
//------------------------------------------------------------------------------------------------------------------------------------------//
    }
//------------------------------------------------------------------------------------------------------------------------------------------//
    // Define CustomerProfile class 
    public class CustomerProfile : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
//------------------------------------------------------------------------------------------------------------------------------------------//
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//