using ChecklistManager.Models;
using Hangfire;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ChecklistManager
{
    // TODO: Logic is mostly done, just need automatic clearing of completed one-off tasks,
    // plus automatic resetting of scheduled tasks (easy to do with a daily cron job at midnight, or maybe a specified reset time to account
    // for significantly offsite servers)
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            string taskConnectionString = new SqliteConnectionStringBuilder(builder.Configuration.GetConnectionString("TaskDb"))
            {
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ConnectionString;

            using (var connection = new SqliteConnection(taskConnectionString))
            {
                connection.Open();
                var createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText = "CREATE TABLE IF NOT EXISTS Tasks (Id INTEGER PRIMARY KEY AUTOINCREMENT, Description TEXT NOT NULL, AssignedTo TEXT, State INTEGER NOT NULL, AssignmentLevel INTEGER NOT NULL, ScheduleString TEXT);";
                createTableCmd.ExecuteNonQuery();
            }

            builder.Services.AddDbContext<ChecklistTaskContext>(opt => opt.UseSqlite(taskConnectionString));
            builder.Services.AddHangfire(config => config.UseInMemoryStorage());

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
