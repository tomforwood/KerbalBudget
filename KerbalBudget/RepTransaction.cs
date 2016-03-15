using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KerbalBudget
{
    class RepTransaction : Transaction
    {
        private const string SUPPORT_NAME = "vesselName";

        private readonly double totalSupport;

        public RepTransaction(Category category, TransactionReasons reason, 
            String comment, double amount, double total) 
            :base(category, reason, comment, amount, total)
        {
            totalSupport = BudgetHistory.Instance.currentSupport;
        }

        public RepTransaction(Category category, TransactionReasons reason, 
            String comment, double amount, double totalRep, double totalSupport)
            :base(category, reason, comment, amount, totalRep)
        {
            this.totalSupport = totalSupport;
        }


        public RepTransaction(ConfigNode node) :base(node)
        {
            if (node.HasValue(SUPPORT_NAME))
            {
                totalSupport = double.Parse(node.GetValue(SUPPORT_NAME));
            }
        }

        public override void save(ConfigNode node)
        {
            base.save(node);
            node.AddValue(SUPPORT_NAME, totalSupport);
        }

        internal override void writeCSV(TextWriter writer)
        {
            writeContent(writer);
            writer.Write(",");
            writer.Write(totalSupport);
            writer.WriteLine();

        }

        public new static void writeHeader(TextWriter writer)
        {
            writer.WriteLine(defaultHeader + ", totalSupport");
        }
    }
}
