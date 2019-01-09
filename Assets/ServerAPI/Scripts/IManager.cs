using System.Collections;
using System;

namespace ServerAPI
{
    public interface IManager
    {
        IEnumerator RequestRoutine(string url, string data, string method, bool requireToken, bool requireWssToken, Action<string, int> callback);
    }
}