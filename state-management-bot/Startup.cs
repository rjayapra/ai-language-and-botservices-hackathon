// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with EchoBot .NET Template version v4.17.1

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.BotBuilderSamples.Bots;
using Microsoft.BotBuilderSamples.Dialogs;


namespace Microsoft.BotBuilderSamples
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient().AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.MaxDepth = HttpHelper.BotMessageSerializerSettings.MaxDepth;
            });

            // Create the Bot Framework Authentication to be used with the Bot Adapter.
            services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

            // Create the Bot Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            /* JSON SERIALIZER - Uncomment the code in this section to use a JsonSerializer with a custom SerializationBinder configuration. */
            // Note: the AllowedTypesSerializationBinder limits the objects the storage is able to read and write, by providing a list of types used to allow or deny. It can be used to increase security.

            /* var jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings
             {
                  TypeNameHandling = TypeNameHandling.All,
                  MaxDepth = null,
                  SerializationBinder = new AllowedTypesSerializationBinder(
                      new List<Type>
                      {
                          typeof(Dictionary<string, object>),
                          typeof(ConversationData),
                          typeof(UserProfile)
                      })
             });
             */
            /* JSON SERIALIZER - Uncomment the code in this section to use a JsonSerializer with a custom SerializationBinder configuration. */

            // Create the storage we'll be using for User and Conversation state.

            // Option 1 : (Memory is great for testing purposes - examples of implementing storage with
            // Azure Blob Storage or Cosmos DB are below).
            //var storage = new MemoryStorage();

            /* Option 2: AZURE BLOB STORAGE - Uncomment the code in this section to use Azure blob storage */
            //Read configuration settings BlobStorageConnectionString
            var blobStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=mlwksp4949708036;AccountKey=COedqZQYGXny9saNgkxJ5ufLFJo2i3GkWaKjUZ9oClJE5Oaj1Ou6vk0MCY3ZvLh/Q8uQLZd3SHtD+ASt6p3m0Q==;EndpointSuffix=core.windows.net";//Configuration.GetSection("BlobStorageConnectionString")?.Value;
            var blobStorageContainerName = "bot-state-store";//Configuration.GetSection("BlobStorageContainerName")?.Value;
                        
            var storage = new BlobsStorage(blobStorageConnectionString, blobStorageContainerName);

            // Option 2a: With a custom JSON SERIALIZER, use this instead.
            // var storage = new BlobsStorage("<blob-storage-connection-string>", "bot-state", jsonSerializer);

            /* END AZURE BLOB STORAGE */

            /* Option 3: COSMOSDB STORAGE - Uncomment the code in this section to use CosmosDB storage */

            /* var cosmosDbStorageOptions = new CosmosDbPartitionedStorageOptions()
             {
                 CosmosDbEndpoint = "<endpoint-for-your-cosmosdb-instance>",
                 AuthKey = "<your-cosmosdb-auth-key>",
                 DatabaseId = "<your-database-id>",
                 ContainerId = "<cosmosdb-container-id>"
             };

             var storage = new CosmosDbPartitionedStorage(cosmosDbStorageOptions);
            */
            // Option 3a: With a custom JSON SERIALIZER, use this instead.
            // var storage = new CosmosDbPartitionedStorage(cosmosDbStorageOptions, jsonSerializer);

            /* END COSMOSDB STORAGE */

            // Create the User state passing in the storage layer.
            var userState = new UserState(storage);
            services.AddSingleton(userState);

            // Create the Conversation state passing in the storage layer.
            var conversationState = new ConversationState(storage);
            services.AddSingleton(conversationState);

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            // services.AddTransient<IBot, BlobStateManagementBot>();

            services.AddSingleton<UserProfileDialog>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, DialogBot<UserProfileDialog>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }
    }
}
