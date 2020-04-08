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

namespace industryLog.API.Functions
{
    public class DeleteProduct
    {
        private readonly MongoClient _mongoClient;
        private readonly ILogger _logger;
        private readonly IConfiguration _config;

        private readonly IMongoCollection<Product> _IndustryLog;

        public DeleteProduct(
            MongoClient mongoClient,
            ILogger<DeleteProduct> logger,
            IConfiguration config)
        {
            _mongoClient = mongoClient;
            _logger = logger;
            _config = config;

            var database = _mongoClient.GetDatabase(Environment.GetEnvironmentVariable($"DATABASE_NAME"));
            _IndustryLog = database.GetCollection<Product>(Environment.GetEnvironmentVariable($"COLLECTION_NAME"));
        }

        [FunctionName(nameof(DeleteProduct))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "Product/{id}")] HttpRequest req,
            string id)
        {
            IActionResult returnValue = null;

            try
            {
                var productToDelete = _IndustryLog.DeleteOne(product => product.Id == id);

                if (productToDelete == null)
                {
                    _logger.LogInformation($"Product with id: {id} does not exist. Delete failed");
                    returnValue = new StatusCodeResult(StatusCodes.Status404NotFound);
                }

                returnValue = new StatusCodeResult(StatusCodes.Status200OK);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Could not delete item. Exception thrown: {ex.Message}");
                returnValue = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return returnValue;
        }
    }
}