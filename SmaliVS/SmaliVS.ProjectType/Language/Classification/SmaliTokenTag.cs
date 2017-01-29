using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Irony.Parsing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace SmaliVS.Language.Classification
{
    [Export(typeof(ITaggerProvider))]
    [ContentType("Smali")]
    [TagType(typeof(ClassificationTag))]
    internal sealed class SmaliTokenTagProvider : ITaggerProvider
    {
        [Export]
        [Name("Smali")]
        [BaseDefinition("code")]
        internal static ContentTypeDefinition SmaliContentType { get; set; }

        [Export]
        [FileExtension(".smali")]
        [ContentType("Smali")]
        internal static FileExtensionToContentTypeDefinition SmaliFileType { get; set; }

        [Import]
        internal IClassificationTypeRegistryService ClassificationTypeRegistry { get; set; }

        [Import]
        internal IBufferTagAggregatorFactoryService AggregatorFactory { get; set; }

        [Import]
        internal IClassificationTypeRegistryService ClassificationTypeRegistryService { get; set; }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            TaggerColors.Instance.Init(ClassificationTypeRegistryService);
            return new SmaliTokenTagger(buffer) as ITagger<T>;
        }
    }

    internal sealed class SmaliTokenTagger : ITagger<ClassificationTag>
    {
        public ITextBuffer Buffer1 { get; }
        readonly Scanner _scanner;

        internal SmaliTokenTagger(ITextBuffer buffer)
        {
            Buffer1 = buffer;
            var grammar = SmaliGrammar.Instance;
            var parser = new Parser(grammar) { Context = { Mode = ParseMode.VsLineScan } };
            _scanner = parser.Scanner;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }

        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (SnapshotSpan curSpan in spans)
            {
                // Get our start and end line
                var startLine = curSpan.Start.GetContainingLine();
                var endLine = curSpan.End.GetContainingLine();

                for (int x = startLine.LineNumber; x < endLine.LineNumber; x++)
                {
                    var curLine = curSpan.Snapshot.GetLineFromLineNumber(x);
                    var curLineText = curLine.GetText();
                    _scanner.VsSetSource(curLineText, 0);

                    int state = 0; Token token;
                    while ((token = _scanner.VsReadToken(ref state)) != null)
                    {
                        var tokenSpan = new SnapshotSpan(curSpan.Snapshot, new Span(curLine.Start.Position + token.Location.Position, token.Length));
                        if (tokenSpan.IntersectsWith(curSpan))
                            yield return new TagSpan<ClassificationTag>(tokenSpan, TaggerColors.Instance.TagColors[token.EditorInfo.Type]);
                    }
                }
            }
        }
    }
}
