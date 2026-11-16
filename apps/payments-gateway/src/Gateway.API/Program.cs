using Gateway.API.Configurations;
using Gateway.API.Extensions;
using Serilog;
using Serilog.Debugging;
using System.Reflection;

SelfLog.Enable(Console.Error);

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
{
    ConnectionStrings connectionStrings = new(builder.Configuration);
    connectionStrings.Validate();

    builder.AddLogging(connectionStrings.MongoDb);
    builder.Services.AddErrorHandling();
    builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());
    builder.Services.AddVersioning();
    builder.Services.AddHealthMonitoring(connectionStrings);
}

WebApplication app = builder.Build();
{
    app.UseGlobalization();
    app.UseErrorHandling();
    //app.UseAuthentication();
    //app.UseAuthorization();
    app.MapEndpoints();
    app.UseDocumentation();
    app.UseHttpsRedirection();
    app.UseHealthMonitoring();

    app.Lifetime.ApplicationStarted.Register(() =>
    {
        Log.Information("Application started and server is ready.");
    });

    app.Run();
}