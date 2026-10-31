using Gastos.App.ViewModels;

namespace Gastos.App.Views;

public partial class DespesasPage : ContentPage
{
    private readonly DespesasViewModel _vm;

    public DespesasPage(DespesasViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    // Recarrega toda vez que a tela volta ao topo, inclusive depois de salvar
    // no formulário.
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.CarregarCommand.Execute(null);
    }
}
