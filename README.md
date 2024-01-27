# MinimalApi
## Fornecedor Minimal API
Projeto de estudo de API mínima seguindo o tutorial do Eduardo Pires - Desenvolvedor IO.

Desenvolvimento de uma API mínima baseada em cadastro de fornecedor com autenticação via token - Bearer Authetication.

## Comandos CLI para este e outros projetos
- Listar todas as versões instaladas do .NET
> `dotnet --info`

- Listar SDKs instalados
> `dotnet --list-sdks`

- Listar Runtimes instalados
> `dotnet --list-runtimes`

- Listar tipos de projetos disponíveis para criação
> `dotnet new`
>
> `dotnet new list`

- Criar projeto com versão específica do .NET
> `dotnet new web -o MyWebApp -f net5.0`

- Criar arquivo `global.json` na raiz da pasta de projetos para rodar todos projetos com a versão uma única versão

`{
	"projects": ["src"],
	"sdk": {
		"version": "^6.0.201"
	}
}`


## Outro comandos importantes
- Comandos EF Migrations para o Context do Identity
Migration Add Initial Version Context
> `dotnet ef migrations add initial-jwt-context --context NetDevPackAppDbContext`

- Update Database Context
> `dotnet ef database update --context NetDevPackAppDbContext`

- Habiliar certificados de dev
> `dotnet dev-certs https --check --trust`
