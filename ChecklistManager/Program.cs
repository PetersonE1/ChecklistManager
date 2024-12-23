using ChecklistManager.Models;
using Microsoft.EntityFrameworkCore;

namespace ChecklistManager
{
    // TODO: Logic is mostly done, just need an actual (not in-memory) database and automatic clearing of completed one-off tasks,
    // plus automatic resetting of scheduled tasks (easy to do with a daily cron job at midnight, or maybe a specified reset time to account
    // for significantly offsite servers)
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddDbContext<ChecklistTaskContext>(opt => opt.UseInMemoryDatabase("TaskList"));
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

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
