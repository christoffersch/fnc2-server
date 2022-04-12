using Godot;
using System;

public class areaUpdateNetworking : Node
{
    Global global;
    public override void _Ready() => global = GetNode("/root/Global") as Global;

    // Broadcast chat message to everyone in area
    [Remote] public void _sendChatMessage(string inAreaCurrently, string loggedInAs, string message) {
        foreach (int uniqueID in global.activeAreasInformation[inAreaCurrently] as Godot.Collections.Array)
            RpcId(uniqueID, "receiveChatMessage", loggedInAs, message);
    }

    // A user has joined an area - inform everyone in area to instance newly joined player // triggered from areaNetworking.cs
    public void newUserJoinedArea(string areaName, string username, Vector3 spawnCoordinates)
    {
        int usernameID = (int)global.onlineUsersByUsername[username];
        using (Godot.Collections.Array rpcIDs = global.activeAreasInformation[areaName] as Godot.Collections.Array)
        {
            foreach (int uniqueID in rpcIDs)
            {
                if (uniqueID != usernameID)
                    using (Godot.Collections.Array customization = global.usersInformation[username] as Godot.Collections.Array)
                        RpcId(uniqueID, "newUserJoinedArea", new Godot.Collections.Dictionary() { { username + "$playerobject", new Godot.Collections.Array() { new Vector3(spawnCoordinates.x, spawnCoordinates.y, spawnCoordinates.z), 0, customization[3] } } });
            }
        }
    }

    // A user has left an area - inform everyone in area to remove player // triggered fom areaNetworking.cs
    public void newUserLeftArea(string inAreaCurrently, string playerObjectString)
    {
        using (Godot.Collections.Array rpcIDs = global.activeAreasInformation[inAreaCurrently] as Godot.Collections.Array) {
            foreach (int uniqueID in rpcIDs)
                RpcId(uniqueID, "newUserLeftArea", playerObjectString);
        }
    }

    // A user has moved position - broadcast new position to everyone in area
    [Remote] public void _receivePlayerTranslation(Vector3 currentPosition, Vector3 newPosition, string inAreaCurrently, string loggedInAs)
    {
        // Tell all users in area about new position
        using (Godot.Collections.Array rpcIDs = global.activeAreasInformation[inAreaCurrently] as Godot.Collections.Array) {
            foreach (int uniqueID in rpcIDs)
                RpcId(uniqueID, "receivePlayerTranslation", currentPosition, newPosition, loggedInAs);
        }
        
        // Update position in memory
        System.Collections.Generic.List<Tuple<string, Godot.Collections.Array>> objectData = global.areasObjectData[inAreaCurrently];
        Godot.Collections.Array customizationArray = objectData.Find(y => y.Item1 == loggedInAs + "$playerobject").Item2[2] as Godot.Collections.Array;
        objectData.RemoveAll(y => y.Item1 == loggedInAs + "$playerobject");
        objectData.Add(new Tuple<string, Godot.Collections.Array>(loggedInAs + "$playerobject", new Godot.Collections.Array() { new Vector3(newPosition.x, 
                                                                                                                                newPosition.y, 
                                                                                                                                newPosition.z),
                                                                                                                                0, 
                                                                                                                                customizationArray }));
    }
}   
