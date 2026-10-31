using Gastos.App.Views;

namespace Gastos.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // O formulário é aberto por cima da lista, então entra como rota
        // registrada e não como ShellContent.
        Routing.RegisterRoute(nameof(DespesaFormPage), typeof(DespesaFormPage));
    }
}
