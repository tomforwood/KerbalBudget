using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Contracts;
using static KerbalBudget.KerbalBudget;

namespace KerbalBudget
{
    class RepMonitor : TransactionMonitor
    {
        private static RepMonitor instance;

        public static RepMonitor getInstance()
        {
            if (instance == null) instance = new RepMonitor();
            return instance;
        }

        private RepMonitor() { }

        internal override void Start()
        {
            Log("Starting Reputation Monitoring");
            base.Start();

            GameEvents.Modifiers.OnCurrencyModified.Add(onRepChanged);


        }

        private void onRepChanged(CurrencyModifierQuery data)
        {
            //double delta = repAmount - BudgetHistory.activeHistory.currentRep;
            double delta = data.GetEffectDelta(Currency.Reputation);
            delta = data.GetInput(Currency.Reputation);
            if (delta == 0) return;
            BudgetHistory.Instance.currentSupport += delta;
            double total = Reputation.Instance.reputation;
            TransactionReasons reason = data.reason;

            Log(String.Format("Reputation changed by {0} to {1} because of {2}", delta, "?", reason));

            
            switch (reason)
            {
                case TransactionReasons.ContractAdvance:
                case TransactionReasons.ContractPenalty:
                case TransactionReasons.ContractDecline:
                case TransactionReasons.ContractReward:
                    RepTransaction transaction = new RepTransaction(Transaction.Category.Contract,
                        reason, "", delta, total);
                    BudgetHistory.Instance.addRepTransaction(transaction);
                    break;
                case TransactionReasons.Progression:
                    transaction = new RepTransaction(Transaction.Category.Progress,
                        reason, mostRecentProgress, delta, total);
                    BudgetHistory.Instance.addRepTransaction(transaction);
                    break;
                default:
                    transaction = new RepTransaction(Transaction.Category.Unknown,
                        reason, "", delta, total);
                    BudgetHistory.Instance.addRepTransaction(transaction);
                    break;
            }
        }
        internal override void Stop()
        {
            base.Stop();
            GameEvents.Modifiers.OnCurrencyModified.Add(onRepChanged);
        }
    }
}
