using System.Collections.Generic;
using Sektionsliga.Models;

namespace Sektionsliga.Services.Language;

public interface ILanguageService
{
    List<LanguageOptionModel> GetAvailableLanguages();
}
