using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.Util;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSwag;
using NSwag.Generation.Processors.Security;

namespace ImageRecognition.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            ConfigureDynamoDB();
        }

        public IConfiguration Configuration { get; private set; }

        private void ConfigureDynamoDB()
        {
            string value;
            if ((value = this.Configuration["AppOptions:TableAlbum"]) != null)
            {
                AWSConfigsDynamoDB.Context.AddMapping(new TypeMapping(typeof(Models.Album), value));
            }
            if ((value = this.Configuration["AppOptions:TablePhoto"]) != null)
            {
                AWSConfigsDynamoDB.Context.AddMapping(new TypeMapping(typeof(Models.Photo), value));
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppOptions>(Configuration.GetSection("AppOptions"));

            services.AddAWSService<Amazon.DynamoDBv2.IAmazonDynamoDB>();
            services.AddAWSService<Amazon.S3.IAmazonS3>();

            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

            services.AddResponseCompression(options =>
            {
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.EnableForHttps = true;
                options.MimeTypes = new[] { "application/json", "text/tab-separated-values", "application/javascript", "text/csv", "text" };
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var region = Configuration["AWS:Region"];
                    if (string.IsNullOrEmpty(region))
                    {
                        region = Amazon.Runtime.FallbackRegionFactory.GetRegionEndpoint().SystemName;
                    }

                    var audience = Configuration["AWS:UserPoolClientId"];
                    var authority = $"https://cognito-idp.{region}.amazonaws.com/" + Configuration["AWS:UserPoolId"];

                    Console.WriteLine($"Configure JWT option, Audience: {audience}, Authority: {authority}");

                    options.Audience = audience;
                    options.Authority = authority;
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy =>
                    policy.RequireClaim("cognito:groups", "Admin"));
            });

            services.AddControllers();

            // Register the Swagger services
            services.AddSwaggerDocument(document =>
            {
                // Add an authenticate button to Swagger for JWT tokens
                document.OperationProcessors.Add(new OperationSecurityScopeProcessor("JWT"));
                document.DocumentProcessors.Add(new SecurityDefinitionAppender("JWT", new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    Name = "Authorization",
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Description = "Type into the textbox: Bearer {your JWT token}. You can get a JWT token from /Authorization/Authenticate."
                }));

                // Post process the generated document
                document.PostProcess = d =>
                {
                    d.Info.Title = "ImageRecognition API";
                    d.Info.Description = "API to manage albums and photos.";
                    d.Info.Version = "1.0.0";
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseResponseCompression();

            app.UseHttpsRedirection();

            app.UseXRay("ImageRecognition.API");

            // Register the Swagger generator and the Swagger UI middlewares
            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Welcome to running ASP.NET Core on AWS Lambda");
                });
            });
        }
    }
}
