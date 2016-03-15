using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Upgradeables;
using static KerbalBudget.KerbalBudget;

namespace KerbalBudget
{
    class FundsMonitor : TransactionMonitor
    {
        private static FundsMonitor instance;

        public static FundsMonitor getInstance()
        {
            if (instance == null) instance = new FundsMonitor();
            return instance;
        }

        private FundsMonitor() { }

        internal override void Start()
        {
            Log("Starting Funds Monitoring");
            base.Start();
            //Specific Funds events
            GameEvents.OnVesselRollout.Add(onLaunch);
            GameEvents.OnPartPurchased.Add(onPartPurchased);
            GameEvents.OnCrewmemberHired.Add(onCrewRecruited);
            GameEvents.OnKSCFacilityUpgraded.Add(onFacilityUpgrade);
            GameEvents.OnKSCStructureRepaired.Add(onFacilityRepair);

            GameEvents.OnFundsChanged.Add(onFundsChanged);

        }

        private void onFacilityRepair(DestructibleBuilding data)
        {
            String name = data.id;
            float repairCost = data.RepairCost;
            Log("repaired " + name + " for " + repairCost);
        }

        private void onFacilityUpgrade(UpgradeableFacility facility, int level)
        {
            String factilityName = facility.name;
            double cost = facility.GetDowngradeCost();
            Log("Upgraded " + facility + " for " + cost);
        }

        private void onFundsChanged(double fundsAmount, TransactionReasons reason)
        {
            double delta = fundsAmount - BudgetHistory.Instance.currentFunds;
            Log(String.Format("Funds changed by {0} to {1} because of {2}", delta, fundsAmount, reason));
            switch (reason)
            {
                case TransactionReasons.VesselRecovery:
                    recoveryTransaction(delta, fundsAmount);
                    break;
                case TransactionReasons.VesselRollout:
                    //handled in onRollout instead;
                    break;
                case TransactionReasons.ContractAdvance:
                case TransactionReasons.ContractPenalty:
                case TransactionReasons.ContractDecline:
                case TransactionReasons.ContractReward:
                    contractTransaction(delta, fundsAmount, reason);
                    break;
                case TransactionReasons.RnDPartPurchase:
                    //handled in OnPartPurchased instead;
                    break;
                case TransactionReasons.CrewRecruited:
                    //handled in onCrewRecruited
                    break;
                case TransactionReasons.StructureConstruction:
                case TransactionReasons.StructureRepair:
                    addFundsTransaction(Transaction.Category.Structures, reason, "", delta, fundsAmount);
                    break;
                case TransactionReasons.Progression:
                    addFundsTransaction(Transaction.Category.Progress, reason, mostRecentProgress, delta, fundsAmount);
                    break;
                default:
                    Log("WARNING Unexpected Funds transaction");
                    addFundsTransaction(Transaction.Category.Unknown, reason, "", delta, fundsAmount);
                    break;
            }
            if (BudgetHistory.Instance != null)
            {
                BudgetHistory.Instance.currentFunds = fundsAmount;
            }
        }


        private void onCrewRecruited(ProtoCrewMember crew, int hiredKerbals)
        {
            Log("crew hired " + crew.name);
            float cost = GameVariables.Instance.GetRecruitHireCost(hiredKerbals - 1);
            Log("Calculated hire cost = " + cost);
            addFundsTransaction(Transaction.Category.Personnel, TransactionReasons.CrewRecruited, crew.name, cost);
        }



        private void onPartPurchased(AvailablePart part)
        {
            addFundsTransaction(Transaction.Category.PartsUnlock, TransactionReasons.RnDPartPurchase, part.name, part.entryCost);
        }

        private void contractTransaction(double contractDelta, double newFundsAmount, TransactionReasons reason)
        {
            addFundsTransaction(Transaction.Category.Contract, reason, "", contractDelta, newFundsAmount);
        }

        private void recoveryTransaction(double recoveryDelta, double newFundsAmount)
        {
            addFundsTransaction(Transaction.Category.VesselCost, TransactionReasons.VesselRecovery, recoveredVessel, recoveryDelta, newFundsAmount);
        }

        private void onLaunch(ShipConstruct data)
        {
            Log("new vessel");
            float dryCost = 0;
            float fuelCost = 0;
            data.GetShipCosts(out dryCost, out fuelCost);
            Log("new vessel dry=" + dryCost + " fuel cost=" + fuelCost);
            addFundsTransaction(Transaction.Category.VesselCost,
                    TransactionReasons.VesselRollout, data.shipName+"-dryCost", -dryCost);
            addFundsTransaction(Transaction.Category.VesselCost,
                    TransactionReasons.VesselRollout, data.shipName+"-fuelCost", -fuelCost);

        }

        private void addFundsTransaction(Transaction.Category category, TransactionReasons reason, String comment, double amount, double total)
        {
            if (BudgetHistory.Instance == null)
            {
                Log("Warning!!! Transactions are going unrecorded because history is null");
            }
            else
            {
                BudgetHistory.Instance.addFundsTransaction(new FundsTransaction(category,
                    reason, comment, amount, total));
            }
        }

        private void addFundsTransaction(Transaction.Category category, TransactionReasons reason, String comment, double amount)
        {
            if (BudgetHistory.Instance == null)
            {
                Log("Warning!!! Transactions are going unrecorded because history is null");
            }
            else
            {
                BudgetHistory.Instance.addFundsTransaction(new FundsTransaction(category,
                    reason, comment, amount));
            }
        }

        internal override void Stop()
        {
            base.Stop();

            GameEvents.OnVesselRollout.Remove(onLaunch);
            GameEvents.OnPartPurchased.Remove(onPartPurchased);
            GameEvents.OnCrewmemberHired.Remove(onCrewRecruited);
            GameEvents.OnKSCFacilityUpgraded.Remove(onFacilityUpgrade);
            GameEvents.OnKSCStructureRepaired.Remove(onFacilityRepair);

            GameEvents.OnFundsChanged.Remove(onFundsChanged);
        }

    }
}
