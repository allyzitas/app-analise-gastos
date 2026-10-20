using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Gastos.Api.Servicos;

/// <summary>
/// Emite o token JWT que mantém a sessão do usuário.
///
/// O token é um texto assinado contendo o identificador do usuário e uma data
/// de validade. A API confere a assinatura a cada requisição e sabe de quem se
/// trata sem consultar o banco.
/// </summary>
public class ServicoToken
{
    private readonly string _chave;
    private readonly int _diasValidade;

    public ServicoToken(IConfiguration configuracao)
    {
        _chave = configuracao["Jwt:Chave"]
            ?? throw new InvalidOperationException(
                "Jwt:Chave não configurada. Em desenvolvimento, use 'dotnet user-secrets'.");

        if (Encoding.UTF8.GetByteCount(_chave) < 32)
            throw new InvalidOperationException("Jwt:Chave deve ter ao menos 32 bytes.");

        _diasValidade = configuracao.GetValue("Jwt:DiasValidade", 7);
    }

    public (string token, DateTime expiraEm) Gerar(Guid usuarioId, string email)
    {
        var expiraEm = DateTime.UtcNow.AddDays(_diasValidade);

        var credenciais = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_chave)),
            SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            claims: new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuarioId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            },
            expires: expiraEm,
            signingCredentials: credenciais);

        return (new JwtSecurityTokenHandler().WriteToken(jwt), expiraEm);
    }
}
