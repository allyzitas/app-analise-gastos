using Gastos.Shared.Dominio;

namespace Gastos.Shared.Contratos;

// ============================================================================
// Contrato da API — formato das mensagens trocadas entre o aplicativo e o
// servidor. Fica na biblioteca compartilhada para que os dois lados usem
// exatamente as mesmas classes, sem risco de divergência.
// ============================================================================

// ---------- Autenticação ----------

public record CadastroRequisicao(string Email, string Senha);

public record LoginRequisicao(string Email, string Senha);

/// <summary>
/// Resposta da autenticação. O token é guardado no dispositivo pelo
/// armazenamento seguro do sistema operacional e enviado no cabeçalho
/// Authorization das requisições seguintes.
/// </summary>
public record LoginResposta(Guid UsuarioId, string Token, DateTime ExpiraEm);

// ---------- Sincronização ----------

/// <summary>
/// Envio do dispositivo para o servidor. Carrega as operações acumuladas na
/// fila e a data da última sincronização bem-sucedida, usada para o servidor
/// saber o que devolver.
/// </summary>
public record SincronizacaoEnvio(
    DateTime? UltimaSincronizacao,
    List<Despesa> Despesas,
    List<Categoria> Categorias,
    List<ItemDespesa> Itens,
    List<DocumentoFiscal> Documentos);

/// <summary>
/// Resposta do servidor: o que mudou de lá para cá desde a última
/// sincronização, mais o carimbo de tempo desta rodada.
/// </summary>
public record SincronizacaoRetorno(
    DateTime Momento,
    List<Despesa> Despesas,
    List<Categoria> Categorias,
    List<ItemDespesa> Itens,
    List<DocumentoFiscal> Documentos);

// ---------- Erros ----------

public record ErroResposta(string Mensagem);
