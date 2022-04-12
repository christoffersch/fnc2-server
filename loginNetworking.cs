using Godot;
using System;

public class loginNetworking : Node
{
    Global global;
    Node mainBehavior;
    Node sqlConnector;

    public override void _Ready()
    {
        global = GetNode("/root/Global") as Global;
        mainBehavior = GetNode("/root/main/Scripts/mainBehavior");
        sqlConnector = GetNode("/root/main/Scripts/sqlConnector");
    }

    // CHECK IF A USER REGISTERED OR NOT
    [Remote] public void _checkIfUserIsRegistered(string sub, string googleName, int uniqueID)
    {
        bool userFound = false;
        foreach (string x in global.usersInformation.Keys)
        {
            Godot.Collections.Array temp = global.usersInformation[x] as Godot.Collections.Array;
            if (sub == temp[0] as string)
            {
                RpcId(uniqueID, "checkIfUserIsRegistered_back", true, googleName, x);
                userFound = true;
                break;
            }
        }
        if (!userFound)
            RpcId(uniqueID, "checkIfUserIsRegistered_back", false, googleName, 0);
    }

    // CHECK IF USERNAME IS TAKEN (ON USER CREATION) 1/2
    [Remote] public void _checkIfUsernameIsTaken(string username, string propertyName, int genderOption, string color, string aboutYou, string googleSub, int uniqueID)
    {
        bool usernameFound = false;
        foreach (string x in /*global.registeredUsers_usernames*/ global.usersInformation.Keys)
        {
            if (x == username)
            {
                RpcId(uniqueID, "checkIfUsernameIsTaken_back", username, true);
                usernameFound = true;
                break;
            }
        }
        // username is not taken, create that user in database
        if (!usernameFound)
            sqlConnector.Call("addUserToDatabase", username, propertyName, genderOption, color, aboutYou, googleSub, uniqueID);
    }

    // tell client that user has been registered 2/2 (triggered from sqlConnector)
    public void userSuccessfullyRegistered(string username, int uniqueID) => RpcId(uniqueID, "checkIfUsernameIsTaken_back", username, false);

    // USER HAS SUCCESSFULLY LOGGED IN
    [Remote] public void _userIsNowLoggedIn(string loggedInAs, int uniqueID)
    {
        // Fill user information
        sqlConnector.Call("addUserDatabaseInformationToMemory", loggedInAs);

        // Change connection status in server GUI
        mainBehavior.Call("changeUserConnectionStatus", loggedInAs, uniqueID);
    }
}
