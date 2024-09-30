using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Net;

namespace ST10296167_CLDV6212_FunctionsApp.Functions
{
    public class UploadQueueMessageFunction
    {
//------------------------------------------------------------------------------------------------------------------------------------------//
        [Function("SendMessageToQueue")]
        public static async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req,
        FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("SendMessageToQueueFunction");
            logger.LogInformation("Processing a queue message request.");

            try
            {
                // Parse request body
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var requestData = JsonSerializer.Deserialize<QueueRequestData>(requestBody);
                logger.LogInformation($"Received message: {requestData.Message}");

                // Get connection string
                string connectionString = "DefaultEndpointsProtocol=https;AccountName=mycldvstorage;AccountKey=3O52Un2Yu2gpAPNbUHofCiXEHlSHNPUM1Xqv8vLcNQkgFEAyY7B3mbUXpHoIv65g/PpAHPCjDeQ5+AStIwsu5Q==;EndpointSuffix=core.windows.net";

                // Initialize QueueServiceClient
                QueueServiceClient queueServiceClient = new QueueServiceClient(connectionString);
                logger.LogInformation("QueueServiceClient created.");

                // Get the Queue name or create one
                var queueClient = queueServiceClient.GetQueueClient(requestData.QueueName);
                await queueClient.CreateIfNotExistsAsync();
                logger.LogInformation($"Queue '{requestData.QueueName}' exists or was created.");

                // Send message to queue
                await queueClient.SendMessageAsync(requestData.Message);
                logger.LogInformation($"Message '{requestData.Message}' sent to queue '{requestData.QueueName}'.");

                // Return success message
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync("Message successfully added to the queue.");
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error processing queue message: {ex.Message}");

                // Return error message
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Failed to add message to the queue.");
                return errorResponse;
            }
        }
//------------------------------------------------------------------------------------------------------------------------------------------//
    }
//------------------------------------------------------------------------------------------------------------------------------------------//
    // Helper class for deserializing request data
    public class QueueRequestData
    {
        public string QueueName { get; set; }
        public string Message { get; set; }
    }
//------------------------------------------------------------------------------------------------------------------------------------------//
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//