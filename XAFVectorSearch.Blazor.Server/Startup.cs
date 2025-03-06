using Azure;
using Azure.AI.OpenAI;
using DevExpress.AIIntegration;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.Services;
using DevExpress.ExpressApp.WebApi.Services;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.OpenApi.Models;
using Microsoft.SemanticKernel;
using System.Net.Mime;
using System.Text.Json.Serialization;
using XAFVectorSearch.Blazor.Server.ContentDecoders;
using XAFVectorSearch.Blazor.Server.Helpers;
using XAFVectorSearch.Blazor.Server.Services;
using XAFVectorSearch.Blazor.Server.Settings;

namespace XAFVectorSearch.Blazor.Server;

public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(typeof(Microsoft.AspNetCore.SignalR.HubConnectionHandler<>), typeof(ProxyHubConnectionHandler<>));

        services.AddRazorPages();
        services.AddServerSideBlazor();
        services.AddHttpContextAccessor();
        services.AddScoped<CircuitHandler, CircuitHandlerProxy>();



        services.AddXaf(Configuration, builder =>
        {
            builder.Services.AddDevExpressBlazor();

            AppSettings appSettings = null;
            AzureOpenAISettings aiSettings = null;
            if (!ModuleHelper.IsDesignMode)
            {
                appSettings = builder.Services.ConfigureAndGet<AppSettings>(Configuration, nameof(AppSettings))!;
                aiSettings = builder.Services.ConfigureAndGet<AzureOpenAISettings>(Configuration, "AzureOpenAI")!;

                var azureOpenAIClient = new AzureOpenAIClient(
            new Uri(aiSettings.ChatCompletion.Endpoint),
            new AzureKeyCredential(aiSettings.ChatCompletion.ApiKey));
                builder.Services.AddChatClient(azureOpenAIClient?.AsChatClient(aiSettings?.ChatCompletion?.ModelId));
                builder.Services.AddDevExpressAI((config) =>
                {
                    config.RegisterOpenAIAssistants(
                           azureOpenAIClient, aiSettings?.ChatCompletion?.ModelId);
                });
            }





            builder.UseApplication<XAFVectorSearchBlazorApplication>();

            builder.AddXafWebApi(webApiBuilder =>
            {
                webApiBuilder.ConfigureOptions(options =>
                {
                    // Make your business objects available in the Web API and generate the GET, POST, PUT, and DELETE HTTP methods for it.
                    // options.BusinessObject<YourBusinessObject>();
                });
            });

            builder.Modules
                .AddConditionalAppearance()
                .AddFileAttachments()
                .AddValidation(options =>
                {
                    options.AllowValidationDetailsAccess = false;
                })
                .Add<Module.XAFVectorSearchModule>()
                .Add<XAFVectorSearchBlazorModule>();
            builder.ObjectSpaceProviders
                .AddEFCore(options => options.PreFetchReferenceProperties())
                    .WithDbContext<Module.BusinessObjects.XAFVectorSearchDBContext>((serviceProvider, options) =>
                    {
                        // Uncomment this code to use an in-memory database. This database is recreated each time the server starts. With the in-memory database, you don't need to make a migration when the data model is changed.
                        // Do not use this code in production environment to avoid data loss.
                        // We recommend that you refer to the following help topic before you use an in-memory database: https://docs.microsoft.com/en-us/ef/core/testing/in-memory
                        //options.UseInMemoryDatabase("InMemory");
                        string connectionString = null;
                        if (!ModuleHelper.IsDesignMode)
                        {

                            if (Configuration.GetConnectionString("ConnectionString") != null)
                            {
                                connectionString = Configuration.GetConnectionString("ConnectionString");
                            }

                            ArgumentNullException.ThrowIfNull(connectionString);
                            options.UseSqlServer(connectionString, options => options.UseVectorSearch());

                        }
#if EASYTEST
                        if(Configuration.GetConnectionString("EasyTestConnectionString") != null) {
                            connectionString = Configuration.GetConnectionString("EasyTestConnectionString");
                        }
#endif

                        options.UseChangeTrackingProxies();
                        options.UseObjectSpaceLinkProxies();
                        options.UseLazyLoadingProxies();
                    })
                .AddNonPersistent();

#pragma warning disable EXTEXP0018 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            if (!ModuleHelper.IsDesignMode)
            {
                builder.Services.AddHybridCache(options =>
                {

                    options.DefaultEntryOptions = new()
                    {
                        LocalCacheExpiration = appSettings?.MessageExpiration
                    };

                });
            }
#pragma warning restore EXTEXP0018 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            builder.Services.ConfigureHttpClientDefaults(builder =>
            {
                builder.AddStandardResilienceHandler();
            });

            // Semantic Kernel is used to generate embeddings and to reformulate questions taking into account all the previous interactions,
            // so that embeddings themselves can be generated more accurately.

            //builder.Services.AddSingleton(TimeProvider.System);

#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            if (!ModuleHelper.IsDesignMode)
            {
                builder.Services.AddKernel()
                .AddAzureOpenAITextEmbeddingGeneration(aiSettings?.Embedding?.Deployment, aiSettings?.Embedding?.Endpoint, aiSettings?.Embedding?.ApiKey, dimensions: aiSettings?.Embedding?.Dimensions)
                .AddAzureOpenAIChatCompletion(aiSettings?.ChatCompletion?.Deployment, aiSettings?.ChatCompletion?.Endpoint, aiSettings?.ChatCompletion?.ApiKey);
            }
#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            builder.Services.AddSingleton<TokenizerService>();
            builder.Services.AddSingleton<ChatService>();
            builder.Services.AddScoped<VectorSearchService>();

            builder.Services.AddKeyedSingleton<IContentDecoder, PdfContentDecoder>(MediaTypeNames.Application.Pdf);
            builder.Services.AddKeyedSingleton<IContentDecoder, DocxContentDecoder>("application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            builder.Services.AddKeyedSingleton<IContentDecoder, TextContentDecoder>(MediaTypeNames.Text.Plain);

            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
        });

        services
            .AddControllers()
            .AddOData((options, serviceProvider) =>
            {
                options
                    .AddRouteComponents("api/odata", new EdmModelBuilder(serviceProvider).GetEdmModel())
                    .EnableQueryFeatures(100);
            });

        services.AddSwaggerGen(c =>
        {
            c.EnableAnnotations();
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "XAFVectorSearch API",
                Version = "v1",
                Description = @"Use AddXafWebApi(options) in the XAFVectorSearch.Blazor.Server\Startup.cs file to make Business Objects available in the Web API."
            });
        });

        services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(o =>
        {
            //The code below specifies that the naming of properties in an object serialized to JSON must always exactly match
            //the property names within the corresponding CLR type so that the property names are displayed correctly in the Swagger UI.
            //XPO is case-sensitive and requires this setting so that the example request data displayed by Swagger is always valid.
            //Comment this code out to revert to the default behavior.
            //See the following article for more information: https://learn.microsoft.com/en-us/dotnet/api/system.text.json.jsonserializeroptions.propertynamingpolicy
            o.JsonSerializerOptions.PropertyNamingPolicy = null;
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "XAFVectorSearch WebApi v1");
            });
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. To change this for production scenarios, see: https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        app.UseHttpsRedirection();
        app.UseRequestLocalization();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseXaf();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapXafEndpoints();
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
            endpoints.MapControllers();
        });
    }
}
