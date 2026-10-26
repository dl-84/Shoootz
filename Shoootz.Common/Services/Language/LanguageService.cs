using System.Collections.Generic;
using Shoootz.Common.Models.Language;
using Shoootz.Common.Services.Graphics;

namespace Shoootz.Common.Services.Language;

public class LanguageService(IGraphicsService graphicsService) : ILanguageService
{
    private readonly List<string> _availableLanguages = ["de", "en"];

    public List<LanguageOptionModel> GetAvailableLanguages()
    {
        List<LanguageOptionModel> result = [];

        foreach (string language in _availableLanguages)
        {
            result.Add(new LanguageOptionModel(language, graphicsService.GetFlag(language)));
        }

        return result;
    }
}
