# ADR-0001: Repositório único (monorepo) para App e API

- **Data:** 2026-10-17
- **Status:** aceita
- **Responsável:** Alanis Ortolani

## Contexto

O sistema tem duas partes em C#: o aplicativo .NET MAUI (com banco local, cache e fila de sincronização) e a API que sincroniza os dados com o servidor. A equipe é uma dupla, e o projeto deve ficar centralizado no GitHub (código, quadro de tarefas e registro de decisões).

## Decisão

Manter App, API e uma biblioteca compartilhada (`Shared`, com modelos e contratos de sincronização) em um único repositório e em uma única solução do Visual Studio, organizados em `src/`.

Fluxo de branches: `main` (estável, recebe código a cada entrega), `develop` (integração) e branches `feature/`, `fix/` e `docs/` integradas por Pull Request com revisão da dupla.

## Alternativas consideradas

- Dois repositórios (App e API separados) — exigiria versionar e publicar o projeto `Shared` à parte e dividiria issues e quadro de tarefas.
- GitHub Flow puro (apenas `main` + branches curtas) — mais simples, mas sem um ponto estável que corresponda às entregas da disciplina.

## Consequências

- Uma mudança no contrato de sincronização altera App e API no mesmo Pull Request.
- Um único quadro (GitHub Projects) e um único histórico de decisões.
- Cada entrega vira uma release com tag (`v0.1.0` para os 20% do escopo, `v1.0.0` para a apresentação).
