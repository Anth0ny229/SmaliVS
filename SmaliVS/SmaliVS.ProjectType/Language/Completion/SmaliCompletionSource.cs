using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;

namespace SmaliVS.Language.Completion
{
    internal class SmaliCompletionSource : ICompletionSource
    {
        private readonly SmaliCompletionSourceProvider _mSourceProvider;
        private readonly ITextBuffer _mTextBuffer;
        private List<Microsoft.VisualStudio.Language.Intellisense.Completion> _mCompList;
        private bool _mIsDisposed;

        public SmaliCompletionSource(SmaliCompletionSourceProvider sourceProvider, ITextBuffer textBuffer)
        {
            _mSourceProvider = sourceProvider;
            _mTextBuffer = textBuffer;
        }

        void ICompletionSource.AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            // Create our completion list and add it
            var imgToUse = _mSourceProvider.GetImageSource(StandardGlyphGroup.GlyphGroupMethod);
            _mCompList = SmaliGrammar.Instance.OpCodes.Select(v => new Microsoft.VisualStudio.Language.Intellisense.Completion(v.NameNoSyntax, v.NameNoSyntax, v.Description, imgToUse, null)).ToList();
            
            completionSets.Add(new CompletionSet(
                "Tokens",    //the non-localized title of the tab
                "Tokens",    //the display title of the tab
                FindTokenSpanAtPosition(session.GetTriggerPoint(_mTextBuffer),
                    session),
                _mCompList,
                null));
        }

        private ITrackingSpan FindTokenSpanAtPosition(ITrackingPoint point, ICompletionSession session)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            SnapshotPoint currentPoint = (session.TextView.Caret.Position.BufferPosition) - 1;
            ITextStructureNavigator navigator = _mSourceProvider.NavigatorService.GetTextStructureNavigator(_mTextBuffer);
            TextExtent extent = navigator.GetExtentOfWord(currentPoint);
            return currentPoint.Snapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive);
        }

        public void Dispose()
        {
            if (!_mIsDisposed)
            {
                GC.SuppressFinalize(this);
                _mIsDisposed = true;
            }
        }
    }
}
