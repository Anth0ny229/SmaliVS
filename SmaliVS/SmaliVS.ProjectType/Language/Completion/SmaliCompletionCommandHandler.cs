using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace SmaliVS.Language.Completion
{
    [Export(typeof(IVsTextViewCreationListener))]
    [Name("Smali completion handler")]
    [ContentType("Smali")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal class SmaliCompletionHandlerProvider : IVsTextViewCreationListener
    {
#pragma warning disable 0649
        [Import]
        internal IVsEditorAdaptersFactoryService AdapterService;
        [Import]
        internal ICompletionBroker CompletionBroker;
        [Import]
        internal SVsServiceProvider ServiceProvider;
#pragma warning restore 0649

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            ITextView textView = AdapterService.GetWpfTextView(textViewAdapter);
            if (textView == null)
                return;

            Func<SmaliCompletionCommandHandler> createCommandHandler =
                () => new SmaliCompletionCommandHandler(textViewAdapter, textView, this);
            textView.Properties.GetOrCreateSingletonProperty(createCommandHandler);
        }
    }

    internal class SmaliCompletionCommandHandler : IOleCommandTarget
    {
        private readonly IOleCommandTarget _mNextCommandHandler;
        private readonly ITextView _mTextView;
        private readonly SmaliCompletionHandlerProvider _mProvider;
        private ICompletionSession _mSession;

        internal SmaliCompletionCommandHandler(IVsTextView textViewAdapter, ITextView textView, SmaliCompletionHandlerProvider provider)
        {
            _mTextView = textView;
            _mProvider = provider;

            //add the command to the command chain
            textViewAdapter.AddCommandFilter(this, out _mNextCommandHandler);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return _mNextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (VsShellUtilities.IsInAutomationFunction(_mProvider.ServiceProvider))
            {
                return _mNextCommandHandler.Exec(ref pguidCmdGroup, nCmdId, nCmdexecopt, pvaIn, pvaOut);
            }
            //make a copy of this so we can look at it after forwarding some commands
            uint commandId = nCmdId;
            char typedChar = char.MinValue;
            //make sure the input is a char before getting it
            if (pguidCmdGroup == VSConstants.VSStd2K && nCmdId == (uint)VSConstants.VSStd2KCmdID.TYPECHAR)
            {
                typedChar = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
            }

            //check for a commit character
            if (nCmdId == (uint)VSConstants.VSStd2KCmdID.RETURN
                || nCmdId == (uint)VSConstants.VSStd2KCmdID.TAB
                || (char.IsWhiteSpace(typedChar) || char.IsPunctuation(typedChar)))
            {
                //check for a a selection
                if (_mSession != null && !_mSession.IsDismissed)
                {
                    //if the selection is fully selected, commit the current session
                    if (_mSession.SelectedCompletionSet.SelectionStatus.IsSelected)
                    {
                        _mSession.Commit();
                        //also, don't add the character to the buffer
                        return VSConstants.S_OK;
                    }
                    else
                    {
                        //if there is no selection, dismiss the session
                        _mSession.Dismiss();
                    }
                }
            }

            //pass along the command so the char is added to the buffer
            int retVal = _mNextCommandHandler.Exec(ref pguidCmdGroup, nCmdId, nCmdexecopt, pvaIn, pvaOut);
            bool handled = false;
            if (!typedChar.Equals(char.MinValue) && char.IsLetterOrDigit(typedChar))
            {
                if (_mSession == null || _mSession.IsDismissed) // If there is no active session, bring up completion
                {
                    TriggerCompletion();
                    _mSession.Filter();
                }
                else    //the completion session is already active, so just filter
                {
                    _mSession.Filter();
                }
                handled = true;
            }
            else if (commandId == (uint)VSConstants.VSStd2KCmdID.BACKSPACE   //redo the filter if there is a deletion
                || commandId == (uint)VSConstants.VSStd2KCmdID.DELETE)
            {
                if (_mSession != null && !_mSession.IsDismissed)
                    _mSession.Filter();
                handled = true;
            }
            if (handled) return VSConstants.S_OK;
            return retVal;
        }

        private void TriggerCompletion()
        {
            //the caret must be in a non-projection location 
            SnapshotPoint? caretPoint =
            _mTextView.Caret.Position.Point.GetPoint(
            textBuffer => (!textBuffer.ContentType.IsOfType("projection")), PositionAffinity.Predecessor);
            if (!caretPoint.HasValue)
            {
                return;
            }

            _mSession = _mProvider.CompletionBroker.CreateCompletionSession
         (_mTextView,
                caretPoint.Value.Snapshot.CreateTrackingPoint(caretPoint.Value.Position, PointTrackingMode.Positive),
                true);

            //subscribe to the Dismissed event on the session 
            _mSession.Dismissed += OnSessionDismissed;
            _mSession.Start();
        }

        private void OnSessionDismissed(object sender, EventArgs e)
        {
            _mSession.Dismissed -= OnSessionDismissed;
            _mSession = null;
        }
    }
}
