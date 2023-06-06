using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AccelByte.Core;
using AccelByte.Models;
using Steamworks;
using UnityEngine;

public class SteamHelper
{
    private bool isInitialized;
    Callback<GetAuthSessionTicketResponse_t> m_AuthTicketResponseCallback;
    HAuthTicket m_AuthTicket;
    string m_SessionTicket = "";
    public SteamHelper()
    {
        isInitialized = SteamManager.Initialized;
    }
    
    public string SessionTicket
    {
        get { return m_SessionTicket; }
    }

    private WaitForSeconds halfSeconds = new WaitForSeconds(0.5f);
    public async void GetToken(Action<string> onFinished)
    {
        if (isInitialized && String.IsNullOrEmpty(m_SessionTicket))
        {
            m_AuthTicketResponseCallback = Callback<GetAuthSessionTicketResponse_t>.Create(OnAuthCallback);
            var buffer = new byte[1024];
            m_AuthTicket = SteamUser.GetAuthSessionTicket(buffer, buffer.Length, out var ticketSize);

            Array.Resize(ref buffer, (int)ticketSize);

            // The ticket is not ready yet, wait for OnAuthCallback.
            m_SessionTicket = BitConverter.ToString(buffer).Replace("-", string.Empty);
            while (String.IsNullOrEmpty(m_SessionTicket))
            {
                await Task.Delay(700);
            }
        }
        onFinished?.Invoke(m_SessionTicket);
    }
    
    void OnAuthCallback(GetAuthSessionTicketResponse_t callback)
    {
        // Call Unity Authentication SDK to sign in or link with Steam.
        Debug.Log("Steam Login success. Session Ticket: " + m_SessionTicket);
    }
}
