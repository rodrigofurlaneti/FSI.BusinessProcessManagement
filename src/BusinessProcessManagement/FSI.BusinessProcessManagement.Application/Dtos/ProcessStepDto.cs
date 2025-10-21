using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSI.BusinessProcessManagement.Application.Dtos
{
    public class ProcessStepDto
    {
        public long StepId { get; set; }
        public long ProcessId { get; set; }
        public string? StepName { get; set; }
        public int StepOrder { get; set; }
        public long? AssignedRoleId { get; set; }
    }
}
