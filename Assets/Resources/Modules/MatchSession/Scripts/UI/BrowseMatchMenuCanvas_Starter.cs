using System;
using System.Collections.Generic;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BrowseMatchMenuCanvas_Starter : MenuCanvas
{
    [SerializeField] private MatchSessionItem matchSessionItemPrefab;
    [SerializeField] private RectTransform matchItemContainer;
    [SerializeField] private Button refreshBtn;
    [SerializeField] private Button backButton;
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private RectTransform mainPanel;
    [SerializeField] private GameObject noMatchFoundInfo;
    [SerializeField] private LoadingPanel loadingPanel;
    [SerializeField] private ErrorPanel errorPanel;
    private readonly List<BrowseMatchItemModel> _loadedModels = new List<BrowseMatchItemModel>();
    private readonly List<MatchSessionItem> _instantiatedView = new List<MatchSessionItem>();
    private readonly List<SessionV2GameSession> _gameSessions = new List<SessionV2GameSession>();
    private const float ViewItemHeight = 75;
    private void Start()
    {
        backButton.onClick.AddListener(MenuManager.Instance.OnBackPressed);
        refreshBtn.onClick.AddListener(BrowseMatchSession);
        _scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        BrowseMatchSessionEventListener.Init(_gameSessions);
        BrowseMatchSessionEventListener.OnUpdate = OnGameSessionUpdated;
        GameManager.OnDisconnectedInMainMenu += OnDisconnectedFromMainMenu;
        BrowseMatchSession();
    }

    #region BrowseMatchSession
    private void BrowseMatchSession()
    {
        //TODO call browse match session wrapper to get available match session
        Debug.Log($"Browse Match Session not yet implemented");
    }
    private void OnBrowseMatchSessionFinished(BrowseMatchResult result)
    {
        //TODO cache and render match session result
    }
    private void CancelBrowseMatchSession()
    {
        HideLoadingBackToMainPanel();
        BrowseMatchSessionWrapper.CancelBrowseMatchSessions();
    }
    #endregion BrowseMatchSession

    #region RetrieveNextPage
    private void OnScrollValueChanged(Vector2 scrollPos)
    {
        //TODO if scroll rect reaches bottom of the list fetch next page
    }
    private void OnNextPageMatchSessionsRetrieved(BrowseMatchResult nextPageResult)
    {
        //TODO handle and render fetch next page result
    }
    #endregion RetrieveNextPage
    
    #region JoinMatchSession
    private void JoinMatch(JoinMatchSessionRequest request)
    {
        ShowLoading("Joining Match Session...", CancelJoinMatchSession);
        BrowseMatchSessionWrapper
            .JoinMatchSession(request.MatchSessionId, request.GameMode, OnJoinedMatchSession);
    }
    private void CancelJoinMatchSession()
    {
        HideLoadingBackToMainPanel();
        BrowseMatchSessionWrapper.CancelJoinMatchSession();
    }
    private void OnJoinedMatchSession(string errorMessage)
    {
        //success joined match session will be handled by BrowseMatchSessionWrapper
        if (!String.IsNullOrEmpty(errorMessage))
        {
            ShowError($"Join Match Session Failed: {errorMessage}");
        }
    }
    #endregion JoinMatchSession

    #region EventCallback
    private void OnDisconnectedFromMainMenu(string disconnectReason)
    {
        ShowError($"disconnected from server, reason:{disconnectReason}");
    }

    private void OnGameSessionUpdated(SessionV2GameSession result)
    {
        var updatedModel = _loadedModels.Find(m => m.MatchSessionId == result.id);
        updatedModel?.Update(result);
        var currentMenu = MenuManager.Instance.GetCurrentMenu();
        if (currentMenu is MatchLobbyMenu matchLobbyMenu)
        {
            matchLobbyMenu.Refresh();
        }
    }
    #endregion EventCallback

    #region ViewState
    private void HideLoadingBackToMainPanel()
    {
        loadingPanel.gameObject.SetActive(false);
        mainPanel.gameObject.SetActive(true);
    }
    
    private void ShowError(string errorMessage)
    {
        mainPanel.gameObject.SetActive(false);
        loadingPanel.gameObject.SetActive(false);
        errorPanel.Show(errorMessage, HideError);
    }

    private void ShowLoading(string loadingInfo, UnityAction cancelCallback=null)
    {
        mainPanel.gameObject.SetActive(false);
        loadingPanel.Show(loadingInfo, cancelCallback);
        errorPanel.gameObject.SetActive(false);
    }

    private void HideError()
    {
        errorPanel.gameObject.SetActive(false);
        mainPanel.gameObject.SetActive(true);
    }
    #endregion ViewState
    
    private void RenderResult(SessionV2GameSession[] gameSessions, int previousPageCount=0)
    {
        for (var i = 0; i < gameSessions.Length; i++)
        {
            var gameSession = gameSessions[i];
            _gameSessions.Add(gameSession);
            var model = new BrowseMatchItemModel(gameSession, previousPageCount + i);
            _loadedModels.Add(model);
            var viewItem = GetAvailableViewItem();
            viewItem.SetData(model, JoinMatch);
            _instantiatedView.Add(viewItem);
        }
        matchItemContainer.sizeDelta = new Vector2(0, (_loadedModels.Count)* ViewItemHeight);
    }
    private void Reset()
    {
        foreach (var matchSessionItem in _instantiatedView)
        {
            matchSessionItem.gameObject.SetActive(false);
        }
        matchItemContainer.sizeDelta = Vector2.zero;
    }
    private MatchSessionItem GetAvailableViewItem()
    {
        var instantiatedView = 
            _instantiatedView.Find(v => !v.gameObject.activeSelf);
        if (instantiatedView == null)
        {
            return Instantiate(matchSessionItemPrefab, matchItemContainer, false);
        }
        else
        {
            return instantiatedView;
        }
    }
    
    #region MenuCanvasOverride
    public override GameObject GetFirstButton()
    {
        return refreshBtn.gameObject;
    }
    public override AssetEnum GetAssetEnum()
    {
        return AssetEnum.BrowseMatchMenuCanvas;
    }
    #endregion MenuCanvasOverride
}