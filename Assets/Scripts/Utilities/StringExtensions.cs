using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nukebox.Utilities
{
    public static class StringExtensions
    {
        private const string colorTagFormat = "<color=#{1}>{0}</color>";

        public static string ToColoredString(this string @string, Color color)
        {
            return string.Format(colorTagFormat, @string, ColorUtility.ToHtmlStringRGBA(color));
        }

        public static string ToColoredString(this string @string, Color32 color)
        {
            return string.Format(colorTagFormat, @string, ColorUtility.ToHtmlStringRGBA(color));
        }

    }
}
