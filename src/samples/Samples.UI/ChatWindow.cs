
//------------------------------------------------------------------------------

//  <auto-generated>
//      This code was generated by:
//        TerminalGuiDesigner v1.1.0.0
//      You can make changes to this file and they will not be overwritten when saving.
//  </auto-generated>
// -----------------------------------------------------------------------------
namespace Samples.UI
{
    using Terminal.Gui;


    public partial class ChatWindow
    {

        readonly ChatWindowConfig _config;
        ListView _debugList;
        ListView _chatList;

        public TextField _chatMessage;
        private static readonly List<string> _peers = new List<string>();
        private static readonly List<string> _messages = new List<string>();
        private static readonly List<string> _debugMessages = new List<string>();
        private string _username;

        public ChatWindow(ChatWindowConfig config)
        {
            InitializeComponent();

        }

        public void AddPeer(string user)
        {
            Application.MainLoop.Invoke(() =>
            {
                _peers.Add(user);
                // Tell view to redraw
                userListView.SetNeedsDisplay();
            });
        }

        public void AddChatHistory(string from, string message)
        {
            Application.MainLoop.Invoke(() =>
                {
                    _messages.Add($"{from}: {message}");
                    _chatList.SetSource(_messages);
                    chatListView.Text = "";
                });
        }

        public void AddDebug(string logMessage)
        {
            Application.MainLoop.Invoke(() =>
                {
                    _debugMessages.Add($"{logMessage}");
                    listLogMessages.SetSource(_debugMessages);
                });
        }
    }
}
