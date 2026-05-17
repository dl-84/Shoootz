using System.Collections.Generic;
using Sektionsliga.Models;
using Sektionsliga.Services.Flag;

namespace Sektionsliga.Services.Language;

public class LanguageService(IFlagService flagService) : ILanguageService
{
    private readonly List<string> AvailableLanguages = ["de", "en"];
    
    public List<LanguageOptionModel> GetAvailableLanguages()
    {
        List<LanguageOptionModel> result = [];

        foreach (string language in AvailableLanguages)
        {
            result.Add(new LanguageOptionModel(language, flagService.GetFlag(language)));
        }
        
        return result;
    }
}
