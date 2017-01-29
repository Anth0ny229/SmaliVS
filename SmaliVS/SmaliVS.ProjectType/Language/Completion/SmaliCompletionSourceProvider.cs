using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace SmaliVS.Language.Completion
{
    [Export(typeof(ICompletionSourceProvider))]
    [ContentType("Smali")]
    [Name("Smali completion")]
    internal class SmaliCompletionSourceProvider : ICompletionSourceProvider
    {
#pragma warning disable 0649
        [Import] internal ITextStructureNavigatorSelectorService NavigatorService;
        [Import] public IGlyphService GlyphService;
#pragma warning restore 0649

        public System.Windows.Media.ImageSource GetImageSource(StandardGlyphGroup group, StandardGlyphItem item = StandardGlyphItem.GlyphItemPublic)
        {
            return GlyphService.GetGlyph(group, item);
        }

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            return new SmaliCompletionSource(this, textBuffer);
        }
    }
}
