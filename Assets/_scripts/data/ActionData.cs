using UnityEngine;

[CreateAssetMenu(fileName = "NewAction", menuName = "Race/Action Skill")]
public class ActionData : ScriptableObject
{
    public string actionName;
    public Sprite icon;
    [Range(1, 4)] public int level = 1;
    public string description;

    // Atributos de jogo
    public int cost; // Turbo/Estamina
    public int power; // O quanto move ou o quanto ataca
}