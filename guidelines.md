# Projekt-Guidelines für Codex

Diese Datei dient als Arbeitskontext für weitere Entwicklungsdurchgänge. Sie soll aktuell gehalten werden, wenn sich Architektur, Workflow, Ziele oder wichtige Projektentscheidungen ändern.

## Projektinfo

- Projektname: KI_NPC_Krimi_Prototyp
- Kontext: Bachelorarbeits-Prototyp für KI-gestützte NPC-Dialoge in einem kleinen Krimi-Szenario.
- Fokus: Dialoglogik, NPC-Profile, Wissensgrenzen, GameState, Memory, Constraints, Promptaufbau und Logging.
- Nicht-Fokus: Grafikqualität, komplexes Movement, fertiges Spielsystem oder produktive KI-Anbindung.
- Aktueller Prototyp-Ansatz: Dummy-Modus plus sicherer API-Modus, damit UI, State, Promptstruktur, Memory und evaluierbares Logging testbar sind.
- Bachelorarbeits-Entscheidung: Kein ausführlicher Vergleich lokales LLM vs. Cloud-LLM; Fokus auf cloud/API-basiertes LLM-Konzept und kontrolliertes NPC-Dialogsystem.

## Worum es geht

Der Prototyp soll untersuchen und demonstrieren, wie NPCs in einem Krimi-Kontext kontrolliert mit Wissen umgehen können. Jeder NPC hat ein Profil, erlaubtes Wissen, gesperrtes Wissen, Constraints und einen eigenen Antwortstil. Spielerfragen werden zusammen mit State und Memory zu einem Prompt-/Kontextpaket verarbeitet.

Die drei ersten NPCs sind:

- Clara Weber: Haushälterin, kontrolliert und ausweichend, mit internem Täterwissen.
- Anton Stein: Neffe, nervös und defensiv, falsche Fährte.
- Mira Feld: Nachbarin und Zeugin, vorsichtig und unsicher.

## Tech Stack

- Unity: 6000.3.15f1
- Projektart: 2D/UI-basierter Unity-Prototyp
- Render Pipeline: Universal Render Pipeline / 2D Renderer
- UI aktuell: UnityEngine.UI mit automatisch erzeugtem dreigeteiltem Dashboard.
- TextMeshPro: Nicht im Manifest vorhanden; nur verwenden, wenn es später bewusst hinzugefügt oder im Projekt verfügbar ist.
- Input: Unity Input System ist installiert; für UI wird beim Auto-Aufbau `InputSystemUIInputModule` verwendet.
- MCP: Unity MCP ist verbunden und kann für Szene, Assets, Scripts, Console und Play-Mode-Prüfungen genutzt werden.

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
      IDialogueResponder.cs
      ResponseMode.cs
      ApiConfig.cs
      ApiDialogueResponder.cs
      PromptBuilder.cs
      DummyDialogueResponder.cs
      DialogueManager.cs
    UI/
      ChatUIController.cs
    Logging/
      DialogueLogger.cs
```

Die Szene `Assets/Scenes/SampleScene.unity` enthält ein `DialogueBootstrap`-GameObject mit `DialogueManager` und `ChatUIController`. Die UI wird beim Start automatisch erzeugt, falls keine Inspector-Referenzen gesetzt sind.

## Wichtige Entwicklungsregeln

- Keine API-Keys ins Projekt schreiben.
- Keine externen Packages hinzufügen, außer der Nutzer fordert es klar an.
- Bestehende Dateien nicht löschen, wenn es nicht ausdrücklich verlangt wurde.
- C#-Code muss kompilierbar bleiben.
- Nach Script-Änderungen Unity kompilieren lassen und die Console auf Errors/Warnungen prüfen.
- Szene nach relevanten Änderungen speichern.
- Bei UI-Änderungen Play Mode starten und mindestens prüfen, ob keine Runtime-Errors auftreten.
- Dokumentationsnotiz für den Nutzer liefern: Was wurde gemacht, warum, und aktueller Projektstand.
- Sichtbare deutsche Texte in UI, Prompts, NPC-Profilen, Chatantworten und Logs verwenden echte Umlaute und ß. Technische IDs und interne Matcher bleiben unverändert.

## Wichtige Unity-MCP-Workflows

Vor größeren Unity-Änderungen:

1. `mcpforunity://editor/state` lesen.
2. Bei Script-Änderungen nach Refresh/Compile warten, bis Unity nicht mehr kompiliert.
3. `read_console` für Errors/Warnungen nutzen.
4. Bei Szenenänderungen `manage_scene(action="save")` ausführen.
5. Für mehrere Unity-Aktionen bevorzugt `batch_execute` nutzen.

Nützliche MCP-Ressourcen:

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
- NPC-Auswahl für Clara, Anton und Mira ist vorhanden.
- Spieler kann eine Frage eingeben und senden.
- Dummy-Antworten unterscheiden sich je NPC.
- PromptBuilder erzeugt ein Kontextpaket mit Systemrolle, NPC-Profil, erlaubtem Wissen, gesperrten Wissensbereichen als Ausweichhinweis, State, Memory, Constraints und Spielereingabe.
- DialogueLogger schreibt Prompt, Antwort, aktiven NPC, State-Flags, erlaubtes Wissen und Constraints in die Unity Console.
- Persistentes Evaluationslogging schreibt JSONL nach `Application.persistentDataPath/dialogue_logs.jsonl`.
- JSONL und Textlog werden explizit mit UTF-8 geschrieben.
- Jeder Logeintrag enthält u.a. `mode`, `responseMode`, `promptVersion`, `testCaseId`, NPC-Daten, State, erlaubtes Wissen, Constraints, Prompt und Antwort.
- Aktuelle Prompt-Version: `v0.2-controlled-context`.
- Die UI enthält ein Testfall-ID-Feld mit Standardwert `manual`.
- State-Flags können im UI per Toggle geändert werden.
- Auto-State-Progression ist für Evaluation standardmäßig deaktiviert und kann im Debug-Panel per `Auto-State` Toggle aktiviert werden.
- Responder sind über `IDialogueResponder` abstrahiert.
- `ResponseMode` unterscheidet `Dummy` und `Api`; Standard bleibt `Dummy`.
- `ApiConfig` liest den API-Key ausschließlich aus der lokalen Environment Variable `OPENAI_API_KEY`.
- `ApiDialogueResponder` nutzt im API-Modus die OpenAI Responses API über `UnityWebRequest`.
- Endpoint: `https://api.openai.com/v1/responses`
- Modell: `gpt-5.4-mini`
- Request-Konfiguration: Prompt als `input`, `max_output_tokens` ca. 220, `temperature` 0.4, keine Tools.
- Ohne `OPENAI_API_KEY` gibt der API-Modus eine klare Fehlermeldung im Chat zurück.
- Memory kann für den aktiven NPC oder für alle NPCs zurückgesetzt werden.
- Die Runtime-UI nutzt CanvasScaler mit 1920 x 1080 Reference Resolution und ein dunkles Krimi-Farbschema.
- Layout: Titel oben, NPC-Karten links, Chat in der Mitte, Debug/State rechts, Eingabe unten.
- Dummy-Modus bleibt offline und ist weiterhin Standard.
- Keine API-Keys oder Secrets in Code, Assets, Resources oder StreamingAssets speichern.

## Nächste sinnvolle Schritte

- NPC-Profile aus dem Code in ScriptableObjects oder JSON auslagern.
- Prompt-Debug-Ansicht lesbarer machen.
- Memory-Regeln ausbauen, zum Beispiel Gewichtung oder Zusammenfassung nach NPC.
- API-Antwortqualität mit festen Testfällen evaluieren.
- Tests oder einfache Editor-Checks für PromptBuilder und DummyResponder ergänzen.

## Dokumentationsstil

Bei jeder größeren Änderung am Projekt eine kurze Notiz für die Bachelorarbeits-Dokumentation liefern:

- Was wurde gemacht?
- Warum wurde es gemacht?
- Welche Architektur-/Designentscheidung steckt dahinter?
- Was ist der aktuelle Stand?
- Welche Einschränkungen oder offenen Punkte gibt es?
