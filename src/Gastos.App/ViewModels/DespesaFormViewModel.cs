using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gastos.App.Dados;
using Gastos.App.Servicos;
using Gastos.Shared.Dominio;

namespace Gastos.App.ViewModels;

[QueryProperty(nameof(Id), "id")]
public partial class DespesaFormViewModel : ObservableObject
{
    private readonly RepositorioDespesa _despesas;
    private readonly RepositorioCategoria _categorias;
    private readonly Sessao _sessao;

    private Guid _idAtual = Guid.Empty;

    public DespesaFormViewModel(
        RepositorioDespesa despesas, RepositorioCategoria categorias, Sessao sessao)
    {
        _despesas = despesas;
        _categorias = categorias;
        _sessao = sessao;
    }

    /// <summary>Preenchido pelo Shell quando a tela abre para edição.</summary>
    public string Id { get; set; } = string.Empty;

    public ObservableCollection<Categoria> Categorias { get; } = new();

    [ObservableProperty] private string _valorTexto = string.Empty;
    [ObservableProperty] private string _estabelecimento = string.Empty;
    [ObservableProperty] private DateTime _data = DateTime.Today;
    [ObservableProperty] private Categoria? _categoria;
    [ObservableProperty] private string _aviso = string.Empty;
    [ObservableProperty] private string _titulo = "Nova despesa";
    [ObservableProperty] private bool _podeExcluir;

    public bool TemAviso => !string.IsNullOrEmpty(Aviso);

    partial void OnAvisoChanged(string value) => OnPropertyChanged(nameof(TemAviso));

    [RelayCommand]
    private async Task CarregarAsync()
    {
        Categorias.Clear();
        foreach (var categoria in await _categorias.GarantirPadraoAsync(_sessao.UsuarioId))
            Categorias.Add(categoria);

        if (!Guid.TryParse(Id, out var id)) return;

        var despesa = await _despesas.ObterAsync(id);
        if (despesa is null) return;

        _idAtual = despesa.Id;
        Titulo = "Editar despesa";
        PodeExcluir = true;

        ValorTexto = despesa.Valor.ToString("0.00");
        Estabelecimento = despesa.Estabelecimento;
        Data = despesa.Data;
        Categoria = Categorias.FirstOrDefault(c => c.Id == despesa.CategoriaId);
    }

    [RelayCommand]
    private async Task SalvarAsync()
    {
        // Aceita tanto vírgula quanto ponto: o teclado numérico do Android
        // oferece um ou outro conforme o idioma do aparelho.
        var texto = ValorTexto.Trim().Replace(',', '.');

        if (!decimal.TryParse(texto,
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture,
                out var valor) || valor <= 0)
        {
            Aviso = "Informe um valor maior que zero.";
            return;
        }

        if (Categoria is null)
        {
            Aviso = "Escolha uma categoria.";
            return;
        }

        var despesa = new Despesa
        {
            Id = _idAtual == Guid.Empty ? Guid.NewGuid() : _idAtual,
            UsuarioId = _sessao.UsuarioId,
            Valor = decimal.Round(valor, 2),
            Data = Data.Date,
            Estabelecimento = Estabelecimento.Trim(),
            CategoriaId = Categoria.Id,
            Origem = TipoOrigem.Manual,
            // No lançamento manual o dado já nasce validado pela pessoa. A
            // confirmação existe para o que é extraído automaticamente.
            Confirmada = true
        };

        await _despesas.SalvarAsync(despesa);
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task ExcluirAsync()
    {
        if (_idAtual == Guid.Empty) return;

        await _despesas.ExcluirAsync(_idAtual);
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private static Task CancelarAsync() => Shell.Current.GoToAsync("..");
}
