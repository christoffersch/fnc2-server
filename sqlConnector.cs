using Godot;
using System;
using System.Data.SQLite;

public class sqlConnector : Node
{
    Global global;
    Node mainBehavior;
    Node loginNetworking;

    public override void _Ready()
    {
        global = GetNode("/root/Global") as Global;
        mainBehavior = GetNode("/root/main/Scripts/mainBehavior");
        loginNetworking = GetNode("/root/main/Scripts/loginNetworking");

        initializeRegisteredUsers();
        initializeAllAreas();
    }

    // -------------------------------------------------
    // ----------------- LOGIN PHASE -------------------
    // -------------------------------------------------

    // PUT ALL REGISTERED USERS INTO GLOBAL DICTIONARY "global.usersInformation"
    private void initializeRegisteredUsers()
    {
        mainBehavior.Call("clearRegisteredUsers");
        using (SQLiteConnection conn = new SQLiteConnection("Data Source=Databases/users.db"))
        {
            conn.Open();

            using (SQLiteCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM users";
                cmd.CommandType = System.Data.CommandType.Text;
                SQLiteDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    mainBehavior.Call("fillRegisteredUsers", rdr[0].ToString(), rdr[1].ToString());

                    // Add to global arrays
                    if (!global.usersInformation.Contains(rdr[0].ToString()))
                    {
                        global.usersInformation.Add(rdr[0], new Godot.Collections.Array() { rdr[1] });
                    }
                }
                rdr.Close();
            }
            conn.Close();
        }
    }

    // ADD NEWLY REGISTERED USER TO DATABASE
    private void addUserToDatabase(string username, string propertyName, int genderOption, string color, string aboutYou, string googleSub, int uniqueID)
    {
        using (SQLiteConnection conn = new SQLiteConnection("Data Source=Databases/users.db"))
        {
            conn.Open();

            using (SQLiteCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO users ('sub', 'username', 'degree', 'home', 'customization', 'about', 'registerDate') VALUES ('" + googleSub + "', '" + username + "', 'user', '" + propertyName + "', '" + genderOption + "," + color + "', '" + aboutYou + "', '" + System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName((int)OS.GetDate()["month"]).Substring(0, 3) + " " + OS.GetDate()["day"] + ", " + OS.GetDate()["year"] + "');";
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.ExecuteNonQuery();

                // Add to global dictionary of users
                global.usersInformation.Add(username, new Godot.Collections.Array() { googleSub });
            }
            conn.Close();
            initializeRegisteredUsers();
            loginNetworking.Call("userSuccessfullyRegistered", username, uniqueID);
        }
    }

    // STORE DATABASE USER INFORMATION IN MEMORY
    private void addUserDatabaseInformationToMemory(string loggedInAs)
    {
        using (SQLiteConnection conn = new SQLiteConnection("Data Source=Databases/users.db"))
        {
            conn.Open();

            using (SQLiteCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM users WHERE username='" + loggedInAs + "';";
                cmd.CommandType = System.Data.CommandType.Text;
                SQLiteDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    global.usersInformation[loggedInAs] = new Godot.Collections.Array() { rdr[1].ToString(), // google sub
                                                                                          rdr[2].ToString(), // user degree
                                                                                          rdr[3].ToString(), // home
                                                                                          rdr[4].ToString(), // customization
                                                                                          rdr[5].ToString(), // contacts
                                                                                          rdr[6].ToString(), // contact requests
                                                                                          rdr[7].ToString(), // about 
                                                                                          rdr[8].ToString(), // insignia
                                                                                          rdr[9].ToString(), // clubs
                                                                                          rdr[10].ToString() }; // register date
                rdr.Close();
            }
            conn.Close();
        }

        // Parse the string arrays into usable arrays
        parseStringArray(3, loggedInAs); // customization
        parseStringArray(4, loggedInAs); // contacts
        parseStringArray(5, loggedInAs); // contact requests
        parseStringArray(7, loggedInAs); // insignia
        parseStringArray(8, loggedInAs); // clubs
    }

    private void parseStringArray(int index, string username)
    {
        Godot.Collections.Array userInfoArray = global.usersInformation[username] as Godot.Collections.Array;
        Godot.Collections.Array tempArray = new Godot.Collections.Array();

        string stringArray = userInfoArray[index].ToString();
        foreach (string x in stringArray.Split(","))
            tempArray.Add(x);

        userInfoArray[index] = tempArray;
        tempArray.Dispose();
        userInfoArray.Dispose();
    }

    // -------------------------------------------------
    // ---------------- AREA RELATED -------------------
    // -------------------------------------------------
    
    // PUT ALL AREAS AND THEIR INFORMATION INTO GLOBAL DICTIONARY "areasInformation"
    private void initializeAllAreas()
    {
        using (SQLiteConnection conn = new SQLiteConnection("Data Source=Databases/areasInformation.db"))
        {
            conn.Open();

            using (SQLiteCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM information";
                cmd.CommandType = System.Data.CommandType.Text;
                SQLiteDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    Godot.Collections.Array privilegeHoldersArray = new Godot.Collections.Array();
                    foreach (string x in rdr[4].ToString().Split(","))
                        privilegeHoldersArray.Add(x);

                    global.allAreasInformation.Add(rdr[0].ToString(), new Godot.Collections.Array() { rdr[1].ToString()/*owner*/, rdr[2].ToString()/*size*/, rdr[3].ToString()/*description*/, privilegeHoldersArray });
                }
                rdr.Close();
            }
            conn.Close();
        }
    }

    // FETCH AREA OBJECTS FROM DATABASE
    private void fetchAreaObjectDataFromDatabase(string areaName, int uniqueID)
    {
        global.areasObjectData.Add(areaName, new System.Collections.Generic.List<Tuple<string, Godot.Collections.Array>>());
        //Godot.Collections.Dictionary objectDictionary = new Godot.Collections.Dictionary();
        var objectList = new System.Collections.Generic.List<Tuple<string, Godot.Collections.Array>>();

        using (SQLiteConnection conn = new SQLiteConnection("Data Source=Databases/areasObjectData.db"))
        {
            conn.Open();

            using (SQLiteCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * from '" + areaName + "'";
                cmd.CommandType = System.Data.CommandType.Text;
                SQLiteDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    objectList.Add(new Tuple<string, Godot.Collections.Array>(rdr[0].ToString(), new Godot.Collections.Array() { rdr[1].ToString(), rdr[2].ToString(), rdr[3].ToString(), rdr[4].ToString() }));
                    global.areasObjectData[areaName] = objectList;
                }
                rdr.Close();
            }
            conn.Close();
        }
    }

    // -------------------------------------------------
    // ---------------- USER RELATED -------------------
    // -------------------------------------------------

    // Make two users contacts
    private void makeContacts(string loggedInAs, string userAccepted)
    {
        try {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=Databases/users.db"))
            {
                conn.Open();

                using (SQLiteCommand cmd = conn.CreateCommand())
                {
                    // ---- PART 1 : ADD TO CONTACTS ---- //
                    // ---------------------------------- //
                    // First, get the contact information already present from loggedInAs
                    cmd.CommandText = "SELECT contacts FROM users WHERE username='" + loggedInAs + "';";
                    cmd.CommandType = System.Data.CommandType.Text;
                    SQLiteDataReader rdr_loggedInAs = cmd.ExecuteReader();

                    string contacts_loggedInAs = null;
                    while (rdr_loggedInAs.Read())
                        contacts_loggedInAs = rdr_loggedInAs[0].ToString();
                    rdr_loggedInAs.Close();

                    // Now, add userAccepted to loggedInAs' contacts
                    cmd.CommandText = "UPDATE users SET contacts='" + contacts_loggedInAs + "," + userAccepted + "' WHERE username='" + loggedInAs + "';";
                    cmd.ExecuteNonQuery();

                    // Secondly, get the contact information already present from userAccepted
                    cmd.CommandText = "SELECT contacts FROM users WHERE username='" + userAccepted + "';";
                    SQLiteDataReader rdr_userAccepted = cmd.ExecuteReader();

                    string contacts_userAccepted = null;
                    while (rdr_userAccepted.Read())
                        contacts_userAccepted = rdr_userAccepted[0].ToString();
                    rdr_userAccepted.Close();

                    // Now, add loggedInAs to userAccepted's contacts
                    cmd.CommandText = "UPDATE users SET contacts='" + contacts_userAccepted + "," + loggedInAs + "' WHERE username='" + userAccepted + "';";
                    cmd.ExecuteNonQuery();

                    // ---- PART 2 : REMOVE FROM CONTACT REQUESTS ---- //
                    // ----------------------------------------------- //
                    // First, get the contact request information already present from loggedInAs
                    cmd.CommandText = "SELECT contactRequests FROM users WHERE username='" + loggedInAs + "';";
                    SQLiteDataReader rdr_contactRequests = cmd.ExecuteReader();

                    string contactRequests = null;
                    while (rdr_contactRequests.Read())
                        contactRequests = rdr_contactRequests[0].ToString();
                    rdr_contactRequests.Close();

                    contactRequests = contactRequests.Replace("," + userAccepted, "");

                    // Now, remove userAccepted from loggedInAs' contact requests
                    cmd.CommandText = "UPDATE users SET contactRequests='" + contactRequests + "' WHERE username='" + loggedInAs + "';";
                    cmd.ExecuteNonQuery();

                    // Update user information in memory
                    addUserDatabaseInformationToMemory(loggedInAs);
                    if (global.onlineUsersByUsername.Contains(userAccepted))
                        addUserDatabaseInformationToMemory(userAccepted);
                }
                conn.Close();
            }
        }
        catch (SystemException e) {
            GD.Print("Oof, SQL error: " + e); }
    }

    // Remove contacts
    private void removeContacts(string loggedInAs, string contactToBeRemoved)
    {
        using (SQLiteConnection conn = new SQLiteConnection("Data Source=Databases/users.db"))
        {
            conn.Open();

            using (SQLiteCommand cmd = conn.CreateCommand())
            {
                // First, get the contact information already present from loggedInAs
                cmd.CommandText = "SELECT contacts FROM users WHERE username='" + loggedInAs + "';";
                cmd.CommandType = System.Data.CommandType.Text;
                SQLiteDataReader rdr_loggedInAs = cmd.ExecuteReader();

                string contacts_loggedInAs = null;
                while (rdr_loggedInAs.Read())
                    contacts_loggedInAs = rdr_loggedInAs[0].ToString();
                rdr_loggedInAs.Close();

                contacts_loggedInAs = contacts_loggedInAs.Replace("," + contactToBeRemoved, "");

                // Now, remove contactToBeRemoved from loggedInAs' contacts
                cmd.CommandText = "UPDATE users SET contacts='" + contacts_loggedInAs + "' WHERE username='" + loggedInAs + "';";
                cmd.ExecuteNonQuery();

                // Secondly, get the contact information already present from contactToBeRemoved
                cmd.CommandText = "SELECT contacts FROM users WHERE username='" + contactToBeRemoved + "';";
                SQLiteDataReader rdr_contactToBeRemoved = cmd.ExecuteReader();

                string contacts_contactToBeRemoved = null;
                while (rdr_contactToBeRemoved.Read())
                    contacts_contactToBeRemoved = rdr_contactToBeRemoved[0].ToString();
                rdr_contactToBeRemoved.Close();

                contacts_contactToBeRemoved = contacts_contactToBeRemoved.Replace("," + loggedInAs, "");

                // Now, remove loggedInAs from contactToBeRemoved's contacts
                cmd.CommandText = "UPDATE users SET contacts='" + contacts_contactToBeRemoved + "' WHERE username='" + contactToBeRemoved + "';";
                cmd.ExecuteNonQuery();

                // Update user information in memory
                addUserDatabaseInformationToMemory(loggedInAs);
                if (global.onlineUsersByUsername.Contains(contactToBeRemoved))
                    addUserDatabaseInformationToMemory(contactToBeRemoved);
            }
            conn.Close();
        }
    }

    // Add contact request
    private void addContactRequest(string loggedInAs, string contactToBeAdded)
    {
        using (SQLiteConnection conn = new SQLiteConnection("Data Source=Databases/users.db"))
        {
            conn.Open();

            using (SQLiteCommand cmd = conn.CreateCommand())
            {
                // First, get the contact information already present from loggedInAs
                cmd.CommandText = "SELECT contactRequests FROM users WHERE username='" + contactToBeAdded + "';";
                cmd.CommandType = System.Data.CommandType.Text;
                SQLiteDataReader rdr = cmd.ExecuteReader();

                string contacts = null;
                while (rdr.Read())
                    contacts = rdr[0].ToString();
                rdr.Close();

                // Now, add request to contactToBeAdded (if it's not already there)
                if (!contacts.Contains(loggedInAs))
                {
                    cmd.CommandText = "UPDATE users SET contactRequests='" + contacts + "," + loggedInAs + "' WHERE username='" + contactToBeAdded + "';";
                    cmd.ExecuteNonQuery();

                    // Update information in memory
                    if (global.onlineUsersByUsername.Contains(contactToBeAdded))
                        addUserDatabaseInformationToMemory(contactToBeAdded);
                }
            }
            conn.Close();
        }
    }

    // Remove contact request
    private void removeContactRequest(string loggedInAs, string userDeclined)
    {
        using (SQLiteConnection conn = new SQLiteConnection("Data Source=Databases/users.db"))
        {
            conn.Open();

            using (SQLiteCommand cmd = conn.CreateCommand())
            {
                // First, get the contact information already present from loggedInAs
                cmd.CommandText = "SELECT contactRequests FROM users WHERE username='" + loggedInAs + "';";
                cmd.CommandType = System.Data.CommandType.Text;
                SQLiteDataReader rdr = cmd.ExecuteReader();

                string contacts = null;
                while (rdr.Read())
                    contacts = rdr[0].ToString();
                rdr.Close();

                // Now, remove request from userDeclined
                cmd.CommandText = "UPDATE users SET contactRequests='" + contacts.Replace("," + userDeclined, "") + "' WHERE username='" + loggedInAs + "';";
                cmd.ExecuteNonQuery();
            }
            conn.Close();
        }

        // Update information in memory
        addUserDatabaseInformationToMemory(loggedInAs);
    }
}
