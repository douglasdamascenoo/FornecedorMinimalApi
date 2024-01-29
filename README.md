# MinimalApi
Projeto de estudo de API mínima seguindo o tutorial do Eduardo Pires - Desenvolvedor IO.

Desenvolvimento de uma API mínima baseada em cadastro de fornecedor com autenticação via token - Bearer Authetication.

## CLI 
1. SDKs e Runtimes
- Listar todas as versões instaladas do .NET
> `dotnet --info`

- Listar SDKs instalados
> `dotnet --list-sdks`

- Listar Runtimes instalados
> `dotnet --list-runtimes`

2. CLI Projetos
- Listar tipos de projetos disponíveis para criação
> `dotnet new`
>
> `dotnet new list`

- Criar projeto com versão específica do .NET
> `dotnet new web -o MyWebApp -f netX.X`

- Criar arquivo `global.json` na raiz da pasta de projetos para rodar todos projetos com a versão uma única versão

`{
	"projects": ["src"],
	"sdk": {
		"version": "^6.0.201"
	}
}`

3. CLI Projetos com Solution
- Criar diretório do projeto
> `mkdir MeuApp`

- Navegar para o diretório criado
> `cd .\MeuApp\`

- Criar arquivo de Solução
> `dotnet new sln -n MeuApp`

- Criar diretório `src`
> `mkdir src`

- Navegar para o diretório `src`
> `cd .\src\`

- Criar projeto de aplicação web
> `dotnet new mvc -n MeuWebApp`

- Navegar para o diretório do projeto
> `cd .\MeuWebApp\`

- Adicionar projeto à solução
> `dotnet sln ..\..\MeuApp.sln add .\MeuWebApp.csproj`

4. CLI Migrations
- Comandos EF Migrations para o Context do Identity
Migration Add Initial Version Context
> `dotnet ef migrations add initial-jwt-context --context NetDevPackAppDbContext`

- Update Database Context
> `dotnet ef database update --context NetDevPackAppDbContext`

- Habiliar certificados de dev
> `dotnet dev-certs https --check --trust`
