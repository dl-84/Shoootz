using System.Collections.Generic;
using Sektionsliga.Models;
using Sektionsliga.Services.Flag;

namespace Sektionsliga.Services.Language;

public class LanguageService(IFlagService flagService) : ILanguageService
{
    public List<LanguageOptionModel> GetAvailableLanguages() =>
        [
            new LanguageOptionModel("de", flagService.GetFlag("de")),
            new LanguageOptionModel("en", flagService.GetFlag("en")),
        ];
}
