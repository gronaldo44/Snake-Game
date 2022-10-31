// Author: Daniel Kopta, May 2019
// Demo chat server using NetworkController library
// University of Utah

using System;
using System.Collections.Generic;
using NetworkUtil;
using System.Text.RegularExpressions;

namespace ChatServer;
/// <summary>
/// A simple server for receiving text messages from multiple clients
/// and broadcasting the messages out
/// </summary>
class ChatServer
{
    // A map of clients that are connected, each with an ID
    private Dictionary<long, SocketState> clients;

    static void Main(string[] args)
    {
        ChatServer server = new ChatServer();
        server.StartServer();

        // Sleep to prevent the program from closing,
        // since all the real work is done in separate threads.
        // StartServer is non-blocking.
        Console.Read();
    }

    /// <summary>
    /// Initialized the server's state
    /// </summary>
    public ChatServer()
    {
        clients = new Dictionary<long, SocketState>();
    }

    /// <summary>
    /// Start accepting Tcp sockets connections from clients
    /// </summary>
    public void StartServer()
    {
        // This begins an "event loop"
        Networking.StartServer(NewClientConnected, 11000);

        Console.WriteLine("Server is running");
    }

    /// <summary>
    /// Method to be invoked by the networking library
    /// when a new client connects (see line 41)
    /// </summary>
    /// <param name="state">The SocketState representing the new client</param>
    private void NewClientConnected(SocketState state)
    {
        if (state.ErrorOccurred)
            return;

        // Save the client state
        // Need to lock here because clients can disconnect at any time
        lock (clients)
        {
            clients[state.ID] = state;
        }

        // change the state's network action to the 
        // receive handler so we can process data when something
        // happens on the network
        state.OnNetworkAction = ReceiveMessage;

        Networking.GetData(state);
    }

    /// <summary>
    /// Method to be invoked by the networking library
    /// when a network action occurs (see lines 64-66)
    /// </summary>
    /// <param name="state"></param>
    private void ReceiveMessage(SocketState state)
    {
        // Remove the client if they aren't still connected
        if (state.ErrorOccurred)
        {
            RemoveClient(state.ID);
            return;
        }

        ProcessMessage(state);
        // Continue the event loop that receives messages from this client
        Networking.GetData(state);
    }


    /// <summary>
    /// Given the data that has arrived so far, 
    /// potentially from multiple receive operations, 
    /// determine if we have enough to make a complete message,
    /// and process it (print it and broadcast it to other clients).
    /// </summary>
    /// <param name="sender">The SocketState that represents the client</param>
    private void ProcessMessage(SocketState state)
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

            Console.WriteLine("received message from client " + state.ID + ": \"" + p.Substring(0, p.Length - 1) + "\"");

            // Remove it from the SocketState's growable buffer
            state.RemoveData(0, p.Length);

            // Broadcast the message to all clients
            // Lock here beccause we can't have new connections 
            // adding while looping through the clients list.
            // We also need to remove any disconnected clients.
            HashSet<long> disconnectedClients = new HashSet<long>();
            lock (clients)
            {
                foreach (SocketState client in clients.Values)
                {
                    if (!Networking.Send(client.TheSocket!, "Message from client " + state.ID + ": " + p))
                        disconnectedClients.Add(client.ID);
                }
            }
            foreach (long id in disconnectedClients)
                RemoveClient(id);
        }
    }

    /// <summary>
    /// Removes a client from the clients dictionary
    /// </summary>
    /// <param name="id">The ID of the client</param>
    private void RemoveClient(long id)
    {
        Console.WriteLine("Client " + id + " disconnected");
        lock (clients)
        {
            clients.Remove(id);
        }
    }
}
