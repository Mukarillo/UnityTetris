using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Net.NetworkInformation;
using System.Threading;
using TMPro;

public class GameLobby : MonoBehaviourPunCallbacks {
[SerializeField] string playerName = "Player";
[SerializeField] string appVersion = "0.0";
[SerializeField] string roomName = "Public Room";
[SerializeField] string sceneName = "SceneName"; 
[SerializeField] [Range(1,byte.MaxValue)]byte maxPlayers = 2; 

[SerializeField]TextMeshProUGUI nameInput = null;
[SerializeField]TextMeshProUGUI roomInput = null;
[SerializeField]TextMeshProUGUI statusText = null;
//[SerializeField]Text playerTotal = null;
//[SerializeField]Slider playerSlider = null;
[SerializeField]TMP_Dropdown mapSelector = null;
[SerializeField]TextMeshProUGUI mapSelecter = null;
[SerializeField]Toggle serverVisability = null;

List<RoomInfo> createdRooms = new List<RoomInfo>(); 
Vector2 roomListScroll = Vector2.zero; 
bool joiningRoom = false;
public bool GUIActive { get; set; } = true;
public string PlayerName { get => playerName; set => playerName =  value ; }

void Start() {
    Cursor.lockState = CursorLockMode.None;
    PhotonNetwork.AutomaticallySyncScene = true; 
    if (!PhotonNetwork.IsConnected) {
    PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = appVersion; 
    PhotonNetwork.ConnectUsingSettings(); 
    // settings in PhotonServerSettings in Unity        
    }
}    
public override void OnDisconnected(DisconnectCause cause) {
    Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + cause.ToString() + " ServerAddress: " + PhotonNetwork.ServerAddress);    
}

// connected to master PUN server    
public override void OnConnectedToMaster() {
    Debug.Log("OnConnectedToMaster");        
    PhotonNetwork.JoinLobby(TypedLobby.Default);    
}    
public override void OnRoomListUpdate(List<RoomInfo> roomList) {
    Debug.Log("We have received the Room list");        
    createdRooms = roomList;    
}

private void Update() {
    if (statusText != null) statusText.text = "Status: " + PhotonNetwork.NetworkClientState;
    if (mapSelector != null) {
    byte count = 1;
    switch(mapSelector.value) {
    case 0: count = 1; break;
    }
    maxPlayers = count;
    }
    if (mapSelecter != null) sceneName = mapSelecter.text;
    //print(sceneName);
}

void OnGUI() { if (GUIActive) GUI.Window(0, new Rect(Screen.width / 2 + 100, Screen.height / 2 - 170, 255, 170), LobbyWindow, "Games");    }    

void LobbyWindow(int index) { 
    roomListScroll = GUILayout.BeginScrollView(roomListScroll, false, true);
    if (createdRooms.Count == 0) { GUILayout.Label("No Rooms were created yet..."); }
    else { for (int i = 0; i < createdRooms.Count; i++) {
    GUILayout.BeginHorizontal("box");
    GUILayout.Label(createdRooms[i].Name, GUILayout.Width(70));
    GUILayout.Label(createdRooms[i].PlayerCount + "/" + createdRooms[i].MaxPlayers);
    GUILayout.FlexibleSpace();                
    if (GUILayout.Button("Join Room")) {
    joiningRoom = true;                    
    // set player name                    
    PhotonNetwork.NickName = (PlayerName.Trim() == "") ? SystemInfo.deviceName : PlayerName;     
    
    // join the Room                    
    PhotonNetwork.JoinRoom(createdRooms[i].Name);
    }
    GUILayout.EndHorizontal();
    }
    }
    GUILayout.EndScrollView();        
    if (GUILayout.Button("Refresh", GUILayout.Width(100))) { if (PhotonNetwork.IsConnected) {   
    //// re-join Lobby to get the latest Room list                
    PhotonNetwork.JoinLobby(TypedLobby.Default);
    }else{
    //// not connected, estabilish a new connection                
    PhotonNetwork.ConnectUsingSettings();            
    }} 
}    

public void Click_CreateRoom() {
    if (roomInput != null) roomName = roomInput.text; 
    if (roomName != "") {
    joiningRoom = true;
    RoomOptions roomOptions = new RoomOptions(); 
    roomOptions.IsOpen = true; 
    roomOptions.IsVisible = (serverVisability != null) ? serverVisability.isOn : true; 
    roomOptions.MaxPlayers = maxPlayers; // set any number
    PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);            
    }
}

public override void OnCreateRoomFailed(short returnCode, string message) { 
    Debug.Log("OnCreateRoomFailed got called. This can happen if the room exists (even if not visible). Try another room name."); 
    joiningRoom = false; 
}

public override void OnJoinRoomFailed(short returnCode, string message) { 
    Debug.Log("OnJoinRoomFailed got called. This can happen if the room is not existing or full or closed."); 
    joiningRoom = false; 
}

public override void OnJoinRandomFailed(short returnCode, string message) { 
    Debug.Log("OnJoinRandomFailed got called. This can happen if the room is not existing or full or closed."); 
    joiningRoom = false; 
}

public override void OnCreatedRoom() { 
    Debug.Log("OnCreatedRoom");        
    // set player name        
    PhotonNetwork.NickName = (nameInput != null && nameInput.text.Trim() != "") ? nameInput.text : PlayerName;        
    // load the scene (make sure the scene is added to build settings)  
    //Debug.Log($"{roomName} : {debugWorld}");
    PhotonNetwork.LoadLevel(sceneName);
}    

public override void OnJoinedRoom() {        
    Debug.Log("OnJoinedRoom");    
}

}