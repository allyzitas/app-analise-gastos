using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gastos.App.Dados;
using Gastos.App.Servicos;
using Gastos.App.Views;
using Gastos.Shared.Dominio;

namespace Gastos.App.ViewModels;

public partial class DespesasViewModel : ObservableObject
{
    private readonly RepositorioDespesa _despesas;
    private readonly RepositorioCategoria _categorias;
    private readonly Sessao _sessao;

    public DespesasViewModel(
        RepositorioDespesa despesas, RepositorioCategoria categorias, Sessao sessao)
    {
        _despesas = despesas;
        _categorias = categorias;
        _sessao = sessao;
    }

    public ObservableCollection<LinhaDespesa> Linhas { get; } = new();

    [ObservableProperty] private decimal _total;
    [ObservableProperty] private string _periodo = string.Empty;
    [ObservableProperty] private bool _carregando;

    public bool Vazio => !Carregando && Linhas.Count == 0;

    partial void OnCarregandoChanged(bool value) => OnPropertyChanged(nameof(Vazio));

    [RelayCommand]
    private async Task CarregarAsync()
    {
        if (!_sessao.Ativa) return;

        Carregando = true;

        try
        {
            // Mês corrente. O seletor de período entra junto com os dashboards,
            // no incremento de visualização.
            var hoje = DateTime.Today;
            var inicio = new DateTime(hoje.Year, hoje.Month, 1);
            var fim = inicio.AddMonths(1).AddDays(-1);

            Periodo = inicio.ToString("MMMM 'de' yyyy");

            var categorias = (await _categorias.GarantirPadraoAsync(_sessao.UsuarioId))
                .ToDictionary(c => c.Id, c => c.Nome);

            var registros = await _despesas.ListarAsync(_sessao.UsuarioId, inicio, fim);

            Linhas.Clear();
            foreach (var despesa in registros)
            {
                categorias.TryGetValue(despesa.CategoriaId, out var nome);
                Linhas.Add(new LinhaDespesa(
                    despesa.Id,
                    string.IsNullOrWhiteSpace(despesa.Estabelecimento)
                        ? "Sem estabelecimento"
                        : despesa.Estabelecimento,
                    nome ?? "Sem categoria",
                    despesa.Valor,
                    despesa.Data));
            }

            Total = registros.Sum(d => d.Valor);
        }
        finally
        {
            Carregando = false;
            OnPropertyChanged(nameof(Vazio));
        }
    }

    [RelayCommand]
    private static Task NovaAsync() =>
        Shell.Current.GoToAsync(nameof(DespesaFormPage));

    [RelayCommand]
    private static Task EditarAsync(LinhaDespesa linha) =>
        Shell.Current.GoToAsync($"{nameof(DespesaFormPage)}?id={linha.Id}");

    [RelayCommand]
    private async Task SairAsync()
    {
        _sessao.Encerrar();
        await Shell.Current.GoToAsync("//login");
    }
}

/// <summary>
/// O que a lista mostra. É separado de <see cref="Despesa"/> porque a tela
/// precisa do nome da categoria, e a entidade guarda apenas o identificador.
/// </summary>
public record LinhaDespesa(
    Guid Id, string Estabelecimento, string Categoria, decimal Valor, DateTime Data)
{
    public string ValorFormatado => Valor.ToString("C");
    public string DataFormatada => Data.ToString("dd/MM");
}
