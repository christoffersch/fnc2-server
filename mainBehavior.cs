using Godot;
using System;

public class mainBehavior : Node
{
    RichTextLabel outputLog;
    Tree registeredUsers_tree;
    Tree onlineUsers_tree;
    Tree activeAreas_tree;
    Tree areaInspectorUsers_tree;
    Global global;

    public override void _Ready()
    {
        outputLog = GetParent().GetNode("../outputLog") as RichTextLabel;
        registeredUsers_tree = GetParent().GetNode("../registeredUsers_tree") as Tree;
        onlineUsers_tree = GetParent().GetNode("../onlineUsers_tree") as Tree;
        activeAreas_tree = GetParent().GetNode("../activeAreas_tree") as Tree;
        areaInspectorUsers_tree = GetParent().GetNode("../areaInspectorUsers_tree") as Tree;
        global = GetNode("/root/Global") as Global;

        // Initiate trees
        TreeItem onlineUsers_treeRoot = onlineUsers_tree.CreateItem();
        onlineUsers_treeRoot.SetText(0, "Online users");
        TreeItem onlineUsers_activeAreas = activeAreas_tree.CreateItem();
        onlineUsers_activeAreas.SetText(0, "Active areas");
    }

    // STARTING THE SERVER (forward to connectionHandler)
    private void _on_startServer_button_pressed() => GetNode("../connectionHandler").Call("_on_startServer_button_pressed_forward");

    // STOPPING THE SERVER (forward to connectionHandler)
    private void _on_stopServer_button_pressed() => GetNode("../connectionHandler").Call("_on_stopServer_button_pressed_forward");


    // TREE PLOPPIN' ------------------------------------------
    // CLEAR REGISTERED USERS TREE
    public void clearRegisteredUsers()
    {
        registeredUsers_tree.Clear();
        TreeItem registeredUsers_treeRoot = registeredUsers_tree.CreateItem();
        registeredUsers_treeRoot.SetText(0, "Registered users");
    }
        
    // FILL REGISTERED USERS IN TREE
    public void fillRegisteredUsers(string username, string sub)
    {
        TreeItem item = registeredUsers_tree.CreateItem();
        item.SetText(0, username);
        item.SetText(1, sub);
        registeredUsers_tree.Update();
    }

    // ADD USER TO ONLINE USERS TREE
    public void addToOnlineUsers(string id)
    {
        TreeItem item = onlineUsers_tree.CreateItem();
        item.SetText(0, id);
        item.SetText(1, "Connected");
    }

    // CHANGE USER CONNECTION STATUS TO "LOGGED IN"
    public void changeUserConnectionStatus(string loggedInAs, int uniqueID)
    {
        TreeItem item = onlineUsers_tree.GetRoot().GetChildren();
        while (item != null)
        {
            if (item.GetText(0) == uniqueID.ToString())
            {
                item.SetText(1, loggedInAs);
                break;
            }
            item = item.GetNext();
        }
        onlineUsers_tree.Update();
        global.onlineUsersByUsername.Add(loggedInAs, uniqueID);
    }

    // REMOVE USER FROM ONLINE USERS TREE and FROM global.activeAreasInformation
    public void removeFromOnlineUsers(string id)
    {
        TreeItem item = onlineUsers_tree.GetRoot().GetChildren();
        while (item != null)
        {
            if (item.GetText(0) == id)
            {
                onlineUsers_tree.GetRoot().RemoveChild(item);
                break;
            }
            item = item.GetNext();
        }
        onlineUsers_tree.Update();
    }

    // UPDATE ACTIVE AREAS
    public void updateActiveAreas()
    {
        activeAreas_tree.Clear();
        TreeItem root = activeAreas_tree.CreateItem();
        root.SetText(0, "Active areas");
        foreach (string x in global.activeAreasInformation.Keys)
        {
            TreeItem item = activeAreas_tree.CreateItem();
            item.SetText(0, x);
            activeAreas_tree.Update();
        }
    }

    // FILL USERS IN AREA INSPECTOR USERS TREE
    private void _on_activeAreas_tree_cell_selected()
    {
        areaInspectorUsers_tree.Clear();
        TreeItem root = areaInspectorUsers_tree.CreateItem();
        root.SetText(0, "Active users");

        if (activeAreas_tree.GetSelected() != null)
        {
            foreach (int x in global.activeAreasInformation[activeAreas_tree.GetSelected().GetText(0)] as Godot.Collections.Array)
            {
                TreeItem item = areaInspectorUsers_tree.CreateItem();
                item.SetText(0, x.ToString());
                areaInspectorUsers_tree.Update();
            }
        }
    }

    // -------------------------------------------------------
    // OUTPUT TO LOG
    private void output(string x) => outputLog.AddText("\n" + Godot.OS.GetTime() + " >> " + x);
}
