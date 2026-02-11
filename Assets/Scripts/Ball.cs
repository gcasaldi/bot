using UnityEngine;

namespace PopAndStack
{
    public class Ball : MonoBehaviour
    {
        public int Level { get; set; }
        public bool IsMerging { get; private set; }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Ball other = collision.collider.GetComponent<Ball>();
            if (other == null)
            {
                return;
            }

            if (IsMerging || other.IsMerging)
            {
                return;
            }

            if (Level != other.Level)
            {
                return;
            }

            if (GetInstanceID() > other.GetInstanceID())
            {
                return;
            }

            IsMerging = true;
            other.IsMerging = true;
            GameManager.Instance.MergeBalls(this, other);
        }
    }
}
