using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Connection
builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// 2. Register Services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<LocalFileService>();

// 3. Session Setup
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".RaceDay.Session";
});

// 4. Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseSession();
app.UseStaticFiles(); // THIS ALLOWS IMAGES IN WWWROOT TO SHOW
app.UseAuthorization();
app.MapControllers();

app.Run();