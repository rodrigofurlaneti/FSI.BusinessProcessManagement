# Camada Application - BusinessProcessManagement

A camada **Application** é responsável por coordenar as operações entre o **Domínio** e a **Interface (API, UI)**.  
Ela implementa os **casos de uso (Use Cases)** do sistema, traduzindo regras de negócio em ações práticas, sem conter lógica de persistência ou de interface.

---

## 📂 Estrutura da pasta `FSI.BusinessProcessManagement.Application`

FSI.BusinessProcessManagement.Application
│
├── Dtos
│ ├── DepartmentDto.cs
│ ├── UsuarioDto.cs
│ ├── RoleDto.cs
│ ├── UserRoleDto.cs
│ ├── ScreenDto.cs
│ ├── AuditLogDto.cs
│ ├── ProcessoBPMDto.cs
│ ├── ProcessStepDto.cs
│ ├── ProcessExecutionDto.cs
│ ├── RoleScreenPermissionDto.cs
│
├── Interfaces
│ ├── IDepartmentAppService.cs
│ ├── IUsuarioAppService.cs
│ ├── IRoleAppService.cs
│ ├── IUserRoleAppService.cs
│ ├── IScreenAppService.cs
│ ├── IAuditLogAppService.cs
│ ├── IProcessoBPMAppService.cs
│ ├── IProcessStepAppService.cs
│ ├── IProcessExecutionAppService.cs
│ ├── IRoleScreenPermissionAppService.cs
│
└── Services
├── DepartmentAppService.cs
├── UsuarioAppService.cs
├── RoleAppService.cs
├── UserRoleAppService.cs
├── ScreenAppService.cs
├── AuditLogAppService.cs
├── ProcessoBPMAppService.cs
├── ProcessStepAppService.cs
├── ProcessExecutionAppService.cs
├── RoleScreenPermissionAppService.cs


---

## 🧩 Função de cada pasta

### 📦 Dtos
Objetos de transporte de dados.  
Transformam entidades de domínio em representações simples usadas por camadas externas (API, UI).

- Não contêm lógica de negócio.  
- Usados para entrada e saída de dados.

---

### 📄 Interfaces
Definem contratos para os **serviços de aplicação**.  
Cada interface descreve os métodos expostos para manipular uma entidade (CRUD, busca, etc.).

Exemplo:
```csharp
public interface IDepartmentAppService
{
    IEnumerable<DepartmentDto> GetAll();
    DepartmentDto? GetById(long id);
    void Insert(DepartmentDto dto);
    void Update(DepartmentDto dto);
    void Delete(long id);
}
