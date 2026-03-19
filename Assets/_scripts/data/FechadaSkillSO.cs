using UnityEngine;
using TacticsRace.Grid;
using System.Collections.Generic;

namespace TacticsRace.Skills
{
    [CreateAssetMenu(fileName = "FechadaSkill", menuName = "TacticsRace/Skills/Fechada")]
    public class FechadaSkillSO : SkillSO
    {
        public override bool CanUse(Vector3Int currentPos, int agility) => true;

        // Retorna as posições relativas que serão bloqueadas conforme o nível
        public List<Vector3Int> GetBlockOffsets(int level, Transform carTransform)
        {
            List<Vector3Int> offsets = new List<Vector3Int>();

            // O próprio carro já bloqueia (Nível 1 é o padrão do sistema)

            if (level >= 2) // Lado Direito ou Esquerdo (Vamos padronizar ou Sortear)
                offsets.Add(Vector3Int.RoundToInt(carTransform.right));

            if (level >= 3) // Ambos os lados
                offsets.Add(Vector3Int.RoundToInt(-carTransform.right));

            if (level >= 4) // Quadrado de trás
                offsets.Add(Vector3Int.RoundToInt(-carTransform.up));

            return offsets;
        }
    }
}