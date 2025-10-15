using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Data;
using Warudo.Core.Data.Models;
using Warudo.Core.Scenes;
using Warudo.Plugins.Core.Assets;
using Warudo.Plugins.Core.Utils;

namespace Warudo.Plugins.Scene.Assets
{
    public abstract class ADebuggableAsset : Asset
    {
        // // Debug
        // DEBUG_SECTION = 2000,
        // DEBUG_MODE_INPUT = 2001,
        // CLEAR_DEBUG_LOG_TRIGGER = 2002,
        // // Debug Info
        // DEBUG_INFO_SECTION = 2500,
        // DEBUG_LOG_HEADER = 2501,
        // DEBUG_LOG_MESSAGE = 2502,

        private const string DEFAULT_DEBUG_LOG_MESSAGE = "Debug log messages will appear here.";
        private const string DEFAULT_DEBUG_INFO_MESSAGE = "Debug info will appear here";

        [Section("👁️‍🗨️ Debug", UnityConstraintUIOrdering.DEBUG_SECTION)]
        [DataInput(UnityConstraintUIOrdering.DEBUG_MODE_INPUT)]
        [Label("DEBUG")]
        public bool DebugMode = false;

        [Trigger(UnityConstraintUIOrdering.CLEAR_DEBUG_LOG_TRIGGER)]
        [HiddenIf(nameof(DebugMode), Is.False)]
        public void ClearDebugLog()
        {
            debugMessages.Clear();
            SetDataInput(nameof(DebugMessage), DEFAULT_DEBUG_LOG_MESSAGE, broadcast: true);
        }

        [Section("👁️‍🗨️ Debug Info", UnityConstraintUIOrdering.DEBUG_INFO_SECTION)]
        [SectionHiddenIf(nameof(DebugMode), Is.False)]
        [Markdown(order: UnityConstraintUIOrdering.DEBUG_LOG_HEADER, primary: true)]
        public string DebugLogHeader = "Log Window";

        [Markdown(UnityConstraintUIOrdering.DEBUG_LOG_MESSAGE)]
        public string DebugMessage = DEFAULT_DEBUG_LOG_MESSAGE;


        [Markdown(order: UnityConstraintUIOrdering.DEBUG_INFO_HEADER, primary: true)]
        public string DebugInfoHeader = "Debug Info";

        [Markdown(UnityConstraintUIOrdering.DEBUG_INFO)]
        public string DebugInfo = DEFAULT_DEBUG_INFO_MESSAGE;

        private List<string> debugMessages = new List<string>();

        public void DebugToast(string msg)
        {
            // if (!DebugMode)
            //     return;

            Context.Service.Toast(Warudo.Core.Server.ToastSeverity.Info, "Debug", msg);
        }

        public void DebugLog(string msg)
        {
            // if (!DebugMode)
            //     return;

            debugMessages.Add(msg);
            SetDataInput(
                nameof(DebugMessage),
                String.Join(
                    "<br>",
                    debugMessages.Skip(Math.Max(0, debugMessages.Count - 10)).ToArray()
                ),
                broadcast: true
            );
        }

        protected virtual void UpdateDebugInfo() { }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (DebugMode)
            {
                UpdateDebugInfo();
            }
        }
    }
}
