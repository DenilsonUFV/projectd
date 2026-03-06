public interface IMinigameLogic
{
    void OnStart();
    void OnUpdate();
    void OnFinish();
    float GetProgress(); // Para a barra visual
}