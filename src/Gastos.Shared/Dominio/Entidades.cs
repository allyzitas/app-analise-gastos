namespace Gastos.Shared.Dominio;

// ============================================================================
// Classes de domínio — correspondem ao diagrama de classes da Seção 2.3 do
// relatório técnico.
//
// São POCOs: não carregam atributos de nenhuma biblioteca de persistência.
// Isso permite que o MESMO conjunto de classes seja usado pelo aplicativo
// (SQLite) e pela API (MySQL), com o mapeamento declarado em cada DbContext.
//
// Convenções adotadas:
//  - Identificadores em Guid, gerados no dispositivo. Com banco local e
//    sincronização, inteiros sequenciais colidiriam entre aparelhos.
//  - AtualizadoEm é o critério de resolução de conflito (vence o mais recente).
//  - Excluido implementa a exclusão lógica: apagar fisicamente faria a
//    sincronização seguinte restaurar o registro a partir do servidor.
// ============================================================================

/// <summary>Base das entidades que trafegam entre o dispositivo e o servidor.</summary>
public abstract class EntidadeSincronizavel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public bool Excluido { get; set; }
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;

    /// <summary>Marca o registro como alterado agora, para efeito de conflito.</summary>
    public void Tocar() => AtualizadoEm = DateTime.UtcNow;
}

public class Usuario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Resumo criptográfico da senha (BCrypt). A senha em si nunca é
    /// armazenada, de modo que um vazamento da base não a revela.
    /// </summary>
    public string SenhaHash { get; set; } = string.Empty;

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    public List<Categoria> Categorias { get; set; } = new();
    public List<Despesa> Despesas { get; set; } = new();
}

public class Categoria : EntidadeSincronizavel
{
    public string Nome { get; set; } = string.Empty;

    /// <summary>Distingue as categorias oferecidas pelo sistema das criadas pelo usuário.</summary>
    public bool Padrao { get; set; }

    /// <summary>Nulo nas categorias padrão, que não pertencem a ninguém.</summary>
    public Guid? UsuarioId { get; set; }
}

public class Despesa : EntidadeSincronizavel
{
    public Guid UsuarioId { get; set; }
    public decimal Valor { get; set; }
    public DateTime Data { get; set; } = DateTime.Today;
    public string Estabelecimento { get; set; } = string.Empty;
    public Guid CategoriaId { get; set; }
    public TipoOrigem Origem { get; set; } = TipoOrigem.Manual;

    /// <summary>
    /// Falso enquanto o usuário não validar os dados extraídos automaticamente.
    /// Nenhum dado capturado por imagem ou QR Code é gravado sem confirmação.
    /// </summary>
    public bool Confirmada { get; set; }

    public List<ItemDespesa> Itens { get; set; } = new();
    public DocumentoFiscal? Documento { get; set; }
}

public class ItemDespesa : EntidadeSincronizavel
{
    public Guid DespesaId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Quantidade { get; set; } = 1;
    public decimal ValorUnitario { get; set; }

    /// <summary>
    /// Opcional: permite que uma única compra distribua valores entre
    /// categorias distintas.
    /// </summary>
    public Guid? CategoriaId { get; set; }

    public decimal Total => Quantidade * ValorUnitario;
}

public class DocumentoFiscal : EntidadeSincronizavel
{
    public Guid DespesaId { get; set; }

    /// <summary>
    /// Chave de acesso de 44 posições lida no QR Code. A fotografia do
    /// documento não é armazenada: a chave basta para recuperá-lo na SEFAZ.
    /// </summary>
    public string ChaveAcesso { get; set; } = string.Empty;

    public DateTime DataEmissao { get; set; }

    /// <summary>Indica se os itens já foram obtidos a partir da chave.</summary>
    public bool Consultado { get; set; }
}

public class Alerta : EntidadeSincronizavel
{
    public Guid DespesaId { get; set; }
    public string Mensagem { get; set; } = string.Empty;

    /// <summary>Média da categoria que motivou o alerta, guardada para auditoria.</summary>
    public decimal MediaHistorica { get; set; }

    public bool Lido { get; set; }
}

public class Analise : EntidadeSincronizavel
{
    public Guid UsuarioId { get; set; }
    public DateTime PeriodoInicio { get; set; }
    public DateTime PeriodoFim { get; set; }

    /// <summary>Leitura interpretativa dos gastos, gerada em linguagem natural.</summary>
    public string Texto { get; set; } = string.Empty;

    public DateTime GeradaEm { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Fila de operações pendentes de envio ao servidor. Existe apenas no banco
/// local: é o que preserva as alterações feitas sem conexão.
/// </summary>
public class FilaSincronizacao
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Nome da classe alterada, por exemplo "Despesa".</summary>
    public string Entidade { get; set; } = string.Empty;

    public Guid EntidadeId { get; set; }
    public TipoOperacao Tipo { get; set; }

    /// <summary>Registro serializado em JSON, no estado em que foi enfileirado.</summary>
    public string Conteudo { get; set; } = string.Empty;

    public StatusSincronizacao Status { get; set; } = StatusSincronizacao.Pendente;
    public int Tentativas { get; set; }

    /// <summary>
    /// Ordem em que a operação ocorreu. A fila precisa ser enviada nesta ordem:
    /// sem isso, o servidor pode receber uma edição antes da inserção.
    /// </summary>
    public DateTime CriadaEm { get; set; } = DateTime.UtcNow;
}
