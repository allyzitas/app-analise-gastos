using System.Security.Claims;
using Gastos.Api.Dados;
using Gastos.Shared.Contratos;
using Gastos.Shared.Dominio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Gastos.Api.Endpoints;

public static class EndpointsSincronizacao
{
    public static void MapearSincronizacao(this IEndpointRouteBuilder rotas)
    {
        var grupo = rotas.MapGroup("/sinc").WithTags("Sincronização").RequireAuthorization();

        // ------------------------------------------------------------------
        // Troca bidirecional em uma única requisição: o dispositivo envia o que
        // acumulou na fila e recebe o que mudou no servidor desde a última
        // sincronização.
        // ------------------------------------------------------------------
        grupo.MapPost("/", async (
            SincronizacaoEnvio envio, ContextoApi db, ClaimsPrincipal quem) =>
        {
            var usuarioId = ObterUsuarioId(quem);
            if (usuarioId is null) return Results.Unauthorized();

            var momento = DateTime.UtcNow;

            // ---------- 1. Subida ----------
            await ReceberAsync(db.Categorias, envio.Categorias, db);
            await ReceberAsync(db.Despesas, MarcarDono(envio.Despesas, usuarioId.Value), db);
            await ReceberAsync(db.Itens, envio.Itens, db);
            await ReceberAsync(db.Documentos, envio.Documentos, db);

            await db.SaveChangesAsync();

            // ---------- 2. Descida ----------
            var desde = envio.UltimaSincronizacao ?? DateTime.MinValue;

            var despesas = await db.Despesas
                .Where(d => d.UsuarioId == usuarioId && d.AtualizadoEm > desde)
                .AsNoTracking().ToListAsync();

            var idsDespesas = despesas.Select(d => d.Id).ToList();

            var categorias = await db.Categorias
                .Where(c => c.UsuarioId == usuarioId && c.AtualizadoEm > desde)
                .AsNoTracking().ToListAsync();

            var itens = await db.Itens
                .Where(i => idsDespesas.Contains(i.DespesaId) && i.AtualizadoEm > desde)
                .AsNoTracking().ToListAsync();

            var documentos = await db.Documentos
                .Where(f => idsDespesas.Contains(f.DespesaId) && f.AtualizadoEm > desde)
                .AsNoTracking().ToListAsync();

            return Results.Ok(new SincronizacaoRetorno(
                momento, despesas, categorias, itens, documentos));
        });
    }

    private static Guid? ObterUsuarioId(ClaimsPrincipal quem)
    {
        // O identificador vem da claim "sub" do token, não do corpo da
        // requisição: assim um usuário não consegue gravar dados em nome de
        // outro apenas alterando o JSON que envia.
        var bruto = quem.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? quem.FindFirstValue("sub");

        return Guid.TryParse(bruto, out var id) ? id : null;
    }

    private static List<Despesa> MarcarDono(List<Despesa> despesas, Guid usuarioId)
    {
        foreach (var d in despesas) d.UsuarioId = usuarioId;
        return despesas;
    }

    /// <summary>
    /// Aplica no servidor os registros que chegaram do dispositivo.
    ///
    /// Política de conflito: prevalece a alteração mais recente, comparada pelo
    /// campo AtualizadoEm. A versão preterida é descartada — limitação assumida
    /// e registrada no relatório. A exclusão chega como um registro comum com
    /// Excluido = true, nunca como remoção física.
    /// </summary>
    private static async Task ReceberAsync<T>(
        DbSet<T> conjunto, List<T> recebidos, ContextoApi db)
        where T : EntidadeSincronizavel
    {
        if (recebidos.Count == 0) return;

        var ids = recebidos.Select(r => r.Id).ToList();
        var existentes = await conjunto
            .Where(e => ids.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id);

        foreach (var recebido in recebidos)
        {
            if (existentes.TryGetValue(recebido.Id, out var atual))
            {
                if (recebido.AtualizadoEm > atual.AtualizadoEm)
                    db.Entry(atual).CurrentValues.SetValues(recebido);
            }
            else
            {
                conjunto.Add(recebido);
            }
        }
    }
}
