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
    public class GetProduct
    {
        private readonly MongoClient _mongoClient;
        private readonly ILogger _logger;
        private readonly IConfiguration _config;

        private readonly IMongoCollection<Product> _IndustryLog;

        public GetProduct(
            MongoClient mongoClient,
            ILogger<GetProduct> logger,
            IConfiguration config)
        {
            _mongoClient = mongoClient;
            _logger = logger;
            _config = config;

            var database = _mongoClient.GetDatabase(Environment.GetEnvironmentVariable($"DATABASE_NAME"));
            _IndustryLog = database.GetCollection<Product>(Environment.GetEnvironmentVariable($"COLLECTION_NAME"));
        }

        [FunctionName(nameof(GetProduct))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Product/{id}")] HttpRequest req,
            string id)
        {
            IActionResult returnValue = null;

            try
            {
                var result = _IndustryLog.Find(album => album.Id == id).FirstOrDefault();

                if (result == null)
                {
                    _logger.LogWarning("That item doesn't exist!");
                    returnValue = new NotFoundResult();
                }
                else
                {
                    returnValue = new OkObjectResult(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Couldn't find Product with id: {id}. Exception thrown: {ex.Message}");
                returnValue = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return returnValue;
        }
    }
}