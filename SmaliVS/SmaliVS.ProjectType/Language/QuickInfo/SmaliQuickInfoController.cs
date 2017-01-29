using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace SmaliVS.Language.QuickInfo
{
    internal class SmaliQuickInfoController : IIntellisenseController
    {
        private ITextView _mTextView;
        private readonly IList<ITextBuffer> _mSubjectBuffers;
        private readonly SmaliQuickInfoControllerProvider _mProvider;

        internal SmaliQuickInfoController(ITextView textView, IList<ITextBuffer> subjectBuffers, SmaliQuickInfoControllerProvider provider)
        {
            _mTextView = textView;
            _mSubjectBuffers = subjectBuffers;
            _mProvider = provider;

            _mTextView.MouseHover += OnTextViewMouseHover;
        }

        private void OnTextViewMouseHover(object sender, MouseHoverEventArgs e)
        {
            //find the mouse position by mapping down to the subject buffer
            SnapshotPoint? point = _mTextView.BufferGraph.MapDownToFirstMatch
                 (new SnapshotPoint(_mTextView.TextSnapshot, e.Position),
                PointTrackingMode.Positive,
                snapshot => _mSubjectBuffers.Contains(snapshot.TextBuffer),
                PositionAffinity.Predecessor);

            if (point == null) return;
            ITrackingPoint triggerPoint = point.Value.Snapshot.CreateTrackingPoint(point.Value.Position,
                PointTrackingMode.Positive);

            if (_mProvider.QuickInfoBroker.IsQuickInfoActive(_mTextView)) return;
            _mProvider.QuickInfoBroker.TriggerQuickInfo(_mTextView, triggerPoint, true);
        }

        public void Detach(ITextView textView)
        {
            if (_mTextView != textView) return;
            _mTextView.MouseHover -= OnTextViewMouseHover;
            _mTextView = null;
        }

        public void ConnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
        }

        public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
        }
    }
}
