﻿//*******************************
// Thx https://github.com/aspnet/Entropy/tree/master/samples/Mvc.RenderViewToString
//*******************************
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.WebEncoders;
using SmartCode.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace SmartCode.TemplateEngine.Impl
{
    public class OfficialRazorTemplateEngine : ITemplateEngine
    {
        public bool Initialized { get; private set; }
        public string Name { get; private set; } = "OfficialRazor";
        private string _root = AppPath.Relative("OfficialRazorTemplates");
        private IServiceScopeFactory _scopeFactory;
        public void Initialize(IDictionary<string, string> paramters)
        {
            Initialized = true;
            if (paramters != null)
            {
                if (paramters.TryGetValue("Name", out string name))
                {
                    Name = name;
                }
                if (paramters.TryGetValue("Root", out string root))
                {
                    _root = root;
                }
            }
            InitializeServices();
        }

        public Task<string> Render(BuildContext context)
        {
            using (var serviceScope = _scopeFactory.CreateScope())
            {
                var helper = serviceScope.ServiceProvider.GetRequiredService<OfficialRazorViewToStringRenderer>();
                return helper.RenderViewToStringAsync(context.Build.Template, context);
            }
        }

        private void InitializeServices()
        {
            var services = ConfigureDefaultServices();
            var serviceProvider = services.BuildServiceProvider();
            _scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        }
        private IServiceCollection ConfigureDefaultServices()
        {
            var services = new ServiceCollection();
            IFileProvider fileProvider = new PhysicalFileProvider(_root);
            services.AddSingleton<IHostingEnvironment>(new HostingEnvironment
            {
                ApplicationName = Assembly.GetEntryAssembly().GetName().Name,
                WebRootFileProvider = fileProvider,
            });
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Clear();
                options.FileProviders.Add(fileProvider);
            });
            var diagnosticSource = new DiagnosticListener("Microsoft.AspNetCore");
            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            services.AddSingleton<DiagnosticSource>(diagnosticSource);
            services.AddLogging();
            services.AddMvc();
            services.AddTransient<OfficialRazorViewToStringRenderer>();
            return services;
        }
    }
}
