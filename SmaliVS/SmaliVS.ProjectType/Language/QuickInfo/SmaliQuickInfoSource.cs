using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;

namespace SmaliVS.Language.QuickInfo
{
    internal class SmaliQuickInfoSource : IQuickInfoSource
    {
        private readonly SmaliQuickInfoSourceProvider _mProvider;
        private readonly ITextBuffer _mSubjectBuffer;
        private readonly Dictionary<string, string> _mDictionary;
        private bool _mIsDisposed;

        public SmaliQuickInfoSource(SmaliQuickInfoSourceProvider provider, ITextBuffer subjectBuffer)
        {
            _mProvider = provider;
            _mSubjectBuffer = subjectBuffer;

            //these are the method names and their descriptions
            _mDictionary = SmaliGrammar.Instance.OpCodes.Select((value) => new { value.NameNoSyntax, value.Description })
                      .ToDictionary(pair => pair.NameNoSyntax, pair => pair.Description);

            //m_dictionary.Add("add", "int add(int firstInt, int secondInt)\nAdds one integer to another.");
            //m_dictionary.Add("subtract", "int subtract(int firstInt, int secondInt)\nSubtracts one integer from another.");
            //m_dictionary.Add("multiply", "int multiply(int firstInt, int secondInt)\nMultiplies one integer by another.");
            //m_dictionary.Add("divide", "int divide(int firstInt, int secondInt)\nDivides one integer by another.");
        }

        public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> qiContent, out ITrackingSpan applicableToSpan)
        {
            // Map the trigger point down to our buffer.
            SnapshotPoint? subjectTriggerPoint = session.GetTriggerPoint(_mSubjectBuffer.CurrentSnapshot);
            if (!subjectTriggerPoint.HasValue)
            {
                applicableToSpan = null;
                return;
            }
            
            ITextSnapshot currentSnapshot = subjectTriggerPoint.Value.Snapshot;

            //look for occurrences of our QuickInfo words in the span
            ITextStructureNavigator navigator = _mProvider.NavigatorService.GetTextStructureNavigator(_mSubjectBuffer);

           

            TextExtent extent = navigator.GetExtentOfWord(subjectTriggerPoint.Value);
            if (extent.IsSignificant)
            {
                var tt1 = navigator.GetSpanOfNextSibling(extent.Span);
                var tt2 = navigator.GetSpanOfFirstChild(extent.Span);
                var tt3 = navigator.GetSpanOfPreviousSibling(extent.Span);
                var tt4 = navigator.GetSpanOfEnclosing(extent.Span);
                var t1 = tt1.GetText();
                var t2 = tt2.GetText();
                var t3 = tt3.GetText();
            }

            string searchText = extent.Span.GetText();

            foreach (string key in _mDictionary.Keys)
            {
                int foundIndex = searchText.IndexOf(key, StringComparison.CurrentCultureIgnoreCase);
                if (foundIndex > -1)
                {
                    applicableToSpan = currentSnapshot.CreateTrackingSpan
                        (
                                                //querySpan.Start.Add(foundIndex).Position, 9, SpanTrackingMode.EdgeInclusive
                                                extent.Span.Start + foundIndex, key.Length, SpanTrackingMode.EdgeInclusive
                        );

                    string value;
                    _mDictionary.TryGetValue(key, out value);
                    qiContent.Add(value ?? "");

                    return;
                }
            }

            applicableToSpan = null;
        }

        private void GetFullWord(TextExtent currentSnapshot)
        {
            int start = currentSnapshot.Span.Start.Position;
            int end = currentSnapshot.Span.End.Position;

          //  languageTextOps.GetWordExtent((IVsTextLayer)this._docData, ta, WORDEXTFLAGS.WORDEXT_FINDTOKEN, textSpans);
        }

        public void Dispose()
        {
            if (_mIsDisposed) return;
            GC.SuppressFinalize(this);
            _mIsDisposed = true;
        }
    }
}
