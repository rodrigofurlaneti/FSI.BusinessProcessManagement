# 🧩 FSI.BusinessProcessManagement.Domain

## 📘 Visão Geral

A camada **Domain** representa o **núcleo do domínio de negócios** do sistema **Business Process Management (BPM)**.  
Aqui estão concentradas todas as **entidades**, **regras de negócio**, **validações**, **interfaces de abstração** e **serviços de domínio** que regem o comportamento interno da aplicação.

Ela é **totalmente independente de infraestrutura, UI e banco de dados**, sendo a base para todas as outras camadas da solução.

---

## 🏗️ Estrutura de Pastas

FSI.BusinessProcessManagement.Domain
│
├── 2.1-Entities
│ ├── Department.cs
│ ├── Usuario.cs
│ ├── Role.cs
│ ├── UserRole.cs
│ ├── Screen.cs
│ ├── AuditLog.cs
│ ├── ProcessoBPM.cs
│ ├── ProcessStep.cs
│ ├── ProcessExecution.cs
│ ├── RoleScreenPermission.cs
│
├── 2.2-Interfaces
│ ├── IRepositoryBase.cs
│ ├── IDepartmentRepository.cs
│ ├── IUsuarioRepository.cs
│ ├── IRoleRepository.cs
│ ├── IUserRoleRepository.cs
│ ├── IScreenRepository.cs
│ ├── IAuditLogRepository.cs
│ ├── IProcessoBPMRepository.cs
│ ├── IProcessStepRepository.cs
│ ├── IProcessExecutionRepository.cs
│ ├── IRoleScreenPermissionRepository.cs
│
└── 2.3-Services
├── ValidationService.cs
├── BusinessRulesService.cs
└── DomainNotification.cs


---

## 🧱 2.1 - Entities (Entidades de Domínio)

As **entidades** representam as **principais tabelas** e **conceitos do domínio BPM**, contendo propriedades, relacionamentos e **métodos com regras de negócio isoladas**.

| Entidade | Função | Regras de Negócio Principais |
|-----------|--------|-------------------------------|
| **Department** | Representa um setor ou departamento. | Garante unicidade do nome e descrição opcional. |
| **Usuario** | Usuário do sistema. | Valida formato de e-mail e status ativo/inativo. |
| **Role** | Perfis de acesso. | Cada perfil deve ter nome único e descrição clara. |
| **UserRole** | Associação N:N entre usuários e perfis. | Evita duplicação de vínculo entre mesmo usuário e perfil. |
| **Screen** | Telas/módulos acessíveis do sistema. | Usada para mapear permissões e auditorias. |
| **AuditLog** | Registro de ações realizadas. | Captura usuário, tela e ação executada. |
| **ProcessoBPM** | Representa um processo de negócio. | Cada processo pertence a um setor e pode ter várias etapas. |
| **ProcessStep** | Etapas de um processo. | Controla ordem de execução e perfil responsável. |
| **ProcessExecution** | Histórico da execução de processos. | Garante consistência entre processo, etapa e status. |
| **RoleScreenPermission** | Define permissões por tela. | Liga perfis (Roles) a telas (Screens) com ações específicas (Ex: Visualizar, Editar). |

Cada classe possui:
- **Propriedades** mapeadas para colunas SQL;
- **Métodos de validação internos** (`Validate()` ou `IsValid()`), lançando exceções de domínio;
- **Construtores inteligentes**, que garantem consistência do estado do objeto;
- **Registros de data/hora** de criação e atualização automáticos.

---

## 🧩 2.2 - Interfaces

A pasta **Interfaces** contém contratos genéricos e específicos usados para abstrair o acesso a dados, mantendo o domínio desacoplado da infraestrutura.

| Interface | Descrição |
|------------|------------|
| **IRepositoryBase\<T>** | Define operações CRUD genéricas para todas as entidades. |
| **IDepartmentRepository**, **IUsuarioRepository**, etc. | Contratos específicos de cada entidade. |
| **IProcessExecutionRepository** | Inclui métodos personalizados como `GetByProcessAndStatus()`. |
| **IRoleScreenPermissionRepository** | Interface para controle de permissões entre roles e telas. |

Essas interfaces serão implementadas na camada **Infrastructure.Persistence**, garantindo inversão de dependência (DIP).

---

## ⚙️ 2.3 - Services

A pasta **Services** centraliza **lógicas de domínio mais complexas** que não pertencem a uma única entidade.  
Esses serviços podem ser usados por **aplicações** ou **camadas superiores** sem violar o encapsulamento do domínio.

| Classe | Função |
|--------|--------|
| **ValidationService** | Valida entidades antes da persistência (ex.: FKs, campos obrigatórios). |
| **BusinessRulesService** | Aplica regras de negócio globais (ex.: não permitir execução fora de ordem). |
| **DomainNotification** | Classe utilitária para coletar erros de validação e comunicar inconsistências. |

Esses serviços **não acessam o banco diretamente**, apenas operam sobre entidades e interfaces de repositórios.

---

## 🧠 Conceitos Aplicados

### ✅ DDD (Domain-Driven Design)
- Cada entidade é um **Aggregate Root** com regras próprias.
- Repositórios seguem a **abstração de persistência** do DDD.
- A camada é **imutável e desacoplada** das demais.

### ✅ SOLID
- **S**: Cada entidade trata apenas da sua própria responsabilidade.  
- **O**: Regras de negócio podem ser estendidas sem alterar código existente.  
- **L**: Subtipos respeitam contratos das interfaces base.  
- **I**: Interfaces pequenas e específicas.  
- **D**: Domínio depende apenas de abstrações (interfaces), nunca de implementações concretas.

---

## 🚫 O Que Não Deve Estar Aqui

❌ Acesso a banco de dados  
❌ Chamadas HTTP ou API  
❌ Lógicas de UI ou persistência  
❌ Configurações de infraestrutura  

Tudo isso será responsabilidade da camada **Infrastructure** e **Application**.

---

## 📚 Resumo

| Pasta | Função |
|--------|--------|
| `Entities` | Modelos centrais e regras de negócio do domínio. |
| `Interfaces` | Contratos e abstrações para repositórios. |
| `Services` | Regras de domínio e validações complexas. |

---

## ✍️ Autor
**Rodrigo Luiz Madeira Furlaneti**  
Projeto: **FSI.BusinessProcessManagement**  
Versão: `.NET 8.0`  
Data: Outubro / 2025
