using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Azure Key Vault configuration whenever KeyVaultName is set (local dev uses appsettings, Azure uses Key Vault)
var keyVaultName = builder.Configuration["KeyVaultName"];
if (!string.IsNullOrEmpty(keyVaultName))
{
    var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
    builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());
}

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure routing to use lowercase URLs
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

// Add Entity Framework
builder.Services.AddDbContext<TaskManagerDbContext>(options =>
{
    // Prefer Key Vault secret name; fall back to standard connection string config
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? builder.Configuration["DatabaseConnectionString"];
    options.UseSqlServer(connectionString);
});

// Add application services
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IUserInvitationService, UserInvitationService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            "https://localhost:3000",
            "http://localhost:3000",
            "https://*.azurestaticapps.net"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthorization();
app.MapControllers();

// Apply migrations and seed initial data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TaskManagerDbContext>();
    context.Database.Migrate();
    await DataSeeder.SeedAsync(context);
}

app.Run();
