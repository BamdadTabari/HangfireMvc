using Hangfire.SqlServer;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Data.SqlClient;
using System.Configuration;

namespace HangfireMvc
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Register all areas, filters, routes, and bundles
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Get connection string from web.config
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Connection string 'DefaultConnection' not found in configuration.");
            }

            // Query to create the database if it doesn't exist
            string createDatabaseQuery = @"
                IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'HangfireDBMVC')
                BEGIN
                    CREATE DATABASE HangfireDBMVC;
                END";

            try
            {
                // Connect to SQL Server master database
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open(); // Open the connection to SQL Server
                    Console.WriteLine("Connected to SQL Server successfully.");

                    // Execute the database creation query
                    using (SqlCommand command = new SqlCommand(createDatabaseQuery, connection))
                    {
                        command.ExecuteNonQuery();
                        Console.WriteLine("Database 'HangfireDBMVC' created successfully (if it didn't already exist).");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while creating the database: " + ex.Message);
                throw; // Optionally re-throw the exception to stop the application
            }

            // Configure Hangfire with the connection string
            GlobalConfiguration.Configuration
                .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                {
                    PrepareSchemaIfNecessary = true // Automatically create Hangfire schema if needed
                });

            // Start the Hangfire server with options
            var options = new BackgroundJobServerOptions
            {
                WorkerCount = Environment.ProcessorCount * 2 // Adjust worker count as per your need
            };
            new BackgroundJobServer(options);

            // Register the Hangfire Dashboard at "/hangfire"
            RouteTable.Routes.MapOwinPath("/hangfire", app => app.UseHangfireDashboard());
        }
    }
}
