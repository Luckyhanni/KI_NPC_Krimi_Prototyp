public interface IDialogueResponder
{
    string GenerateResponse(NpcProfile profile, string playerInput, GameState state, NpcMemory memory, string prompt);
}
