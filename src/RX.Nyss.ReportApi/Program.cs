using Microsoft.AspNetCore.Builder;
using RX.Nyss.ReportApi.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureDependencies(builder.Configuration);
var app = builder.Build();

app.UseCustomExceptionHandler();
app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nyss Report API V1"));
app.MapControllers();

app.Run();
