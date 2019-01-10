using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ServerAPI;
using Newtonsoft.Json;

public static class LoginManager
{
    public struct Account
    {
        public string username;
        public string email;
    }

    public struct LoginResponse
    {
        int result;
        public string token;
        public string wsToken;
        public Account account;
    }

    public static bool isLoggedIn { get; private set; }
    public static string token { get; private set; }
    public static string wsToken { get; private set; }

    public static void SendLogin(string user, string pass, System.Action<LoginResponse, int> callback)
    {
        var data = new 
        {
            username = user,
            password = pass,
            game = "tribunals",
        };

        API.POST<LoginResponse>("raincrow/login", JsonConvert.SerializeObject(data), (response, result) => OnLoginResponse(response, result, callback), false);
    }

    private static void OnLoginResponse(LoginResponse response, int result, System.Action<LoginResponse, int> callback)
    {
        if (result == 200)
        {
            isLoggedIn = true;
            token = response.token;
            wsToken = response.wsToken;
        }
        else
        {
            isLoggedIn = false;
            token = "";
            wsToken = "";
        }
        callback(response, result);
    }
}
