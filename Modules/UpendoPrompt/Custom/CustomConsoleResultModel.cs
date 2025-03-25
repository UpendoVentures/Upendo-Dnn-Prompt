using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Services.Localization;
using Upendo.Modules.UpendoPrompt.Components;


namespace Upendo.Modules.UpendoPrompt.Custom
{
    public class CustomConsoleResultModel : ConsoleResultModel
    {
        public CustomConsoleResultModel()
        {
        }

        public CustomConsoleResultModel(string output) : base(output)
        {
        }

        public new string Output
        {
            get => base.Output; 
            set => base.Output = ModifyOutput(value); 
        }

        private static string ModifyOutput(string output)
        {
            var upendoSponsorMessage = Localization.GetString(Constants.LocalizationKeys.UpendoSponsorMessage, Constants.PromptLocalResourceFile);
            return $"{output}{upendoSponsorMessage}";
        }

    }
}