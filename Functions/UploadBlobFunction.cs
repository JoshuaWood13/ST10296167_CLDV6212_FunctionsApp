using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace ST10296167_CLDV6212_FunctionsApp.Functions
{

    public static class UploadImageToBlobFunction
    {
//------------------------------------------------------------------------------------------------------------------------------------------//
        [Function("UploadImageToBlob")]
        public static async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req,
        FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("UploadImageToBlob");
            logger.LogInformation("Starting image upload request.");

            try
            {
                // Get connection string
                string connectionString = "DefaultEndpointsProtocol=https;AccountName=mycldvstorage;AccountKey=3O52Un2Yu2gpAPNbUHofCiXEHlSHNPUM1Xqv8vLcNQkgFEAyY7B3mbUXpHoIv65g/PpAHPCjDeQ5+AStIwsu5Q==;EndpointSuffix=core.windows.net";

                // Initialize BlobServiceClient 
                BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

                // Get Blob Container
                var containerClient = blobServiceClient.GetBlobContainerClient("product-images");

                // Get image file header
                if (req.Headers.TryGetValues("x-file-name", out var fileNameHeader))
                {
                    var fileName = fileNameHeader.FirstOrDefault();
                    if (string.IsNullOrEmpty(fileName))
                    {
                        logger.LogWarning("File name is empty or missing.");
                        var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                        await errorResponse.WriteStringAsync("File name is empty or missing.");
                        return errorResponse;
                    }

                    // Create BlobClient
                    logger.LogInformation($"Creating BlobClient for file: {fileName}.");
                    var blobClient = containerClient.GetBlobClient(fileName);

                    // Read from request body
                    logger.LogInformation("Reading the request body stream.");
                    using var stream = new MemoryStream();
                    await req.Body.CopyToAsync(stream);
                    stream.Position = 0; 

                    // Upload the image file to Blob Storage
                    logger.LogInformation($"Uploading file {fileName} to Blob storage.");
                    await blobClient.UploadAsync(stream, overwrite: true);

                    // Log successful upload
                    logger.LogInformation($"Image {fileName} uploaded successfully.");

                    // Return a success message
                    var successResponse = req.CreateResponse(HttpStatusCode.OK);
                    await successResponse.WriteStringAsync($"Image {fileName} uploaded successfully.");
                    return successResponse;
                }
                else
                {
                    // Return an error message
                    logger.LogWarning("'x-file-name' header is missing.");
                    var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await errorResponse.WriteStringAsync("File name header is missing.");
                    return errorResponse;
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error uploading image: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("An error occurred while uploading the image.");
                return errorResponse;
            }
        }
//------------------------------------------------------------------------------------------------------------------------------------------//
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//