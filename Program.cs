using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using api.Data;
using api.Helpers;
using api.Interfaces;
using api.Models;
using api.Profiles;
using api.Reposetory;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args); 

// add services to container
   builder.Services.Configure<CloudenarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
             builder.Services.AddScoped<IPhoto,PhotoReposetory>();
             builder.Services.AddScoped<ITutorial,TutorialRepository>();
             builder.Services.AddAutoMapper(typeof(ProjectAutoMaper).Assembly);
             builder.Services.AddScoped<IHomeWork,HomeWorkRepository>();

builder.Services.AddControllers().AddNewtonsoftJson(options =>{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();
string connString = "";
if (builder.Environment.IsDevelopment())
    connString = builder.Configuration.GetConnectionString("DefaultConnection");
else
{
    // Use connection string provided at runtime by FlyIO.
    var connUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

    // Parse connection URL to connection string for Npgsql
    connUrl = connUrl.Replace("postgres://", string.Empty);
    var pgUserPass = connUrl.Split("@")[0];
    var pgHostPortDb = connUrl.Split("@")[1];
    var pgHostPort = pgHostPortDb.Split("/")[0];
    var pgDb = pgHostPortDb.Split("/")[1];
    var pgUser = pgUserPass.Split(":")[0];
    var pgPass = pgUserPass.Split(":")[1];
    var pgHost = pgHostPort.Split(":")[0];
    var pgPort = pgHostPort.Split(":")[1];

    connString = $"Server={pgHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};";
}
builder.Services.AddDbContext<ApplicationDbContext>(options => {
    
    options.UseNpgsql(connString);
});
builder.Services.AddIdentity<ApplicationUser,IdentityRole>(Options=>{
                Options.Password.RequiredLength = 8;
                Options.Password.RequireUppercase = true;
                Options.Password.RequireLowercase = true;
                Options.Password.RequireDigit = true;
            }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
            builder.Services.AddAuthentication(options=>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }
            ).AddJwtBearer(options=>
            {
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = false,
                   ValidateAudience = true,
                   ValidAudience = builder.Configuration["AuthSetting:Audience"],
                   ValidIssuer = builder.Configuration["AuthSetting:Issuer"],
                   RequireExpirationTime = true,
                   IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AuthSetting:Key"])),
                   ValidateIssuerSigningKey = true
                };
            });

var app = builder.Build();
// seeding data
Configure(app);
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

            app.UseRouting();
            app.UseHttpsRedirection();

           app.UseCors(x=> x.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:5001","http://localhost:4200"));
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.MapControllers();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name:"api",
                    pattern:"{controller}/{action}/{id?}"
                );
                endpoints.MapFallbackToController("Index","FallBack");
            });
            app.Run();
            async void Configure(WebApplication host){
   using var scope = host.Services.CreateScope();
   var services = scope.ServiceProvider;
   try{
    var context = services.GetRequiredService<ApplicationDbContext>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    
     context.Database.Migrate();
    await Seed.SeedReflictions(context);
                await Seed.SeedSkills(context);
                await Seed.SeedTutorials(context);
                await Seed.SeedAdmin(userManager,roleManager);
                await Seed.SeedHomeWorks(context);
     
   }catch(Exception e){
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(e,"ann error accured while migration");
   }
            }
