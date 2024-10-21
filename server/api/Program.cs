using dataAccess;
using System.Text.Json.Serialization;
using dataAccess.interfaces;
using dataAccess.Models;
using Microsoft.EntityFrameworkCore;
using service.Services;
using _service;
using _service.Validators;
using dataAccess.Repositories;
using FluentValidation;
using service.Validators;
using api.Middleware;
using Npgsql;

public class Program
{
    public static void Main(string[] args)
    {
        
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddControllers().AddJsonOptions(opt =>
        {
            opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });
        builder.Services.AddProblemDetails();
        builder.Services.AddOpenApiDocument(configure =>
        {
            configure.Title = "Lobster paper Shop";
         
        });
        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddScoped<IOrder, OrderRepository>();
        builder.Services.AddScoped<ITrait, TraitRepository>();
        builder.Services.AddScoped<ITraitService, TraitService>();
        builder.Services.AddScoped<IPaper, PaperRepository>();
        builder.Services.AddScoped<IPaperService, PaperService>();
        builder.Services.AddScoped<ICustomerService, CustomerService>();


        builder.Services.AddValidatorsFromAssemblyContaining<CreatePaperValidator>();
        builder.Services.AddValidatorsFromAssemblyContaining<UpdatePaperValidator>();
        builder.Services.AddValidatorsFromAssemblyContaining<OrderDtoValidator>();
        builder.Services.AddValidatorsFromAssemblyContaining<TraitDtoValidator>();

        builder.Services.AddDbContext<MyDbContext>(options =>
        {

            // Google Cloud SQL miljø variabler
            var cloudSqlConnectionName = Environment.GetEnvironmentVariable("CLOUD_SQL_CONNECTION_NAME");
            var dbUser = Environment.GetEnvironmentVariable("DB_USER");
            var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
            var dbName = Environment.GetEnvironmentVariable("DB_NAME");
    
            // Lav Cloud SQL connection string
            var cloudSqlConnectionString = !string.IsNullOrEmpty(cloudSqlConnectionName) && 
                                           !string.IsNullOrEmpty(dbUser) && 
                                           !string.IsNullOrEmpty(dbPassword) && 
                                           !string.IsNullOrEmpty(dbName)
                ? new NpgsqlConnectionStringBuilder
                {
                    Host = $"/cloudsql/{cloudSqlConnectionName}",  // Cloud SQL socket
                    Username = dbUser,
                    Password = dbPassword,
                    Database = dbName,
                    SslMode = SslMode.Disable
                }.ToString()
                : null;
    
            var connectionString = cloudSqlConnectionString ?? builder.Configuration.GetConnectionString("MyDbConn");
    
            options.UseNpgsql(connectionString);
        });


        var app = builder.Build();


      

		app.UseMiddleware<RequestLoggingMiddleware>();

        app.MapControllers();
        app.UseOpenApi();
        app.UseSwaggerUi();

        app.UseCors(opts => {
            opts.AllowAnyOrigin();
            opts.AllowAnyMethod();
            opts.AllowAnyHeader();
        });



       var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
		var url = $"http://0.0.0.0:{port}";
		app.Run(url);
    }
}

