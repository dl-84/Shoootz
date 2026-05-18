using System.Collections.Generic;
using Sektionsliga.Models;
using Sektionsliga.Services.Grafik;

namespace Sektionsliga.Services.Language;

internal class LanguageService(IGrafikService grafikService) : ILanguageService
{
    private readonly List<string> availableLanguages = ["de", "en"];

    public List<LanguageOptionModel> GetAvailableLanguages()
    {
        List<LanguageOptionModel> result = [];

        foreach (string language in availableLanguages)
        {
            result.Add(new LanguageOptionModel(language, grafikService.GetFlag(language)));
        }

        return result;
    }
}
