using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using IndustryLog.API.Models;
using IndustryLog.API.Helpers;

namespace IndustryLog.API.Functions
{
    public class UpdateProduct
    {
        private readonly MongoClient _mongoClient;
        private readonly ILogger _logger;
        private readonly IConfiguration _config;

        private readonly IMongoCollection<Product> _IndustryLog;

        public UpdateProduct(
            MongoClient mongoClient,
            ILogger<UpdateProduct> logger,
            IConfiguration config)
        {
            _mongoClient = mongoClient;
            _logger = logger;
            _config = config;

            var database = _mongoClient.GetDatabase(Environment.GetEnvironmentVariable($"DATABASE_NAME"));
            _IndustryLog = database.GetCollection<Product>(Environment.GetEnvironmentVariable($"COLLECTION_NAME"));
        }

        [FunctionName(nameof(UpdateProduct))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "Product/{id}")] HttpRequest req,
            string id)
        {
            IActionResult returnValue = null;

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var updatedResult = JsonConvert.DeserializeObject<Product>(requestBody);

            updatedResult.Id = id;

            try
            {
                var replacedItem = _IndustryLog.ReplaceOne(product => product.Id == id, updatedResult);

                if (replacedItem == null)
                {
                    returnValue = new NotFoundResult();
                }
                else
                {
                    returnValue = new OkObjectResult(updatedResult);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Could not update Product with id: {id}. Exception thrown: {ex.Message}");
                returnValue = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return returnValue;
        }
    }
}