using Gastos.Shared.Dominio;
using Microsoft.EntityFrameworkCore;

namespace Gastos.App.Dados;

/// <summary>
/// Banco local do aparelho (SQLite). É a fonte de verdade do aplicativo: as
/// telas leem e gravam sempre aqui, com ou sem conexão, e a fila de
/// sincronização leva as alterações ao servidor depois.
///
/// Usa as mesmas classes de domínio da API (biblioteca Gastos.Shared); só o
/// mapeamento muda. A tabela de usuários não existe aqui: senha e e-mail ficam
/// apenas no servidor, e o aparelho guarda só o identificador da conta.
/// </summary>
public class ContextoLocal : DbContext
{
    /// <summary>
    /// Pasta privada do aplicativo. No Android, é apagada junto com a
    /// desinstalação e não fica acessível a outros aplicativos.
    /// </summary>
    public static string CaminhoBanco =>
        Path.Combine(FileSystem.AppDataDirectory, "gastos.db3");

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Despesa> Despesas => Set<Despesa>();
    public DbSet<ItemDespesa> Itens => Set<ItemDespesa>();
    public DbSet<DocumentoFiscal> Documentos => Set<DocumentoFiscal>();
    public DbSet<FilaSincronizacao> Fila => Set<FilaSincronizacao>();

    protected override void OnConfiguring(DbContextOptionsBuilder opcoes) =>
        opcoes.UseSqlite($"Data Source={CaminhoBanco}");

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Categoria>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Nome).HasMaxLength(80).IsRequired();
            e.HasIndex(c => c.UsuarioId);
        });

        mb.Entity<Despesa>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.Estabelecimento).HasMaxLength(160);
            // Toda consulta da lista filtra por usuário e período.
            e.HasIndex(d => new { d.UsuarioId, d.Data });
            // Itens e documento são gravados pelas próprias tabelas, como na
            // API; a navegação não é mapeada para não haver dois caminhos de
            // gravação do mesmo dado.
            e.Ignore(d => d.Itens);
            e.Ignore(d => d.Documento);
        });

        mb.Entity<ItemDespesa>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.Descricao).HasMaxLength(200);
            e.Ignore(i => i.Total);
            e.HasIndex(i => i.DespesaId);
        });

        mb.Entity<DocumentoFiscal>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.ChaveAcesso).HasMaxLength(44).IsRequired();
            e.HasIndex(f => f.DespesaId);
        });

        mb.Entity<FilaSincronizacao>(e =>
        {
            e.HasKey(f => f.Id);
            // O envio busca os pendentes na ordem em que ocorreram.
            e.HasIndex(f => new { f.Status, f.CriadaEm });
        });
    }
}
