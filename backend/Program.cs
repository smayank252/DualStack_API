using Azure.Storage.Blobs;
using Azure.Data.Tables;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Load Configuration
var config = builder.Configuration;

// Get Azure Storage Connection String
var storageConnectionString = config["AzureStorage:ConnectionString"];
if (string.IsNullOrEmpty(storageConnectionString))
{
    throw new InvalidOperationException("Azure Storage connection string is missing. Check appsettings.json.");
}

// Register Azure Storage Clients
builder.Services.AddSingleton(new BlobServiceClient(storageConnectionString));
builder.Services.AddSingleton(new TableServiceClient(storageConnectionString));

// ** Add Application Insights **
var appInsightsConnectionString = config["ApplicationInsights:ConnectionString"];
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = appInsightsConnectionString;
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Default route message
app.MapGet("/", () => "Welcome to FormApp API! Use /api/form to access the API.");

// Set Kestrel to listen on port 5000
var port = config["Kestrel:Endpoints:Http:Url"] ?? "http://localhost:5000";
app.Urls.Add(port);

app.Run();
