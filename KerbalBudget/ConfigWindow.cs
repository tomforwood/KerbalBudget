using System;
using UnityEngine;
using static KerbalBudget.KerbalBudget;

namespace KerbalBudget
{
    internal class ConfigWindow : SimpleWindow
    {
        public ConfigWindow() 
            :base("KerbalBudget", 200,200)
        {
        }

        protected override void DrawContents(int id)
        {
            GUILayout.Label("Chose game mode");
            GUILayout.BeginVertical(HighLogic.Skin.textArea);
            String[] enabledOptions = { "Record Keeping", "Periodic Budget" };
            int budgeting = BudgetHistory.Instance.readOnly ? 0 : 1;
            DrawRadioList(false, ref budgeting, enabledOptions);
            BudgetHistory.Instance.readOnly = budgeting == 0;
            GUILayout.EndVertical();

            if (!BudgetHistory.Instance.readOnly) { 
                GUILayout.BeginVertical(HighLogic.Skin.textArea);
                DrawCheckbox(ref BudgetHistory.Instance.convertFieldScience, "Convert field science");
                DrawCheckbox(ref PeriodicPayments.Instance.acceleratedStart, "Accelerated Start");
                GUILayout.EndVertical();
            }
            else
            {
                //TODO - is there a way to find out/fix what size the section is?
                GUILayout.Space(78);
            }

            if (GUILayout.Button("Done", HighLogic.Skin.button, GUILayout.Width(75), GUILayout.Height(36)))
            {
                SetVisible(false);
                PeriodicPayments.Instance.resetNextFunding();
            }
        }
    }
}