using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlexVirlan.Entities;

namespace AlexVirlan.Utilities
{
    public static class Extensions
    {
        #region String
        public static bool INOE(this string? str) => string.IsNullOrEmpty(str);

        public static string ReplaceIfEmpty(this string? str, string replacement)
        {
            return string.IsNullOrEmpty(str) ? replacement : (string)str;
        }

        public static int ToInt(this string? str)
        {
            if (string.IsNullOrEmpty(str)) { return 0; }
            bool parsed = int.TryParse(str, out int val);
            if (parsed) { return val; }
            return 0;
        }

        public static string Repeat(this string s, int n) => new StringBuilder(s.Length * n).Insert(0, s, n).ToString();

        public static string? Repeat(this string? str, int times = 2, StringRepeatType repeatType = StringRepeatType.SBInsert)
        {
            if (str.INOE()) { return str; }
            if (times < 2) { times = 2; }
            switch (repeatType)
            {
                case StringRepeatType.Replace:
                    return new string('X', times).Replace("X", str);

                case StringRepeatType.Concat:
                    return string.Concat(Enumerable.Repeat(str, times));

                default:
                case StringRepeatType.SBInsert:
                    return new StringBuilder(str.Length * times).Insert(0, str, times).ToString();

                case StringRepeatType.SBAppendJoin:
                    return new StringBuilder(str.Length * times).AppendJoin(str, new string[times + 1]).ToString();
            }
        }
        #endregion

        #region TreeView
        public static void CheckTreeNodeCollection(this TreeNodeCollection treeNodeCollection, bool isChecked = true)
        {
            foreach (TreeNode trNode in treeNodeCollection)
            {
                trNode.Checked = isChecked;
                if (trNode.Nodes.Count > 0) { CheckTreeNodeCollection(trNode.Nodes, isChecked); }
            }
        }

        public static void CheckAllChildNodes(this TreeNode treeNode, bool isChecked = true)
        {
            foreach (TreeNode node in treeNode.Nodes)
            {
                node.Checked = isChecked;
                if (node.Nodes.Count > 0)
                {
                    CheckAllChildNodes(node, isChecked);
                }
            }
        }

        public static void ChecktParents(this TreeNode treeNode, bool isChecked = true)
        {
            TreeNode parent = treeNode.Parent;
            if (parent == null) { return; }
            if (!isChecked && HasCheckedNode(parent)) { return; }
            parent.Checked = isChecked;
            ChecktParents(parent, isChecked);
        }

        public static bool HasCheckedNode(this TreeNode treeNode)
        {
            return treeNode.Nodes.Cast<TreeNode>().Any(node => node.Checked);
        }

        public static IEnumerable<TreeNode> Descendants(this TreeNodeCollection treeNodeCollection)
        {
            foreach (TreeNode node in treeNodeCollection)
            {
                yield return node;
                foreach (TreeNode child in node.Nodes.Descendants())
                {
                    yield return child;
                }
            }
        }

        public static IEnumerable<TreeNode> Descendants(this TreeNode treeNode)
        {
            yield return treeNode;
            foreach (TreeNode child in treeNode.Nodes.Descendants())
            {
                yield return child;
            }
        }

        public static IEnumerable<TreeNode> Descendants(this TreeView treeView)
        {
            foreach (TreeNode node in treeView.Nodes.Descendants()) { yield return node; }
        }
        #endregion

        #region Bool
        public static string ToStringSafely(this bool? b, string valueIfNull = "")
        {
            if (b is null) { return string.IsNullOrEmpty(valueIfNull) ? "" : valueIfNull; }
            return b.ToString();
        }
        #endregion

        #region Double
        public static string ToStringSafely(this double? value, string valueIfNull = "")
        {
            if (value is null) { return string.IsNullOrEmpty(valueIfNull) ? "" : valueIfNull; }
            return value.ToString();
        }
        #endregion

        #region Decimal
        public static int ToInt(this decimal value)
        {
            return Convert.ToInt32(value);
        }
        #endregion

        #region Object
        public static string ToStringSafely(this object? @object)
        {
            if (@object is null) { return string.Empty; }
            string? stringifiedObject = @object.ToString();
            return (stringifiedObject is null ? string.Empty : stringifiedObject);
        }
        #endregion

        #region Color
        public static string ToHex(this Color c) => c.IsEmpty ? "" : $"#{c.R:X2}{c.G:X2}{c.B:X2}";

        public static string ToRGB(this Color c) => c.IsEmpty ? "" : $"RGB({c.R},{c.G},{c.B})";

        public static Color Invert(this Color color)
        {
            Color invertedColor = Color.FromArgb(color.ToArgb() ^ 0xffffff);

            if (invertedColor.R > 110 && invertedColor.R < 150 &&
                invertedColor.G > 110 && invertedColor.G < 150 &&
                invertedColor.B > 110 && invertedColor.B < 150)
            {
                int avg = (invertedColor.R + invertedColor.G + invertedColor.B) / 3;
                avg = avg > 128 ? 200 : 60;
                invertedColor = Color.FromArgb(avg, avg, avg);
            }

            return invertedColor;
        }
        #endregion

        #region Others
        public static string Get(this JObject? obj, string keyName)
        {
            if (obj is null ||
                string.IsNullOrEmpty(keyName) ||
                !obj.ContainsKey(keyName))
            { return string.Empty; }
            return obj[keyName]?.ToString() ?? "";
        }
        #endregion
    }
}
