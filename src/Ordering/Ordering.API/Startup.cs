using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Ordering.Application.Handlers;
using Ordering.Core.Repositories.Base;
using Ordering.Infrastructure.Data;
using Ordering.Infrastructure.Repositories;
using Ordering.Infrastructure.Repositories.Base;
using EventBusRabbitMQ;
using EventBusRabbitMQ.Procuder;
using Ordering.API.Extensions;
using Ordering.API.RabbitMQ;
using RabbitMQ.Client;

namespace Ordering.API
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

            services.AddControllers();

            services.AddDbContext<OrderContext>(c =>
                c.UseSqlServer(Configuration.GetConnectionString("OrderConnection")),ServiceLifetime.Singleton);

             services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
             services.AddScoped(typeof(IOrderRepository), typeof(OrderRepository));
            services.AddTransient<IOrderRepository, OrderRepository>();

            services.AddAutoMapper(typeof(Startup));

            services.AddMediatR(typeof(CheckoutOrderHandler).GetTypeInfo().Assembly);


            services.AddSingleton<IRabbitMQConnection>(sp =>
             {
                 var factory = new ConnectionFactory()
                 {
                     HostName = Configuration["EventBus:HostName"]
                 };
                 if (!string.IsNullOrEmpty(Configuration["EventBus:UserName"]))
                 {
                     factory.UserName = Configuration["EventBus:UserName"];
                 }

                 if (!string.IsNullOrEmpty(Configuration["EventBus:Password"]))
                 {
                     factory.UserName = Configuration["EventBus:Password"];
                 }
                 return new RabbitMQConnection(factory);
             });

             services.AddSingleton<EventBusRabbitMQConsumer>();
            #region Swagger Dependencies

            services.AddSwaggerGen(c =>
             {
                 c.SwaggerDoc("v1", new OpenApiInfo { Title = "Order API", Version = "v1" });
             });

             #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseRabbitMQListener();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order API V1");
            });
        }
    }
}
