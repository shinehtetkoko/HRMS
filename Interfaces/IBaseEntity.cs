using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Interfaces
{
    public interface IAuditable
    {
        DateTime? updated_at { get; set; }
    }
}
