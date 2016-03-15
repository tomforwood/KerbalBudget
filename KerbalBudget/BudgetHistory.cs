using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Contracts;
using static KerbalBudget.KerbalBudget;

namespace KerbalBudget
{

    [KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[]{
        GameScenes.FLIGHT, GameScenes.SPACECENTER, GameScenes.TRACKSTATION, GameScenes.EDITOR})]
    public class BudgetHistory : ScenarioModule
    {
        private const string FUNDS_TRANSACTIONS = "FundsTransactions";
        private const string SCIENCE_TRANSACTIONS = "ScienceTransactions";
        private const string REP_TRANSACTIONS = "RepTransactions";

        public static BudgetHistory Instance{ get; private set; }

        [KSPField(isPersistant = true)]
        public double currentFunds=0;

        [KSPField(isPersistant = true)]
        public double currentScience = 0;

        [KSPField(isPersistant = true)]
        public double currentSupport = 0;

        [KSPField(isPersistant = true)]
        public bool convertContractRewards= true;

        [KSPField(isPersistant = true)]
        public bool convertFieldScience = true;

        [KSPField(isPersistant = true)]
        public bool isNewGame = true;

        //true if we are only recording transactions
        //false if we are doing the periodic transaction thing
        [KSPField(isPersistant = true)]
        public bool readOnly = false;

        List<FundsTransaction> funds = new List<FundsTransaction>();
        List<RepTransaction> reputation = new List<RepTransaction>();
        List<ScienceTransaction> science = new List<ScienceTransaction>();

        public void Start()
        {

            Log("BudgetHistory=" + BudgetHistory.Instance);

            Log("newGame=" + BudgetHistory.Instance?.isNewGame);
            if (BudgetHistory.Instance?.isNewGame ?? false)
            {
                Log("New game detected Aking for preferences");
                BudgetHistory.Instance.isNewGame = false;
                ConfigWindow window = new ConfigWindow();
                window.SetVisible(true);
            }
        }

        public override void OnLoad(ConfigNode gameNode) {
            base.OnLoad(gameNode);
            Instance = this;

            funds.Clear();
            if (gameNode.HasNode(FUNDS_TRANSACTIONS))
            {
                foreach (ConfigNode transaction in gameNode.GetNodes(FUNDS_TRANSACTIONS)) {
                    funds.Add(new FundsTransaction(transaction));
                }
            }
            else
            {
                //we haven't recorded any transactions for this game yet - put in an initial one
                currentFunds = Funding.Instance.Funds;
                funds.Add(new FundsTransaction(Transaction.Category.Initial,
                    TransactionReasons.Progression, "Start of new game", currentFunds));
            }

            science.Clear();
            if (gameNode.HasNode(SCIENCE_TRANSACTIONS))
            {
                foreach (ConfigNode transaction in gameNode.GetNodes(SCIENCE_TRANSACTIONS))
                {
                    science.Add(new ScienceTransaction(transaction));
                }
            }
            else
            {
                currentScience = ResearchAndDevelopment.Instance.Science;
                science.Add(new ScienceTransaction(Transaction.Category.Initial, 
                    TransactionReasons.Progression, "Inital rep", currentScience, currentScience));
            }

            reputation.Clear();
            if (gameNode.HasNode(REP_TRANSACTIONS))
            {
                foreach (ConfigNode transaction in gameNode.GetNodes(REP_TRANSACTIONS))
                {
                    reputation.Add(new RepTransaction(transaction));
                }
            }
            else
            {
                float currentRep = Reputation.Instance.reputation;
                reputation.Add(new RepTransaction(Transaction.Category.Initial, 
                    TransactionReasons.Progression, "Inital rep", currentRep, currentRep));
                currentSupport = currentRep;
            }
        }

        internal void addRepTransaction(RepTransaction transaction)
        {
            reputation.Add(transaction);
        }

        internal void addScienceTransaction(ScienceTransaction transaction)
        {
            science.Add(transaction);
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            foreach (FundsTransaction transaction in funds)
            {
                ConfigNode fundsNode = new ConfigNode(FUNDS_TRANSACTIONS);
                transaction.save(fundsNode);
                node.AddNode(fundsNode);
            }
            foreach (ScienceTransaction transaction in science)
            {
                ConfigNode fundsNode = new ConfigNode(SCIENCE_TRANSACTIONS);
                transaction.save(fundsNode);
                node.AddNode(fundsNode);
            }

            foreach (RepTransaction transaction in reputation)
            {
                ConfigNode repTrans = new ConfigNode(REP_TRANSACTIONS);
                transaction.save(repTrans);
                node.AddNode(repTrans);
            }
        }

        public void addFundsTransaction(FundsTransaction transaction)
        {
            funds.Add(transaction);
        }

        internal void writeCSV(TextWriter writer)
        {
            writer.WriteLine("FUNDS");
            FundsTransaction.writeHeader(writer);
            foreach (FundsTransaction transaction in funds)
            {
                transaction.writeCSV(writer);
            }
            writer.WriteLine("Science");
            ScienceTransaction.writeHeader(writer);
            foreach (ScienceTransaction transaction in science)
            {
                transaction.writeCSV(writer);
            }
            writer.WriteLine("Reputation");
            RepTransaction.writeHeader(writer);
            foreach (RepTransaction transaction in reputation)
            {
                transaction.writeCSV(writer);
            }
        }
    }
}
