using Godot;
using System;

public class passportNetworking : Node
{
    Global global;
    Node sqlConnector;

    public override void _Ready()
    {
        global = GetNode("/root/Global") as Global;
        sqlConnector = GetNode("/root/main/Scripts/sqlConnector");
    }

    [Remote] public void _requestUserOverview(string username, string loggedInAs, int uniqueID)
    {
        // Check if user has this person as a contact already / if a contact request is pending from user being checked
        int contactStatus = 0;
        Godot.Collections.Array usrInfArray_loggedInAs = global.usersInformation[loggedInAs] as Godot.Collections.Array;
        Godot.Collections.Array contactsArray_loggedInAs = usrInfArray_loggedInAs[4] as Godot.Collections.Array;

        if (contactsArray_loggedInAs.Contains(username))
            contactStatus = 1;

        contactsArray_loggedInAs = usrInfArray_loggedInAs[5] as Godot.Collections.Array;

        if (contactsArray_loggedInAs.Contains(username))
            contactStatus = 2;

        // If user is online, we already have access to this data
        if (global.onlineUsersByUsername.Contains(username))
        {
            Godot.Collections.Array userInfArray_username = global.usersInformation[username] as Godot.Collections.Array;
            Godot.Collections.Array contactsArray_username = userInfArray_username[4] as Godot.Collections.Array;
            string userOverview = "REGISTERED: " + userInfArray_username[9] + "\nCONTACTS: " + contactsArray_username.Count + "\nRESIDE IN: UNKNOWN";

            // Check if user has already sent a contact request
            contactsArray_username = userInfArray_username[5] as Godot.Collections.Array; // in this case, this array is the USER CONTACT REQUESTS ARRAY
            if (contactsArray_username.Contains(loggedInAs))
                contactStatus = 3;

            RpcId(uniqueID, "receiveUserOverview", contactStatus, true /*online*/, userOverview, userInfArray_username[6] /*about*/, userInfArray_username[3] /*customization*/, userInfArray_username[7] /*insignia*/);

            userInfArray_username.Dispose();
            contactsArray_username.Dispose();
        }
        // If not, we have to request it from SQL (when done, remove data from memory again)
        else
        {
            sqlConnector.Call("addUserDatabaseInformationToMemory", username);
            Godot.Collections.Array userInfArray_username = global.usersInformation[username] as Godot.Collections.Array;
            Godot.Collections.Array contactsArray_username = userInfArray_username[4] as Godot.Collections.Array;
            string userOverview = "REGISTERED: " + userInfArray_username[9] + "\nCONTACTS: " + contactsArray_username.Count + "\nRESIDE IN: UNKNOWN";

            // Check if user has already sent a contact request
            contactsArray_username = userInfArray_username[5] as Godot.Collections.Array; // in this case, this array is the USER CONTACT REQUESTS ARRAY
            if (contactsArray_username.Contains(loggedInAs))
                contactStatus = 3;

            RpcId(uniqueID, "receiveUserOverview", contactStatus, false /*offline*/, userOverview, userInfArray_username[6] /*about*/, userInfArray_username[3] /*customization*/, userInfArray_username[7] /*insignia*/);
            GetNode("/root/main/Scripts/connectionHandler").Call("clearRedundantUserInformationFromMemory", username);

            userInfArray_username.Dispose();
            contactsArray_username.Dispose();
        }
    }
}

