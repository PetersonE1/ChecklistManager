using ChecklistManager.Models;
using Hangfire;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ChecklistManager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // TEMP DEBUG
            string dir = Directory.GetCurrentDirectory();
            string[] dirs = Directory.GetDirectories(Directory.GetCurrentDirectory());
            Console.WriteLine(dir);
            foreach (string d in dirs)
            {
                Console.WriteLine(d);
            }

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
                createTableCmd.CommandText = "CREATE TABLE IF NOT EXISTS Tasks (Id INTEGER PRIMARY KEY AUTOINCREMENT, Description TEXT NOT NULL, AssignedTo TEXT, DoneBy TEXT, State INTEGER NOT NULL, AssignmentLevel INTEGER NOT NULL, ScheduleString TEXT, HighPriority INTEGER NOT NULL);";
                createTableCmd.ExecuteNonQuery();

                createTableCmd.CommandText = "CREATE TABLE IF NOT EXISTS Members (Name TEXT PRIMARY KEY, Score INTEGER NOT NULL);";
                createTableCmd.ExecuteNonQuery();
            }

            builder.Services.AddDbContext<ChecklistTaskContext>(opt => opt.UseSqlite(taskConnectionString));
            builder.Services.AddDbContext<MemberContext>(opt => opt.UseSqlite(taskConnectionString));
            builder.Services.AddHangfire(config => config.UseInMemoryStorage());
            builder.Services.AddHangfireServer();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            app.UseSwagger();
            app.UseSwaggerUI();
            //}

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.UseHangfireDashboard();

            RecurringJobOptions jobOptions = new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Local
            };
            RecurringJob.AddOrUpdate<ChecklistTaskContext>("daily_reset", call => call.DailyReset(), "0 0 * * *", jobOptions);

            app.Run();
        }
    }
}
