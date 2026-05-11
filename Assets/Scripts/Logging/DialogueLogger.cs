using System;
using System.IO;
using System.Text;
using UnityEngine;

public class DialogueLogger
{
    private readonly bool writeFile;
    private readonly string logFilePath;

    public DialogueLogger(bool writeFile = true)
    {
        this.writeFile = writeFile;
        logFilePath = Path.Combine(Application.persistentDataPath, "dialogue_dummy_log.txt");
    }

    public void LogTurn(NpcProfile profile, GameState state, string playerInput, string prompt, string dummyResponse)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        StringBuilder builder = new StringBuilder();

        builder.AppendLine("========== Dialogue Dummy Log ==========");
        builder.AppendLine("Timestamp: " + timestamp);
        builder.AppendLine("NPC-ID: " + profile.id);
        builder.AppendLine("NPC-Name: " + profile.displayName);
        builder.AppendLine("Spielerfrage: " + playerInput);
        builder.AppendLine("Aktive State-Flags: " + state.GetActiveStateSummary());
        builder.AppendLine("Erlaubtes Wissen: " + string.Join(", ", profile.allowedKnowledge));
        builder.AppendLine("Constraints: " + string.Join(", ", profile.constraints));
        builder.AppendLine("Prompt:");
        builder.AppendLine(prompt);
        builder.AppendLine("Dummy-Antwort:");
        builder.AppendLine(dummyResponse);
        builder.AppendLine("========================================");

        Debug.Log(builder.ToString());

        if (!writeFile)
        {
            return;
        }

        try
        {
            File.AppendAllText(logFilePath, builder + Environment.NewLine);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("DialogueLogger konnte nicht in die Logdatei schreiben: " + exception.Message);
        }
    }
}
