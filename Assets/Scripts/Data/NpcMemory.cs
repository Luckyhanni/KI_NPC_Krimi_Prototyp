using System;
using System.Collections.Generic;
using System.Text;

[Serializable]
public class NpcMemory
{
    public string npcId;
    public List<DialogueTurn> turns = new List<DialogueTurn>();
    public string relationshipState = "neutral";

    public NpcMemory(string npcId)
    {
        this.npcId = npcId;
    }

    public void AddTurn(string playerInput, string npcResponse)
    {
        turns.Add(new DialogueTurn(playerInput, npcResponse));
    }

    public void Clear()
    {
        turns.Clear();
        relationshipState = "neutral";
    }

    public string GetMemorySummary()
    {
        if (turns.Count == 0)
        {
            return "Bisher keine Gespraechserinnerungen.";
        }

        int startIndex = Math.Max(0, turns.Count - 3);
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Beziehungsstatus: " + relationshipState);
        builder.AppendLine("Letzte relevante Turns:");

        for (int i = startIndex; i < turns.Count; i++)
        {
            DialogueTurn turn = turns[i];
            builder.Append("- Spieler: ");
            builder.AppendLine(turn.playerInput);
            builder.Append("- NPC: ");
            builder.AppendLine(turn.npcResponse);
        }

        return builder.ToString().TrimEnd();
    }
}
