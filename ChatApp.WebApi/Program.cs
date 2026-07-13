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

// Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        policy.SetIsOriginAllowed(_ => true) // Cho phép mọi origin lúc dev
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Cấu hình Cookie Auth
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.None;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
    });

// Đăng ký SignalR
builder.Services.AddSignalR();

var app = builder.Build();

app.UseMiddleware<ChatApp.WebApi.Middleware.ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Chạy Seeder để tạo dữ liệu ảo
app.SeedDatabase();

app.UseCors("AllowClient");

// Cho phép load file tĩnh (index.html)
app.UseStaticFiles();
app.UseBlazorFrameworkFiles();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Map SignalR Hub
app.MapHub<ChatHub>("/chathub");

app.MapFallbackToFile("index.html");

app.Run();