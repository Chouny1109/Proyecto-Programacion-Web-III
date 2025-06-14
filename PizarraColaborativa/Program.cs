using Entidades.EF;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PizarraColaborativa.Hubs;
using Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<ITrazoMemoryService,TrazoMemoryService>();
builder.Services.AddSingleton<ITextoMemoryService,TextoMemoryService>();
builder.Services.AddSingleton<IAccionMemoryService,AccionMemoryService>();

builder.Services.AddHostedService<PizarraPersistenceService>();

builder.Services.AddSignalR();
builder.Services.AddScoped<IPizarraService, PizarraService>();
builder.Services.AddScoped<IInvitacionService, InvitacionService>();

builder.Services.AddDbContext<ProyectoPizarraContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Connection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 4;
    options.Password.RequireNonAlphanumeric = false; 
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ProyectoPizarraContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Cuenta/Login";
    options.AccessDeniedPath = "/Cuenta/AccesoDenegado";
});

var app = builder.Build();

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
app.UseAuthentication();

app.UseAuthorization();

app.MapHub<DibujoHub>("/dibujohub");
app.MapHub<ChatHub>("/chathub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
