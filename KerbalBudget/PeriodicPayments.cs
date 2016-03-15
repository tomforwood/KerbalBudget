using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static KerbalBudget.KerbalBudget;

namespace KerbalBudget
{
    [KSPScenario(ScenarioCreationOptions.AddToNewCareerGames| 
        ScenarioCreationOptions.AddToExistingCareerGames,
        new GameScenes[]{GameScenes.FLIGHT, GameScenes.SPACECENTER, GameScenes.TRACKSTATION })]
    public class PeriodicPayments : ScenarioModule
    {
        [KSPField(isPersistant = true)]
        public double lastFundingTime = 0;

        [KSPField(isPersistant = true)]
        public double nextFundingTime = double.PositiveInfinity;

        [KSPField(isPersistant = true)]
        public bool acceleratedStart = true;

        [KSPField(isPersistant = true)]
        public static float fundScienceSplit { get; private set; } = 0.5f;

        [KSPField(isPersistant = true)]
        public static float fundScienceSplitTarget { get; private set; } = 0.5f;

        private const float SUPPORT_DECAY_RATE = 0.2f;

        public static PeriodicPayments Instance;

        public static List<PeriodicTransaction> transactions = new List<PeriodicTransaction>();

        static PeriodicPayments()
        {
            transactions.Add(new FundsPayment());
            transactions.Add(new SciencePayment());
            transactions.Add(new SupportDecay());
        }

        internal void resetNextFunding()
        {
            double currentTime = Planetarium.GetUniversalTime();
            nextFundingTime = nextPaymentTime(currentTime + 1);
        }

        public PeriodicPayments()
        {
            Instance = this;
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            Log("next payment time =" + nextFundingTime);
        }

        public void FixedUpdate()
        {
            if (HighLogic.CurrentGame.Mode!=Game.Modes.CAREER) return;
            double currentTime = Planetarium.GetUniversalTime();
            if (currentTime >=nextFundingTime)
            {
                Log("current time=" + currentTime + " nextTime=" + nextFundingTime + " lastTime=" + lastFundingTime);
                foreach (PeriodicTransaction trans in transactions)
                {
                    trans.doTransaction(currentTime, lastFundingTime);
                }
                nextFundingTime = nextPaymentTime(currentTime + 1);
                Log("current time=" + currentTime + " nextTime=" + nextFundingTime + " lastTime=" + lastFundingTime);
                lastFundingTime = currentTime;

            }

        }
        
        //in accelerated mode you get payments every hour for the first day, then daily for the first month, then montly
        private double[,] acceleratedThresholds = { { 0, KSPUtil.Hour }, { KSPUtil.KerbinDay, KSPUtil.KerbinDay }, { KSPUtil.KerbinYear / 10, KSPUtil.KerbinYear / 10 } };

        public double nextPaymentTime(double currentTime)
        {
            double interval = 0;
            if (acceleratedStart)
            {
                for (int i=0;i<acceleratedThresholds.GetLength(0);i++)
                {
                    if (acceleratedThresholds[i,0]<currentTime)
                    {
                        interval = acceleratedThresholds[i, 1];
                    }
                }
            }
            else
            {
                interval = KSPUtil.KerbinYear / 10;
            }
            double nextTime = Math.Ceiling(currentTime / interval) * interval;
            return nextTime;
        }

        class FundsPayment : PeriodicTransaction
        {
            const double FUNDS_K = 431;

            public void doTransaction(double currentTime, double lastTime)
            {
                double currentSupport = BudgetHistory.Instance.currentSupport;
                double fundPayment = FUNDS_K * currentSupport * fundScienceSplit;
                Log("Periodic funding paying " + fundPayment);
                Funding.Instance.AddFunds(fundPayment, TransactionReasons.Strategies);
            }
        }

        class SciencePayment : PeriodicTransaction
        {
            const double SCIENCE_K = 0.14175;

            public void doTransaction(double currentTime, double lastTime)
            {
                double currentSupport = BudgetHistory.Instance.currentSupport;
                double sciencePayment = SCIENCE_K * currentSupport * (1-fundScienceSplit);
                Log("Periodic science paying " + sciencePayment);
                ResearchAndDevelopment.Instance.AddScience((float)sciencePayment, TransactionReasons.Strategies);
            }
        }

        class SupportDecay :PeriodicTransaction
        {
            const double decayConstant = 0.2;

            public void doTransaction(double currentTime, double lastTime)
            {
                BudgetHistory.Instance.currentSupport *= (1 - decayConstant);
                Log("Periodic support decayed to " + BudgetHistory.Instance.currentSupport);
            }
        }

    }
}
