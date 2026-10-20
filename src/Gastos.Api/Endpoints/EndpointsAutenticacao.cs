using Gastos.Api.Dados;
using Gastos.Api.Servicos;
using Gastos.Shared.Contratos;
using Gastos.Shared.Dominio;
using Microsoft.EntityFrameworkCore;

namespace Gastos.Api.Endpoints;

public static class EndpointsAutenticacao
{
    /// <summary>Categorias oferecidas a todo usuário recém-cadastrado.</summary>
    private static readonly string[] CategoriasPadrao =
    {
        "Alimentação", "Transporte", "Moradia", "Saúde",
        "Educação", "Lazer", "Vestuário", "Outros"
    };

    public static void MapearAutenticacao(this IEndpointRouteBuilder rotas)
    {
        var grupo = rotas.MapGroup("/auth").WithTags("Autenticação");

        // ------------------------------------------------------------------
        // Cadastro
        //
        // Pede apenas endereço eletrônico e senha. Nome e documentos não são
        // coletados: é a aplicação concreta do princípio da necessidade
        // discutido no relatório (minimização da identificação do usuário).
        // ------------------------------------------------------------------
        grupo.MapPost("/cadastro", async (
            CadastroRequisicao req, ContextoApi db, ServicoToken tokens) =>
        {
            var email = req.Email.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
                return Results.BadRequest(new ErroResposta("Informe um endereço de e-mail válido."));

            if (req.Senha.Length < 8)
                return Results.BadRequest(new ErroResposta("A senha deve ter ao menos 8 caracteres."));

            if (await db.Usuarios.AnyAsync(u => u.Email == email))
                return Results.Conflict(new ErroResposta("Este e-mail já está cadastrado."));

            var usuario = new Usuario
            {
                Email = email,
                // BCrypt gera e embute o salt: duas pessoas com a mesma senha
                // produzem hashes diferentes.
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(req.Senha)
            };

            db.Usuarios.Add(usuario);

            // As categorias padrão nascem junto com a conta e já pertencem ao
            // usuário, de modo que ele pode renomeá-las sem afetar os demais.
            db.Categorias.AddRange(CategoriasPadrao.Select(nome => new Categoria
            {
                Nome = nome,
                Padrao = true,
                UsuarioId = usuario.Id
            }));

            await db.SaveChangesAsync();

            var (token, expiraEm) = tokens.Gerar(usuario.Id, usuario.Email);
            return Results.Ok(new LoginResposta(usuario.Id, token, expiraEm));
        });

        // ------------------------------------------------------------------
        // Login
        // ------------------------------------------------------------------
        grupo.MapPost("/login", async (
            LoginRequisicao req, ContextoApi db, ServicoToken tokens) =>
        {
            var email = req.Email.Trim().ToLowerInvariant();
            var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

            // A mesma mensagem para e-mail inexistente e senha errada: informar
            // qual dos dois falhou revelaria quem tem conta no sistema.
            if (usuario is null || !BCrypt.Net.BCrypt.Verify(req.Senha, usuario.SenhaHash))
                return Results.Unauthorized();

            var (token, expiraEm) = tokens.Gerar(usuario.Id, usuario.Email);
            return Results.Ok(new LoginResposta(usuario.Id, token, expiraEm));
        });
    }
}
