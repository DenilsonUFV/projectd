using UnityEngine;
using TacticsRace.Grid;
using System.Collections.Generic;

namespace TacticsRace.Skills
{
    [CreateAssetMenu(fileName = "DriftSkill", menuName = "TacticsRace/Skills/Drift")]
    public class DriftSkillSO : SkillSO
    {
        [Header("Configuração de Drift")]
        public int[] boostAmount = { 1, 2, 3, 5 }; // Boost de movimento por nível

        public override bool CanUse(Vector3Int currentPos, int agility)
        {
            // Apenas checa a célula EXATA onde o carro parou
            return TilemapGridManager.Instance.IsCurveTile(currentPos);
        }
    }
}