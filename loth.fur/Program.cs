

using loth.fur;

var builder = WebApplication.CreateBuilder(args).Inject();

builder.Services.AddControllers().AddInject();

builder.Services.AddConfigurableOptions<AppInfoOptions>();

builder.Services.AddRemoteRequest();


var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseInject();

app.MapControllers();

app.Run();