namespace Camelot.Api;

using System;
using System.Text;

using Camelot.Api.Data;
using Camelot.Api.Service;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

public class Startup
{
  public Startup(IConfiguration configuration) => this.Configuration = configuration;

  public IConfiguration Configuration { get; }

  // This method gets called by the runtime. Use this method to add services to the container.
  public void ConfigureServices(IServiceCollection services)
  {

    var connString = Environment.GetEnvironmentVariable("DB_CONNECTION");
    Console.WriteLine(connString);
    services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connString));

    services.AddCors();

    services.AddControllers();
    services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "Camelot.Api", Version = "v1" }));

    var key = Environment.GetEnvironmentVariable("JWT_KEY");

    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(
           Encoding.ASCII.GetBytes(key)
        ),
        });
    // 
    services.AddScoped<BoardService>();
    services.AddScoped<CollectionService>();
    services.AddScoped<UserService>();
  }

  // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
  public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
  {
    try
    {
      using (var scope = app.ApplicationServices.CreateScope())
      {
        var dbContext = scope.ServiceProvider.GetService<AppDbContext>();
        dbContext.Database.Migrate();
        Console.WriteLine("db migrated");
      }
    }
    catch (Exception e)
    {
      Console.WriteLine(e.Message);
    }

    if (env.IsDevelopment())
    {
      app.UseDeveloperExceptionPage();
      app.UseSwagger();
      app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Camelot.Api v1"));
    }

    app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("X-Auth-Token"));

    app.UseHttpsRedirection();

    app.UseRouting();

    app.UseAuthorization();

    app.UseEndpoints(endpoints => endpoints.MapControllers());
  }
}
