﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MinConSys.Core.Interfaces.Repository;
using MinConSys.Core.Interfaces.Services;
using MinConSys.Core.Services;
using MinConSys.Infrastructure.Data;
using MinConSys.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinConSys.DI
{
    public static class DependencyInjection
    {
        public static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Configuración de appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Registrar configuración
            services.AddSingleton<IConfiguration>(configuration);

            // Registrar fábrica de conexiones
            services.AddSingleton<ConnectionFactory>(sp =>
                new ConnectionFactory(configuration.GetConnectionString("DefaultConnection")));

            // Registrar repositorios
            services.AddScoped<ILoginRepository, LoginRepository>();
            services.AddScoped<IPersonaRepository, PersonaRepository>();
            // Más repositorios...

            // Registrar servicios de negocio
            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<IPersonaService, PersonaService>();
            // Más servicios...

            return services.BuildServiceProvider();
        }
    }
}
