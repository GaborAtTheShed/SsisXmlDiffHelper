using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SsisXmlDiffHelper.Models
{
    public class ExecutableDtsTask
    {
        public string? Name { get; set; }
        public string? RefId { get; set; }
        public string? Desc { get; set; }
        public string? Disabled { get; set; }
        public IEnumerable<DataFlowComponent>? DataFlowComponents { get; set; }
        public string? SqlTaskScript { get; set; }
    }
}
