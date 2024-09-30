using Azure.Storage.Files.Shares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace ST10296167_CLDV6212_FunctionsApp.Functions
{
    public class UploadFileFunction
    {
//------------------------------------------------------------------------------------------------------------------------------------------//
        // This function stores an uploaded file in Azure File storage
        [Function("UploadFileToAzureFiles")]
        public static async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req,
        FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("UploadFileToAzureFiles");
            logger.LogInformation("Starting file upload request.");

            try
            {
                // Get connection string
                string connectionString = "DefaultEndpointsProtocol=https;AccountName=mycldvstorage;AccountKey=3O52Un2Yu2gpAPNbUHofCiXEHlSHNPUM1Xqv8vLcNQkgFEAyY7B3mbUXpHoIv65g/PpAHPCjDeQ5+AStIwsu5Q==;EndpointSuffix=core.windows.net";

                // Initialize the ShareServiceClient
                var shareServiceClient = new ShareServiceClient(connectionString);

                // Get the file header
                if (!req.Headers.TryGetValues("x-file-name", out var fileNameHeader))
                {
                    logger.LogWarning("'x-file-name' header is missing.");
                    var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await errorResponse.WriteStringAsync("File name header is missing.");
                    return errorResponse;
                }

                var fileName = fileNameHeader.FirstOrDefault();
                logger.LogInformation($"File name: {fileName}");

                // Get the File Share name or create one
                var shareClient = shareServiceClient.GetShareClient("uploaded-files");
                bool shareExists = await shareClient.ExistsAsync();
                if (!shareExists)
                {
                    logger.LogInformation($"Share 'uploaded-files' does not exist. Creating...");
                    await shareClient.CreateAsync();
                }

                // Get root directory client
                var directoryClient = shareClient.GetRootDirectoryClient();
                var fileClient = directoryClient.GetFileClient(fileName);

                // Read file content from request body
                using var stream = new MemoryStream();
                await req.Body.CopyToAsync(stream);
                stream.Position = 0;

                // Upload to File Shares
                await fileClient.CreateAsync(stream.Length);
                await fileClient.UploadAsync(stream);

                logger.LogInformation($"File {fileName} uploaded successfully.");

                // Return a success message
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync($"File {fileName} uploaded successfully.");
                return response;
            }
            catch (Exception ex)
            {
                // Log and return upload error 
                logger.LogError($"Error uploading file: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("An error occurred while uploading the file.");
                return errorResponse;
            }
        }
//------------------------------------------------------------------------------------------------------------------------------------------//
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//