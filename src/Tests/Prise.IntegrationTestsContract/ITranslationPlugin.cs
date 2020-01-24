using System.Collections.Generic;
using System.Threading.Tasks;

namespace Prise.IntegrationTestsContract
{
    public class TranslationInput
    {
        public string ContentToTranslate { get; set; }
    }

    public class TranslationOutput
    {
        public string Translation { get; set; }
        public string ToLanguage { get; set; }
        public decimal Accuracy { get; set; }
    }

    public interface ITranslationPlugin
    {
        Task<IEnumerable<TranslationOutput>> Translate(TranslationInput input);
    }
}
