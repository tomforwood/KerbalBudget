using System;
using UnityEngine;
using static KerbalBudget.KerbalBudget;

namespace KerbalBudget
{
    internal abstract class SimpleWindow
    {
        private string windowTitle;
        private int windowId;
        protected Rect windowPos;
        private bool visible;
        private GUIStyle styleCheckbox;

        protected SimpleWindow(string windowTitle, float defaultWidth, float defaultHeight)
        {
            this.windowTitle = windowTitle;
            this.windowId = windowTitle.GetHashCode() + new System.Random().Next(65536);
            windowPos = new Rect((Screen.width - defaultWidth) / 2, (Screen.height - defaultHeight) / 2, defaultWidth, defaultHeight);

            styleCheckbox = new GUIStyle(new GUIStyle(HighLogic.Skin.toggle));

            visible = false;
        }

        public bool IsVisible()
        {
            return visible;
        }

        public virtual void SetVisible(bool newValue)
        {
            if (newValue)
            {
                if (!visible)
                {
                    RenderingManager.AddToPostDrawQueue(3, new Callback(DrawWindow));
                }
            }
            else
            {
                if (visible)
                {
                    RenderingManager.RemoveFromPostDrawQueue(3, new Callback(DrawWindow));
                }
            }

            this.visible = newValue;
        }

        protected virtual void DrawWindow()
        {
            if (visible)
            {
            GUI.skin = HighLogic.Skin;
                
                windowPos = GUILayout.Window(windowId, windowPos, DrawContents, windowTitle, GUILayout.ExpandWidth(true),
                    GUILayout.ExpandHeight(true), GUILayout.MinWidth(64), GUILayout.MinHeight(64));

            }
        }

        protected abstract void DrawContents(int id);

        internal Boolean DrawRadioList(Boolean Horizontal, ref Int32 Selected, params String[] Choices)
        {
            Int32 InitialChoice = Selected;

            if (Horizontal)
                GUILayout.BeginHorizontal();
            else
                GUILayout.BeginVertical();

            for (Int32 intChoice = 0; intChoice < Choices.Length; intChoice++)
            {
                //checkbox
                GUILayout.BeginHorizontal();
                if (GUILayout.Toggle((intChoice == Selected), "", styleCheckbox))
                    Selected = intChoice;
                //button that looks like a label
                if (GUILayout.Button(Choices[intChoice],HighLogic.Skin.label))
                    Selected = intChoice;
                GUILayout.EndHorizontal();
            }
            if (Horizontal)
                GUILayout.EndHorizontal();
            else
                GUILayout.EndVertical();

            if (InitialChoice != Selected)
                Log(String.Format("Radio List Changed:{0} to {1}", InitialChoice, Selected));


            return !(InitialChoice == Selected);
        }

        internal Boolean DrawCheckbox(ref Boolean blnVar, String content, params GUILayoutOption[] options)
        {
            // return DrawToggle(ref blnVar, strText, KACResources.styleCheckbox, options);
            Boolean blnReturn = false;
            Boolean blnToggleInitial = blnVar;

            /*if (settings.SelectedSkin == Settings.DisplaySkin.Default)
                GUILayout.Space(-3);*/

            GUILayout.BeginHorizontal();
            //Draw the radio
            blnVar = GUILayout.Toggle(blnVar, "", HighLogic.Skin.toggle, options);
            //Spacing
            GUILayout.Space(5);

            //And the button like a label
            if (GUILayout.Button(content, HighLogic.Skin.label, options))
            {
                //if its clicked then toggle the boolean
                blnVar = !blnVar;
                Log("Toggle Changed:" + blnVar);
            }

            GUILayout.EndHorizontal();
            /*if (settings.SelectedSkin == Settings.DisplaySkin.Default)
                GUILayout.Space(-3);*/

            //If output value doesnt = input value
            if (blnToggleInitial != blnVar)
            {
                //KACWorker.DebugLogFormatted("Toggle recorded:" + blnVar);
                blnReturn = true;
            }
            return blnReturn;
        }
    }
}