using NetworkUtil;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace ChatClient;

public partial class MainPage : ContentPage
{
    private SocketState theServer;

    public MainPage()
    {
        InitializeComponent();
    }


    /// <summary>
    /// Connect button event handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnConnectClicked(object sender, EventArgs e)
    {
        if (serverAddress.Text == "")
        {
            DisplayAlert("Error", "Please enter a server address", "OK");
            return;
        }

        // Disable the controls and try to connect
        connectButton.IsEnabled = false;
        serverAddress.IsEnabled = false;

        Networking.ConnectToServer(OnConnect, serverAddress.Text, 11000);
    }

    /// <summary>
    /// Method to be invoked by the networking library when a connection is made
    /// (see line 34)
    /// </summary>
    /// <param name="state"></param>
    private void OnConnect(SocketState state)
    {
        if (state.ErrorOccurred)
        {
            // TODO: Left as an exercise, allow the user to try to reconnect
            Dispatcher.Dispatch(() => DisplayAlert("Error", "Error connecting to server. Please restart the client.", "OK"));
            return;
        }

        theServer = state;

        // Start an event loop to receive messages from the server
        state.OnNetworkAction = ReceiveMessage;
        Networking.GetData(state);
    }

    /// <summary>
    /// Method to be invoked by the networking library when 
    /// a network action occurs (see lines 54-55)
    /// </summary>
    /// <param name="state"></param>
    private void ReceiveMessage(SocketState state)
    {
        if (state.ErrorOccurred)
        {
            // TODO: Left as an exercise, allow the user to try to reconnect
            Dispatcher.Dispatch( () => DisplayAlert( "Error", "Error while receiving. Please restart the client.", "OK" ) );
            return;
        }
        ProcessMessages(state);

        // Continue the event loop
        // state.OnNetworkAction has not been changed, 
        // so this same method (ReceiveMessage) 
        // will be invoked when more data arrives
        Networking.GetData(state);
    }

    /// <summary>
    /// Process any buffered messages separated by '\n'
    /// Display them, then remove them from the buffer.
    /// </summary>
    /// <param name="state"></param>
    private void ProcessMessages(SocketState state)
    {
        string totalData = state.GetData();
        string[] parts = Regex.Split(totalData, @"(?<=[\n])");

        // Loop until we have processed all messages.
        // We may have received more than one.

        foreach (string p in parts)
        {
            // Ignore empty strings added by the regex splitter
            if (p.Length == 0)
                continue;
            // The regex splitter will include the last string even if it doesn't end with a '\n',
            // So we need to ignore it if this happens. 
            if (p[p.Length - 1] != '\n')
                break;

            Console.WriteLine("Adding text:" + p);
            // Display the message
            // "messages" is the big message text box in the form.
            // We must use a Dispatcher, because only the thread 
            // that created the GUI can modify it.
            Dispatcher.Dispatch(
              () => messages.Text = p + messages.Text);

            // Then remove it from the SocketState's growable buffer
            state.RemoveData(0, p.Length);
        }
    }

    /// <summary>
    /// This is the event handler when the enter key is pressed in the messageToSend box
    /// </summary>
    /// <param name="sender">The Form control that fired the event</param>
    /// <param name="e">The key event arguments</param>
    private void OnMessageEnter(object sender, EventArgs e)
    {
        // Append a newline, since that is our protocols terminating character for a message.
        string message = messageToSendBox.Text + "\n";
        // Reset the textbox
        messageToSendBox.Text = "";
        // Send the message to the server
        Networking.Send(theServer.TheSocket, message);
    }
}


