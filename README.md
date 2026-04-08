# Sistema de Gerenciamento de Produtos, Categorias e Clientes (SIC)

## 📋 Descrição

Sistema desenvolvido com arquitetura DDD (Domain-Driven Design) em .NET 8 para gerenciamento de:
- **Produtos**: CRUD completo com validações
- **Categorias**: Organização de produtos
- **Clientes**: Gerenciamento de informações de clientes

## 🏗️ Arquitetura

```
TesteDDD.Domain/           → Entities, Repositories (interfaces)
TesteDDD.Application/      → Services, DTOs, Exceptions
TesteDDD.Infrastructure/   → Data Context, Repository implementations
TesteDDD.Api/              → Controllers, API endpoints
TesteDDD.Communication/    → Requests e Responses DTOs
```

## 🔧 Tecnologias

- **.NET 8.0**
- **Entity Framework Core** - ORM
- **SQL Server** - Banco de dados
- **Swagger/OpenAPI** - Documentação de API
- **xUnit** - Testes de integração

## 📦 Instalação

### Pré-requisitos
- .NET 8.0 SDK
- SQL Server (local ou remoto)

### Configuração

1. **Clone o repositório**
```bash
git clone <repository-url>
cd SIC
```

2. **Configure a connection string** em `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SIC_DB;Trusted_Connection=true;"
  }
}
```

3. **Execute as migrations**
```bash
dotnet ef database update --project src/Backend/TesteDDD.Infrastructure --startup-project src/Backend/TesteDDD.Api
```

4. **Inicie a aplicação**
```bash
cd src/Backend/TesteDDD.Api
dotnet run
```

A API estará disponível em: `https://localhost:7190`
Swagger em: `https://localhost:7190/swagger`

## 📚 Endpoints

### Produtos
- `GET /api/produto` - Listar todos
- `GET /api/produto/{id}` - Obter por ID
- `POST /api/produto` - Criar novo
- `PUT /api/produto/{id}` - Atualizar
- `DELETE /api/produto/{id}` - Deletar

### Categorias
- `GET /api/categoria` - Listar todos
- `GET /api/categoria/{id}` - Obter por ID
- `POST /api/categoria` - Criar nova
- `PUT /api/categoria/{id}` - Atualizar
- `DELETE /api/categoria/{id}` - Deletar

### Clientes
- `GET /api/cliente` - Listar todos
- `GET /api/cliente/{id}` - Obter por ID
- `POST /api/cliente` - Criar novo
- `PUT /api/cliente/{id}` - Atualizar
- `DELETE /api/cliente/{id}` - Deletar

## 📝 Exemplos de Requisição

### Criar Categoria
```bash
POST /api/categoria
Content-Type: application/json

{
  "name": "Eletrônicos",
  "descricao": "Produtos eletrônicos diversos"
}
```

### Criar Produto
```bash
POST /api/produto
Content-Type: application/json

{
  "nome": "Notebook",
  "preco": 2500.00,
  "categoriaId": "550e8400-e29b-41d4-a716-446655440000"
}
```

### Criar Cliente
```bash
POST /api/cliente
Content-Type: application/json

{
  "nome": "João Silva",
  "endereco": "Rua Principal, 123",
  "cep": "12345-678"
}
```

## ✅ Melhorias Implementadas

### 1. **Correção de Relacionamentos**
- ✅ Corrigido relacionamento Categoria-Produto (1:N)
- ✅ Removido ProdutoId de Categoria
- ✅ Adicionado ICollection<Produto> em Categoria

### 2. **Validações Melhoradas**
- ✅ Validação de CEP (formato e dígitos)
- ✅ MinLength em nomes (3 caracteres)
- ✅ MinLength em descrições (5 caracteres)
- ✅ Validação de Categoria antes de criar Produto
- ✅ ArgumentException em Entidades (ao invés de Exception)

### 3. **Correções de Tipo**
- ✅ CategoriaId: `int` → `Guid`
- ✅ Propriedades privadas em Cliente
- ✅ Construtor correto em Produto

### 4. **Melhorias no Serviço**
- ✅ ProdutoService injeta ICategoriaRepository
- ✅ Validação de existência de Categoria
- ✅ Mensagens de erro corrigidas (ortografia)

### 5. **DTOs Melhorados**
- ✅ Removido ProdutoId de RequestCategoriaJson
- ✅ Adicionado CategoriaId em ResponseProdutoJson
- ✅ Validações com MinLength
- ✅ Mensagens corrigidas

### 6. **Tratamento de Erros**
- ✅ CustomBusinessRuleException com código de erro
- ✅ Mensagens de erro estruturadas
- ✅ ProblemDetails em respostas HTTP

## 🧪 Testes

```bash
# Executar testes de integração
dotnet test tests/TesteDDD.Api.IntegrationTests
```

## 🐛 Tratamento de Erros

A API retorna respostas padronizadas:

**Erro de validação (400):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Erro de regra de negócio.",
  "status": 400,
  "detail": "Nome do produto é obrigatório.",
  "instance": "/api/produto",
  "code": "PRODUTO_NOME_INVALIDO"
}
```

**Recurso não encontrado (404):**
```json
{
  "message": "Produto com ID xxx não encontrado."
}
```

## 📖 Padrões Utilizados

- **DDD** (Domain-Driven Design)
- **Repository Pattern** - Abstração de dados
- **Service Pattern** - Lógica de negócio
- **DTO Pattern** - Transfer de dados
- **Exception Handling** - Tratamento centralizado

## 🔐 Segurança

- ✅ Validação de entrada em todos endpoints
- ✅ SQL Injection prevenido (EF Core parameterizado)
- ✅ Mensagens de erro seguras
- ✅ HTTPS habilitado

## 📌 Roadmap Futuro

- [ ] Autenticação e Autorização (JWT)
- [ ] Paginação em listagens
- [ ] Filtros avançados
- [ ] Soft Delete
- [ ] Auditoria
- [ ] Cache (Redis)
- [ ] Docker

## 👨‍💻 Contribuindo

1. Criar branch para feature (`git checkout -b feature/AmazingFeature`)
2. Commit mudanças (`git commit -m 'Add AmazingFeature'`)
3. Push para branch (`git push origin feature/AmazingFeature`)
4. Abrir Pull Request

## 📄 Licença

Este projeto está sob licença MIT.

---

**Versão**: 1.0.0  
**Última atualização**: Abril 2026