﻿using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using TechnicalSupportService.Business;
using TechnicalSupportService.Services;

namespace TechnicalSupportService.App_Start
{
    public class AutofacConfig
    {
        public static void Register( HttpConfiguration configuration)
        {
            var builder = new ContainerBuilder();
            builder. RegisterApiControllers(Assembly.GetExecutingAssembly());
            builder.RegisterWebApiFilterProvider(configuration);
            ConfigureDependencies(builder);
            var container = builder.Build();
            configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }

        public static void ConfigureDependencies(ContainerBuilder builder)
        {
            //RegisterModules
            builder.RegisterModule(new AutofacBusinessModule());
        }
    }

    public class AutofacBusinessModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder.Register(x => new RequestOperations()).As<IRequestOperations>().SingleInstance();
            builder.Register(x => new QueueRequestsHandler()).As<IQueueRequestsHandler>().SingleInstance();
        }
    }
}