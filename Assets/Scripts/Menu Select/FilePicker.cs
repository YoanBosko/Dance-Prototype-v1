using UnityEngine;
using TMPro;
using SFB;
using System.IO;
using UnityEngine.SceneManagement;

public class FilePicker : MonoBehaviour
{
    public TMP_Text songFileNameText;
    public TMP_Text tapMidiFileNameText;
    public TMP_Text holdMidiFileNameText;

    public void PickSongFile()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Pilih Lagu", "", new[] { new ExtensionFilter("Audio", "mp3", "wav", "ogg") }, false);
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            songFileNameText.text = Path.GetFileName(paths[0]);
            SongDataBridge.songPath = paths[0];
        }
    }

    public void PickTapMidiFile()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Pilih Tap MIDI", "", new[] { new ExtensionFilter("MIDI", "mid", "midi") }, false);
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            tapMidiFileNameText.text = Path.GetFileName(paths[0]);
            SongDataBridge.tapMidiPath = paths[0];
        }
    }

    public void PickHoldMidiFile()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Pilih Hold MIDI", "", new[] { new ExtensionFilter("MIDI", "mid", "midi") }, false);
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            holdMidiFileNameText.text = Path.GetFileName(paths[0]);
            SongDataBridge.holdMidiPath = paths[0];
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
