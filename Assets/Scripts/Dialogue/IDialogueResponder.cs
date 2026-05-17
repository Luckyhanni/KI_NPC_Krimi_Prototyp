using System;
using System.Collections;

public interface IDialogueResponder
{
    IEnumerator GenerateResponse(NpcProfile profile, string playerInput, GameState state, NpcMemory memory, string prompt, Action<string> onResponse);
}
