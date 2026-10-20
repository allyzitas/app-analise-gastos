# Como contribuir

## Branches

| Branch | Para quê |
|---|---|
| `main` | Versão estável. Só recebe código vindo da `develop` em cada entrega. Cada entrega gera uma *tag* (ex.: `v0.1.0`). |
| `develop` | Integração do trabalho em andamento. |
| `feature/<numero-issue>-<descricao>` | Uma funcionalidade. Ex.: `feature/12-leitura-qrcode` |
| `fix/<numero-issue>-<descricao>` | Correção de bug. Ex.: `fix/20-total-mensal` |
| `docs/<descricao>` | Somente documentação. |

Fluxo:

1. Pegue (ou crie) uma issue no quadro do projeto.
2. Crie a branch a partir da `develop`.
3. Faça commits pequenos e frequentes.
4. Abra um Pull Request para a `develop`, citando a issue (`Closes #12`).
5. A outra pessoa da dupla revisa e aprova antes do merge.

**Nunca faça commit direto na `main`.**

## Padrão de commits (Conventional Commits)

```
<tipo>: <descrição curta no imperativo>
```

| Tipo | Uso |
|---|---|
| `feat` | nova funcionalidade |
| `fix` | correção de bug |
| `docs` | documentação |
| `refactor` | mudança de código sem alterar comportamento |
| `test` | testes |
| `chore` | configuração, dependências, build |

Exemplos:

```
feat: adiciona leitura do QR Code da nota fiscal
fix: corrige soma do total mensal
docs: registra decisão sobre banco local
```

## Versões e entregas

Cada entrega da disciplina vira uma *release* no GitHub com tag semântica:

| Tag | Marco |
|---|---|
| `v0.1.0` | Código com 20% do escopo (03/11/2026) |
| `v1.0.0` | Versão apresentada à comissão (24–26/11/2026) |

## Registro de decisões

Decisões de arquitetura ou de tecnologia são registradas em `docs/decisoes/` usando o modelo `0000-modelo.md`.
