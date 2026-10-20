# Sistema de Análise de Gastos

Projeto Integrador — Curso Superior de Tecnologia em Análise e Desenvolvimento
de Sistemas, IFSP Câmpus Guarulhos.

Alanis Angelo Ortolani e Arthur Alves Lima
Orientador: Prof. Giovani Fonseca

---

## O que já está aqui

Este repositório contém o esqueleto da sprint 1. O código das camadas de dados,
autenticação e sincronização está escrito; as telas ainda não.

```
Gastos.sln
src/
  Gastos.Shared/      biblioteca compartilhada: classes de domínio e contrato da API
  Gastos.Api/         ASP.NET Core — autenticação e sincronização (MySQL)
  Gastos.App/         arquivos do aplicativo .NET MAUI (SQLite)
```

`Gastos.Shared` não depende de nenhuma biblioteca externa. Isso é deliberado:
as mesmas classes de domínio são usadas pelo aplicativo e pela API, e cada lado
declara seu próprio mapeamento. Não existe risco de os dois divergirem.

---

## Antes de rodar

Instale:

- **Visual Studio 2022** ou superior, com as cargas de trabalho
  *Desenvolvimento para dispositivos móveis com .NET* e
  *Desenvolvimento web e ASP.NET*;
- **.NET SDK 10**;
- **MySQL Server** e **MySQL Workbench**.

No MySQL Workbench, crie o banco vazio:

```sql
CREATE DATABASE gastos CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

As tabelas são criadas sozinhas na primeira execução da API.

---

## Criar o projeto do aplicativo

O projeto MAUI não está no repositório porque precisa ser gerado pela ferramenta.
Faça uma vez:

1. No Visual Studio: **Arquivo → Novo → Projeto → Aplicativo .NET MAUI**.
2. Nome `Gastos.App`, local `src/`. Isso cria `src/Gastos.App/Gastos.App.csproj`
   ao lado das pastas `Dados/` e `Servicos/` que já existem aqui.
3. Em **Propriedades do projeto**, deixe apenas o Android como destino.
4. Adicione a referência a `Gastos.Shared` e os pacotes:

```
Microsoft.EntityFrameworkCore.Sqlite
CommunityToolkit.Mvvm
```

5. Em `MauiProgram.cs`, registre os serviços:

```csharp
builder.Services.AddDbContext<ContextoLocal>();
builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(EnderecoApi) });
builder.Services.AddSingleton<ServicoAutenticacao>();
builder.Services.AddTransient<RepositorioDespesa>();
```

Onde `EnderecoApi` é `http://10.0.2.2:5000` para o emulador Android
(`10.0.2.2` é como o emulador enxerga o `localhost` da máquina) ou o IP da
máquina na rede local, se o teste for em aparelho físico.

6. Na primeira execução, crie o banco local:

```csharp
using var db = new ContextoLocal();
db.Database.EnsureCreated();
```

---

## Configurar os segredos da API

A senha do MySQL e a chave do JWT **não** vão para o `appsettings.json`, que é
versionado. Use o cofre local do .NET:

```bash
cd src/Gastos.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:MySql" "server=localhost;database=gastos;user=root;password=SUA_SENHA"
dotnet user-secrets set "Jwt:Chave" "uma-frase-longa-e-aleatoria-com-pelo-menos-32-caracteres"
```

Cada integrante faz isso na própria máquina. Os valores ficam fora do Git.

---

## Rodar

```bash
cd src/Gastos.Api
dotnet run
```

A API sobe e o Swagger abre em `/swagger`, onde dá para testar o cadastro e o
login sem precisar do aplicativo pronto.

---

## Endpoints

| Método | Rota | Protegido | O que faz |
|---|---|---|---|
| POST | `/auth/cadastro` | não | Cria a conta e devolve o token |
| POST | `/auth/login` | não | Autentica e devolve o token |
| POST | `/sinc` | sim | Envia a fila e recebe o que mudou no servidor |

As rotas protegidas exigem o cabeçalho `Authorization: Bearer <token>`.

---

## Decisões que o código implementa

Cada uma está justificada no Capítulo 2 do relatório técnico.

**Identificadores em `Guid`.** O dispositivo precisa criar registros sem
consultar o servidor. Inteiros sequenciais colidiriam entre aparelhos.

**Exclusão lógica (`Excluido`).** Apagar a linha do banco local faria a
sincronização seguinte restaurar o registro a partir do servidor.

**Conflito pela alteração mais recente (`AtualizadoEm`).** Quando o mesmo
registro muda nos dois lados, vence o mais recente. A versão preterida se perde
— limitação assumida e registrada no relatório.

**Senha com BCrypt.** O banco guarda o resumo criptográfico, nunca a senha.

**Cadastro só com e-mail.** Nome e documentos não são coletados. É a aplicação
concreta do princípio da necessidade, tema de pesquisa atribuído pelo
orientador.

**Credenciais no servidor.** A cadeia de conexão do MySQL e a chave do JWT ficam
na API. O pacote instalado no dispositivo não as contém.

---

## Fluxo de trabalho

Cada item do backlog é desenvolvido em uma ramificação própria e entra na
`main` por *pull request*, revisada pelo outro integrante. Isso mantém a `main`
sempre executável e faz com que os dois conheçam todas as partes do sistema —
o que importa na apresentação, já que a comissão pode perguntar qualquer parte
a qualquer um.
