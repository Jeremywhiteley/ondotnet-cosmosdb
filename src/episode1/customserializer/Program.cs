﻿using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;

namespace episode1
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            await Program.InitializeContainers(configuration);

            await Program.WithCustomSerializerAsync(configuration);
            await Program.WithSerializerOptionsAsync(configuration);
        }

        static async Task WithCustomSerializerAsync(IConfiguration configuration)
        {
            CosmosClient client = new CosmosClientBuilder(configuration.GetConnectionString("Cosmos"))
                    .WithApplicationName("OnDotNetRocks")
                    .WithCustomSerializer(new TextJsonSerializer())
                    .Build();

            ModelTextJson model = new ModelTextJson()
            {
                TheIdentifier = Guid.NewGuid().ToString(),
                DescriptiveTitle = "With custom System.Text.Json serializer!"
            };

            Container container = client.GetContainer("OnDotNet", "episode1serializer");
            ItemResponse<ModelTextJson> createdItem = await container.CreateItemAsync(model);

            Console.WriteLine($"Used custom serializer to create item {createdItem.Resource.TheIdentifier}");
        }

        static async Task WithSerializerOptionsAsync(IConfiguration configuration)
        {
            CosmosClient client = new CosmosClientBuilder(configuration.GetConnectionString("Cosmos"))
                    .WithApplicationName("OnDotNetRocks")
                    .WithSerializerOptions(new CosmosSerializationOptions(){
                        IgnoreNullValues = true,
                        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                    })
                    .Build();

            ModelJsonNet model = new ModelJsonNet()
            {
                Id = Guid.NewGuid().ToString(),
                DescriptiveTitle = "With customized JSON.Net!"
            };

            Container container = client.GetContainer("OnDotNet", "episode1serializer");
            ItemResponse<ModelJsonNet> createdItem = await container.CreateItemAsync(model);

            Console.WriteLine($"Used serializer options to create item {createdItem.Resource.Id}");
        }

        static async Task InitializeContainers(IConfiguration configuration)
        {
            CosmosClient client = new CosmosClientBuilder(configuration.GetConnectionString("Cosmos"))
                .Build();
            Database database = await client.CreateDatabaseIfNotExistsAsync("OnDotNet");
            await database.CreateContainerIfNotExistsAsync("episode1serializer", "/id");
        }
    }
}
