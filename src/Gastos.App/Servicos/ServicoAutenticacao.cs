using System.Net.Http.Json;
using Gastos.Shared.Contratos;

namespace Gastos.App.Servicos;

/// <summary>
/// Conversa com os endpoints /auth da API e guarda o resultado no
/// armazenamento seguro do sistema operacional (Keystore, no Android).
///
/// O aparelho guarda apenas o identificador da conta e o token. A senha nunca
/// é gravada: depois do primeiro login, o aplicativo abre direto, mesmo sem
/// internet.
/// </summary>
public class ServicoAutenticacao
{
    private const string ChaveToken = "gastos.token";
    private const string ChaveUsuario = "gastos.usuarioId";
    private const string ChaveExpiracao = "gastos.tokenExpira";

    private readonly HttpClient _http;

    public ServicoAutenticacao(HttpClient http) => _http = http;

    public Task<bool> EntrarAsync(string email, string senha) =>
        EnviarAsync("/auth/login", new LoginRequisicao(email.Trim(), senha));

    public Task<bool> CadastrarAsync(string email, string senha) =>
        EnviarAsync("/auth/cadastro", new CadastroRequisicao(email.Trim(), senha));

    /// <summary>
    /// Devolve falso quando a API recusa (credencial errada, e-mail repetido).
    /// Falha de rede não é tratada aqui: a exceção sobe para a tela, que
    /// mostra a mensagem adequada.
    /// </summary>
    private async Task<bool> EnviarAsync<T>(string rota, T corpo)
    {
        using var resposta = await _http.PostAsJsonAsync(rota, corpo);
        if (!resposta.IsSuccessStatusCode) return false;

        var login = await resposta.Content.ReadFromJsonAsync<LoginResposta>();
        if (login is null) return false;

        await SecureStorage.SetAsync(ChaveToken, login.Token);
        await SecureStorage.SetAsync(ChaveUsuario, login.UsuarioId.ToString());
        await SecureStorage.SetAsync(ChaveExpiracao, login.ExpiraEm.ToString("O"));
        return true;
    }

    public async Task<Guid?> ObterUsuarioIdAsync()
    {
        try
        {
            var texto = await SecureStorage.GetAsync(ChaveUsuario);
            return Guid.TryParse(texto, out var id) ? id : null;
        }
        catch (Exception)
        {
            // O Android pode invalidar a chave de criptografia (por exemplo,
            // ao restaurar um backup). Nesse caso, pede-se login de novo.
            SecureStorage.RemoveAll();
            return null;
        }
    }

    /// <summary>Usado pela sincronização, no cabeçalho Authorization.</summary>
    public Task<string?> ObterTokenAsync() => SecureStorage.GetAsync(ChaveToken);

    public void Sair()
    {
        SecureStorage.Remove(ChaveToken);
        SecureStorage.Remove(ChaveUsuario);
        SecureStorage.Remove(ChaveExpiracao);
    }
}
