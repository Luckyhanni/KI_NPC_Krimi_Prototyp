using System;
using System.Collections.Generic;

[Serializable]
public class GameState
{
    public bool hasFoundBrokenKey;
    public bool hasFoundBurnedLetter;
    public bool hasFoundDebtNote;
    public bool hasAnalyzedWine;
    public bool hasQuestionedClaraAlibi;
    public bool hasAskedMiraAboutNight;
    public bool caseSolved;
    public string confrontedNpcId;

    public string GetActiveStateSummary()
    {
        List<string> activeFlags = new List<string>();

        if (hasFoundBrokenKey)
        {
            activeFlags.Add("Gebrochener Schluessel gefunden");
        }

        if (hasFoundBurnedLetter)
        {
            activeFlags.Add("Angebrannter Brief gefunden");
        }

        if (hasFoundDebtNote)
        {
            activeFlags.Add("Schuldschein gefunden");
        }

        if (hasAnalyzedWine)
        {
            activeFlags.Add("Wein analysiert");
        }

        if (hasQuestionedClaraAlibi)
        {
            activeFlags.Add("Claras Alibi wurde hinterfragt");
        }

        if (hasAskedMiraAboutNight)
        {
            activeFlags.Add("Mira wurde zur Tatnacht befragt");
        }

        if (caseSolved)
        {
            activeFlags.Add("Fall ist geloest");
        }

        if (!string.IsNullOrWhiteSpace(confrontedNpcId))
        {
            activeFlags.Add("Konfrontierter NPC: " + confrontedNpcId);
        }

        return activeFlags.Count == 0 ? "Keine aktiven State-Flags." : string.Join("; ", activeFlags);
    }
}
