using System;
using System.Collections.Generic;

[Serializable]
public class NpcProfile
{
    public string id;
    public string displayName;
    public string role;
    public string personality;
    public string motivation;
    public string speechStyle;
    public List<string> allowedKnowledge = new List<string>();
    public List<string> blockedKnowledge = new List<string>();
    public List<string> evasionStrategies = new List<string>();
    public List<string> constraints = new List<string>();
}
