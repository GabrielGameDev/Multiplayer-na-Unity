using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Threading.Tasks;

public class LobbyManager : MonoBehaviour
{

    public TMP_InputField playerNameInput;
    public TMP_InputField lobbyCodeInput;

    Lobby hostLobby, joinnedLobby;

    // Start is called before the first frame update
    async void Start()
    {
        await UnityServices.InitializeAsync();
    }

    // Update is called once per frame
    void Update()
    {

    }

    async Task Authenticate()
    {

		if (AuthenticationService.Instance.IsSignedIn)
		{
			return;
		}

        AuthenticationService.Instance.ClearSessionToken();

		AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Logado como " + AuthenticationService.Instance.PlayerId);
        };        

		await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    async public void CreateLobby()
    {
        try
        {

            await Authenticate();

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                Player = new Player
                {

                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerNameInput.text) }
                    }

                }
            };

			Lobby lobby = await Unity.Services.Lobbies.LobbyService.Instance.CreateLobbyAsync("Lobby", 4, createLobbyOptions);

            Debug.Log("Criou o lobby " + lobby.LobbyCode);

			hostLobby = lobby;
            joinnedLobby = hostLobby;

			InvokeRepeating("LobbyHeartBeat", 15, 15);

		}
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }


    }
      


    async void LobbyHeartBeat()
    {
        if (hostLobby == null)
            return;

        await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
        Debug.Log("Atualizou lobby");
        UpdateLobby();
		ShowPlayersOnLobby();
	}
    
    async public void JoinLobby()
    {
        try
        {
            await Authenticate();

			JoinLobbyByCodeOptions createLobbyOptions = new JoinLobbyByCodeOptions
			{
				Player = new Player
				{

					Data = new Dictionary<string, PlayerDataObject>
					{
						{ "name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerNameInput.text) }
					}

				}
			};

			Lobby lobby = await Unity.Services.Lobbies.LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCodeInput.text, createLobbyOptions);

            joinnedLobby = lobby;

			Debug.Log("Entrou no lobby " + lobby.LobbyCode);
			ShowPlayersOnLobby();
		}
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

    }

    async void UpdateLobby()
    {
		if (joinnedLobby == null)
			return;

        joinnedLobby = await LobbyService.Instance.GetLobbyAsync(joinnedLobby.Id);
        
	}

	void ShowPlayersOnLobby()
	{
        foreach (Player player in joinnedLobby.Players)
        {
			Debug.Log(player.Data["name"].Value);
		}
    }
}
