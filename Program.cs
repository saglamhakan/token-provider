using MockApi.Services;
using MockApi.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();

builder.Services.Configure<TokenOptions>(builder.Configuration.GetSection("Token"));
builder.Services.Configure<MockEndpoints>(builder.Configuration.GetSection("Mock"));
builder.Services.Configure<PollingOptions>(builder.Configuration.GetSection("Polling"));

builder.Services.AddHttpClient<ITokenService, TokenService>();
builder.Services.AddHttpClient<OrderClient>();

builder.Services.AddHostedService<OrderPoller>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();