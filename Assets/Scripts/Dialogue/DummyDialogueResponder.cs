using System;

public class DummyDialogueResponder
{
    public string GenerateDummyResponse(NpcProfile profile, string playerInput, GameState state)
    {
        if (string.IsNullOrWhiteSpace(playerInput))
        {
            return "Bitte gib zuerst eine Frage ein.";
        }

        string normalizedInput = playerInput.ToLowerInvariant();

        switch (profile.id)
        {
            case "clara":
                return GenerateClaraResponse(normalizedInput, state);
            case "anton":
                return GenerateAntonResponse(normalizedInput, state);
            case "mira":
                return GenerateMiraResponse(normalizedInput, state);
            default:
                return profile.displayName + " schweigt. Fuer diesen NPC gibt es noch keine Dummy-Antwort.";
        }
    }

    private static string GenerateClaraResponse(string input, GameState state)
    {
        if (ContainsAny(input, "taeter", "täter", "schuld", "mord", "umgebracht", "getoetet", "getötet"))
        {
            return state.caseSolved
                ? "Wenn der Fall wirklich geklaert ist, werde ich mich nicht laenger hinter Andeutungen verstecken."
                : "Ich verstehe Ihre Sorge, aber solche Anschuldigungen helfen niemandem. Ich kann nur sagen, dass in diesem Haus vieles missverstanden wurde.";
        }

        if (ContainsAny(input, "hintertuer", "hintertür", "schluessel", "schlüssel"))
        {
            return "Die Hintertuer wurde oft benutzt, besonders vom Personal. Ich achte normalerweise sehr darauf, dass sie geschlossen bleibt.";
        }

        return "Ich moechte korrekt bleiben: Ich habe getan, was meine Stellung im Haus verlangte. Mehr sollte man daraus nicht machen.";
    }

    private static string GenerateAntonResponse(string input, GameState state)
    {
        if (ContainsAny(input, "verdacht", "verdaechtig", "verdächtig", "schuld", "geld", "schulden", "schuldschein"))
        {
            return "Ja, ich hatte Schulden. Das macht mich aber nicht zum Moerder. Viktor hat jeden unter Druck gesetzt, nicht nur mich.";
        }

        if (ContainsAny(input, "streit", "viktor", "alibi"))
        {
            return "Der Streit war laut, na und? In dieser Familie wird staendig gestritten. Daraus gleich ein Geständnis zu machen, ist absurd.";
        }

        return "Ich weiss nicht, warum alle immer zuerst mich ansehen. Ich habe genug Probleme, auch ohne diese Unterstellungen.";
    }

    private static string GenerateMiraResponse(string input, GameState state)
    {
        if (ContainsAny(input, "nacht", "gesehen", "licht", "arbeitszimmer", "geraeusch", "geräusch"))
        {
            return "Ich bin mir nicht ganz sicher. Ich glaube, im Arbeitszimmer brannte noch Licht, und spaeter habe ich ein dumpfes Geraeusch gehoert.";
        }

        if (ContainsAny(input, "taeter", "täter", "wer war", "schuld"))
        {
            return "Ich moechte niemanden falsch beschuldigen. Ich habe nur Bruchstuecke gesehen, keine sichere Taeteridentitaet.";
        }

        return "Ich kann nur vorsichtig sagen, was mir aufgefallen ist. Vielleicht ist es wichtig, vielleicht bilde ich mir manches auch nur ein.";
    }

    private static bool ContainsAny(string input, params string[] needles)
    {
        foreach (string needle in needles)
        {
            if (input.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }
}
