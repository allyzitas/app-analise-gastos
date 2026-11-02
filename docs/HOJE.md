# Plano dos 4 dias — entrega do código em 03/11

Meta mínima da entrega: **autenticação, cadastro manual de despesas e lista do
mês, rodando no emulador Android, com os dados gravados no SQLite do aparelho.**
Isso cobre RF01 e a autenticação, e é um 20% do escopo defensável na banca.

---

## Antes de tudo: comece o download agora

No Visual Studio Installer, marque **Desenvolvimento para dispositivos móveis
com .NET** e mande instalar. São vários gigabytes entre carga de trabalho, SDK
do Android e imagem do emulador, e numa conexão doméstica isso leva de uma a
três horas. **Inicie o download antes de ler o resto deste arquivo** e faça as
outras tarefas enquanto ele roda.

Enquanto baixa, já dá para:

- subir o repositório no GitHub, se ainda não estiver lá;
- criar o banco no MySQL Workbench:
  `CREATE DATABASE gastos CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;`
- cadastrar as tarefas abaixo como *issues* no quadro do projeto.

---

## Hoje, sábado 31

O objetivo do dia é um só: **o aplicativo abrir no emulador e salvar uma
despesa.** Se isso funcionar hoje, a entrega está garantida e os outros três
dias viram folga.

1. **Criar o projeto MAUI.** Arquivo → Novo → Projeto → Aplicativo .NET MAUI,
   nome `Gastos.App`, dentro de `src/`. Nas propriedades, deixe só o Android
   como destino.

2. **Adicionar as referências.** No `Gastos.App.csproj`:

   ```xml
   <ItemGroup>
     <PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.0" />
     <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.0" />
   </ItemGroup>
   <ItemGroup>
     <ProjectReference Include="..\Gastos.Shared\Gastos.Shared.csproj" />
   </ItemGroup>
   ```

3. **Copiar os arquivos deste pacote** para dentro de `src/Gastos.App/`,
   mantendo as pastas `Views`, `ViewModels`, `Servicos` e `Dados`. Os arquivos
   `MauiProgram.cs`, `App.xaml.cs`, `AppShell.xaml` e `AppShell.xaml.cs`
   substituem os que o Visual Studio gerou.

4. **Criar o banco na primeira execução.** Em `App.xaml.cs`, dentro do
   construtor, antes de `MainPage`:

   ```csharp
   using (var db = new Gastos.App.Dados.ContextoLocal())
       db.Database.EnsureCreated();
   ```

5. **Rodar no emulador e corrigir o que aparecer.** Esta é a parte que consome
   tempo: na primeira compilação de um projeto MAUI quase sempre falta um
   pacote ou sobra um arquivo do modelo. Reserve a tarde para isso.

6. **Testar o caminho completo:** abrir, tocar em *Entrar sem servidor*,
   cadastrar uma despesa, ver a despesa na lista e o total somado no topo.
   Feche e reabra o aplicativo: a despesa tem que continuar lá.

> O botão **Entrar sem servidor** só existe em compilação de desenvolvimento.
> Ele permite trabalhar nas telas hoje, sem depender da API estar pronta.

---

## Domingo 1

Autenticação de ponta a ponta, que é a parte que pode dar errado e por isso fica
isolada num dia só.

1. Configurar os segredos da API, cada um na própria máquina:

   ```bash
   cd src/Gastos.Api
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:MySql" "server=localhost;database=gastos;user=root;password=SUA_SENHA"
   dotnet user-secrets set "Jwt:Chave" "uma-frase-longa-e-aleatoria-com-pelo-menos-32-caracteres"
   ```

2. `dotnet run` e testar cadastro e login pelo Swagger, em `/swagger`.
3. No emulador, cadastrar uma conta de verdade pelo aplicativo.
4. Conferir no Workbench que o usuário apareceu na tabela e que a coluna de
   senha guarda um resumo criptográfico, não a senha.

Se a API não subir hoje, **pare e entregue sem ela**. O aplicativo funciona
inteiro com o banco local, e isso é exatamente o que a arquitetura escolhida
permite. Dizer isso no relatório é melhor do que atrasar a entrega.

---

## Segunda 2

Acabamento e empacotamento da entrega.

1. Rodar o aplicativo e tirar prints de cada tela.
2. Atualizar o `README.md` com os prints e o que a versão faz.
3. Fechar as *issues* concluídas no quadro.
4. Criar a *release* `v0.1.0` no GitHub, apontando para a `main`.

---

## Terça 3 — entrega

Só enviar. Deixe o dia livre para imprevisto.

---

## Se o tempo apertar, corte nesta ordem

1. A sincronização com o servidor. O aplicativo funciona sem ela.
2. A edição e a exclusão de despesas. Cadastrar e listar já demonstram o RF01.
3. O cadastro de conta pelo aplicativo, usando o Swagger para criar o usuário.

**Não corte:** o aplicativo abrir no emulador e gravar uma despesa que
sobrevive ao fechamento. Sem isso não existe código funcional para entregar.
