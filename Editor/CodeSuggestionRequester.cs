using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Message = CodeSuggester.ChatGPTRequester.Message;

namespace CodeSuggester
{
    public class CodeSuggestionRequester
    {
        private readonly Action<string> onRequestCompleted;

        private CancellationTokenSource cancellationTokenSource;
        
        private static readonly List<Message> defaultMessages = new()
        {
            new Message(Message.RoleType.System, "あなたは優秀なAIアシスタントです。"),
            new Message(Message.RoleType.User, "Unityで使用している以下のコードの改善点を列挙してください。" +
                                               "参考となる公式リファレンスや公式ドキュメントのURLも必ず明記してください。")
        };

        public CodeSuggestionRequester(Action<string> onRequestCompleted)
        {
            this.onRequestCompleted = onRequestCompleted;
        }

        public void Request(string apiKey, string message)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                onRequestCompleted?.Invoke("API keyを入力してください");
                return;
            }

            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();

            try
            {
                RequestAsync(apiKey, message, cancellationTokenSource.Token).Forget();
            }
            catch
            {
                // ignored
            }
        }

        private async UniTask RequestAsync(string apiKey, string message, CancellationToken token)
        {
            var messages = CreateRequestMessages(message);
            var requestData = new ChatGPTRequester.RequestData(){ messages = messages };
            var response = await ChatGPTRequester.RequestAsync(requestData, apiKey, token);
            var responseMessage = response.choices[0].message;
            onRequestCompleted?.Invoke(responseMessage.content);
        }

        private List<Message> CreateRequestMessages(string originCode)
        {
            return new List<Message>(defaultMessages)
            {
                new(Message.RoleType.User, originCode)
            };
        }
    }
}