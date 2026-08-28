# API Biblioteca — Livros e Empréstimos

API REST em ASP.NET Core (.NET 10) para uma biblioteca controlar livros e empréstimos, seguindo a arquitetura em camadas:

```
Controller → Service → Repository → DbContext → Banco de Dados
```

Construída com o mesmo "esqueleto" técnico do seu projeto `ApiProdutoCategoria` (mesmo TargetFramework, mesmo padrão de `appsettings.json`, EF Core + SQL Server), mas atendendo a **todos** os requisitos do desafio: entidades Livro/Empréstimo, Repositories separados dos Services, regras de negócio e migrations.

---

## 1. Estrutura do projeto

```
ApiBiblioteca/
└── ApiBiblioteca/
    ├── Controllers/
    │   ├── LivrosController.cs
    │   └── EmprestimosController.cs
    ├── Services/
    │   ├── LivroService.cs
    │   └── EmprestimoService.cs
    ├── Repositories/
    │   ├── LivroRepository.cs
    │   └── EmprestimoRepository.cs
    ├── Models/
    │   ├── Livro.cs
    │   └── Emprestimo.cs
    ├── Data/
    │   └── AppDbContext.cs
    ├── Exceptions/           (NotFoundException e ConflictException)
    ├── Migrations/
    ├── Program.cs
    ├── appsettings.json
    └── ApiBiblioteca.http    (requisições prontas para testar)
```

Sem interfaces e sem middleware — tudo com classes concretas e `try/catch` direto no Controller, no mesmo estilo do seu `ApiProdutoCategoria` (veja `CategoriaController`, que já fazia `try { ... } catch (Exception ex) { return BadRequest(ex.Message); }`).

**Responsabilidade de cada camada** (fale isso na apresentação):
- **Controller**: recebe a requisição HTTP, chama o Service dentro de um `try/catch` e devolve o `ActionResult` certo (`NotFound`, `Conflict`, `Ok`...). Não tem regra de negócio nem acessa o banco.
- **Service** (`LivroService`, `EmprestimoService`): concentra **toda** a regra de negócio (ISBN duplicado, disponibilidade do livro, devolução, etc). Recebe o Repository no construtor e nunca importa o `AppDbContext`.
- **Repository** (`LivroRepository`, `EmprestimoRepository`): é a **única** classe que enxerga o `AppDbContext`. Só sabe "salvar", "buscar", "atualizar", "remover" — não sabe nada de regra de negócio.
- **DbContext**: mapeamento das entidades para as tabelas do banco (`Data/AppDbContext.cs`), incluindo o índice único do ISBN e o relacionamento Livro 1—N Empréstimo.
- **Exceptions** (`NotFoundException`, `ConflictException`): duas classes bem simples que o Service lança com uma mensagem (ex: `throw new ConflictException("ISBN duplicado")`), e o Controller captura no `catch` para decidir o status HTTP.

## 2. Entidades e relacionamento

- **Livro**: `Id`, `Titulo`, `Autor`, `Isbn` (único), `AnoPublicacao`, `Editora`, `Disponivel`.
- **Emprestimo**: `Id`, `LivroId` (FK), `NomeUsuario`, `DataEmprestimo`, `DataDevolucao` (nulo até a devolução).
- Relação: **1 Livro → N Empréstimos** (um livro pode ter vários empréstimos ao longo do tempo, mas nunca dois *ativos* ao mesmo tempo — isso é regra de negócio, não é modelado como restrição estrutural).

## 3. Data Annotations (Models)

Exemplos usados em `Livro.cs` e `Emprestimo.cs`: `[Required]`, `[StringLength]`, `[Range]`, `[RegularExpression]` (valida que o ISBN só tem números/hífens). Ao enviar um `POST /api/livros` sem `titulo`, por exemplo, o próprio `[ApiController]` já devolve `400 Bad Request` automaticamente antes de chegar no Service — mostre isso ao vivo no Postman.

## 4. Regras de negócio implementadas (nos Services)

| # | Regra | Onde | Exceção lançada | HTTP |
|---|-------|------|------------------|------|
| 1 | ISBN não pode se repetir | `LivroService.CriarAsync` / `AtualizarAsync` | `ConflictException` | 409 |
| 2 | Livro precisa existir para emprestar | `EmprestimoService.CriarAsync` | `NotFoundException` | 404 |
| 3 | Livro precisa estar disponível | `EmprestimoService.CriarAsync` | `ConflictException` | 409 |
| 4 | Devolução seta `DataDevolucao` e `Disponivel = true` | `EmprestimoService.DevolverAsync` | — | 200 |
| 5 | Não pode devolver duas vezes | `EmprestimoService.DevolverAsync` | `ConflictException` | 409 |

O Service lança `NotFoundException`/`ConflictException` com a mensagem já pronta, e cada método do Controller tem seu próprio `try/catch` decidindo o status HTTP — igual ao padrão do `CategoriaController` do seu outro projeto, só que diferenciando 404 de 409.

## 5. Endpoints

**Livros**
```
GET    /api/livros
GET    /api/livros/{id}
POST   /api/livros
PUT    /api/livros/{id}
DELETE /api/livros/{id}
```

**Empréstimos**
```
GET  /api/emprestimos
GET  /api/emprestimos/{id}
POST /api/emprestimos            { "livroId": "...", "nomeUsuario": "..." }
PUT  /api/emprestimos/{id}/devolver
```

Use o arquivo `ApiBiblioteca.http` (ou importe as mesmas requisições no Postman) — já tem exemplos de sucesso e de erro (ISBN duplicado, livro indisponível, livro inexistente, devolução repetida).

---

## 6. O que você precisa fazer para rodar o projeto

Eu não tenho como instalar o SDK do .NET nem rodar `dotnet ef` neste ambiente (sem acesso à internet/CLI aqui), então **as migrations foram escritas manualmente** no mesmo formato que o `dotnet ef migrations add` geraria. Antes de apresentar, siga estes passos na sua máquina:

### 6.1 Pré-requisitos
- .NET SDK 10 instalado (`dotnet --version`)
- SQL Server LocalDB (já vem com o Visual Studio) ou ajuste a `ConnectionString` em `appsettings.json` para o seu SQL Server/Docker.

### 6.2 Restaurar pacotes e validar a build
```bash
cd ApiBiblioteca/ApiBiblioteca
dotnet restore
dotnet build
```

### 6.3 Conferir/gerar as migrations
As migrations já estão na pasta `Migrations/`. Como recomendação **forte**: apague a migration incluída e gere de novo com o EF Tools, para garantir 100% de compatibilidade com a versão do seu SDK/pacotes instalados:

```bash
dotnet tool install --global dotnet-ef   # se ainda não tiver
dotnet ef migrations remove              # remove a que eu escrevi à mão (opcional)
dotnet ef migrations add CriaDbBiblioteca
dotnet ef database update
```

Se preferir manter a migration que já está no projeto, basta rodar:
```bash
dotnet ef database update
```

### 6.4 Rodar a API
```bash
dotnet run
```
A API sobe em `http://localhost:5180` (ajustável em `Properties/launchSettings.json`). Acesse `/openapi/v1.json` (Development) ou use o `.http`/Postman.

---

## 7. Roteiro sugerido para a apresentação (5–15 min)

1. **Estrutura**: abra `Controllers/`, `Services/`, `Repositories/`, `Models/`, `Data/` e explique a responsabilidade de cada um (seção 1 acima).
2. **Banco de dados**: mostre `Migrations/` gerada e o relacionamento Livro → Empréstimo no `AppDbContext.OnModelCreating`.
3. **Data Annotation**: tente um `POST /api/livros` sem `titulo` no Postman → mostre o `400` automático.
4. **Regra de negócio**: empreste um livro (`POST /api/emprestimos`) e tente emprestar de novo o mesmo `livroId` → mostre o `409 Conflict` vindo do `EmprestimoService`.
5. **Repository**: abra `LivroRepository.cs`/`EmprestimoRepository.cs` e explique: é a única classe que injeta `AppDbContext`; o Service não sabe nada de EF Core, só chama métodos como `ObterPorIdAsync`/`AdicionarAsync`.
6. **Demonstração no Postman**: `POST /api/livros` (sucesso) → `GET /api/livros` → `PUT /api/livros/{id}` → um erro (ISBN duplicado ou empréstimo de livro indisponível).

---

## 8. Próximos passos que você pode adicionar (opcional, se sobrar tempo)

- Autenticação (JWT) nos endpoints.
- Paginação em `GET /api/livros` e `GET /api/emprestimos`.
- Interfaces (`ILivroRepository`, `ILivroService`, etc.) + injeção de dependência por interface, se quiser aplicar Inversão de Dependência e facilitar testes automatizados com mocks.
- DTOs de entrada/saída em vez de expor as entidades diretamente nos Controllers.
