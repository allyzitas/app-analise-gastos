using Gastos.App.Dados;
using Gastos.App.Servicos;
using Gastos.App.ViewModels;
using Gastos.App.Views;
using Microsoft.Extensions.Logging;

namespace Gastos.App;

public static class MauiProgram
{
    /// <summary>
    /// O emulador do Android enxerga o localhost da máquina como 10.0.2.2.
    /// Em aparelho físico, troque pelo IP da máquina na rede local.
    /// </summary>
    private const string EnderecoApi = "http://10.0.2.2:5000";

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Dados. O contexto é transitório de propósito: um DbContext longo
        // acumula entidades rastreadas e passa a devolver dados desatualizados.
        builder.Services.AddTransient<ContextoLocal>();
        builder.Services.AddTransient<RepositorioDespesa>();
        builder.Services.AddTransient<RepositorioCategoria>();

        // Rede e sessão.
        builder.Services.AddSingleton(new HttpClient
        {
            BaseAddress = new Uri(EnderecoApi),
            // Sem isso, uma API fora do ar deixa a tela de login travada por
            // mais de um minuto.
            Timeout = TimeSpan.FromSeconds(10)
        });
        builder.Services.AddSingleton<ServicoAutenticacao>();
        builder.Services.AddSingleton<Sessao>();

        // Telas.
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<DespesasViewModel>();
        builder.Services.AddTransient<DespesasPage>();
        builder.Services.AddTransient<DespesaFormViewModel>();
        builder.Services.AddTransient<DespesaFormPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
