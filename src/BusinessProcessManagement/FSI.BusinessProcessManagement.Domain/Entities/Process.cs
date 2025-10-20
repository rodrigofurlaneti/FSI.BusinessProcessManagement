using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSI.BusinessProcessManagement.Domain.Entities
{
    #region ProcessoBPM (Process)
    public void SetDescription(string? description)
    {
        Description = description?.Trim();
        Touch();
    }
    public void SetDepartment(long? departmentId)
    {
        DepartmentId = departmentId;
        Touch();
    }
    public ProcessStep AddStep(string stepName, int stepOrder, long? assignedRoleId = null)
    {
        if (string.IsNullOrWhiteSpace(stepName)) throw new DomainException("StepName is required.");
        if (_steps.Any(s => s.StepOrder == stepOrder))
            throw new DomainException($"A step with order {stepOrder} already exists for this process.");


        var step = new ProcessStep(this.Id, stepName, stepOrder, assignedRoleId);
        _steps.Add(step);
        Touch();
        return step;
    }
    public void RemoveStep(long stepId)
    {
        var step = _steps.FirstOrDefault(s => s.Id == stepId || s.StepId == stepId);
        if (step == null) throw new DomainException("Step not found in process.");
        _steps.Remove(step);
        Touch();
    }
    public ProcessExecution StartExecution(long stepId, long? userId = null)
    {
        var step = _steps.FirstOrDefault(s => s.StepId == stepId);
        if (step == null) throw new DomainException("Step not found in process.");
        var exec = new ProcessExecution(this.Id, step.StepId, userId);
        _executions.Add(exec);
        Touch();
        return exec;
    }

#endregion
}
