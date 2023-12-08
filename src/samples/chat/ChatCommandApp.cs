using System.Runtime.CompilerServices;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Chat;

public class ChatCommandApp
{

    private static int USER_PANEL_WIDTH = 30;
    Layout _root;
    Layout _debugPanel;
    Layout _chatHistoryPanel;
    Layout _userListPanel; 

    string _chatHistory;
    string _debugMessages;
    List<string> _users = new List<string>();
    

    public ChatCommandApp()
    {

    }

    public void AddUser(string user) {
        
        _users.Add(user.Substring(USER_PANEL_WIDTH));

        _userListPanel.Update(
            new Panel(
                Align.Left(
                    new Markup(string.Join(Environment.NewLine, _users)),
                    VerticalAlignment.Top))
                .Expand());
        
        AnsiConsole.Write(_root);
    }

    public void AddChatHistory(string from, string message) {
        _chatHistory += Environment.NewLine + $"[grey]{from}[/]: {message}";
        // Update the left column
        _chatHistoryPanel.Update(    
            new Panel(
                Align.Left(
                    new Markup(_chatHistory),
                    VerticalAlignment.Top))
                .Expand());

        AnsiConsole.Write(_root);
    }

    public void AddDebugMessage(string message) {
        var now = DateTime.Now.ToString("HH:mm:ss.FFF");
        _debugMessages += Environment.NewLine + $"[grey]<{now}>[/] {message}";
        // Update the left column
        _debugPanel.Update(    
            new Panel(
                Align.Left(
                    new Markup(_debugMessages),
                    VerticalAlignment.Top))
                .Expand());

        // Render the layout
        AnsiConsole.Write(_root);
    }

    public void Build() {

        // Create the layout
        _root = new Layout("Root")
            .SplitRows(
                
                new Layout("Top")
                    .Size(6)
                    .SplitColumns(
                        _debugPanel = new Layout("Debug")),

                new Layout("Bottom")
                .SplitColumns(
                        _chatHistoryPanel = new Layout("ChatHistory"),
                        _userListPanel = new Layout("UserList")
                            .Size(USER_PANEL_WIDTH)
                        ));

        // Render the layout
        AnsiConsole.Write(_root);
    }
}
