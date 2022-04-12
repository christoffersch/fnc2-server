using Godot;
using System;

public class connectionHandler : Node
{
    Node mainBehavior;
    Global global;

    public override void _Ready()
    {
        global = GetNode("/root/Global") as Global;
        mainBehavior = GetNode("/root/main/Scripts/mainBehavior");
        GetTree().Connect("network_peer_disconnected", this, "_player_disconnected");
        GetTree().Connect("network_peer_connected", this, "_player_connected");
    }

    // STARTING THE SERVER ----------------------------
    private void _on_startServer_button_pressed_forward()
    {
        NetworkedMultiplayerENet server = new NetworkedMultiplayerENet();
        server.CreateServer(5000, 100);
        GetTree().NetworkPeer = server;
        SetNetworkMaster(GetTree().GetNetworkUniqueId());
        mainBehavior.Call("output", "Server started.");
    }

    // STOPPING THE SERVER ----------------------------
    private void _on_stopServer_button_pressed_forward()
    {
        GetTree().NetworkPeer = null;
        mainBehavior.Call("output", "Server stopped.");
    }

    // PLAYER HAS CONNECTED ---------------------------
    public void _player_connected(int id)
    {
        mainBehavior.Call("output", "Player with ID " + id + " has connected.");
        mainBehavior.Call("addToOnlineUsers", id.ToString());
    }

    // PLAYER HAS DISCONNECTED ------------------------
    public void _player_disconnected(int id)
    {
        foreach (string x in global.onlineUsersByUsername.Keys)
        {
            int uniqueID = (int)global.onlineUsersByUsername[x];
            if (uniqueID == id)
            {
                foreach (string y in global.activeAreasInformation.Keys)
                {
                    Godot.Collections.Array tempUserArray = global.activeAreasInformation[y] as Godot.Collections.Array;
                    if (tempUserArray.Contains(uniqueID))
                    {
                        tempUserArray.Remove(uniqueID);
                        global.activeAreasInformation[y] = tempUserArray;

                        if (tempUserArray.Count == 0)
                        {
                            global.activeAreasInformation.Remove(y);
                            global.areasObjectData.Remove(y);
                        }
                        // Inform everyone in area that user has quit game (left the area)
                        if  (tempUserArray.Count >= 1)
                            GetNode("/root/main/Scripts/areaUpdateNetworking").Call("newUserLeftArea", y, x + "$playerobject");
                        break;
                    }
                }
                global.onlineUsersByUsername.Remove(x);
                clearRedundantUserInformationFromMemory(x);
                mainBehavior.Call("output", x + " with peer ID " + uniqueID + " quit the game.");
                mainBehavior.Call("removeFromOnlineUsers", uniqueID.ToString());
                break;
            }
        }

        mainBehavior.Call("updateActiveAreas");
        mainBehavior.Call("_on_activeAreas_tree_cell_selected");
    }

    // Remove irrelevant users information (leaving only username and sub)
    private void clearRedundantUserInformationFromMemory(string username)
    {
        Godot.Collections.Array temp = global.usersInformation[username] as Godot.Collections.Array;
        temp = new Godot.Collections.Array() { temp[0] };
        global.usersInformation[username] = temp;
    }
}
