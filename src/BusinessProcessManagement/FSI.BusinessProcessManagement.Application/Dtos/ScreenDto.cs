using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSI.BusinessProcessManagement.Application.Dtos
{
    public class ScreenDto
    {
        public long ScreenId { get; set; }
        public string? ScreenName { get; set; }
        public string? Description { get; set; }
    }
}
