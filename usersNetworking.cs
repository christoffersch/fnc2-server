using Godot;
using System;

public class usersNetworking : Node
{
    Global global;
    Node sqlConnector;

    public override void _Ready()
    {
        global = GetNode("/root/Global") as Global;
        sqlConnector = GetNode("/root/main/Scripts/sqlConnector");
    }

    // CLIENT HAS REQUESTED TO SHOW ITS CONTACTS
    [Remote] public void _requestContacts(string loggedInAs, int uniqueID)
    {
        // Since client is online, we have the user's contact information stored in memory (global.usersInformation)
        Godot.Collections.Array userInformation = global.usersInformation[loggedInAs] as Godot.Collections.Array;
        Godot.Collections.Array contactsArray = userInformation[4] as Godot.Collections.Array;

        // Now make a dictionary with the user's contact and their online status: KEY username VALUE onlineStatus as boolean
        Godot.Collections.Dictionary contactsDictionary = new Godot.Collections.Dictionary();

        foreach (string x in contactsArray)
        {
            if (global.onlineUsersByUsername.Contains(x))
                contactsDictionary.Add(x, true);
            else
                contactsDictionary.Add(x, false);
        }

        // Send client the array of contacts
        RpcId(uniqueID, "retrieveContacts", contactsDictionary);
    }

    // CLIENT HAS REQUESTED TO SHOW ITS CONTACT REQUESTS (same concept as above)
    [Remote] public void _requestContactRequests(string loggedInAs, int uniqueID)
    {
        Godot.Collections.Array userInformation = global.usersInformation[loggedInAs] as Godot.Collections.Array;
        Godot.Collections.Array contactRequestsArray = userInformation[5] as Godot.Collections.Array;

        RpcId(uniqueID, "retrieveContactRequests", contactRequestsArray);
    }

    // CLIENT HAS REQUESTED A USER SEARCH
    [Remote] public void _searchForUsers(string search, int uniqueID)
    {
        Godot.Collections.Dictionary searchResults = new Godot.Collections.Dictionary();
        foreach (string x in global.usersInformation.Keys)
        {
            if (x.Contains(search, StringComparison.OrdinalIgnoreCase))
            {
                bool onlineStatus = false;
                if (global.onlineUsersByUsername.Contains(x))
                    onlineStatus = true;
                searchResults.Add(x, onlineStatus);
            }
        }
        RpcId(uniqueID, "retrieveUsersSearchResult", searchResults);
    }

    // REQUEST, ACCEPT, DECLINE OR REMOVE CONTACTS -----------------
    [Remote] public void _acceptContactRequest(string loggedInAs, string userAccepted)
    {
        sqlConnector.Call("makeContacts", loggedInAs, userAccepted);

        // Notify whomever sent the request that it has been accepted (if they're online)
        if (global.onlineUsersByUsername.Contains(userAccepted))
            RpcId((int)global.onlineUsersByUsername[userAccepted], "contactRequestAccepted_notif", loggedInAs);
    }
    [Remote] public void _declineContactRequest(string loggedInAs, string userDeclined) => sqlConnector.Call("removeContactRequest", loggedInAs, userDeclined);
    [Remote] public void _removeContact(string loggedInAs, string contactToBeRemoved) => sqlConnector.Call("removeContacts", loggedInAs, contactToBeRemoved);
    [Remote] public void _addContactRequest(string loggedInAs, string contactToBeAdded)
    {
        sqlConnector.Call("addContactRequest", loggedInAs, contactToBeAdded);

        // Notify whoever is receiving the request (if they're online)
        if (global.onlineUsersByUsername.Contains(contactToBeAdded))
            RpcId((int)global.onlineUsersByUsername[contactToBeAdded], "contactRequestReceived_notif", loggedInAs);
    }
    // -----------------------------------------------------------

    // CLIENT HAS REQUESTED USERS OVERVIEW
    [Remote] public void _requestUsersOverview(int uniqueID) => RpcId(uniqueID, "retrieveUsersOverview", global.onlineUsersByUsername.Count, global.usersInformation.Count);
}
