namespace Gastos.Shared.Dominio;

/// <summary>
/// Por qual caminho o lançamento entrou no sistema.
/// Permite medir, ao final do projeto, quantas despesas vieram de cada origem —
/// insumo direto para o capítulo de resultados.
/// </summary>
public enum TipoOrigem
{
    Manual = 0,
    QrCode = 1,
    Imagem = 2,
    Boleto = 3,
    Planilha = 4
}

/// <summary>Operação registrada na fila de sincronização.</summary>
public enum TipoOperacao
{
    Insercao = 0,
    Atualizacao = 1,
    Exclusao = 2
}

/// <summary>Situação de um item da fila de sincronização.</summary>
public enum StatusSincronizacao
{
    Pendente = 0,
    Enviando = 1,
    Concluida = 2,
    Falha = 3
}
