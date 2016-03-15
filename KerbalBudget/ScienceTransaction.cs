using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KerbalBudget
{
    class ScienceTransaction : Transaction
    {
        private const string VESSEL_NAME = "vesselName";
        private const string DESCRIPTION = "description";

        private readonly String vesselName;
        private readonly String description;

        public ScienceTransaction(ConfigNode node) : base(node)
        {
            vesselName = node.GetValue(VESSEL_NAME);
            description = node.GetValue(DESCRIPTION);
        }

        public ScienceTransaction(Category category, TransactionReasons reason,
            String comment, double amount, double total)
            :base(category, reason, comment, amount, total)
        {
        }

        public ScienceTransaction(Category category, TransactionReasons reason,
            String comment, double amount, double total, String vesselName, String biome)
            : base(category, reason, comment, amount, total)
        {
            this.vesselName = vesselName;
            this.description = biome;
        }

        public override void save(ConfigNode node)
        {
            base.save(node);
            if (vesselName != null) node.AddValue(VESSEL_NAME, vesselName);
            if (description != null) node.AddValue(DESCRIPTION, description);
        }

        internal override void writeCSV(TextWriter writer)
        {
            writeContent(writer);
            writer.Write(",\"");
            writer.Write(escapeQuotes(vesselName));
            writer.Write("\",\"");
            writer.Write(escapeQuotes(description));
            writer.Write("\"");
            writer.WriteLine();

        }

        public new static void writeHeader(TextWriter writer)
        {
            writer.WriteLine(defaultHeader + ", vesselName, description");
        }
    }
}
