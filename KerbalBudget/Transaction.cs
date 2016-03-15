using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KerbalBudget
{
    public class Transaction
    {
        private const string UNIVERSE_TIME = "universeTime";
        private const string CATEGORY = "category";
        private const string REASON = "reason";
        private const string COMMENT = "comment";
        private const string AMOUNT = "amount";
        private const string TOTAL = "total";

        public enum Category {Initial, VesselCost, Structures, Progress, Contract,
            ScienceExperiment,
            Personnel,
            PartsUnlock,
            Unknown,
            TechUnlock
        }

        public readonly double universeTime;
        public readonly Category category;
        public TransactionReasons reason;
        public string comment;
        public double amount;
        public double total;

        public Transaction(Category category, TransactionReasons reason,
            String comment, double amount, double total)
        {
            this.category = category;
            universeTime = Planetarium.GetUniversalTime();
            this.reason = reason;
            this.comment = comment;
            this.amount = amount;
            this.total = total;
        }

        public Transaction(ConfigNode node)
        {
            universeTime = float.Parse(node.GetValue(UNIVERSE_TIME));
            category = (Category)Enum.Parse(typeof(Category), node.GetValue(CATEGORY));
            reason = (TransactionReasons)Enum.Parse(typeof(TransactionReasons), node.GetValue(REASON));
            comment = node.GetValue(COMMENT);
            amount = double.Parse(node.GetValue(AMOUNT));
            total = double.Parse(node.GetValue(TOTAL));
        }

        internal virtual void writeCSV(TextWriter writer)
        {
            writeContent(writer);
            writer.WriteLine();
        }

        protected void writeContent(TextWriter writer)
        {
            writer.Write(universeTime);
            writer.Write(',');
            writer.Write(convertUTToKerbalTime(universeTime));
            writer.Write(",\"");
            writer.Write(escapeQuotes(category.ToString()));
            writer.Write("\",\"");
            writer.Write(escapeQuotes(reason.ToString()));
            writer.Write("\",\"");
            writer.Write(escapeQuotes(comment));
            writer.Write("\",");
            writer.Write(amount);
            writer.Write(",");
            writer.Write(total);
        }

        public virtual void save(ConfigNode node)
        {
            node.AddValue(UNIVERSE_TIME, universeTime);
            node.AddValue(CATEGORY, category);
            node.AddValue(REASON, reason);
            node.AddValue(COMMENT, comment);
            node.AddValue(AMOUNT, amount);
            node.AddValue(TOTAL, total);
        }

        static String convertUTToKerbalTime(double ut)
        {
            StringBuilder builder = new StringBuilder();
            int fraction = (int)(ut * 1000 % 1000);
            int secs = (int)ut % KSPUtil.Minute;
            int mins = (int)(ut % KSPUtil.Hour/KSPUtil.Minute);
            int hours = (int)(ut % KSPUtil.KerbinDay/KSPUtil.Hour);
            int days = (int)(ut % KSPUtil.KerbinYear/KSPUtil.KerbinDay)+1;//year and day count from 1
            int years = (int)(ut / KSPUtil.KerbinYear) +1;
            return String.Format("{0}y{1}d {2}:{3}:{4}.{5}", years, days, hours, mins, secs, fraction);
        }

        internal static String escapeQuotes(String s)
        {
            return s?.Replace("\"", "\"\"");
        }

        protected const String defaultHeader = "TimeDecimal, TimeKerbal, Category, TransactionReason, Comment, Amount, Total";

        internal static void writeHeader(TextWriter writer)
        {
            writer.WriteLine(defaultHeader);
        }
    }
}
