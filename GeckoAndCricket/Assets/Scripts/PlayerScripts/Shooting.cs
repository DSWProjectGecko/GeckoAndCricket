using UnityEngine;

namespace PlayerScripts
{
    public class Shooting : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        public Transform bulletPoint;
        public GameObject bullet;
        
        private Transform _aimT;

        public float bulletSpeed = 50;

        // Start is called before the first frame update
        private void Awake()
        {
            _aimT = transform.Find("Aim");
        }

        // Update is called once per frame
        private void Update()
        {
            AimingAndShooting();
        }

        private void AimingAndShooting()
        {
            Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f;

            Vector3 aimDirection = (mousePosition - transform.position).normalized;
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            _aimT.eulerAngles = new Vector3(0, 0, angle);

            if (!Input.GetMouseButtonDown(0)) return;

            GameObject bulletClone = Instantiate(bullet);
            bulletClone.transform.position = bulletPoint.transform.position;
            bulletClone.transform.rotation = Quaternion.Euler(0, 0, angle);

            bulletClone.GetComponent<Rigidbody2D>().velocity = bulletPoint.right * bulletSpeed;
        }
    }
}
