using IceCreams.Services;
using Users.Services;
using MyMiddleware;
// using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Logging.ClearProviders();//log4net seriLog
builder.Logging.AddConsole(); //console
// Add services to the container.
builder.Services.AddIceCreamService();
builder.Services.AddUserService();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle



// builder.Logging.SetMinimumLevel(LogLevel.Debug);
// builder.Logging.AddProvider(new FileLoggerProvider("log.txt"));

var app = builder.Build();
app.UseMyLogMiddleware();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseDefaultFiles();
app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
