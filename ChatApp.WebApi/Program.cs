using ChatApp.Application;
using ChatApp.Infrastructure;
using ChatApp.WebApi.Hubs;
using ChatApp.WebApi.Extensions;
using Swashbuckle.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. Kích hoạt DI của tầng Application
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Đăng ký SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Chạy Seeder để tạo dữ liệu ảo
app.SeedDatabase();

// Cho phép load file tĩnh (index.html)
app.UseStaticFiles();

app.UseAuthorization();
app.MapControllers();

// Map SignalR Hub
app.MapHub<ChatHub>("/chathub");

app.Run();