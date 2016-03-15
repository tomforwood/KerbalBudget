using System;
using System.IO;
using Contracts;
using UnityEngine;

namespace KerbalBudget
{
    [KSPAddon(KSPAddon.Startup.PSystemSpawn, true)]
    class KerbalBudget: MonoBehaviour
    {
        private const int CONTRACT_FUNDS_TO_REP = 2875;
        private const float CONTRACT_FUNDS_TO_SCI = 0.886f;
        private ApplicationLauncherButton appLauncherButton;

        
        void Start()
        {
            FundsMonitor.getInstance().Start();
            ScienceMonitor.getInstance().Start();
            RepMonitor.getInstance().Start();

            //Add and remove button as appropriate
            GameEvents.onGUIApplicationLauncherReady.Add(AddAppLauncher);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(RemoveAppLauncher);

            //munge contracts to only offer rep
            GameEvents.Contract.onOffered.Add(onContractOffered);
            GameEvents.Contract.onContractsLoaded.Add(onContractsLoaded);

        }

        private void onContractsLoaded()
        {
            foreach (Contract c in ContractSystem.Instance.Contracts)
            {
                repifyContract(c);
            }
        }

        private void onContractOffered(Contract contract)
        {
            repifyContract(contract);

        }

        public static void repifyContract(Contract contract)
        {
            if (BudgetHistory.Instance?.convertContractRewards??false) {
                Log("Repifying contract " + contract.Title);
                double completionFunds = contract.FundsCompletion;
                double failureFunds = contract.FundsFailure;
                contract.FundsCompletion = 0;
                contract.FundsFailure = 0;
                contract.ReputationCompletion += (float)(completionFunds / CONTRACT_FUNDS_TO_REP);
                contract.ReputationFailure += (float)(failureFunds / CONTRACT_FUNDS_TO_REP);

                float completionSci = contract.ScienceCompletion;
                contract.ScienceCompletion = 0;
                contract.ReputationCompletion += completionSci / CONTRACT_FUNDS_TO_SCI;


                foreach (ContractParameter param in contract.AllParameters)
                {
                    repifyParam(param);
                }
            }
        }

        private static void repifyParam(ContractParameter param)
        {
            Log("Repifying param " + param.Title);
            double completionFunds = param.FundsCompletion;
            double failureFunds = param.FundsFailure;
            param.FundsCompletion = 0;
            param.FundsFailure = 0;
            param.ReputationCompletion += (float)(completionFunds / CONTRACT_FUNDS_TO_REP);
            param.ReputationFailure += (float)(failureFunds / CONTRACT_FUNDS_TO_REP);

            float completionSci = param.ScienceCompletion;
            param.ScienceCompletion = 0;
            param.ReputationCompletion += completionSci / CONTRACT_FUNDS_TO_SCI;
        }

        void Stop()
        {
            FundsMonitor.getInstance().Stop();
            ScienceMonitor.getInstance().Stop();
            RepMonitor.getInstance().Stop();
            
            GameEvents.onGUIApplicationLauncherReady.Remove(AddAppLauncher);
            GameEvents.onGUIApplicationLauncherDestroyed.Remove(RemoveAppLauncher);
            
            GameEvents.Contract.onOffered.Remove(onContractOffered);
            GameEvents.Contract.onContractsLoaded.Remove(onContractsLoaded);
        }

        private void AddAppLauncher()
        {
            if (appLauncherButton != null)
            {
                return;
            }
            ApplicationLauncher applauncher = ApplicationLauncher.Instance;
            if (applauncher == null)
            {
                Log("Cannot add to ApplicationLauncher, instance was null");
                return;
            }
            const ApplicationLauncher.AppScenes scenes =
                ApplicationLauncher.AppScenes.SPACECENTER;
            String iconPath=System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            iconPath += "/Images/budget.png";
            Texture2D icon = new Texture2D(32, 32, TextureFormat.ARGB32, false);
            icon.LoadImage(System.IO.File.ReadAllBytes(iconPath));
            appLauncherButton = ApplicationLauncher.Instance.AddModApplication(OnAppLaunch, OnAppClose, null, null, null, null, scenes, icon);
        }

        private void OnAppLaunch()
        {
            String savePath = KSPUtil.ApplicationRootPath+"/saves/"+ HighLogic.fetch.GameSaveFolder
                + "/funds.csv";
            FileStream fout=File.OpenWrite(savePath);
            TextWriter writer = new StreamWriter(fout);
            BudgetHistory.Instance.writeCSV(writer);
            writer.Close();
        }

        private void OnAppClose()
        {
        }

        private void RemoveAppLauncher()
        {
            ApplicationLauncher appLaunch = ApplicationLauncher.Instance;
            if (appLaunch== null)
            {
                Log("Cannot remove app launch button, launcher was null");
                return;
            }
            if (appLauncherButton== null)
            {
                return;
            }
            appLaunch.RemoveModApplication(appLauncherButton);
            appLauncherButton = null;
            throw new NotImplementedException();
        }

        public static void Log(string message)
        {
            Debug.Log("[KB:" + DateTime.Now + "]: " + message);
        }

        void FixedUpdate()
        {
            if (PeriodicPayments.Instance!= null)
            {
                PeriodicPayments.Instance.FixedUpdate();
            }
        }
    }

}
