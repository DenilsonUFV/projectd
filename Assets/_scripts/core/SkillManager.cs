using UnityEngine;
using TacticsRace.Core;
using TacticsRace.Skills;

public class SkillManager : MonoBehaviour
{
    [Header("PA do Jogador")]
    public int currentPA = 3;
    public int maxPA = 10;

    // Quando o jogador escolhe NÃO andar tudo:
    public void RegainPAFromInertia(int tilesNotMoved)
    {
        if (tilesNotMoved > 0)
        {
            currentPA = Mathf.Min(currentPA + 1, maxPA);
            Debug.Log("Ganhou 1 PA por economizar pneus/combustível!");
        }
    }

    // Exemplo de Habilidades Ofensivas Iniciais
    // 1. Vácuo (Aumenta alcance no próximo turno)
    // 2. Fechada (Bloqueia tiles laterais para o oponente)
    // 3. Pressão (Diminui a agilidade do inimigo próximo)
    // 4. Drift Técnico (Permite ignorar a regra de inércia por 1 movimento)

    public void ApplySkillEffect(SkillSO skill, int level)
    {
        int cost = skill.paCosts[level - 1];

        if (currentPA >= cost)
        {
            currentPA -= cost;
            float power = skill.powerValues[level - 1];

            Debug.Log($"Ativando {skill.skillName} Nível {level}. Poder: {power}");

            // Aqui dispararíamos a animação/efeito na pista
        }
    }
}