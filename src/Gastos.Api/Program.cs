using System.Text;
using Gastos.Api.Dados;
using Gastos.Api.Endpoints;
using Gastos.Api.Servicos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------- banco ----
// A cadeia de conexão fica no servidor, nunca no pacote instalado no
// dispositivo. Em desenvolvimento, use 'dotnet user-secrets' para não
// versionar a senha do MySQL.
var conexao = builder.Configuration.GetConnectionString("MySql")
    ?? throw new InvalidOperationException("ConnectionStrings:MySql não configurada.");

builder.Services.AddDbContext<ContextoApi>(opcoes =>
    opcoes.UseMySql(conexao, ServerVersion.AutoDetect(conexao)));

// ------------------------------------------------------------ segurança ----
builder.Services.AddSingleton<ServicoToken>();

var chaveJwt = builder.Configuration["Jwt:Chave"]
    ?? throw new InvalidOperationException("Jwt:Chave não configurada.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opcoes =>
    {
        opcoes.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveJwt)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Cria o esquema do banco na primeira execução. Para o TCC é suficiente;
// um sistema em produção usaria migrações versionadas.
using (var escopo = app.Services.CreateScope())
{
    escopo.ServiceProvider.GetRequiredService<ContextoApi>().Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "API do Sistema de Análise de Gastos.");
app.MapearAutenticacao();
app.MapearSincronizacao();

app.Run();
