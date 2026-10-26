using System.Collections.Generic;
using Shoootz.Common.Models.Language;

namespace Shoootz.Common.Services.Language;

public interface ILanguageService
{
    List<LanguageOptionModel> GetAvailableLanguages();
}
