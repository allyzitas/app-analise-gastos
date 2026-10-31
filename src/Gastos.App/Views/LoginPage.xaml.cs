using Gastos.App.ViewModels;

namespace Gastos.App.Views;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _vm;

    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;

#if DEBUG
        // O comando é ligado aqui, e não no XAML, porque só existe na
        // compilação de desenvolvimento: no XAML, a compilação Release
        // falharia ao não encontrá-lo.
        BotaoModoLocal.Command = vm.EntrarLocalCommand;
        BotaoModoLocal.IsVisible = true;
#endif
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.VerificarSessaoCommand.Execute(null);
    }
}
