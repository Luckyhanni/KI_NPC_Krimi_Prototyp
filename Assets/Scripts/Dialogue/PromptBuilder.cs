using System.Collections.Generic;
using System.Text;

public class PromptBuilder
{
    public const string PromptVersion = "v0.3-scope-guard";

    public string BuildPrompt(NpcProfile profile, GameState state, NpcMemory memory, string playerInput)
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine("## Systemrolle");
        builder.AppendLine("Prompt-Version: " + PromptVersion);
        builder.AppendLine("Du spielst einen NPC in einem 2D-Krimi-Prototyp. Antworte nur aus der Perspektive des aktiven NPCs.");
        builder.AppendLine("Du bist kein allgemeiner Chatbot.");
        builder.AppendLine("Du beantwortest keine Fragen zu Themen außerhalb des Krimi-Szenarios.");
        builder.AppendLine("Antworte nur zu Themen, die mit dem Fall Viktor Stein, Haus Lindenfels, dem aktiven NPC, der Tatnacht, Hinweisen, Verdacht, Alibi, Beziehungen der Figuren oder bereits bekannten Fallinformationen zusammenhängen.");
        builder.AppendLine("Wenn die Spielereingabe themenfremd ist, antworte kurz im Charakter, dass du dazu nichts beitragen kannst, und lenke zurück auf den Fall.");
        builder.AppendLine("Gib keine allgemeinen Tipps, Erklärungen oder Informationen zu realen Spielen, Technik, Politik, Medizin, Programmierung, Alltagsthemen oder anderen externen Themen.");
        builder.AppendLine("Erfinde keine externen Fakten.");
        builder.AppendLine("Antworte bei Off-Topic-Fragen in 1 bis 2 kurzen Sätzen.");
        builder.AppendLine("Nutze ausschließlich erlaubtes Wissen, beachte Wissensgrenzen und weiche bei gesperrtem Wissen passend aus.");
        builder.AppendLine();

        builder.AppendLine("## NPC-Profil");
        builder.AppendLine("ID: " + profile.id);
        builder.AppendLine("Name: " + profile.displayName);
        builder.AppendLine("Rolle: " + profile.role);
        builder.AppendLine("Persönlichkeit: " + profile.personality);
        builder.AppendLine("Motivation: " + profile.motivation);
        builder.AppendLine("Sprechstil: " + profile.speechStyle);
        builder.AppendLine();

        builder.AppendLine("## Erlaubtes Wissen");
        AppendList(builder, profile.allowedKnowledge, "- ");
        builder.AppendLine();

        builder.AppendLine("## Gesperrtes Wissen");
        builder.AppendLine("Details zu gesperrtem Wissen werden dem NPC nicht direkt offengelegt. Bei solchen Themen gilt:");
        AppendList(builder, profile.evasionStrategies, "- ");
        if (profile.blockedKnowledge.Count > 0)
        {
            builder.AppendLine("Abstrakte Sperrbereiche: " + string.Join(", ", profile.blockedKnowledge));
        }
        builder.AppendLine();

        builder.AppendLine("## Aktueller State");
        builder.AppendLine(state.GetActiveStateSummary());
        builder.AppendLine();

        builder.AppendLine("## Memory");
        builder.AppendLine(memory.GetMemorySummary());
        builder.AppendLine();

        builder.AppendLine("## Constraints");
        AppendList(builder, profile.constraints, "- ");
        builder.AppendLine();

        builder.AppendLine("## Spielereingabe");
        builder.AppendLine(playerInput);

        return builder.ToString();
    }

    private static void AppendList(StringBuilder builder, List<string> entries, string prefix)
    {
        if (entries == null || entries.Count == 0)
        {
            builder.AppendLine(prefix + "Keine Einträge.");
            return;
        }

        foreach (string entry in entries)
        {
            builder.Append(prefix);
            builder.AppendLine(entry);
        }
    }
}
