
---

## 📌 Função de Cada Camada

### **Domain**
- Contém as **entidades ricas** do domínio (`Process`, `Step`, `Execution`, `User`, `Role`, etc.)
- Contém as **interfaces de repositório** e **exceções de domínio**
- **Não tem nenhuma dependência externa**
- É o **coração do sistema**

### **Application**
- Contém **casos de uso (App Services)** e **DTOs**
- Orquestra regras usando o Domain + Repositórios
- **Não conhece a infraestrutura**
- Faz **validação de fluxo, não de regra de negócio**

### **Infrastructure**
- Contém o **EF Core + MySQL**
- Implementação dos **Repositories**
- Implementa o **UnitOfWork**
- Migrations e DbContext

### **API**
- Controllers REST
- Autenticação JWT
- Validações externas
- Apenas chama a aplicação

---

## 🛠️ Tecnologias e Pacotes

| Componente | Versão |
|------------|---------|
| .NET | `8.0` |
| EF Core | `8.0.x` |
| Pomelo MySQL | `9.0.x` |
| JWT Bearer | `Microsoft.AspNetCore.Authentication.JwtBearer` |
| BCrypt para senha | `BCrypt.Net-Next` |
| Swagger | `Swashbuckle` |

---

## 🔑 Autenticação e Segurança

- Login via `/auth/login`
- Retorno: `AccessToken (JWT Bearer)`
- Controle de acesso:
  - **User → Role → Screen → Permission**
- Permissões controlam:
  - `CanView`, `CanCreate`, `CanEdit`, `CanDelete`

---

## 🧬 Fluxo de Negócio (Resumo)

Exemplo real aplicado ao projeto:

**Fluxo A**
1. Passo Inicial 
2. Passo de Aprovação
3. Passo de Revisão

Sistema controla:
✔ qual papel executa cada etapa  
✔ histórico (AuditLog)  
✔ próxima etapa automática  

---

## 🗄️ Database

Principais tabelas:

| Tabela | Função |
|---------|---------|
| `Department` | Setores |
| `User` e `Role` | Autenticação e permissão |
| `Screen` | Menus/Telas |
| `RoleScreenPermission` | ACL |
| `Process / ProcessStep / ProcessExecution` | Motor de fluxo |
| `AuditLog` | Trilha de auditoria |

---

## 🚀 Como Rodar

### 1️⃣ Configure o `appsettings.json`
```json
"ConnectionStrings": {
  "BpmMySql": "server=processdb.mysql.dbaas.com.br;database=processdb;user=processdb;password=%"
}
```

### 2️⃣ Rodar migrations
```shell
cd src/BusinessProcessManagement/FSI.BusinessProcessManagement.Infrastructure
dotnet ef database update
```

### 3️⃣ Rodar API
```shell
cd ../FSI.BusinessProcessManagement.Api
dotnet run
```

### Swagger:
## 📌 https://localhost:5001/swagger

### 👤 Usuário Padrão (Admin)
## Campo	Valor
## Usuário	admin
## Senha	admin123
## Perfil	Administrador

### 📝 Melhorias Futuras (Backlog)

## ✅ Notificações por e-mail

## ✅ Logs de erro centralizados

## 🔲 Integração com filas (RabbitMQ)

## 🔲 Interface Web SPA (React ou Angular)

## 🔲 API Gateway para múltiplos módulos

### 👔 Créditos

## Projeto desenvolvido como referência arquitetural para ambientes corporativos que exigem segurança, rastreabilidade, regras claras de negócio e workflows auditáveis.