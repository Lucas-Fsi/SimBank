## SimBank API

Simbank.api simula operações bancarias reais com cadastro de clientes, abertura de conta, operações bancarias e historico de transações alem de autenticação com JWT. O objetivo é construir uma aplicação completa começando com a api.

## Funcionalidades
Clientes: Cadastro e consulta de clientes com validação de CPF único
Contas: Abertura de contas corrente e poupança vinculadas a clientes
Operações bancárias: Depósito, Saque (com validação de saldo insuficiente) e Transferência entre contas (com atomicidade via transação ACID)
Extrato: Histórico de transações com filtro por período
Autenticação: Registro e login com JWT + hash seguro de senhas (PBKDF2)
Testes automatizados: Cobertura dos cenários de sucesso e falha das operações financeiras


## Tecnologias
Linguagem: C# ASP.NET Core 8
Framework: Entity Framework Core, ORM e migrations 
Banco de dados: PostgreSQL 18
Outros Recursos: JWT (JSON Web Token), xUnit, FluentAssertion, Swagger, OpenAPI 

## Organização
O projeto esta dividido entre Api e testes:
```
SimBankApi/
├── BancoSimulado/
│   ├── Controllers/       # Endpoints da API
│   ├── Models/            # Entidades do domínio (Cliente, Conta, Transacao, Usuario)
│   ├── DTOs/              # Objetos de transferência de dados
│   ├── Data/              # DbContext e configurações do EF Core
│   ├── Services/          # TokenService, SenhaService
│   ├── Middlewares/       # Tratamento centralizado de erros
│   ├── Exceptions/        # Exceções customizadas (NotFoundException, BusinessException...)
│   └── Migrations/        # Histórico de schema do banco
└── SimBank.Tests/
    ├── Helpers/            # DbContextFactory (banco InMemory para testes)
    └── Tests/              # Testes unitários das operações bancárias
```

## Executando o Projeto:

**Pré-requisitos:**

- .NET 8 SDK
- PostgreSQL


**1. Clone o repositório:**
```bash
git clone https://github.com/Lucas-Fsi/SimBank.git
cd SimBank.Api/BancoSimulado
```

**2. Configure a connection string no `appsettings.json`:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=bancosimulado;Username=postgres;Password=SENHA"
}
```

**3. Aplique as migrations:**
```bash
dotnet ef database update
```

**4. Rode a API:**
```bash
dotnet run
```

**5. Acesse o Swagger:**
```
http://localhost:Porta/swagger
```

**Autenticação:**

A API utiliza JWT. Para acessar endpoints protegidos:

1. `POST /api/auth/registrar` — crie um usuário
2. `POST /api/auth/login` — faça login e copie o token retornado
3. No Swagger, clique em **Authorize** e insira:
```
Bearer SEU_TOKEN_AQUI
```

**Principais Endpoints:**

**Auth**

| POST | `/api/auth/registrar` | Registrar usuário |
| POST | `/api/auth/login` | Login e geração de token JWT |

**Clientes**
| GET | `/api/clientes` | Listar todos os clientes |
| GET | `/api/clientes/{id}` | Buscar cliente por ID |
| POST | `/api/clientes` | Cadastrar novo cliente |
| DELETE | `/api/clientes/{id}` | Remover cliente |

**Contas**
| GET | `/api/contas` | Listar todas as contas |
| GET | `/api/contas/{id}` | Buscar conta por ID |
| POST | `/api/contas` | Abrir nova conta |
| POST | `/api/contas/{id}/depositar` | Realizar depósito |
| POST | `/api/contas/{id}/sacar` | Realizar saque |
| POST | `/api/contas/{id}/transferir` | Realizar transferência |
| GET | `/api/contas/{id}/extrato` | Consultar extrato (com filtro por período) |

**Requer token JWT**

**Testes:**

```bash
cd SimBank.Tests
dotnet test
```

Resultado esperado:
```
Aprovado! – Com falha: 0, Aprovado: 7, Ignorado: 0
```

## Cenários testados:
- Depósito com valor válido aumenta o saldo
- Depósito com valor zero lança exceção
- Saque com valor válido reduz o saldo
- Saque com saldo insuficiente lança exceção
- Transferência entre contas atualiza ambos os saldos
- Transferência com saldo insuficiente lança exceção 

## O que o projeto demonstra:

- Transações ACID na transferência: garante que o dinheiro nunca "suma" ou "duplique" em caso de falha
- Middleware centralizado de erros: respostas de erro padronizadas em toda a API
- DTOs: as entidades do banco nunca são expostas diretamente nos endpoints
- Hash seguro de senhas: PBKDF2 com salt aleatório e 100.000 iterações
- JWT stateless: o servidor não armazena sessão
- Banco InMemory nos testes: testes rápidos e isolados

## Melhorias Futuras
- Adicionar Frontend: React ou Angular consumindo a API, com tela de login, dashboard com saldo, formulário de transferência e extrato com gráfico de gastos
- Novas funcionalidades bancárias: Pix simulado com chave por CPF ou email, limite de crédito com cheque especial controlado e rendimento automático mensal na conta poupança
- Melhorias de Segurança: Refresh Token para renovação silenciosa do JWT, Rate Limiting limitando tentativas de login para evitar força bruta, Auditoria registrando qual usuário realizou cada operação e 2FA com código enviado por email no login
- Docker: containerizar a API e o PostgreSQL com Docker Compose, permitindo rodar o projeto inteiro com um único comando sem precisar instalar nada na máquina
