using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JButler
{
    public static class JLog
    {
        public enum Type { NOR, WRN, ERR, }

        public enum TitleColor
        {
            None,
            LootLocker,
        }


        //----------------------------------------------------------------
        private static Color GetColor(TitleColor color)
        {
            return color switch
            {
                TitleColor.None => default,
                TitleColor.LootLocker => new Color(22f / 255f, 212f / 255f, 96f / 255f),  // LootLocker green.
                _ => default,
            };
        }


        //----------------------------------------------------------------
        /// <summary>
        /// Custom debug log method.
        /// </summary>
        /// <param name="showDebugLog">A bool reference, from any managers, that toggles the logging of this debug message.</param>
        /// <param name="context">The debug log message.</param>
        /// <param name="enableTimeStamp">Log the time stamp of this?</param>
        public static void Log(bool showDebugLog, string context, bool enableTimeStamp = false)
        {
            if (!showDebugLog) return;

            string timeStamp = "";
            if (enableTimeStamp) timeStamp = $"<color=orange>[{System.DateTime.Now.ToLongTimeString()}]</color>";

            Debug.Log($"{timeStamp} {context}");
        }


        //----------------------------------------------------------------
        /// <summary>
        /// Custom debug log method.
        /// </summary>
        /// <param name="showDebugLog">A bool reference, from any managers, that toggles the logging of this debug message.</param>
        /// <param name="type">Log type: Normal / Warning / Error</param>
        /// <param name="title">The title of this debug log message, separated from context.</param>
        /// <param name="titleColor">Color options.</param>
        /// <param name="context">The debug log message.</param>
        /// <param name="enableTimeStamp">Log the time stamp of this?</param>
        private static void LogExt(bool showDebugLog, Type type, string title, TitleColor titleColor, string context, bool enableTimeStamp = false)
        {
            if (!showDebugLog) return;

            string timeStamp = "";
            if (enableTimeStamp) timeStamp = $"<color=orange>[{System.DateTime.Now.ToLongTimeString()}]</color>";

            string coloredTitle;
            if (titleColor == TitleColor.None)
                coloredTitle = $"<b>{title} ►</b>";
            else
            {
                Color c = GetColor(titleColor);
                string titleC = string.Format("<color=#{0:X2}{1:X2}{2:X2}>", (byte)(c.r * 255f), (byte)(c.g * 255f), (byte)(c.b * 255f));
                coloredTitle = $"{titleC}<b>{title} ►</b></color>";
            }

            switch (type)
            {
                case Type.NOR: Debug.Log($"{timeStamp} {coloredTitle} {context}"); break;
                case Type.WRN: Debug.LogWarning($"{timeStamp} {coloredTitle} {context}"); break;
                case Type.ERR: Debug.LogError($"{timeStamp} {coloredTitle} {context}"); break;
            }
        }


        //----------------------------------------------------------------
        /// <summary>
        /// Custom debug log method.
        /// </summary>
        /// <param name="showDebugLog">A bool reference, from any managers, that toggles the logging of this debug message.</param>
        /// <param name="title">The title of this debug log message, separated from context.</param>
        /// <param name="titleColor">Color options.</param>
        /// <param name="context">The debug log message.</param>
        /// <param name="enableTimeStamp">Log the time stamp of this?</param>
        public static void Nor(bool showDebugLog, string title, TitleColor titleColor, string context, bool enableTimeStamp = false)
        {
            JLog.LogExt(showDebugLog, JLog.Type.NOR, title, titleColor, context, enableTimeStamp);
        }


        //----------------------------------------------------------------
        /// <summary>
        /// Custom debug log method.
        /// </summary>
        /// <param name="showDebugLog">A bool reference, from any managers, that toggles the logging of this debug message.</param>
        /// <param name="title">The title of this debug log message, separated from context.</param>
        /// <param name="titleColor">Color options.</param>
        /// <param name="context">The debug log message.</param>
        /// <param name="enableTimeStamp">Log the time stamp of this?</param>
        public static void Wrn(bool showDebugLog, string title, TitleColor titleColor, string context, bool enableTimeStamp = false)
        {
            JLog.LogExt(showDebugLog, JLog.Type.WRN, title, titleColor, context, enableTimeStamp);
        }


        //----------------------------------------------------------------
        /// <summary>
        /// Custom debug log method.
        /// </summary>
        /// <param name="showDebugLog">A bool reference, from any managers, that toggles the logging of this debug message.</param>
        /// <param name="title">The title of this debug log message, separated from context.</param>
        /// <param name="titleColor">Color options.</param>
        /// <param name="context">The debug log message.</param>
        /// <param name="enableTimeStamp">Log the time stamp of this?</param>
        public static void Err(bool showDebugLog, string title, TitleColor titleColor, string context, bool enableTimeStamp = false)
        {
            JLog.LogExt(showDebugLog, JLog.Type.ERR, title, titleColor, context, enableTimeStamp);
        }
    }
}