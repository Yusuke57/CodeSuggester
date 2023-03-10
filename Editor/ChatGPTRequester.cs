using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace CodeSuggester
{
    /// <summary>
    /// ChatGPT APIを使ってリクエストを送るクラス
    /// </summary>
    public static class ChatGPTRequester
    {
        private const string REQUEST_URL = "https://api.openai.com/v1/chat/completions";
        private const string REQUEST_METHOD = "POST";

        private static readonly JsonSerializerSettings settings = new()
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public static async UniTask<ResponseData> RequestAsync(RequestData requestData, string apiKey,
            CancellationToken cancellationToken)
        {
            var json = JsonConvert.SerializeObject(requestData, settings);
            Debug.Log($"request: {json}");
            var data = System.Text.Encoding.UTF8.GetBytes(json);

            using var request = new UnityWebRequest(REQUEST_URL, REQUEST_METHOD);
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            request.SetRequestHeader("Content-Type", "application/json");
            request.uploadHandler = new UploadHandlerRaw(data);
            request.downloadHandler = new DownloadHandlerBuffer();

            await request.SendWebRequest().WithCancellation(cancellationToken);

            return JsonConvert.DeserializeObject<ResponseData>(request.downloadHandler.text);
        }
        
        [Serializable]
        public class RequestData
        {
            public string model = "gpt-3.5-turbo";
            public List<Message> messages;
        }

        [Serializable]
        public class Message
        {
            public string role;
            public string content;

            public Message(RoleType roleType, string content)
            {
                role = roleType.ToString().ToLower();
                this.content = content;
            }
            
            [Serializable]
            public enum RoleType
            {
                System,
                User,
                Assistant
            }
        }

        [Serializable]
        public class Choice
        {
            public Message message;
        }

        [Serializable]
        public class ResponseData
        {
            public List<Choice> choices;
        }
    }
}