using System;
using NetworkUtil;

public class GameController
{   // TODO: JSON Compatability
    public string moving;

    // TODO: do we need a constructor?

    /// <summary>
    /// Connects to the argued server's host name on port 11000
    /// </summary>
    /// <param name="hostName"></param>
    public void Connect(string hostName)
    {
        Networking.ConnectToServer(World.UpdateWorld, hostName, 11000);
    }

    // THIS IS WHERE WE LEFT OFF 11/17/2022 3:31pm
}
