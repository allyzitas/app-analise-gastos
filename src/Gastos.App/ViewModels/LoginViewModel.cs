using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gastos.App.Servicos;
using Gastos.App.Views;

namespace Gastos.App.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly ServicoAutenticacao _autenticacao;
    private readonly Sessao _sessao;

    public LoginViewModel(ServicoAutenticacao autenticacao, Sessao sessao)
    {
        _autenticacao = autenticacao;
        _sessao = sessao;
    }

    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _senha = string.Empty;
    [ObservableProperty] private string _aviso = string.Empty;
    [ObservableProperty] private bool _ocupado;

    public bool TemAviso => !string.IsNullOrEmpty(Aviso);

    /// <summary>Usada pelos botões: eles ficam desligados durante a chamada.</summary>
    public bool NaoOcupado => !Ocupado;

    partial void OnAvisoChanged(string value) => OnPropertyChanged(nameof(TemAviso));

    partial void OnOcupadoChanged(bool value) => OnPropertyChanged(nameof(NaoOcupado));

    /// <summary>
    /// Chamado quando a tela aparece. Se já houve login neste aparelho, entra
    /// direto: o aplicativo não exige conexão depois da primeira vez.
    /// </summary>
    [RelayCommand]
    private async Task VerificarSessaoAsync()
    {
        if (await _sessao.RestaurarAsync())
            await AbrirListaAsync();
    }

    [RelayCommand]
    private Task EntrarAsync() => AutenticarAsync(entrando: true);

    [RelayCommand]
    private Task CadastrarAsync() => AutenticarAsync(entrando: false);

    private async Task AutenticarAsync(bool entrando)
    {
        if (Ocupado) return;

        if (string.IsNullOrWhiteSpace(Email) || !Email.Contains('@'))
        {
            Aviso = "Informe um e-mail válido.";
            return;
        }

        if (Senha.Length < 8)
        {
            Aviso = "A senha precisa ter ao menos 8 caracteres.";
            return;
        }

        Ocupado = true;
        Aviso = string.Empty;

        try
        {
            var ok = entrando
                ? await _autenticacao.EntrarAsync(Email, Senha)
                : await _autenticacao.CadastrarAsync(Email, Senha);

            if (!ok)
            {
                Aviso = entrando
                    ? "E-mail ou senha incorretos."
                    : "Não foi possível cadastrar. O e-mail pode já estar em uso.";
                return;
            }

            var id = await _autenticacao.ObterUsuarioIdAsync();
            if (id is null)
            {
                Aviso = "A resposta do servidor veio incompleta.";
                return;
            }

            _sessao.Definir(id.Value);
            await AbrirListaAsync();
        }
        catch (HttpRequestException)
        {
            Aviso = "Não foi possível falar com o servidor. Verifique se a API está rodando.";
        }
        catch (TaskCanceledException)
        {
            Aviso = "O servidor demorou a responder.";
        }
        finally
        {
            Ocupado = false;
        }
    }

#if DEBUG
    /// <summary>
    /// Entra com um usuário local, sem passar pelo servidor. Existe apenas nas
    /// compilações de desenvolvimento, para permitir trabalhar nas telas
    /// enquanto a API ainda não está no ar.
    /// </summary>
    [RelayCommand]
    private async Task EntrarLocalAsync()
    {
        const string chave = "gastos.usuarioLocal";

        var guardado = await SecureStorage.GetAsync(chave);
        if (!Guid.TryParse(guardado, out var id))
        {
            id = Guid.NewGuid();
            await SecureStorage.SetAsync(chave, id.ToString());
        }

        _sessao.Definir(id);
        await AbrirListaAsync();
    }
#endif

    private static Task AbrirListaAsync() =>
        Shell.Current.GoToAsync("//despesas");
}
