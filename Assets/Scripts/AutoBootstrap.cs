using UnityEngine;

namespace PopAndStack
{
    public static class AutoBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureGameManager()
        {
            if (Object.FindObjectOfType<GameManager>() == null)
            {
                GameObject manager = new GameObject("GameManager");
                manager.AddComponent<GameManager>();
            }
        }
    }
}
