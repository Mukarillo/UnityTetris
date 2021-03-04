using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneManagment : MonoBehaviour {

[SerializeField] Slider music = null;
[SerializeField] Slider sfx = null;
[SerializeField] PhotonView pv = null;
[SerializeField] GameObject Endscreen = null;

private void Start() {
    if (pv != null && !pv.IsMine) return;
    if (music != null) music.value = PlayerPrefs.GetFloat("MusicLevels");
}

public void ExitGame() {
    if (pv != null) { if (!pv.IsMine) return; }
    #if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
    #elif UNITY_WEBPLAYER
    Application.OpenURL(webplayerQuitURL);
    #else   
    Application.Quit();
    #endif
}

public void LoadScene(string SceneName) { SceneManager.LoadScene(SceneName); }
public static void LoadSceneFunction(string SceneName) { SceneManager.LoadScene(SceneName); }

public static int GetIntValue(string name) { 
return PlayerPrefs.GetInt(name);}
public static float GetFloatValue(string name) { 
return PlayerPrefs.GetFloat(name);}
public static string GetStringValue(string name) { 
return PlayerPrefs.GetString(name);}

public static void SetValue(string name, int value) { PlayerPrefs.SetInt(name, value); }
public static void SetValue(string name, float value) { PlayerPrefs.SetFloat(name, value); }
public static void SetValue(string name, string value) { PlayerPrefs.SetString(name, value); }

public void SetMusic() { PlayerPrefs.SetFloat("MusicLevels", music.value); }
public void SetSFX() { PlayerPrefs.SetFloat("SFXLevels", sfx.value); }

public void LeaveRoom(bool quitGame = false) {
    if (pv != null) { if (!pv.IsMine) return; }
    PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);
    PhotonNetwork.LeaveRoom(); 
    if (quitGame) { ExitGame(); } else { PhotonNetwork.LoadLevel(0); }
}
public void CloseEndWindow() {     
    if (pv != null) { if (!pv.IsMine) { return; } }
    Endscreen.SetActive(false);
}

}
