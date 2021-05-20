using Amazon;
using Amazon.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageRecognition.Frontend
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            ConfigureDynamoDB();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppOptions>(Configuration.GetSection("AppOptions"));

            services.AddAWSService<Amazon.DynamoDBv2.IAmazonDynamoDB>();
            services.AddAWSService<Amazon.S3.IAmazonS3>();
            services.AddAWSService<Amazon.StepFunctions.IAmazonStepFunctions>();

            services.AddSingleton<ImageRecognitionManager>();

            //services.AddControllersWithViews();
            services.AddRazorPages();
            
            services.AddAWSService<Amazon.SimpleSystemsManagement.IAmazonSimpleSystemsManagement>();
            // This is the usual code we'd see to use Sql Server for our user
            // authentication. Instead, we've changed the application to use
            // an Amazon Cognito User Pool

            //services.AddDbContext<ApplicationDbContext>(options =>
            //    options.UseSqlServer(
            //        Configuration.GetConnectionString("DefaultConnection")));
            //services.AddDatabaseDeveloperPageExceptionFilter();

            //services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            //    .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddCognitoIdentity();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = new PathString("/Identity/Account/Login");
            });

            services.AddDataProtection().PersistKeysToAWSSystemsManager("/ImageRecognition/DataProtection");

            
        }

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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
