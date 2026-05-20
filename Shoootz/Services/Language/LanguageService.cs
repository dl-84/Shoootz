using System.Collections.Generic;
using Shoootz.Models;
using Shoootz.Services.Grafik;

namespace Shoootz.Services.Language;

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
