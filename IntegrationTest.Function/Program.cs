using Guanchen.Monitor;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Modify the logger service configuration using an extension method
builder.Services.ConfigureBusinessLogger();

builder.Services.AddBusinessTracerService(Common.TomatoTrenches.businessActivitySource, "Tomato Teller");

builder.Build().Run();
