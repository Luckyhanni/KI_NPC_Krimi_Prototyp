using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

[Serializable]
public class DialogueLogEntry
{
    public string timestamp;
    public string mode;
    public string responseMode;
    public string promptVersion;
    public string testCaseId;
    public string npcId;
    public string npcDisplayName;
    public string playerInput;
    public string activeStateSummary;
    public List<string> usedAllowedKnowledge;
    public List<string> usedConstraints;
    public string generatedPrompt;
    public string npcResponse;
}

public class DialogueLogger
{
    private readonly bool writeFile;
    private readonly string legacyTextLogFilePath;
    private readonly string jsonlLogFilePath;

    public DialogueLogger(bool writeFile = true)
    {
        this.writeFile = writeFile;
        legacyTextLogFilePath = Path.Combine(Application.persistentDataPath, "dialogue_dummy_log.txt");
        jsonlLogFilePath = Path.Combine(Application.persistentDataPath, "dialogue_logs.jsonl");
    }

    public void LogTurn(NpcProfile profile, GameState state, string playerInput, string prompt, string npcResponse, string promptVersion, string testCaseId, ResponseMode responseMode)
    {
        string timestamp = DateTime.Now.ToString("o");
        string stateSummary = state.GetActiveStateSummary();
        string responseModeValue = responseMode.ToString();
        string modeValue = responseModeValue.ToLowerInvariant();
        StringBuilder builder = new StringBuilder();

        builder.AppendLine("========== Dialogue Log ==========");
        builder.AppendLine("Timestamp: " + timestamp);
        builder.AppendLine("Mode: " + modeValue);
        builder.AppendLine("ResponseMode: " + responseModeValue);
        builder.AppendLine("Prompt-Version: " + promptVersion);
        builder.AppendLine("Testfall-ID: " + testCaseId);
        builder.AppendLine("NPC-ID: " + profile.id);
        builder.AppendLine("NPC-Name: " + profile.displayName);
        builder.AppendLine("Spielerfrage: " + playerInput);
        builder.AppendLine("Aktive State-Flags: " + stateSummary);
        builder.AppendLine("Erlaubtes Wissen: " + string.Join(", ", profile.allowedKnowledge));
        builder.AppendLine("Constraints: " + string.Join(", ", profile.constraints));
        builder.AppendLine("Prompt:");
        builder.AppendLine(prompt);
        builder.AppendLine("Antwort:");
        builder.AppendLine(npcResponse);
        builder.AppendLine("========================================");

        Debug.Log(builder.ToString());

        if (!writeFile)
        {
            return;
        }

        try
        {
            File.AppendAllText(legacyTextLogFilePath, builder + Environment.NewLine);
            DialogueLogEntry entry = new DialogueLogEntry
            {
                timestamp = timestamp,
                mode = modeValue,
                responseMode = responseModeValue,
                promptVersion = promptVersion,
                testCaseId = string.IsNullOrWhiteSpace(testCaseId) ? "manual" : testCaseId,
                npcId = profile.id,
                npcDisplayName = profile.displayName,
                playerInput = playerInput,
                activeStateSummary = stateSummary,
                usedAllowedKnowledge = new List<string>(profile.allowedKnowledge),
                usedConstraints = new List<string>(profile.constraints),
                generatedPrompt = prompt,
                npcResponse = npcResponse
            };
            File.AppendAllText(jsonlLogFilePath, JsonUtility.ToJson(entry) + Environment.NewLine);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("DialogueLogger konnte nicht in die Logdatei schreiben: " + exception.Message);
        }
    }
}
