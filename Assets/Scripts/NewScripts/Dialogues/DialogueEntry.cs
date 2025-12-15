using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueEntry
{
    public string id;
    public string branch;
    public string speaker;
    public string text;
    public string choice_group;
    public string choice_text;
    public string next_id;
    public string next_choice;
    public bool is_end;
}
