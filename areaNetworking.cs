using Godot;
using System;

public class areaNetworking : Node
{
    Global global;
    Node sqlConnector;
    Node mainBehavior;
    Node areaUpdateNetworking;

    public override void _Ready()
    {
        global = GetNode("/root/Global") as Global;
        sqlConnector = GetNode("/root/main/Scripts/sqlConnector");
        mainBehavior = GetNode("/root/main/Scripts/mainBehavior");
        areaUpdateNetworking = GetNode("/root/main/Scripts/areaUpdateNetworking");
    }

    // MAKE USER JOIN AREA
    [Remote] public void _joinArea(string areaToJoin, string inAreaCurrently, string loggedInAs, int uniqueID)
    {
        // If user is already in an area, remove them from being active in that area (1), and from that area's object data (2)
        if (inAreaCurrently != "Nowhere")
        {
            /*(1) ----*/
            using (Godot.Collections.Array tempUserArray_removal = global.activeAreasInformation[inAreaCurrently] as Godot.Collections.Array)
            {
                tempUserArray_removal.Remove(uniqueID);
                global.activeAreasInformation[inAreaCurrently] = tempUserArray_removal;

                /*(2) ----*/
                var objectData = global.areasObjectData[inAreaCurrently] as System.Collections.Generic.List<Tuple<string, Godot.Collections.Array>>;
                objectData.RemoveAll(x => x.Item1 == loggedInAs + "$playerobject");
                global.areasObjectData[inAreaCurrently] = objectData;
                /*(2) Also over the network to users currently in area ----*/
                areaUpdateNetworking.Call("newUserLeftArea", inAreaCurrently, loggedInAs + "$playerobject");

                // If the area that was just left is now empty, remove it from memory
                if (tempUserArray_removal.Count == 0) {
                    global.activeAreasInformation.Remove(inAreaCurrently);
                    global.areasObjectData.Remove(inAreaCurrently);
                }
            }
        }
        mainBehavior.Call("updateActiveAreas");

        // Add user as being active in newly joined area, check if it is already stored in memory / has to be stored anew
        if (global.activeAreasInformation.Contains(areaToJoin))
        {
            using (Godot.Collections.Array tempUserArray_addition = global.activeAreasInformation[areaToJoin] as Godot.Collections.Array)
            {
                tempUserArray_addition.Add(uniqueID);
                global.activeAreasInformation[areaToJoin] = tempUserArray_addition;

                addUserToObjectData(areaToJoin, loggedInAs);
                transferObjects(areaToJoin, uniqueID);
                tempUserArray_addition.Dispose();
            }
            //addUserToArea(false, areaToJoin, loggedInAs, uniqueID);
        }
        else
        {
            sqlConnector.Call("fetchAreaObjectDataFromDatabase", areaToJoin, uniqueID); /* start storing object data in memory */
            global.activeAreasInformation.Add(areaToJoin, new Godot.Collections.Array() { uniqueID });
            mainBehavior.Call("updateActiveAreas");

            addUserToObjectData(areaToJoin, loggedInAs);
            transferObjects(areaToJoin, uniqueID);
            //addUserToArea(true, areaToJoin, loggedInAs, uniqueID);
        }

        mainBehavior.Call("_on_activeAreas_tree_cell_selected");
    }

    private void transferObjects(string areaJoining, int uniqueID)
    {
        var objectList = global.areasObjectData[areaJoining] as System.Collections.Generic.List<Tuple<string, Godot.Collections.Array>>;
        foreach (var item in objectList)
        {
            RpcId(uniqueID, "receiveObject", new Godot.Collections.Dictionary() { { item.Item1, item.Item2 } });
        }
    }

    private void addUserToObjectData(string areaName, string username)
    {
        var objectData = global.areasObjectData[areaName] as System.Collections.Generic.List<Tuple<string, Godot.Collections.Array>>;
        Godot.Collections.Array spawn = objectData[0].Item2; /*spawn HAS to be index 0*/
        Vector3 spawnCoordinates = new Vector3(float.Parse(spawn[0] as string), float.Parse(spawn[1] as string), float.Parse(spawn[2] as string));
        using (Godot.Collections.Array customization = global.usersInformation[username] as Godot.Collections.Array) {
            objectData.Add(new Tuple<string, Godot.Collections.Array>(username + "$playerobject", new Godot.Collections.Array() { new Vector3(spawnCoordinates.x, spawnCoordinates.y, spawnCoordinates.z), 0, customization[3] })); }
        global.areasObjectData[areaName] = objectData;

        // Notify existing users in room that a new has joined
        areaUpdateNetworking.Call("newUserJoinedArea", areaName, username, spawnCoordinates);
    }

    // ---------- AREAS WINDOW -------------
    // USER HAS REQUESTED A SEARCH - SEARCH FOR AREA
    [Remote] public void _searchForAreas(string search, int uniqueID)
    {
        if (global.allAreasInformation.Contains(search) || global.allAreasInformation.Contains(search.ToLower()))
            RpcId(uniqueID, "findArea_back", new Godot.Collections.Array() { search });
        else
        {
            Godot.Collections.Array searchResults = new Godot.Collections.Array();
            string[] searchWords = search.Split(" ");
            foreach (string x in global.allAreasInformation.Keys)
            {
                foreach (string y in searchWords)
                {
                    if (x.Contains(y, StringComparison.OrdinalIgnoreCase))
                    {
                        searchResults.Add(x);
                        break;
                    }
                }
            }
            RpcId(uniqueID, "findArea_back", searchResults);
        }
    }

    // FIND USER'S OWN AREAS
    [Remote] public void _fetchMyAreas(string username, int uniqueID)
    {
        Godot.Collections.Dictionary areasFound = new Godot.Collections.Dictionary();
        foreach (string x in global.allAreasInformation.Keys)
        {
            Godot.Collections.Array informationArray = global.allAreasInformation[x] as Godot.Collections.Array;
            if (informationArray[0] as string == username)
            {
                if (global.activeAreasInformation.Contains(x))
                    areasFound.Add(x, global.activeAreasInformation[x]);
                else
                    areasFound.Add(x, new Godot.Collections.Array());
            }
        }
        RpcId(uniqueID, "requestMyAreas_back", areasFound);
    }

    // PROVIDE ACTIVE AREAS
    [Remote] public void _fetchActiveAreas(int uniqueID) => RpcId(uniqueID, "requestActiveAreas_back", global.activeAreasInformation);

    // PROVIDE AREA INFORMATION
    [Remote] public void _requestAreaInformation(string inAreaCurrently, int uniqueID)
    {
        RpcId(uniqueID, "receiveAreaInformation", global.allAreasInformation[inAreaCurrently] as Godot.Collections.Array);
    }
}

// IGNORE CASE EXTENSION
public static class StringExtensions
{
    public static bool Contains(this string source, string toCheck, StringComparison comp) {
        return source?.IndexOf(toCheck, comp) >= 0; }
}
