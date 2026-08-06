# ControleDeGasto.API

API REST em ASP.NET Core do Controle de Gasto. Autenticação por **cookie** (ASP.NET Core Identity),
proteção CSRF via Antiforgery e rate limiting por IP.

Persistência em **PostgreSQL** através do Npgsql + Entity Framework Core.

## Pré-requisitos

| Ferramenta | Versão usada no projeto |
|---|---|
| .NET SDK | 10.0.x |
| PostgreSQL | qualquer instância acessível (local ou Neon) |
| `dotnet-ef` | 10.0.x |

Instale a ferramenta de migrations, se ainda não tiver:

```bash
dotnet tool install --global dotnet-ef
```

## Configuração inicial

### 1. Criar o `appsettings.Development.json`

O [appsettings.json](ControleDeGasto.API/appsettings.json) versionado funciona como **template**:
todas as chaves existem, mas com valores vazios. Os valores reais ficam em
`appsettings.Development.json`, que **não é versionado** (contém a senha do banco).

Crie o arquivo em `ControleDeGasto.API/appsettings.Development.json`:

```jsonc
{
  "ConnectionStrings": {
    // Substitua pelos dados da sua instância PostgreSQL
    "DefaultConnection": "Host=SEU_HOST; Database=SEU_BANCO; Username=SEU_USUARIO; Password=SUA_SENHA; SSL Mode=VerifyFull; Channel Binding=Require;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Identity": {
    "AllowedUserNameCharacters": "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890_.- "
  },
  "XSRF": {
    "XSRF_HEADER_NAME": "X-XSRF-TOKEN",
    "XSRF_COOKIE_NAME": "XSRF-TOKEN"
  }
}
```

Os valores de `XSRF` **não são livres**: precisam ser idênticos aos constantes em
`src/app/core/config/global.ts` do front-end. Divergência aqui faz toda requisição de escrita
retornar 400.

Para banco local sem TLS, remova `SSL Mode` e `Channel Binding` da connection string.

### 2. Aplicar as migrations

```bash
cd ControleDeGasto.API
dotnet ef database update
```

As roles padrão são criadas automaticamente na inicialização, pelo `RoleSeender` chamado no
[Program.cs](ControleDeGasto.API/Program.cs).

### 3. Confiar no certificado de desenvolvimento

A API redireciona tudo para HTTPS (`UseHttpsRedirection`):

```bash
dotnet dev-certs https --trust
```

## Executando

```bash
cd ControleDeGasto.API
dotnet run --launch-profile https
```

| Endereço | Uso |
|---|---|
| `https://localhost:7104` | HTTPS — é o que o front-end consome |
| `http://localhost:5135` | HTTP (redirecionado para HTTPS) |
| `https://localhost:7104/openapi/v1.json` | Documento OpenAPI (só em Development) |

O front-end ([ControleDeGasto.FrontEnd](../ControleDeGasto.FrontEnd)) precisa rodar em
`https://localhost:4201` — é a única origem liberada na política de CORS `AngularSpaPolicy`.
Alterar a porta de um lado exige alterar o outro.

## Endpoints de autenticação

Todos sob `api/auth`. Controllers nascem com `[Authorize]` por causa da `FallbackPolicy`
configurada no `Program.cs` — o acesso anônimo é exceção explícita.

| Método | Rota | Anônimo | CSRF | Rate limit |
|---|---|---|---|---|
| `GET` | `csrf-token` | sim | — | global |
| `GET` | `me` | não | — | global |
| `POST` | `register` | sim | sim | 3 / 5 min |
| `POST` | `login` | sim | sim | 5 / 1 min |
| `POST` | `logout` | não | sim | global |

Rate limit global: 100 requisições por minuto por IP. Ao exceder, a resposta é `429` com corpo
JSON. O login também aplica bloqueio do Identity: 5 tentativas falhas bloqueiam a conta por
15 minutos (`423`).

### Fluxo esperado pelo cliente

1. `GET api/auth/csrf-token` → grava o cookie `XSRF-TOKEN` (legível por JS, `HttpOnly = false`)
2. `POST api/auth/login` com o header `X-XSRF-TOKEN` → grava o cookie de sessão e reemite o token
3. Requisições seguintes enviam ambos os cookies; `POST`/`PUT`/`PATCH`/`DELETE` exigem o header

O cookie de sessão usa `HttpOnly`, `Secure = Always`, `SameSite = Strict` e expira em 60 minutos
com renovação deslizante.

## Regras de senha e usuário

Definidas no `Program.cs`: mínimo 8 caracteres, exigindo maiúscula, minúscula, dígito e caractere
não alfanumérico. `RequireUniqueEmail` está desabilitado — a identificação é por `UserName`.

## Migrations

```bash
cd ControleDeGasto.API

dotnet ef migrations add NomeDaMigration
dotnet ef database update
dotnet ef migrations list
```

## Arquivos fora do controle de versão

O `.gitignore` fica na **raiz do repositório**, e não na pasta do projeto — regras de `.gitignore`
só valem para o próprio diretório e abaixo. Mantendo-o na raiz, qualquer projeto novo
(`ControleDeGasto.API.Tests/`, por exemplo) já nasce protegido.

| Caminho | Motivo |
|---|---|
| `appsettings.Development.json`, `appsettings.Local.json` | Connection string e segredos |
| `.env`, `.env.*` | Variáveis de ambiente (exceto `.env.example`) |
| `*.pem`, `*.key`, `*.crt`, `*.cer`, `*.p12`, `*.pfx`, `ssl/` | Certificados e chaves privadas |
| `bin/`, `obj/` | Saída de compilação |
| `.vs/`, `*.user` | Arquivos de IDE e preferências pessoais |

Nunca preencha a connection string real no `appsettings.json` versionado — é o erro mais fácil
de cometer aqui, e ele expõe a credencial do banco no histórico do Git.

## Problemas comuns

**`dotnet ef database update` falha na conexão**
`appsettings.Development.json` não existe ou a connection string está incorreta. O
`appsettings.json` versionado tem `DefaultConnection` vazio de propósito.

**Login funciona no Postman mas não no navegador**
O cookie de sessão exige HTTPS e mesma origem (`SameSite=Strict`). Acesse o front-end via
`https://localhost:4201`, não `http://`.

**`POST` retorna 400 sem mensagem clara**
Validação de Antiforgery. Confirme que o cliente chamou `GET api/auth/csrf-token` antes e que
`XSRF_HEADER_NAME` na API coincide com o header enviado pelo front-end.

**`NullReferenceException` na inicialização**
`Identity:AllowedUserNameCharacters` ou `XSRF:XSRF_HEADER_NAME` estão ausentes na configuração.
Ambos são lidos com `!` (null-forgiving) no `Program.cs`.

**Erro de CORS no navegador**
O front-end está em porta diferente de `4201`. Ajuste a origem em `AngularSpaPolicy`
(`Program.cs`) ou a porta em `angular.json`.
