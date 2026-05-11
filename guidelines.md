# Projekt-Guidelines fuer Codex

Diese Datei dient als Arbeitskontext fuer weitere Entwicklungsdurchgaenge. Sie soll aktuell gehalten werden, wenn sich Architektur, Workflow, Ziele oder wichtige Projektentscheidungen aendern.

## Projektinfo

- Projektname: KI_NPC_Krimi_Prototyp
- Kontext: Bachelorarbeits-Prototyp fuer KI-gestuetzte NPC-Dialoge in einem kleinen Krimi-Szenario.
- Fokus: Dialoglogik, NPC-Profile, Wissensgrenzen, GameState, Memory, Constraints, Promptaufbau und Logging.
- Nicht-Fokus: Grafikqualitaet, komplexes Movement, fertiges Spielsystem oder produktive KI-Anbindung.
- Aktueller Prototyp-Ansatz: Zunaechst Dummy-Modus ohne echte KI/API, damit UI, State, Promptstruktur und Logging testbar sind.

## Worum es geht

Der Prototyp soll untersuchen und demonstrieren, wie NPCs in einem Krimi-Kontext kontrolliert mit Wissen umgehen koennen. Jeder NPC hat ein Profil, erlaubtes Wissen, gesperrtes Wissen, Constraints und einen eigenen Antwortstil. Spielerfragen werden zusammen mit State und Memory zu einem Prompt-/Kontextpaket verarbeitet. In der aktuellen Version erzeugt ein Dummy-Responder statt einer echten KI die Antwort.

Die drei ersten NPCs sind:

- Clara Weber: Haushalterin, kontrolliert und ausweichend, mit internem Taeterwissen.
- Anton Stein: Neffe, nervoes und defensiv, falsche Faehrte.
- Mira Feld: Nachbarin und Zeugin, vorsichtig und unsicher.

## Tech Stack

- Unity: 6000.3.15f1
- Projektart: 2D/UI-basierter Unity-Prototyp
- Render Pipeline: Universal Render Pipeline / 2D Renderer
- UI aktuell: UnityEngine.UI
- TextMeshPro: Nicht im Manifest vorhanden; nur verwenden, wenn es spaeter bewusst hinzugefuegt oder im Projekt verfuegbar ist.
- Input: Unity Input System ist installiert; fuer UI wird beim Auto-Aufbau `InputSystemUIInputModule` verwendet.
- MCP: Unity MCP ist verbunden und kann fuer Szene, Assets, Scripts, Console und Play-Mode-Pruefungen genutzt werden.

## Aktuelle Struktur

Wichtige Skripte liegen unter:

```text
Assets/
  Scripts/
    Data/
      NpcProfile.cs
      GameState.cs
      NpcMemory.cs
      DialogueTurn.cs
    Dialogue/
      PromptBuilder.cs
      DummyDialogueResponder.cs
      DialogueManager.cs
    UI/
      ChatUIController.cs
    Logging/
      DialogueLogger.cs
```

Die Szene `Assets/Scenes/SampleScene.unity` enthaelt ein `DialogueBootstrap`-GameObject mit `DialogueManager` und `ChatUIController`. Die UI wird beim Start automatisch erzeugt, falls keine Inspector-Referenzen gesetzt sind.

## Wichtige Entwicklungsregeln

- Keine API-Keys ins Projekt schreiben.
- Keine echten OpenAI/API-Requests implementieren, bis das explizit gewuenscht ist.
- Keine externen Packages hinzufuegen, ausser der Nutzer fordert es klar an.
- Bestehende Dateien nicht loeschen, wenn es nicht ausdruecklich verlangt wurde.
- C#-Code muss kompilierbar bleiben.
- Nach Script-Aenderungen Unity kompilieren lassen und die Console auf Errors/Warnungen pruefen.
- Szene nach relevanten Aenderungen speichern.
- Bei UI-Aenderungen Play Mode starten und mindestens pruefen, ob keine Runtime-Errors auftreten.
- Dokumentationsnotiz fuer den Nutzer liefern: Was wurde gemacht, warum, und aktueller Projektstand.

## Wichtige Unity-MCP-Workflows

Vor groesseren Unity-Aenderungen:

1. `mcpforunity://editor/state` lesen.
2. Bei Script-Aenderungen nach Refresh/Compile warten, bis Unity nicht mehr kompiliert.
3. `read_console` fuer Errors/Warnungen nutzen.
4. Bei Szenenaenderungen `manage_scene(action="save")` ausfuehren.
5. Fuer mehrere Unity-Aktionen bevorzugt `batch_execute` nutzen.

Nuetzliche MCP-Ressourcen:

- `mcpforunity://editor/state`
- `mcpforunity://project/info`
- `mcpforunity://scene/gameobject-api`
- `mcpforunity://instances`
- `mcpforunity://custom-tools`

## Wichtige lokale Befehle

```powershell
git status --short
rg --files
rg -n "Suchbegriff" Assets
Get-Content -Raw "Packages\manifest.json"
```

Nach Unity-Arbeit immer sinnvoll:

```powershell
git status --short
```

## Aktueller Stand

- Erste Dummy-Minimalversion ist implementiert.
- NPC-Auswahl fuer Clara, Anton und Mira ist vorhanden.
- Spieler kann eine Frage eingeben und senden.
- Dummy-Antworten unterscheiden sich je NPC.
- PromptBuilder erzeugt ein Kontextpaket mit Systemrolle, NPC-Profil, erlaubtem Wissen, gesperrten Wissensbereichen als Ausweichhinweis, State, Memory, Constraints und Spielereingabe.
- DialogueLogger schreibt Prompt, Antwort, aktiven NPC, State-Flags, erlaubtes Wissen und Constraints in die Unity Console.
- Optionales File-Logging geht nach `Application.persistentDataPath/dialogue_dummy_log.txt`.
- Es gibt weiterhin keine echte KI/API-Anbindung.

## Naechste sinnvolle Schritte

- NPC-Profile aus dem Code in ScriptableObjects oder JSON auslagern.
- State-Flags ueber UI-Debug-Schalter testbar machen.
- Prompt-Debug-Ansicht lesbarer machen.
- Memory-Regeln ausbauen, zum Beispiel Gewichtung oder Zusammenfassung nach NPC.
- DummyResponder spaeter durch eine klar gekapselte Provider-Schnittstelle ersetzen, ohne API-Keys im Repo zu speichern.
- Tests oder einfache Editor-Checks fuer PromptBuilder und DummyResponder ergaenzen.

## Dokumentationsstil

Bei jeder groesseren Aenderung am Projekt eine kurze Notiz fuer die Bachelorarbeits-Dokumentation liefern:

- Was wurde gemacht?
- Warum wurde es gemacht?
- Welche Architektur-/Designentscheidung steckt dahinter?
- Was ist der aktuelle Stand?
- Welche Einschraenkungen oder offenen Punkte gibt es?
