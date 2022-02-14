#region License

// Distributed under the MIT License
// ============================================================
// Copyright (c) Upendo Ventures, LLC
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
// and associated documentation files (the "Software"), to deal in the Software without restriction, 
// including without limitation the rights to use, copy, modify, merge, publish, distribute, 
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or 
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
// THE SOFTWARE.

#endregion

using System;
using System.IO;
using System.Xml;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;

namespace Upendo.Modules.UpendoPrompt.Components
{
    public abstract class PromptBase : ConsoleCommandBase, IConsoleCommand
    {
        protected abstract void LogError(Exception ex);

        public abstract override ConsoleResultModel Run();

        public override string LocalResourceFile
        {
            get
            {
                return Constants.PromptLocalResourceFile;
            }
        }

        protected string GetConfigFile(string configFile)
        {
            if (configFile.EndsWith(Constants.CONFIG_EXT, StringComparison.InvariantCultureIgnoreCase))
            {
                var configDoc = Config.Load(configFile);
                using (var txtWriter = new StringWriter())
                {
                    using (var writer = new XmlTextWriter(txtWriter))
                    {
                        writer.Formatting = Formatting.Indented;
                        configDoc.WriteTo(writer);
                    }

                    return txtWriter.ToString();
                }
            }
            else
            {
                var doc = File.ReadAllText(Path.Combine(Globals.ApplicationMapPath, configFile));
                return doc;
            }
        }

        protected bool GetLog4netStatus()
        {
            var doc = new XmlDocument();
            doc.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.LOG4NET_CONFIG));

            var node = doc.DocumentElement.SelectSingleNode("/log4net/root/level");
            var log4netStatus = node.Attributes["value"].Value.ToString();

            return (log4netStatus == "All");
        }
    }
}