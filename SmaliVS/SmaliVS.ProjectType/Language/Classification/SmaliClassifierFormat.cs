using System.Collections.Generic;
using Irony.Parsing;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;

namespace SmaliVS.Language.Classification
{
    public class TaggerColors
    {
        private static TaggerColors _instance;
        public static TaggerColors Instance => _instance ?? (_instance = new TaggerColors());

        private IClassificationTypeRegistryService _service;
        public Dictionary<TokenType, ClassificationTag> TagColors { get; private set; }

        public void Init(IClassificationTypeRegistryService service)
        {
            _service = service;
            InitColors();
        }

        private void InitColors()
        {
            TagColors = new Dictionary<TokenType, ClassificationTag>
            {
                [TokenType.Text] = new ClassificationTag(_service.GetClassificationType(PredefinedClassificationTypeNames.Character)),
                [TokenType.Keyword] = new ClassificationTag(_service.GetClassificationType(PredefinedClassificationTypeNames.Keyword)),
                [TokenType.Identifier] = new ClassificationTag(_service.GetClassificationType(PredefinedClassificationTypeNames.Identifier)),
                [TokenType.String] = new ClassificationTag(_service.GetClassificationType(PredefinedClassificationTypeNames.String)),
                [TokenType.Literal] = new ClassificationTag(_service.GetClassificationType(PredefinedClassificationTypeNames.Number)),
                [TokenType.Operator] = new ClassificationTag(_service.GetClassificationType(PredefinedClassificationTypeNames.Operator)),
                [TokenType.LineComment] = new ClassificationTag(_service.GetClassificationType(PredefinedClassificationTypeNames.Comment)),
                [TokenType.Comment] = new ClassificationTag(_service.GetClassificationType(PredefinedClassificationTypeNames.Comment)),
                [TokenType.Delimiter] = new ClassificationTag(_service.GetClassificationType(PredefinedClassificationTypeNames.Identifier))
            };
        }

    }
}
