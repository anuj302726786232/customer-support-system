
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SupportDeskAPI.Auth;
using SupportDeskAPI.Context;
using SupportDeskAPI.Services;

namespace SupportDeskAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddScoped<SupportDeskService>();

            builder.Services.AddSwaggerGen(c =>
            {
                // Define Basic Auth scheme
                c.AddSecurityDefinition("basic", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "basic",
                    In = ParameterLocation.Header,
                    Description = "Basic Authentication header using username and password"
                });

                // Apply security globally to all endpoints
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "basic" }
                        },
                        new string[] {}
                    }
                });
            });

            builder.Services.AddAuthentication("BasicAuthentication")
                .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

            var dbConnection = builder.Configuration.GetConnectionString("DbConnection");
            builder.Services.AddDbContext<SupportDbContext>(options =>
            {
                options.UseMySql(dbConnection, ServerVersion.AutoDetect(dbConnection));
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
