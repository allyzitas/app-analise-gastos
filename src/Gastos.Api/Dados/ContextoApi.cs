using Gastos.Shared.Dominio;
using Microsoft.EntityFrameworkCore;

namespace Gastos.Api.Dados;

/// <summary>
/// Mapeamento das classes de domínio para o banco MySQL do servidor.
/// As classes vêm da biblioteca compartilhada sem atributos de persistência:
/// é aqui que se declara como elas viram tabelas.
/// </summary>
public class ContextoApi : DbContext
{
    public ContextoApi(DbContextOptions<ContextoApi> opcoes) : base(opcoes) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Despesa> Despesas => Set<Despesa>();
    public DbSet<ItemDespesa> Itens => Set<ItemDespesa>();
    public DbSet<DocumentoFiscal> Documentos => Set<DocumentoFiscal>();
    public DbSet<Alerta> Alertas => Set<Alerta>();
    public DbSet<Analise> Analises => Set<Analise>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Usuario>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Email).HasMaxLength(255).IsRequired();
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.SenhaHash).HasMaxLength(100).IsRequired();
            e.Ignore(u => u.Categorias);
            e.Ignore(u => u.Despesas);
        });

        mb.Entity<Categoria>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Nome).HasMaxLength(80).IsRequired();
            e.HasIndex(c => c.UsuarioId);
        });

        mb.Entity<Despesa>(e =>
        {
            e.HasKey(d => d.Id);
            // Valores monetários nunca em ponto flutuante: decimal evita erro
            // de arredondamento em somas e médias.
            e.Property(d => d.Valor).HasPrecision(12, 2);
            e.Property(d => d.Estabelecimento).HasMaxLength(160);
            // Índice composto: toda consulta filtra por usuário e período.
            e.HasIndex(d => new { d.UsuarioId, d.Data });
            e.Ignore(d => d.Itens);
            e.Ignore(d => d.Documento);
        });

        mb.Entity<ItemDespesa>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.Descricao).HasMaxLength(200);
            e.Property(i => i.Quantidade).HasPrecision(12, 3);
            e.Property(i => i.ValorUnitario).HasPrecision(12, 2);
            e.Ignore(i => i.Total);
            e.HasIndex(i => i.DespesaId);
        });

        mb.Entity<DocumentoFiscal>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.ChaveAcesso).HasMaxLength(44).IsRequired();
            e.HasIndex(f => f.DespesaId);
        });

        mb.Entity<Alerta>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Mensagem).HasMaxLength(300);
            e.Property(a => a.MediaHistorica).HasPrecision(12, 2);
        });

        mb.Entity<Analise>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Texto).HasColumnType("TEXT");
            e.HasIndex(a => a.UsuarioId);
        });
    }
}
