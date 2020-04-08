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
    public class CreateProduct
    {
        private readonly MongoClient _mongoClient;
        private readonly ILogger _logger;
        private readonly IConfiguration _config;

        private readonly IMongoCollection<Product> _IndustryLog;

        public CreateProduct(
            MongoClient mongoClient,
            ILogger<CreateProduct> logger,
            IConfiguration config)
        {
            _mongoClient = mongoClient;
            _logger = logger;
            _config = config;

            var database = _mongoClient.GetDatabase(Environment.GetEnvironmentVariable($"DATABASE_NAME"));
            _IndustryLog = database.GetCollection<Product>(Environment.GetEnvironmentVariable($"COLLECTION_NAME"));
        }

        [FunctionName(nameof(CreateProduct))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CreateProduct")] HttpRequest req)
        {
            IActionResult returnValue = null;

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var input = JsonConvert.DeserializeObject<Product>(requestBody);

            var product = new Product
            {
                CompanyNumber = input.CompanyNumber,
                ProductId = input.ProductId,
                Color = input.Color,
                ReleaseDate = input.ReleaseDate
            };

            try
            {
                _IndustryLog.InsertOne(product);
                returnValue = new OkObjectResult(product);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown: {ex.Message}");
                returnValue = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }


            return returnValue;
        }
    }
}