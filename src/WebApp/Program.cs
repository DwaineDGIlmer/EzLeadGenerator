using Core.Extensions;
using WebApp.Extensions;
using WebApp.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Initialize the logging services
builder.Services.InitializeServices(builder.Configuration);
// Configure the application settings first
builder.Services.ConfigureSerpApiSettings(builder.Configuration);
builder.Services.AddResilientHttpClient(builder.Configuration, nameof(EzLeadGenerator));
builder.Services.AddJobsRetrivalService(builder.Configuration);
builder.Services.AddCacheService(builder.Configuration);
builder.Services.AddCompanyProfileStore(builder.Configuration);
builder.Services.AddJobsProfileStore(builder.Configuration);
builder.Services.AddSearchService(builder.Configuration);
builder.Services.AddDisplayRepository();
builder.Services.AddJobSourceService();

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.UseMiddleware<JobServicesMiddleware>();

app.Run();
