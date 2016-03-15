using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static KerbalBudget.KerbalBudget;

namespace KerbalBudget
{
    class ScienceMonitor :TransactionMonitor
    {
        private static ScienceMonitor instance;

        public static ScienceMonitor getInstance()
        {
            if (instance == null) instance = new ScienceMonitor();
            return instance;
        }

        private ScienceMonitor() { }

        internal override void Start()
        {
            Log("Starting Science Monitoring");
            base.Start();
            //Specific Science events
            GameEvents.OnTechnologyResearched.Add(OnTechResearched);
            GameEvents.OnScienceRecieved.Add(onScience);

            GameEvents.OnScienceChanged.Add(onScienceChange);

            GameEvents.OnTriggeredDataTransmission.Add(onLabDataTransmit);

        }

        /// <summary>
        /// I'm pretty sure this method only gets called when a science lab transmits it's data
        /// </summary>
        /// <param name="data"></param>
        private void onLabDataTransmit(ScienceData data)
        {
            String[] experimentDetails = data.subjectID.Split('@');
            float totalScience = ResearchAndDevelopment.Instance?.Science ?? 0;
            ScienceTransaction transaction = new ScienceTransaction(Transaction.Category.ScienceExperiment,
                TransactionReasons.ScienceTransmission, experimentDetails[0], data.dataAmount, 
                totalScience, null, experimentDetails?[1]);
            BudgetHistory.Instance.addScienceTransaction(transaction);
        }

        private void OnTechResearched(GameEvents.HostTargetAction<RDTech, RDTech.OperationResult> data)
        {
            RDTech tech = data.host;
            RDTech.OperationResult result = data.target;
            if (result==RDTech.OperationResult.Successful)
            {
                int scienceCost = -tech.scienceCost;
                float totalScience = ResearchAndDevelopment.Instance?.Science ?? 0;
                String techName = tech.title;
                ScienceTransaction tran = new ScienceTransaction(Transaction.Category.TechUnlock, TransactionReasons.RnDTechResearch, 
                    techName, scienceCost, totalScience);
                BudgetHistory.Instance.addScienceTransaction(tran);
            }
        }

        private void onScience(float scienceAmount, ScienceSubject subject, ProtoVessel vessel, bool recoveryData)
        {
            //Log("on science");
            String[] experimentDetails = subject.id.Split('@');
            float totalScience = ResearchAndDevelopment.Instance?.Science ?? 0;
            ScienceTransaction transaction = new ScienceTransaction(Transaction.Category.ScienceExperiment,
                TransactionReasons.ScienceTransmission, experimentDetails[0], scienceAmount, totalScience, vessel?.vesselName, experimentDetails?[1]);
            BudgetHistory.Instance.addScienceTransaction(transaction);
        }

        private void onScienceChange(float scienceAmount, TransactionReasons reason)
        {
            double delta = scienceAmount - BudgetHistory.Instance.currentScience;
            Log("Science changed by " + delta + " to " + scienceAmount + " because of " + reason);
            switch (reason)
            {
                case TransactionReasons.VesselRecovery:
                case TransactionReasons.ScienceTransmission:
                    //dealt with by Onscience or onLabDataTransmit
                    break;
                case TransactionReasons.ContractAdvance:
                case TransactionReasons.ContractDecline:
                case TransactionReasons.ContractPenalty:
                case TransactionReasons.ContractReward:
                    ScienceTransaction transaction = new ScienceTransaction(Transaction.Category.Contract,
                        reason, "", delta, scienceAmount);
                    BudgetHistory.Instance.addScienceTransaction(transaction);
                    break;
                case TransactionReasons.RnDTechResearch:
                    //dealt with in OnTechResearched
                    break;
                case TransactionReasons.Progression:
                    transaction = new ScienceTransaction(Transaction.Category.Progress,
                        reason, mostRecentProgress, delta, scienceAmount);
                    BudgetHistory.Instance.addScienceTransaction(transaction);
                    break;
                default:
                    Log("WARNING unexpected Science transaction");
                    transaction = new ScienceTransaction(Transaction.Category.Unknown, 
                        reason, "", delta, scienceAmount);
                    BudgetHistory.Instance.addScienceTransaction(transaction);
                    break;
            }

            BudgetHistory.Instance.currentScience = scienceAmount;
        }

        internal override void Stop()
        {
            base.Stop();

            GameEvents.OnTechnologyResearched.Remove(OnTechResearched);
            GameEvents.OnScienceRecieved.Remove(onScience);

            GameEvents.OnScienceChanged.Remove(onScienceChange);
            GameEvents.OnTriggeredDataTransmission.Remove(onLabDataTransmit);
        }
    }
}
