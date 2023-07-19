
using Santander.Backend.Core;
using Santander.Backend.Core.Configuration;
using Santander.Backend.Core.Interface;

var builder = WebApplication.CreateBuilder(args);

var res = builder.Configuration.GetSection(HackerNewsExternalApiOptions.Key).Get<HackerNewsExternalApiOptions>();

builder.Services.AddOptions<CacheOptions>().Bind(builder.Configuration.GetSection(CacheOptions.Key));
builder.Services.AddOptions<RestApiOptions>().Bind(builder.Configuration.GetSection(RestApiOptions.Key));
builder.Services.AddOptions<HackerNewsExternalApiOptions>().Bind(builder.Configuration.GetSection(HackerNewsExternalApiOptions.Key));

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IDataCache, DataCache>();
builder.Services.AddTransient<IHackerNewsService, HackerNewsService>();
builder.Services.AddHttpClient<HackerNewsService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<Program>());

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
//app.UseAuthorization();
app.MapControllers();

app.Run();
