using Services;
using Utils;

var builder = WebApplication.CreateBuilder(args);

// Configura a injeção da configuração vinda do appsettings.{Ambiente}.json
builder.Services.Configure<ChatConfig>(
    builder.Configuration.GetSection("ChatConfig")
);

// Configura a URL baseada no appsettings do ambiente
var chatConfig = builder.Configuration.GetSection("ChatConfig");
var url = chatConfig.GetValue<string>("Url");
if (!string.IsNullOrEmpty(url))
{
    builder.WebHost.UseUrls(url);
}

// Adiciona os serviços necessários
builder.Services.AddSingleton<RSAKeyService>();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment() || 
    app.Environment.IsEnvironment("Bruno") || 
    app.Environment.IsEnvironment("Teste") || 
    app.Environment.IsEnvironment("Felipe"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
