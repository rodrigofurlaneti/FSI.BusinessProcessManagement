-- =========================================================
-- MASSA DE TESTE (MODELO LEGADO/PT-BR)
-- Processo: Fluxo de Embarque Nacional
-- Tabelas: Department, Role, Screen, User, UserRole,
--          RoleScreenPermission, ProcessoBPM, ProcessStep, ProcessExecution
-- =========================================================

-- USE processdb;

-- 1) DEPARTAMENTOS
INSERT INTO Department (DepartmentName, Description, CreatedAt)
VALUES
 ('Operações', 'Coordena operações de pátio e pista', NOW(6)),
 ('Segurança', 'Controle de acesso e inspeção de passageiros', NOW(6)),
 ('Atendimento ao Passageiro', 'Check-in e suporte ao cliente', NOW(6))
ON DUPLICATE KEY UPDATE Description = VALUES(Description);

-- 2) ROLES (perfis)
INSERT INTO Role (RoleName, Description, CreatedAt)
VALUES
 ('Administrador', 'Acesso total ao sistema', NOW(6)),
 ('Supervisor de Operações', 'Coordenação no pátio/portão', NOW(6)),
 ('Agente de Atendimento', 'Check-in/atendimento', NOW(6)),
 ('Agente de Segurança', 'Triagem e inspeção', NOW(6))
ON DUPLICATE KEY UPDATE Description = VALUES(Description);

-- 3) SCREENS (telas/módulos)
INSERT INTO Screen (ScreenName, Description, CreatedAt)
VALUES
 ('Dashboard', 'Indicadores gerais', NOW(6)),
 ('Processos', 'Gestão de processos BPM', NOW(6)),
 ('Embarque', 'Fluxos de embarque', NOW(6)),
 ('Segurança', 'Fluxos de inspeção', NOW(6))
ON DUPLICATE KEY UPDATE Description = VALUES(Description);

-- 4) USUÁRIOS
-- Substitua por um hash BCrypt REAL
SET @PwdHash = '$2a$12$COLOQUE_AQUI_UM_HASH_BCRYPT_VALIDO';

SET @DeptAtend = (SELECT DepartmentId FROM Department WHERE DepartmentName='Atendimento ao Passageiro' LIMIT 1);
SET @DeptOper  = (SELECT DepartmentId FROM Department WHERE DepartmentName='Operações' LIMIT 1);
SET @DeptSeg   = (SELECT DepartmentId FROM Department WHERE DepartmentName='Segurança' LIMIT 1);

INSERT INTO User (DepartmentId, Username, PasswordHash, Email, IsActive, CreatedAt)
SELECT @DeptAtend, 'agente1', @PwdHash, 'agente1@aero.local', 1, NOW(6)
WHERE NOT EXISTS (SELECT 1 FROM User WHERE Username='agente1');

INSERT INTO User (DepartmentId, Username, PasswordHash, Email, IsActive, CreatedAt)
SELECT @DeptOper, 'supervisor1', @PwdHash, 'supervisor1@aero.local', 1, NOW(6)
WHERE NOT EXISTS (SELECT 1 FROM User WHERE Username='supervisor1');

INSERT INTO User (DepartmentId, Username, PasswordHash, Email, IsActive, CreatedAt)
SELECT @DeptSeg, 'seguranca1', @PwdHash, 'seguranca1@aero.local', 1, NOW(6)
WHERE NOT EXISTS (SELECT 1 FROM User WHERE Username='seguranca1');

-- 5) VÍNCULOS DE ROLE (UserRole)
SET @RoleAdmin = (SELECT RoleId FROM Role WHERE RoleName='Administrador' LIMIT 1);
SET @RoleSupOp = (SELECT RoleId FROM Role WHERE RoleName='Supervisor de Operações' LIMIT 1);
SET @RoleAtend = (SELECT RoleId FROM Role WHERE RoleName='Agente de Atendimento' LIMIT 1);
SET @RoleSeg   = (SELECT RoleId FROM Role WHERE RoleName='Agente de Segurança' LIMIT 1);

SET @UAgente     = (SELECT UserId FROM User WHERE Username='agente1' LIMIT 1);
SET @USupervisor = (SELECT UserId FROM User WHERE Username='supervisor1' LIMIT 1);
SET @USeguranca  = (SELECT UserId FROM User WHERE Username='seguranca1' LIMIT 1);

INSERT INTO UserRole (UserId, RoleId, AssignedAt)
SELECT @UAgente, @RoleAtend, NOW(6)
WHERE NOT EXISTS (SELECT 1 FROM UserRole WHERE UserId=@UAgente AND RoleId=@RoleAtend);

INSERT INTO UserRole (UserId, RoleId, AssignedAt)
SELECT @USupervisor, @RoleSupOp, NOW(6)
WHERE NOT EXISTS (SELECT 1 FROM UserRole WHERE UserId=@USupervisor AND RoleId=@RoleSupOp);

INSERT INTO UserRole (UserId, RoleId, AssignedAt)
SELECT @USeguranca, @RoleSeg, NOW(6)
WHERE NOT EXISTS (SELECT 1 FROM UserRole WHERE UserId=@USeguranca AND RoleId=@RoleSeg);

-- (opcional) dar todas as permissões por tela ao ADMIN
INSERT INTO RoleScreenPermission (RoleId, ScreenId, CanView, CanCreate, CanEdit, CanDelete, CreatedAt)
SELECT @RoleAdmin, s.ScreenId, 1,1,1,1, NOW(6)
FROM Screen s
LEFT JOIN RoleScreenPermission p ON p.RoleId=@RoleAdmin AND p.ScreenId=s.ScreenId
WHERE p.RoleScreenPermissionId IS NULL;

-- 6) PROCESSO: Fluxo de Embarque Nacional
SET @DeptProcess = @DeptAtend;
SET @CreatedBy   = @USupervisor;

INSERT INTO ProcessoBPM (DepartmentId, ProcessName, Description, CreatedBy, CreatedAt)
SELECT @DeptProcess, 'Fluxo de Embarque Nacional',
       'Processo operacional: do check-in ao fechamento de porta.', @CreatedBy, NOW(6)
WHERE NOT EXISTS (
    SELECT 1 FROM ProcessoBPM
    WHERE ProcessName='Fluxo de Embarque Nacional' AND DepartmentId=@DeptProcess
);

SET @ProcId = (SELECT ProcessId FROM ProcessoBPM
               WHERE ProcessName='Fluxo de Embarque Nacional' AND DepartmentId=@DeptProcess
               ORDER BY ProcessId DESC LIMIT 1);

-- 7) ETAPAS DO PROCESSO (5 etapas)
-- 1) Check-in (Agente de Atendimento)
-- 2) Despacho de Bagagem (Agente de Atendimento)
-- 3) Controle de Segurança (Agente de Segurança)
-- 4) Embarque no Portão (Supervisor de Operações)
-- 5) Fechamento de Porta (Supervisor de Operações)
INSERT INTO ProcessStep (ProcessId, StepName, StepOrder, AssignedRoleId, CreatedAt)
SELECT @ProcId, 'Check-in', 1, @RoleAtend, NOW(6)
WHERE NOT EXISTS (SELECT 1 FROM ProcessStep WHERE ProcessId=@ProcId AND StepOrder=1);

INSERT INTO ProcessStep (ProcessId, StepName, StepOrder, AssignedRoleId, CreatedAt)
SELECT @ProcId, 'Despacho de Bagagem', 2, @RoleAtend, NOW(6)
WHERE NOT EXISTS (SELECT 1 FROM ProcessStep WHERE ProcessId=@ProcId AND StepOrder=2);

INSERT INTO ProcessStep (ProcessId, StepName, StepOrder, AssignedRoleId, CreatedAt)
SELECT @ProcId, 'Controle de Segurança', 3, @RoleSeg, NOW(6)
WHERE NOT EXISTS (SELECT 1 FROM ProcessStep WHERE ProcessId=@ProcId AND StepOrder=3);

INSERT INTO ProcessStep (ProcessId, StepName, StepOrder, AssignedRoleId, CreatedAt)
SELECT @ProcId, 'Embarque no Portão', 4, @RoleSupOp, NOW(6)
WHERE NOT EXISTS (SELECT 1 FROM ProcessStep WHERE ProcessId=@ProcId AND StepOrder=4);

INSERT INTO ProcessStep (ProcessId, StepName, StepOrder, AssignedRoleId, CreatedAt)
SELECT @ProcId, 'Fechamento de Porta', 5, @RoleSupOp, NOW(6)
WHERE NOT EXISTS (SELECT 1 FROM ProcessStep WHERE ProcessId=@ProcId AND StepOrder=5);

-- Guardar os IDs das etapas
SET @Step1 = (SELECT StepId FROM ProcessStep WHERE ProcessId=@ProcId AND StepOrder=1 LIMIT 1);
SET @Step2 = (SELECT StepId FROM ProcessStep WHERE ProcessId=@ProcId AND StepOrder=2 LIMIT 1);
SET @Step3 = (SELECT StepId FROM ProcessStep WHERE ProcessId=@ProcId AND StepOrder=3 LIMIT 1);
SET @Step4 = (SELECT StepId FROM ProcessStep WHERE ProcessId=@ProcId AND StepOrder=4 LIMIT 1);
SET @Step5 = (SELECT StepId FROM ProcessStep WHERE ProcessId=@ProcId AND StepOrder=5 LIMIT 1);

-- 8) (OPCIONAL) SIMULAÇÃO DE EXECUÇÃO
-- Status usados: 'Iniciado', 'Concluido', 'Cancelado', 'Pendente'
-- Inicia Step 1
INSERT INTO ProcessExecution (ProcessId, StepId, UserId, Status, StartedAt, CreatedAt, Remarks)
SELECT @ProcId, @Step1, @UAgente, 'Iniciado', NOW(6), NOW(6), 'Início do check-in'
WHERE NOT EXISTS (SELECT 1 FROM ProcessExecution WHERE ProcessId=@ProcId AND StepId=@Step1);

UPDATE ProcessExecution
SET Status='Concluido', CompletedAt=NOW(6), Remarks='Check-in concluído'
WHERE ProcessId=@ProcId AND StepId=@Step1;

-- Step 2
INSERT INTO ProcessExecution (ProcessId, StepId, UserId, Status, StartedAt, CreatedAt, Remarks)
SELECT @ProcId, @Step2, @UAgente, 'Iniciado', NOW(6), NOW(6), 'Despacho de bagagem iniciado'
WHERE NOT EXISTS (SELECT 1 FROM ProcessExecution WHERE ProcessId=@ProcId AND StepId=@Step2);

UPDATE ProcessExecution
SET Status='Concluido', CompletedAt=NOW(6), Remarks='Despacho de bagagem concluído'
WHERE ProcessId=@ProcId AND StepId=@Step2;

-- Step 3
INSERT INTO ProcessExecution (ProcessId, StepId, UserId, Status, StartedAt, CreatedAt, Remarks)
SELECT @ProcId, @Step3, @USeguranca, 'Iniciado', NOW(6), NOW(6), 'Raio-X iniciado'
WHERE NOT EXISTS (SELECT 1 FROM ProcessExecution WHERE ProcessId=@ProcId AND StepId=@Step3);

UPDATE ProcessExecution
SET Status='Concluido', CompletedAt=NOW(6), Remarks='Controle de segurança concluído'
WHERE ProcessId=@ProcId AND StepId=@Step3;

-- Step 4
INSERT INTO ProcessExecution (ProcessId, StepId, UserId, Status, StartedAt, CreatedAt, Remarks)
SELECT @ProcId, @Step4, @USupervisor, 'Iniciado', NOW(6), NOW(6), 'Embarque iniciado no portão 12'
WHERE NOT EXISTS (SELECT 1 FROM ProcessExecution WHERE ProcessId=@ProcId AND StepId=@Step4);

UPDATE ProcessExecution
SET Status='Concluido', CompletedAt=NOW(6), Remarks='Embarque finalizado'
WHERE ProcessId=@ProcId AND StepId=@Step4;

-- Step 5 (final)
INSERT INTO ProcessExecution (ProcessId, StepId, UserId, Status, StartedAt, CreatedAt, Remarks)
SELECT @ProcId, @Step5, @USupervisor, 'Iniciado', NOW(6), NOW(6), 'Fechamento de porta iniciado'
WHERE NOT EXISTS (SELECT 1 FROM ProcessExecution WHERE ProcessId=@ProcId AND StepId=@Step5);

UPDATE ProcessExecution
SET Status='Concluido', CompletedAt=NOW(6), Remarks='Porta fechada. Pronto para pushback.'
WHERE ProcessId=@ProcId AND StepId=@Step5;
