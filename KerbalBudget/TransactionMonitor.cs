using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static KerbalBudget.KerbalBudget;

namespace KerbalBudget
{
    class TransactionMonitor
    {
        internal virtual void Start()
        {
            GameEvents.onVesselRecovered.Add(onRecovery);
            GameEvents.OnProgressComplete.Add(onProgress);
        }

        internal virtual void Stop()
        {
            GameEvents.onVesselRecovered.Remove(onRecovery);
            GameEvents.OnProgressComplete.Remove(onProgress);
        }

        /// <summary>
        /// Make a note of the recovered vessel's name so it can be recorded 
        /// when the funds are refunded etc
        /// </summary>
        protected  String recoveredVessel;
        private void onRecovery(ProtoVessel vessel)
        {
            recoveredVessel = vessel.vesselName;
        }

        /// <summary>
        /// Make a note of the most recent progress so it can be matched to changes
        /// of funds etc
        /// </summary>
        protected String mostRecentProgress;
        private void onProgress(ProgressNode data)
        {
            mostRecentProgress = data.Id;
            Log(data.ToString());
        }
    }
}
