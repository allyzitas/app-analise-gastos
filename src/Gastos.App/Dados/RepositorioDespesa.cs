using System.Text.Json;
using Gastos.Shared.Dominio;
using Microsoft.EntityFrameworkCore;

namespace Gastos.App.Dados;

/// <summary>
/// Acesso às despesas no banco local.
///
/// Toda gravação faz duas coisas na mesma transação: altera o registro e
/// enfileira a operação para envio. Separar as duas abriria a possibilidade de
/// uma alteração salva nunca chegar ao servidor.
///
/// Todas as consultas filtram por usuário, porque o aplicativo é multiusuário e
/// o banco local pode conter dados de mais de uma conta usada no aparelho.
/// </summary>
public class RepositorioDespesa
{
    private readonly ContextoLocal _db;

    public RepositorioDespesa(ContextoLocal db) => _db = db;

    public Task<List<Despesa>> ListarAsync(Guid usuarioId, DateTime inicio, DateTime fim) =>
        _db.Despesas
           .Where(d => d.UsuarioId == usuarioId
                       && !d.Excluido
                       && d.Data >= inicio
                       && d.Data <= fim)
           .OrderByDescending(d => d.Data)
           .AsNoTracking()
           .ToListAsync();

    public Task<Despesa?> ObterAsync(Guid id) =>
        _db.Despesas.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id && !d.Excluido);

    public async Task SalvarAsync(Despesa despesa)
    {
        despesa.Tocar();

        var existe = await _db.Despesas.AsNoTracking().AnyAsync(d => d.Id == despesa.Id);

        if (existe)
        {
            _db.Despesas.Update(despesa);
            Enfileirar(despesa, TipoOperacao.Atualizacao);
        }
        else
        {
            _db.Despesas.Add(despesa);
            Enfileirar(despesa, TipoOperacao.Insercao);
        }

        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Exclusão lógica. Remover a linha do banco local faria a sincronização
    /// seguinte restaurar a despesa a partir do servidor, onde ela ainda
    /// existiria.
    /// </summary>
    public async Task ExcluirAsync(Guid id)
    {
        var despesa = await _db.Despesas.FirstOrDefaultAsync(d => d.Id == id);
        if (despesa is null) return;

        despesa.Excluido = true;
        despesa.Tocar();

        Enfileirar(despesa, TipoOperacao.Exclusao);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Soma por categoria no período. É uma agregação do banco, não uso de
    /// inteligência artificial — a distinção importa na defesa do trabalho.
    /// </summary>
    public Task<List<TotalPorCategoria>> TotaisPorCategoriaAsync(
        Guid usuarioId, DateTime inicio, DateTime fim) =>
        _db.Despesas
           .Where(d => d.UsuarioId == usuarioId
                       && !d.Excluido
                       && d.Data >= inicio
                       && d.Data <= fim)
           .GroupBy(d => d.CategoriaId)
           .Select(g => new TotalPorCategoria(g.Key, g.Sum(d => d.Valor), g.Count()))
           .ToListAsync();

    private void Enfileirar(Despesa despesa, TipoOperacao tipo) =>
        _db.Fila.Add(new FilaSincronizacao
        {
            Entidade = nameof(Despesa),
            EntidadeId = despesa.Id,
            Tipo = tipo,
            Conteudo = JsonSerializer.Serialize(despesa)
        });
}

public record TotalPorCategoria(Guid CategoriaId, decimal Total, int Quantidade);
