using UnityEngine;

namespace Devdog.General2
{
    public sealed class SinWave : MonoBehaviour
    {
        public float scale = 1f;
        public float speed = 1f;


        private float _startHeight;
        private void OnEnable()
        {
            _startHeight = transform.position.y;
        }
        
        private void Update()
        {
            var y = Mathf.Sin(Time.time * speed) * scale;
            transform.position = new Vector3(transform.position.x, _startHeight + y, transform.position.z);
        }
    }
}