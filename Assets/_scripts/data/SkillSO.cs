using UnityEngine;

namespace TacticsRace.Skills
{
    [CreateAssetMenu(fileName = "NovaHabilidade", menuName = "TacticsRace/Skill")]
    public class SkillSO : ScriptableObject
    {
        public string skillName;
        public Sprite icon;
       
        public int levelsAvailable = 1; // Quantos níveis esta versão da habilidade libera (1 a 4)

        [Header("Configuração de Níveis")]
        public int[] paCosts = new int[4] { 1, 2, 4, 6 }; // Custo por nível
        public float[] powerValues = new float[4];        // Intensidade do efeito

        public string GetDescription(int level) => $"{skillName} Nível {level} - Custo: {paCosts[level - 1]} PA";

        public virtual bool CanUse(Vector3Int currentPos, int agility)
        {
            return true; // Padrão: pode usar em qualquer lugar
        }

    }
}