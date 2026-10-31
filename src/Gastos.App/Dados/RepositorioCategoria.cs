using System.Text.Json;
using Gastos.Shared.Dominio;
using Microsoft.EntityFrameworkCore;

namespace Gastos.App.Dados;

public class RepositorioCategoria
{
    /// <summary>
    /// Categorias oferecidas a quem ainda não tem nenhuma. As mesmas que a API
    /// cria no cadastro: assim o aplicativo é utilizável antes da primeira
    /// sincronização.
    /// </summary>
    private static readonly string[] Padrao =
    {
        "Alimentação", "Transporte", "Moradia", "Saúde",
        "Educação", "Lazer", "Vestuário", "Outros"
    };

    private readonly ContextoLocal _db;

    public RepositorioCategoria(ContextoLocal db) => _db = db;

    public Task<List<Categoria>> ListarAsync(Guid usuarioId) =>
        _db.Categorias
           .Where(c => !c.Excluido && c.UsuarioId == usuarioId)
           .OrderBy(c => c.Nome)
           .AsNoTracking()
           .ToListAsync();

    /// <summary>
    /// Cria as categorias padrão na primeira execução. Não enfileira para
    /// sincronização quando as categorias vieram do servidor: aqui só entram as
    /// criadas localmente, que precisam subir.
    /// </summary>
    public async Task<List<Categoria>> GarantirPadraoAsync(Guid usuarioId)
    {
        var existentes = await ListarAsync(usuarioId);
        if (existentes.Count > 0) return existentes;

        var novas = Padrao.Select(nome => new Categoria
        {
            Nome = nome,
            Padrao = true,
            UsuarioId = usuarioId
        }).ToList();

        _db.Categorias.AddRange(novas);

        foreach (var categoria in novas)
        {
            _db.Fila.Add(new FilaSincronizacao
            {
                Entidade = nameof(Categoria),
                EntidadeId = categoria.Id,
                Tipo = TipoOperacao.Insercao,
                Conteudo = JsonSerializer.Serialize(categoria)
            });
        }

        await _db.SaveChangesAsync();
        return novas;
    }
}
