using Core.Extensions;
using Loggers.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Initialize the logging services
builder.Services.InitializeServices(builder.Configuration);
builder.Services.InitializeLogging(builder.Configuration);
builder.Services.AddResilientHttpClient(builder.Configuration, nameof(EzLeadGenerator));

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
