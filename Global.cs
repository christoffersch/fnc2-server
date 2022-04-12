using Godot;
using System;

public class Global : Node
{
    // USER RELATED
    public Godot.Collections.Dictionary usersInformation = new Godot.Collections.Dictionary(); // key: username | values: array of [google sub, degree, home, [customization], [contacts], [contactRequests], about, [insignia], [clubs]]
    public Godot.Collections.Dictionary onlineUsersByUsername = new Godot.Collections.Dictionary(); // key: username | value: RPC ID

    // AREA RELATED
    public Godot.Collections.Dictionary activeAreasInformation = new Godot.Collections.Dictionary(); // key: area name | values: array of RPCs in area
    public Godot.Collections.Dictionary allAreasInformation = new Godot.Collections.Dictionary(); // key: area name | array of [owner, size, description, privilegeholders]
    public System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<Tuple<string, Godot.Collections.Array>>> areasObjectData = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<Tuple<string, Godot.Collections.Array>>>();  // key: area name | List of Tuple<"objectName", array[posX, posY, posZ, rotY]>
}