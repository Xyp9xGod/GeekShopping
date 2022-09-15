using GeekShopping.IdentityServer.Configuration;
using GeekShopping.IdentityServer.Initializer;
using GeekShopping.IdentityServer.Model;
using GeekShopping.IdentityServer.Model.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add database connection
var connection = builder.Configuration["MySQLConnection:MySqlConnectionString"];

builder.Services.AddDbContext<MySQLContext>(options => options.
    UseMySql(connection, new MySqlServerVersion(new Version(8, 0, 5))));

// Add identity server
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<MySQLContext>()
    .AddDefaultTokenProviders();

// Add builder identity server
builder.Services.AddIdentityServer(options => 
    {
        options.Events.RaiseErrorEvents = true;
        options.Events.RaiseInformationEvents = true;
        options.Events.RaiseFailureEvents = true;
        options.Events.RaiseSuccessEvents = true;
        options.EmitStaticAudienceClaim = true;
    }).AddInMemoryIdentityResources(IdentityConfiguration.IdentityResources)
        .AddInMemoryApiScopes(IdentityConfiguration.ApiScopes)
        .AddInMemoryClients(IdentityConfiguration.Clients)
        .AddAspNetIdentity<ApplicationUser>()
        .AddDeveloperSigningCredential();

// Dependence Inject to Initialzer Step 1
builder.Services.AddScoped<IDBInitializer, DBInitializer>();


// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Dependence Inject to Initialzer Step 2
var dbInitializerService = app.Services.CreateScope().ServiceProvider.GetService<IDBInitializer>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add IdentityServer
app.UseIdentityServer();

app.UseAuthorization();

// Dependence Inject to Initialzer Step 3
dbInitializerService.Initialize();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
