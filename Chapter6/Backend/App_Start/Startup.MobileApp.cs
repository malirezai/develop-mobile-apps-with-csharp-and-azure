﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Web.Http;
using Backend.DataObjects;
using Backend.Models;
using Microsoft.Azure.Mobile.Server.Authentication;
using Microsoft.Azure.Mobile.Server.Config;
using Owin;

namespace Backend
{
    public partial class Startup
    {
        public static void ConfigureMobileApp(IAppBuilder app)
        {
            var config = new HttpConfiguration();

            new MobileAppConfiguration()
                .AddTablesWithEntityFramework()
                .UseAppServiceAuthentication()
                .ApplyTo(config);

            config.MapHttpAttributeRoutes();

            var migrator = new DbMigrator(new Migrations.Configuration());
            migrator.Update();

            var settings = config.GetMobileAppSettingsProvider().GetMobileAppSettings();
            if (string.IsNullOrEmpty(settings.HostName))
            {
                app.UseAppServiceAuthentication(new AppServiceAuthenticationOptions
                {
                    SigningKey = ConfigurationManager.AppSettings["SigningKey"],
                    ValidAudiences = new[] { ConfigurationManager.AppSettings["ValidAudience"] },
                    ValidIssuers = new[] { ConfigurationManager.AppSettings["ValidIssuer"] },
                    TokenHandler = config.GetAppServiceTokenHandler()
                });
            }

            app.UseWebApi(config);
        }
    }

    public class MobileServiceInitializer : CreateDatabaseIfNotExists<MobileServiceContext>
    {
        protected override void Seed(MobileServiceContext context)
        {
            List<TodoItem> todoItems = new List<TodoItem>
            {
                new TodoItem { Id = Guid.NewGuid().ToString(), Text = "First Item", Complete = false },
                new TodoItem { Id = Guid.NewGuid().ToString(), Text = "Second Item", Complete = false }
            };

            foreach (TodoItem item in todoItems)
            {
                context.Set<TodoItem>().Add(item);
            }

            base.Seed(context);
        }
    }
}