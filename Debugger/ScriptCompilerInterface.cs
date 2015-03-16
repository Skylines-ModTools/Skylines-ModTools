using System;
using System.IO;
using ColossalFramework;
using ColossalFramework.Plugins;
using UnityEngine;

namespace Debugger
{
    public static class ScriptCompilerInterface
    {

        public static void CompileAndRun(string code)
        {
            var tmp = CreateTemporaryDirectory();
            var className = "tmpClass" + UnityEngine.Random.Range(0, int.MaxValue);
            File.WriteAllText(Path.Combine(tmp, "Code.cs"), String.Format(codeTemplate, className, code));
            PluginManager.CompileSourceInFolder(tmp, tmp, new string[]{});
        }

        public static string CreateTemporaryDirectory()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }

        private static string codeTemplate = @"
using System;
using System.IO;
using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace {0}
{
    public static class {0}
    {
        public static void RunCommand()
        {
            {1}
        }
    }
}
";
    }
}
