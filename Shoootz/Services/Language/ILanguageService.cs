using System.Collections.Generic;
using Shoootz.Models;

namespace Shoootz.Services.Language;

internal interface ILanguageService
{
    List<LanguageOptionModel> GetAvailableLanguages();
}
