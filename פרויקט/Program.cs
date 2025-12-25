using IceCreams.Services;
using Users.Services;
using MyMiddleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Token.Services;
using Microsoft.OpenApi.Models; // לצורך הגדרת SecurityScheme ב-Swagger
// using Microsoft.Extensions.Logging; 

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// הגדרת Swagger עם תמיכה ב-Authorization Header (Authorize button)
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "הדבק כאן: Bearer <token>\n(ניתן להעתיק את הטוקן שקיבלת מ-/User/Login)"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Logging.ClearProviders();//log4net seriLog
builder.Logging.AddConsole(); //console
builder.Services.AddIceCreamService();
builder.Services.AddUserService();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = TokenService.GetTokenValidationParameters();
    });

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
