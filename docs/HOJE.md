# Plano até a entrega do código — 03/11

Atualizado em sábado, 31/10, depois da primeira execução no emulador.

Meta mínima da entrega: **autenticação, cadastro manual de despesas e lista do
mês, rodando no emulador Android, com os dados gravados no SQLite do aparelho.**

---

## Sábado 31 — concluído ✅

- [x] Repositório clonado do GitHub e trabalho na branch `feature/estrutura-app`
- [x] Telas integradas: Login, Meus gastos e Nova despesa
- [x] Criadas as classes que faltavam no pacote: `ContextoLocal` (banco SQLite)
      e `ServicoAutenticacao` (login na API)
- [x] Pacotes adicionados: `CommunityToolkit.Mvvm` e
      `Microsoft.EntityFrameworkCore.Sqlite`
- [x] Plataformas reduzidas a Android e Windows
- [x] Datas e valores em português do Brasil
- [x] Emulador Pixel 7 criado no disco D: (variável `ANDROID_AVD_HOME`)
- [x] **Teste completo funcionou:** entrar sem servidor → cadastrar despesa →
      ver na lista com o total → fechar e reabrir → a despesa continua lá

**Falta fechar hoje:**

- [ ] Rodar de novo e conferir "outubro de 2026" e "R$ 150,00"
- [ ] Commit, push e pull request da `feature/estrutura-app` para a `develop`:

  ```
  git -c color.ui=false status -s
  git add .
  git commit -m "feat: telas de login, lista e cadastro de despesas com banco local"
  git push -u origin feature/estrutura-app
  ```

  No GitHub: **Compare & pull request**, com *base* = `develop`. Pedir revisão
  ao Arthur.

---

## Domingo 1 — API e login de verdade

Autenticação de ponta a ponta. É a parte que pode dar errado, por isso fica
isolada num dia só.

1. **Criar o banco** no MySQL Workbench:

   ```sql
   CREATE DATABASE gastos CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
   ```

2. **Configurar os segredos da API**, cada um na própria máquina. No `cmd`:

   ```
   cd C:\Projetos\app-analise-gastos\src\Gastos.Api
   dotnet user-secrets set "ConnectionStrings:MySql" "server=localhost;database=gastos;user=root;password=SUA_SENHA"
   dotnet user-secrets set "Jwt:Chave" "uma-frase-longa-e-aleatoria-com-pelo-menos-32-caracteres"
   ```

   O projeto já tem `UserSecretsId`, então não precisa do `user-secrets init`.
   As senhas ficam fora do repositório.

3. **Subir a API**, no mesmo `cmd`:

   ```
   dotnet run
   ```

   Ela já está configurada para a porta 5000. Se o Windows perguntar sobre o
   firewall, permita. Deixe esse `cmd` aberto enquanto testa.

4. **Testar pelo Swagger** em `http://localhost:5000/swagger`: `POST /auth/cadastro`
   e depois `POST /auth/login` com o mesmo e-mail e senha (mínimo 8 caracteres).

5. **Testar pelo aplicativo:** rodar o app no emulador pelo Visual Studio, com a
   API ainda aberta no `cmd`, e usar **Criar conta**. O app deve abrir direto
   na lista de despesas.

6. **Conferir no Workbench** que a senha não foi gravada:

   ```sql
   SELECT Email, SenhaHash FROM gastos.Usuarios;
   ```

   A coluna `SenhaHash` deve mostrar um texto começando com `$2`, nunca a senha.

Se a API não subir hoje, **pare e entregue sem ela**. O aplicativo funciona
inteiro com o banco local, e é exatamente isso que a arquitetura escolhida
permite. Dizer isso no relatório é melhor do que atrasar a entrega.

---

## Segunda 2 — acabamento e release

1. Testar também **editar** e **excluir** uma despesa (tocar na despesa da
   lista abre o formulário de edição).
2. Rodar o aplicativo e tirar prints de cada tela.
3. Atualizar o `README.md` com os prints e o que a versão faz.
4. Fechar as *issues* concluídas no quadro do projeto.
5. Pull request da `develop` para a `main`.
6. Criar a *release* `v0.1.0` no GitHub, apontando para a `main`.

---

## Terça 3 — entrega

Só enviar. Deixe o dia livre para imprevisto.

---

## Se o tempo apertar, corte nesta ordem

1. A sincronização com o servidor. O aplicativo funciona sem ela.
2. A edição e a exclusão de despesas. Cadastrar e listar já demonstram o RF01.
3. O cadastro de conta pelo aplicativo, usando o Swagger para criar o usuário.

**Não corte:** o aplicativo abrir no emulador e gravar uma despesa que
sobrevive ao fechamento. **Isso já está funcionando.**

---

## Pendências para depois da entrega

- Atualizar `SQLitePCLRaw` para a 3.0 (vulnerabilidade CVE-2025-6965 no SQLite)
  e o `MySqlConnector` da API.
- Restringir o tráfego HTTP sem criptografia no Android, liberado só para os
  testes locais.
- No modo "Entrar sem servidor", lembrar a sessão ao reabrir o app (hoje é
  preciso tocar no botão de novo).
