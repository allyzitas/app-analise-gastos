namespace Gastos.App.Servicos;

/// <summary>
/// Guarda quem está usando o aplicativo durante a execução.
///
/// Existe porque o aplicativo é multiusuário: toda consulta ao banco local
/// precisa saber de quem são os dados. O identificador vem do login e fica no
/// armazenamento seguro, de modo que o usuário não precisa autenticar-se de
/// novo a cada abertura.
/// </summary>
public class Sessao
{
    private readonly ServicoAutenticacao _autenticacao;

    public Sessao(ServicoAutenticacao autenticacao) => _autenticacao = autenticacao;

    public Guid UsuarioId { get; private set; }

    public bool Ativa => UsuarioId != Guid.Empty;

    /// <summary>
    /// Tenta restaurar a sessão guardada. Devolve verdadeiro quando já houve
    /// login antes neste aparelho, hipótese em que o aplicativo abre direto na
    /// lista de despesas, mesmo sem internet.
    /// </summary>
    public async Task<bool> RestaurarAsync()
    {
        var id = await _autenticacao.ObterUsuarioIdAsync();
        if (id is null) return false;

        UsuarioId = id.Value;
        return true;
    }

    public void Definir(Guid usuarioId) => UsuarioId = usuarioId;

    public void Encerrar()
    {
        UsuarioId = Guid.Empty;
        _autenticacao.Sair();
    }
}
