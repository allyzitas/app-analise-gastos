using System.Globalization;
using Gastos.App.Dados;

namespace Gastos.App;

public partial class App : Application
{
    public App()
    {
        // Fixa o português do Brasil para datas e valores (R$ 150,00, "outubro
        // de 2026"), independentemente do idioma configurado no aparelho. O
        // aplicativo é voltado ao público brasileiro.
        var ptBr = new CultureInfo("pt-BR");
        CultureInfo.DefaultThreadCurrentCulture = ptBr;
        CultureInfo.DefaultThreadCurrentUICulture = ptBr;
        CultureInfo.CurrentCulture = ptBr;
        CultureInfo.CurrentUICulture = ptBr;

        InitializeComponent();

        // Cria o arquivo do banco e as tabelas na primeira execução; nas
        // seguintes, não faz nada. Se o modelo de dados mudar, é preciso
        // desinstalar o aplicativo do emulador para o banco ser recriado.
        using var db = new ContextoLocal();
        db.Database.EnsureCreated();
    }

    // No .NET 10 a janela é criada aqui; a propriedade MainPage está obsoleta.
    protected override Window CreateWindow(IActivationState? activationState) =>
        new Window(new AppShell());
}
