using System;

[Serializable]
public class DialogueTurn
{
    public string playerInput;
    public string npcResponse;
    public string timestamp;

    public DialogueTurn(string playerInput, string npcResponse)
    {
        this.playerInput = playerInput;
        this.npcResponse = npcResponse;
        timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
