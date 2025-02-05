using Microsoft.AspNetCore.ResponseCompression;
using StockAdvantage.Client.Pages;
using StockAdvantage.Components;
using StockAdvantage.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();


// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddSingleton<StockService>();

builder.Services.AddSignalR();

builder.Services.AddControllers();



builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        [ "application/octet-stream" ]);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseResponseCompression();
app.UseAntiforgery();
app.MapControllers();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(StockAdvantage.Client._Imports).Assembly);
app.MapHub<StockHub>("/stockhub");
app.Run();
