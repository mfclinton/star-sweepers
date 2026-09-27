using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Linq;

public static class LeaderboardManager
{
    #region Consts

    public const string PLAYER_SCORE_LEADERBOARD = "PlayerScore";

    #endregion

    #region Delegates

    public delegate void OnGetTopNScores(List<PlayerLeaderboardEntry> leaderboard);
    public static OnGetTopNScores onGetTopNScores;

    public delegate void OnGetPlayerRank(PlayerLeaderboardEntry player);
    public static OnGetPlayerRank onGetPlayerRank;

    public delegate void OnSendScore();
    public static OnSendScore onSendScore;

    #endregion

    #region Local Variables

    private static string curUsername;

    #endregion

    #region Get Top N Scores

    public static void GetTopNScores(int count, string leaderboardName = PLAYER_SCORE_LEADERBOARD)
    {
        if(!PlayFabClientAPI.IsClientLoggedIn())
            return;

        GetLeaderboardRequest request = new GetLeaderboardRequest
        {
            StatisticName = leaderboardName,
            StartPosition = 0,
            MaxResultsCount = count
        };

        PlayFabClientAPI.GetLeaderboard(request, result =>
        {
            onGetTopNScores?.Invoke(result.Leaderboard);
        }, OnError);
    }

    #endregion

    #region Get Player Rank

    public static void GetPlayerRank(string leaderboardName = PLAYER_SCORE_LEADERBOARD)
    {
        if(!PlayFabClientAPI.IsClientLoggedIn())
            return;

        GetLeaderboardAroundPlayerRequest request = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = leaderboardName,
            MaxResultsCount = 100 // Adjust this as needed
        };

        PlayFabClientAPI.GetLeaderboardAroundPlayer(request, result =>
        {
            PlayerLeaderboardEntry player = result.Leaderboard
                .FirstOrDefault(entry => entry.DisplayName == curUsername);
            
            if (player != null)
                onGetPlayerRank?.Invoke(player);
        }, error =>
        {
            Debug.Log("There was an error calling the PlayFab API. Details: " + error.GenerateErrorReport());
        });

    }

    #endregion

    #region Update Leaderboard

    public static void SendScore(int score, string leaderboardName = PLAYER_SCORE_LEADERBOARD)
    {
        if(!PlayFabClientAPI.IsClientLoggedIn())
            return;

        var request = new UpdatePlayerStatisticsRequest
        {
            // You could report multiple statistics at once
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate { StatisticName = leaderboardName, Value = score },
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request, result =>
        {
            onSendScore?.Invoke();
        }, OnError);
    }

    #endregion

    # region Login

    public static void LogOut()
    {
        PlayFabClientAPI.ForgetAllCredentials();
    }

    public static void Login(string userName)
    {
        curUsername = userName;

        if(PlayFabClientAPI.IsClientLoggedIn())
            LogOut();

        var request = new LoginWithCustomIDRequest
        {
            CustomId = userName,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    private static void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Successfully logged in player");

        var displayNameRequest = new UpdateUserTitleDisplayNameRequest { DisplayName = curUsername };

        PlayFabClientAPI.UpdateUserTitleDisplayName(displayNameRequest, OnDisplayNameUpdateSuccess, OnDisplayNameUpdateFailure);

    }

    private static void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError($"Error logging in player: {error.GenerateErrorReport()}");
    }

    private static void OnDisplayNameUpdateSuccess(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log("Successfully updated display name to: " + result.DisplayName);
    }

    private static void OnDisplayNameUpdateFailure(PlayFabError error)
    {
        Debug.LogError("Failed to update display name: " + error.GenerateErrorReport());
    }

    private static void OnError(PlayFabError error)
    {
        Debug.LogError($"Error while sending score to leaderboard: {error.GenerateErrorReport()}");
    }

    #endregion
}
