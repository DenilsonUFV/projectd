namespace TacticsRace.Core
{
    public interface IRaceState
    {
        void Enter();
        void Update();
        void Exit();
        string GetStateName(); // Útil para Debug e UI
    }
}