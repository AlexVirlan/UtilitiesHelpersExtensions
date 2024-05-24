using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AlexVirlan.Entities;

namespace AlexVirlan.Utilities
{
    public class Helpers
    {
        #region DLL imports
        [DllImport("user32.dll", EntryPoint = "SetWindowText")]
        private static extern int SetWindowText(IntPtr hWnd, string text);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, string lParam);
        #endregion

        public static string GetCopyPath(string path)
        {
            if (string.IsNullOrEmpty(path)) { return path; }

            string? filePathOnly = Path.GetDirectoryName(path);
            if (filePathOnly is null) { return path; }
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
            string fileExtension = Path.GetExtension(path);

            int index = 0;
            string result = string.Empty;
            while (File.Exists(path))
            {
                index++;
                result = Path.Combine(filePathOnly, $"{fileNameWithoutExtension} ({index}){fileExtension}");
            }
            return result;
        }

        public static FunctionResponse SetStartup(string appName, bool active = true, string args = "")
        {
            if (appName.INOE()) { return new FunctionResponse(error: true, message: "The 'appName' parameter is empty."); }
            RegistryKey? rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (rk is null) { return new FunctionResponse(error: true, message: "The registry key is null."); }
            if (active) { rk.SetValue(appName, $@"""{Application.ExecutablePath}""" + (args.INOE() ? "" : $" {args}")); }
            else { rk.DeleteValue(appName, throwOnMissingValue: false); }
            return new FunctionResponse(error: false);
        }

        public static Color? GetColor(string colorName, Color? fallbackColor = null)
        {
            Color color = Color.FromName(colorName);
            if (color.IsKnownColor)
            {
                return color;
            }
            else
            {
                if (fallbackColor is null) { return null; }
                else { return fallbackColor; }
            }
        }

        public static Size GetSize(string sizeText, Size fallbackSize)
        {
            try
            {
                string[] sizeArray = sizeText.Trim().Split(',', StringSplitOptions.TrimEntries);
                if (sizeArray.Length == 2)
                {
                    bool isNum1 = int.TryParse(sizeArray[0], out int width);
                    bool isNum2 = int.TryParse(sizeArray[1], out int height);
                    if (isNum1 && isNum2) { return new Size(width, height); }
                }
                return fallbackSize;
            }
            catch (Exception)
            {
                return fallbackSize;
            }
        }

        public static PathType GetPathType(string path)
        {
            if (string.IsNullOrEmpty(path)) { return PathType.Unknown; }
            FileAttributes fileAttributes = File.GetAttributes(path);
            if ((fileAttributes & FileAttributes.Directory) == FileAttributes.Directory)
            { return PathType.Directory; }
            else
            { return PathType.File; }
        }

        public static DateTime UnixToDateTime(int unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
        
        public static List<TextEditor> GetTextEditors(bool onlyExisting = true)
        {
            List<TextEditor> textEditors = new List<TextEditor>();

            #region VS Code
            string vsCodeReg = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Classes\vscode\shell\open\command", "", "").ToStringSafely();
            MatchCollection vscMatches = Regex.Matches(vsCodeReg, @"""(.*Code\.exe)""");
            string vscPath = string.Empty;
            if (vscMatches.Count > 0) { vscPath = vscMatches[0].Groups[1].Value; }
            textEditors.Add(new TextEditor("Visual Studio Code", File.Exists(vscPath), vscPath));
            #endregion

            #region Sublime
            string sublimeReg = Registry.GetValue(@"HKEY_CLASSES_ROOT\*\shell\Open with Sublime Text\command", "", "").ToStringSafely();
            if (!string.IsNullOrEmpty(sublimeReg) && sublimeReg.Contains(".exe"))
            { sublimeReg = sublimeReg.Remove(sublimeReg.LastIndexOf(".exe")) + ".exe"; }
            textEditors.Add(new TextEditor("Sublime Text", File.Exists(sublimeReg), sublimeReg));
            #endregion

            #region Notepad++
            RegistryKey localRegKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
                Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);
            string nppReg = localRegKey.OpenSubKey(@"SOFTWARE\Notepad++")?.GetValue("").ToStringSafely() + "\\notepad++.exe";
            textEditors.Add(new TextEditor("Notepad++", File.Exists(nppReg), nppReg.ToStringSafely()));
            #endregion

            #region Atom
            string atomReg = Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Classes\Applications\atom.exe\shell\open\command", "", "").ToStringSafely();
            if (string.IsNullOrEmpty(atomReg))
            { atomReg = Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Classes\atom\shell\open\command", "", "").ToStringSafely(); }
            MatchCollection atomMatches = Regex.Matches(atomReg, @"""(.*atom\.exe)""");
            string atomPath = string.Empty;
            if (atomMatches.Count > 0)
            {
                atomPath = atomMatches[0].Groups[1].Value;
                int atomLindex = atomPath.LastIndexOf(@"\atom\", StringComparison.OrdinalIgnoreCase);
                if (atomLindex > -1) { atomPath = atomPath.Remove(atomLindex) + @"\atom\atom.exe"; }
            }
            textEditors.Add(new TextEditor("Atom", File.Exists(atomPath), atomPath));
            #endregion

            #region Notepad
            bool notepadExists = File.Exists($"{Environment.SystemDirectory}\\notepad.exe");
            textEditors.Add(new TextEditor("Microsoft Notepad", notepadExists, $"{Environment.SystemDirectory}\\notepad.exe"));
            #endregion

            if (onlyExisting) { textEditors = textEditors.Where(te => te.Exists).ToList(); }
            return textEditors;
        }

        public static void ShowNotepadMessage(string? message = null, string? title = null)
        {
            Process? notepad = Process.Start(new ProcessStartInfo("notepad.exe"));
            if (notepad != null)
            {
                notepad.WaitForInputIdle();
                if (!string.IsNullOrEmpty(title)) { _ = SetWindowText(notepad.MainWindowHandle, title); }
                if (!string.IsNullOrEmpty(message))
                {
                    IntPtr child = FindWindowEx(notepad.MainWindowHandle, new IntPtr(0), "Edit", null);
                    _ = SendMessage(child, 0x000C, 0, message);
                }
            }
        }
    }
}
