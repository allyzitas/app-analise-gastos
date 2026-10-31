using Gastos.App.ViewModels;

namespace Gastos.App.Views;

public partial class DespesaFormPage : ContentPage
{
    private readonly DespesaFormViewModel _vm;

    public DespesaFormPage(DespesaFormViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    // Em OnAppearing, e não no construtor, porque o parâmetro de rota (id) só
    // chega depois que o Shell monta a página.
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.CarregarCommand.Execute(null);
    }
}
