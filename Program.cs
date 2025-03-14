using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyBGList.Config;
using MyBGList.Models;
using MyBGList.Swagger;
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

/*app.MapGet("/error", () => Results.Problem());
app.MapGet("/error/test", () =>
{
     throw new Exception("test");
});*/

app.Run();