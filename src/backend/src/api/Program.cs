using System.Reflection;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using Restify.API.Data;
using Swashbuckle.AspNetCore.SwaggerGen;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
	.AddJsonOptions(options =>
		options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve);
	
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	// https://stackoverflow.com/a/60088511/21037183
	options.DocInclusionPredicate((docName, apiDesc) =>
	{
		if (!apiDesc.TryGetMethodInfo(out MethodInfo methodInfo))
		{
			return false;
		}

		IEnumerable<ApiVersion> versions = methodInfo.DeclaringType
			.GetCustomAttributes(true)
			.OfType<ApiVersionAttribute>()
			.SelectMany(a => a.Versions);

		return versions.Any(v => $"v{v.ToString()}" == docName);
	});

	options.SwaggerDoc("v2.0", new OpenApiInfo { Title = "Restify", Version = "v2" });	
});
builder.Services.AddDbContext<RestifyDbContext>(
	options =>
	{
		string connectionString = "server=localhost;user=root;password=;database=restify";
		options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
			.LogTo(Console.WriteLine, LogLevel.Information)
			.EnableSensitiveDataLogging()
			.EnableDetailedErrors();
	});

builder.Services.AddApiVersioning(config =>
{
	config.DefaultApiVersion = new ApiVersion(2);
	config.AssumeDefaultVersionWhenUnspecified = true;
	config.ReportApiVersions = true;
})
.AddApiExplorer(options =>
{
	// https://stackoverflow.com/a/58602364/21037183
	options.GroupNameFormat = "'v'VVV";
	options.SubstituteApiVersionInUrl = true;
});

if (builder.Environment.IsDevelopment())
{
	builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}

var app = builder.Build();
app.UsePathBase("/api");

string storagePath = Path.Combine(builder.Environment.ContentRootPath, "storage");
Directory.CreateDirectory(storagePath);
app.UseStaticFiles(new StaticFileOptions 
{
	FileProvider = new PhysicalFileProvider(storagePath),
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
	    options.SwaggerEndpoint("/swagger/v2.0/swagger.json", "Restify v2 Docs");
    });}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapDefaultControllerRoute();

app.Run();