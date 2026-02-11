using UnityEngine;

namespace PopAndStack
{
    public class GameOverLine : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            Ball ball = other.GetComponent<Ball>();
            if (ball == null)
            {
                return;
            }

            GameManager.Instance.TriggerGameOver();
        }
    }
}
