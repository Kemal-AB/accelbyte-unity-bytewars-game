using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerEntry : MonoBehaviour
{
    [SerializeField] private Image shipAvatar;

    [SerializeField] private TextMeshProUGUI shipName;
    [SerializeField] private Image playerAvatar;
    private string avatarUrl;

    public void Set(TeamState teamState, PlayerState playerState, bool isCurrentPlayer)
    {
        shipAvatar.color = teamState.teamColour;
        shipName.color = teamState.teamColour;
        if (isCurrentPlayer)
        {
            shipName.text = $"You: {playerState.playerName}";
        }
        else
        {
            shipName.text = playerState.playerName;
        }

        avatarUrl = playerState.avatarUrl;
    }

    private readonly Vector2 centerPivot = new Vector2(0.5f, 0.5f);
    private IEnumerator GetAvatarTexture(string avatarURL)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(avatarURL))
        {
            
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                // Get downloaded asset bundle
                Debug.Log($"download avatar from {avatarURL} successfully");
                DownloadHandlerTexture dht = (DownloadHandlerTexture)uwr.downloadHandler;
                
                var texture = DownloadHandlerTexture.GetContent(uwr);
                
                playerAvatar.sprite = Sprite.Create(texture, new Rect(0,0,texture.width,texture.height), centerPivot);
                playerAvatar.SetNativeSize();
            }
        }
    }
    
    private IEnumerator GetAvatar(string avatarURL)
    {
        using (UnityWebRequest uwr = UnityWebRequest.Get(avatarURL))
        {
            
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                Debug.Log($"download avatar from {avatarURL} successfully");
                DownloadHandlerTexture d = (DownloadHandlerTexture)uwr.downloadHandler;
                // Get downloaded asset bundle
                var texture = d.texture;
                playerAvatar.sprite = Sprite.Create(texture, new Rect(0,0,texture.width,texture.height), centerPivot);
                playerAvatar.SetNativeSize();
            }
        }
    }

    private void OnEnable()
    {
        if (!String.IsNullOrEmpty(avatarUrl))
        {
            var sizeDelta = playerAvatar.rectTransform.sizeDelta;
            var imageWidth = (int)sizeDelta.x;
            var imageHeight = (int)sizeDelta.y;
            CacheHelper.LoadTexture(avatarUrl, imageWidth, imageHeight, texture =>
            {
                playerAvatar.sprite = 
                    Sprite.Create(texture, new Rect(0,0,texture.width,texture.height), centerPivot);
            });
        }
    }
}
