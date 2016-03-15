using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KerbalBudget
{
    public interface PeriodicTransaction
    {
        void doTransaction(double currentTime, double lastTime);
    }
}
