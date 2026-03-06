using UnityEngine;
using TacticsRace.Skills;
using TacticsRace.Core; // Certifique-se de que DriverData está neste namespace ou ajuste
using System.Collections.Generic;

namespace TacticsRace.Core
{
    public class PlayerDataManager : MonoBehaviour
    {
        public static PlayerDataManager Instance { get; private set; }

        [Header("Ficha do Piloto Atual")]
        // Aqui você arrasta o ScriptableObject do seu piloto (Takumi, etc)
        public DriverData activeDriver;

        [Header("Habilidades Desbloqueadas (Carreira)")]
        // Listas que persistem entre as corridas
        public List<SkillSO> unlockedOffensiveSkills = new List<SkillSO>();
        public List<SkillSO> unlockedDefensiveSkills = new List<SkillSO>();

        [Header("Recursos de Corrida")]
        public int currentPA = 0;
        public int maxPA = 10;

        // No PlayerDataManager.cs
        [HideInInspector] public int temporaryMovementBoost = 0;

        public void SetBoost(int amount)
        {
            temporaryMovementBoost = amount;
        }

        public void ClearBoost()
        {
            temporaryMovementBoost = 0;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Centralizamos a gestão de PA aqui para ser acessível globalmente
        public void AddPA(int amount)
        {
            currentPA = Mathf.Min(currentPA + amount, maxPA);
            Debug.Log($"PA Total: {currentPA}");
        }

        public bool ConsumePA(int amount)
        {
            if (currentPA >= amount)
            {
                currentPA -= amount;
                return true;
            }
            return false;
        }
    }
}