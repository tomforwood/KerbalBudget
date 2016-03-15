using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KerbalBudget
{
    public class FundsTransaction : Transaction
    {

        public FundsTransaction(Category category, TransactionReasons reason, 
            String comment, double amount, double total)
            :base(category, reason, comment, amount, total)
        {
        }

        public FundsTransaction(Category category, TransactionReasons reason, 
            String comment, double amount)
            :base(category, reason, comment, amount, 
                 Funding.Instance?.Funds??0)
        {
        }


        public FundsTransaction(ConfigNode node) : base(node)
        {
        }
    }
}
