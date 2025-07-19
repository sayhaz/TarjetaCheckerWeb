
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TarjetaCheckerWeb; // 👈 Necesario para usar StatusHub

var builder = WebApplication.CreateBuilder(args);

// Servicios: Razor Pages, Controllers, SignalR
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddSignalR();

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
app.MapControllers();

// Registrar el HUB SignalR en /statushub
app.MapHub<StatusHub>("/statushub");

app.Run();
