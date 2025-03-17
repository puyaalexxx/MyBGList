using System.Data;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyBGList.Config;
using MyBGList.Models;
using MyBGList.Swagger;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddControllers(options =>
    {
        options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(
            x => $"The value '{x}' is invalid.");
        options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(
            x => $"The field {x} must be a number.");
        options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
            (x, y) => $"The value '{x}' is not valid for {y}.");
        options.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(
            () => "A value is required.");

        options.CacheProfiles.Add("NoCache", new CacheProfile { NoStore = true });
        options.CacheProfiles.Add("Any-60", new CacheProfile { Location = ResponseCacheLocation.Any, Duration = 60 });
    }
);
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.ParameterFilter<SortColumnFilter>();
    options.ParameterFilter<SortOrderFilter>();
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(cfg =>
    {
        cfg.WithOrigins(builder.Configuration["AllowedOrigins"] ?? "*");
        cfg.AllowAnyHeader();
        cfg.AllowAnyMethod();
    });

    options.AddPolicy(
        "AnyOrigin",
        cfg =>
        {
            cfg.AllowAnyOrigin();
            cfg.AllowAnyMethod();
            cfg.AllowAnyMethod();
        });
});

builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

//API versioning
builder.Services.AddApiVersioning(options =>
    {
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

//builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 32 * 1024 * 1024;
    options.SizeLimit = 50 * 1024 * 1024;
});

builder.Services.AddMemoryCache();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
});

builder.Host.UseSerilog((ctx, lc) =>
{
    //lc.MinimumLevel.Is(Serilog.Events.LogEventLevel.Warning);
    //lc.MinimumLevel.Override("MyBGList", Serilog.Events.LogEventLevel.Information); // override appsettings.json config

    lc.ReadFrom.Configuration(ctx.Configuration);
    lc.Enrich.WithEnvironmentName();
    lc.Enrich.WithThreadId();

    lc.WriteTo.File("Logs/log.txt", outputTemplate:
        "{Timestamp:HH:mm:ss} [{Level:u3}]" + "[{MachineName} #{ThreadId}]" + "{Message:lj}{NewLine}{Exception}"
        , rollingInterval: RollingInterval.Day); // write logs to file

    lc.WriteTo.MSSqlServer(
        // restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information, // restrict log level for this sink only
        ctx.Configuration.GetConnectionString("DefaultConnection"),
        new MSSqlServerSinkOptions
        {
            TableName = "LogEvents",
            AutoCreateSqlTable = true
        },
        columnOptions: new ColumnOptions
        {
            AdditionalColumns = new[]
            {
                new SqlColumn
                {
                    ColumnName = "SourceContext",
                    PropertyName = "SourceContext",
                    DataType = SqlDbType.NVarChar
                }
            }
        });
}, writeToProviders: true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var descriptions = app.DescribeApiVersions();
        foreach (var description in descriptions)
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                $"MyBGList {description.GroupName.ToUpperInvariant()}");

        options.RoutePrefix =
            string.Empty; // Swagger UI at the root (remove swagger from https://localhost:7074/swagger/index.html)
    });

    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
}

app.UseHttpsRedirection();
app.MapControllers();
app.UseCors();
app.UseResponseCaching();

/*app.MapGet("/error", () => Results.Problem());
app.MapGet("/error/test", () =>
{
     throw new Exception("test");
});*/

app.Run();