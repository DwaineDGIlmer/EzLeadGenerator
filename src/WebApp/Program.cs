using Core.Extensions;
using WebApp.Extensions;
using WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Initialize the logging services
builder.Services.InitializeServices(builder.Configuration);
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

// the first request to the application will trigger the loading of job sources and company profiles
// Load the job source service and update job sources and company profiles
await DataLoadService.LoadAppSourceService(app);

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

app.Run();
