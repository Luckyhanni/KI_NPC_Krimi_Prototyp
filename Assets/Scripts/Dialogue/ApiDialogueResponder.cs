using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ApiDialogueResponder : IDialogueResponder
{
    public const string MissingApiKeyMessage = "API-Modus ist aktiviert, aber kein API-Key wurde gefunden. Bitte OPENAI_API_KEY lokal setzen oder Dummy-Modus verwenden.";
    private const string ResponsesEndpoint = "https://api.openai.com/v1/responses";
    private const string Model = "gpt-5.4-mini";
    private const int MaxOutputTokens = 220;
    private const float Temperature = 0.4f;

    private readonly ApiConfig apiConfig;

    public ApiDialogueResponder(ApiConfig apiConfig)
    {
        this.apiConfig = apiConfig;
    }

    public IEnumerator GenerateResponse(NpcProfile profile, string playerInput, GameState state, NpcMemory memory, string prompt, Action<string> onResponse)
    {
        if (apiConfig == null || !apiConfig.HasApiKey)
        {
            onResponse?.Invoke(MissingApiKeyMessage);
            yield break;
        }

        OpenAIResponsesRequest requestBody = new OpenAIResponsesRequest
        {
            model = Model,
            input = prompt,
            max_output_tokens = MaxOutputTokens,
            temperature = Temperature
        };

        string jsonBody = JsonUtility.ToJson(requestBody);
        byte[] bodyBytes = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(ResponsesEndpoint, UnityWebRequest.kHttpVerbPOST))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiConfig.ApiKey);

            yield return request.SendWebRequest();

            string rawResponse = request.downloadHandler == null ? string.Empty : request.downloadHandler.text;

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("OpenAI Responses API request failed. HTTP " + request.responseCode + ": " + request.error + "\nResponse: " + rawResponse);
                onResponse?.Invoke("API-Anfrage fehlgeschlagen. HTTP " + request.responseCode + ": " + request.error);
                yield break;
            }

            if (TryExtractResponseText(rawResponse, out string responseText))
            {
                onResponse?.Invoke(responseText);
                yield break;
            }

            Debug.LogWarning("OpenAI Responses API response could not be parsed. Raw response: " + rawResponse);
            onResponse?.Invoke("API-Antwort konnte nicht gelesen werden. Details stehen in der Unity Console.");
        }
    }

    private static bool TryExtractResponseText(string rawResponse, out string responseText)
    {
        responseText = string.Empty;

        if (string.IsNullOrWhiteSpace(rawResponse))
        {
            return false;
        }

        OpenAIResponsesResponse response = JsonUtility.FromJson<OpenAIResponsesResponse>(rawResponse);
        if (response == null)
        {
            return false;
        }

        if (response.error != null && !string.IsNullOrWhiteSpace(response.error.message))
        {
            responseText = "API-Fehler: " + response.error.message;
            return true;
        }

        if (!string.IsNullOrWhiteSpace(response.output_text))
        {
            responseText = response.output_text.Trim();
            return true;
        }

        if (response.output == null)
        {
            return false;
        }

        foreach (OpenAIOutputItem outputItem in response.output)
        {
            if (outputItem == null || outputItem.content == null)
            {
                continue;
            }

            foreach (OpenAIContentItem contentItem in outputItem.content)
            {
                if (contentItem != null && !string.IsNullOrWhiteSpace(contentItem.text))
                {
                    responseText = contentItem.text.Trim();
                    return true;
                }
            }
        }

        return false;
    }

    [Serializable]
    private class OpenAIResponsesRequest
    {
        public string model;
        public string input;
        public int max_output_tokens;
        public float temperature;
    }

    [Serializable]
    private class OpenAIResponsesResponse
    {
        public string output_text;
        public OpenAIOutputItem[] output;
        public OpenAIError error;
    }

    [Serializable]
    private class OpenAIOutputItem
    {
        public string type;
        public string role;
        public OpenAIContentItem[] content;
    }

    [Serializable]
    private class OpenAIContentItem
    {
        public string type;
        public string text;
    }

    [Serializable]
    private class OpenAIError
    {
        public string message;
        public string type;
        public string code;
    }
}
