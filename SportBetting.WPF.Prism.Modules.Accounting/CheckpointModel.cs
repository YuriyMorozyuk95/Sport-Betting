using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WsdlRepository.WsdlServiceReference;

namespace SportBetting.WPF.Prism.Modules.Accounting
{
    public class CheckpointModel
    {
        public Boolean IsLastCheckpoint { get; set; }
        public ProfitAccountingCheckpoint ProfitAccountingCheckpoint { get; set; }
    }
}
