# 🪐 Cosmo Cash

Sistema de registro de vendas de perfumes, com painel de resultados e cadastro de clientes.
Produto da **Nebula**: cada cliente recebe uma instância própria (banco, back e front dedicados), configurada por variáveis de ambiente.

## O que faz

- **Vendas** com vários itens; preço de venda e de custo informados a cada venda (snapshot), forma de pagamento (Pix/Cartão), cliente opcional, edição e cancelamento (soft delete).
- **Painel**: lucro, faturamento, vendas, ticket médio e custo, com comparação contra o período anterior; gráfico por dia/mês, por forma de pagamento e rankings de produtos e marcas.
- **Produtos** e **Clientes** (CPF mascarado na listagem, LGPD) com soft delete.
- **Usuários**: perfis `AdminTotal` (gerencia usuários) e `Socio` (tudo, menos usuários).
- **PWA**: instala no celular e funciona no desktop. Tema escuro (Carbon + paleta Cyber Verde Neon).

## Stack

| Camada | Tecnologia |
|---|---|
| Front | Vite + React + TypeScript, `@carbon/react`, `@carbon/charts-react`, Sass, PWA (`vite-plugin-pwa`) |
| Back | .NET 9 em camadas (`API → Application → Repository → Domain`; `Services`), EF Core 9 |
| Banco | PostgreSQL (Neon) |
| Auth | JWT + BCrypt |

## Estrutura

```
backend/   CosmoCash.sln (Domain, Repository, Services, Application, API)
frontend/  App React (Vite)
```

## Como rodar (desenvolvimento)

**Backend** (precisa do .NET 9 SDK):

```bash
cd backend
cp .env-model .env        # preencha CONNECTION_STRING, JWT_KEY, ADMIN_EMAIL, ADMIN_SENHA
dotnet run --project CosmoCash.API --launch-profile http
```

A API sobe em `http://localhost:5136` (Swagger em `/swagger`). No primeiro start ela aplica as migrations e cria o `AdminTotal` inicial.

**Frontend** (Node 20+):

```bash
cd frontend
cp .env-model .env.local  # ajuste VITE_API_URL e VITE_NOME_LOJA
npm install
npm run dev
```

O front sobe em `http://localhost:5173`.

## Variáveis de ambiente

| Onde | Variável | Para quê |
|---|---|---|
| back | `CONNECTION_STRING` | Postgres (aceita a URI `postgresql://...` do Neon) |
| back | `JWT_KEY` | Chave dos tokens (mín. 32 caracteres, única por cliente) |
| back | `CORS_ORIGINS` | Origens do front, separadas por vírgula |
| back | `ADMIN_EMAIL`, `ADMIN_SENHA`, `ADMIN_NOME` | Primeiro `AdminTotal` (só se ainda não existir) |
| front | `VITE_API_URL` | URL da API |
| front | `VITE_NOME_LOJA` | Nome da loja (cabeçalho, login, app instalado) |

> `.env` e `.env.local` nunca vão para o Git. Variáveis `VITE_` entram no build e ficam visíveis no navegador: não coloque segredos nelas.

## Convenções

- Enums trafegam como texto (`"Socio"`, `"Pix"`, `"Concluida"`); no banco são inteiros.
- Datas em UTC no banco; "o dia" do painel e dos filtros é o dia de **São Paulo**.
- Nada é apagado de verdade: usuários, produtos, clientes e vendas usam desativação/cancelamento.
- Todo o backend (inclusive leitura) exige login.
