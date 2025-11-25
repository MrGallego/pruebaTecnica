
using Ascensores.Data.Helper;
using Ascensores.Services;
using Oracle.ManagedDataAccess.Client;

namespace Ascensores
{
    public class Program
    {
        public static void Main(string[] args)
        {

      

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular",
                    policy =>
                    {
                        policy
                            .AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });

            string conn =  builder.Configuration.GetConnectionString("OraclePDB2");
            builder.Services.AddSingleton(new OracleHelper(conn));
            // Register AscensorService as singleton and hosted service
            builder.Services.AddSingleton<AscensorService>();
            builder.Services.AddHostedService(provider => provider.GetRequiredService<AscensorService>());

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowAngular");

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
