public class ApiDialogueResponder : IDialogueResponder
{
    public const string MissingApiKeyMessage = "API-Modus ist aktiviert, aber kein API-Key wurde gefunden. Bitte OPENAI_API_KEY lokal setzen oder Dummy-Modus verwenden.";

    private readonly ApiConfig apiConfig;

    public ApiDialogueResponder(ApiConfig apiConfig)
    {
        this.apiConfig = apiConfig;
    }

    public string GenerateResponse(NpcProfile profile, string playerInput, GameState state, NpcMemory memory, string prompt)
    {
        if (apiConfig == null || !apiConfig.HasApiKey)
        {
            return MissingApiKeyMessage;
        }

        return "API-Modus ist vorbereitet, aber echte API-Anfragen sind in diesem Prototyp-Schritt noch deaktiviert. Bitte Dummy-Modus verwenden.";
    }
}
