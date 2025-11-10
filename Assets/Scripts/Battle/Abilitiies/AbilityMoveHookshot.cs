using System;
using Unity.Netcode;
using UnityEngine;

namespace Triwoinmag
{
    public class AbilityMoveHookshot : NetworkBehaviour
    {
        [field: SerializeField] public bool CanFire { get; private set; }

        public float MaxDistanceToTarget = 100f;
        [SerializeField] private LayerMask _layerMask = new LayerMask();
        [SerializeField] private GameObject _hookshotObject;
        [SerializeField] private float _hooksotSpeed = 5f;
        [SerializeField] private float _minHooksotSpeed = 3f;
        [SerializeField] private float _maxHooksotSpeed = 30f;

        [SerializeField]
        private Vector3 _momentumAterHookshotAborted;
        private Vector3 _momentumAterHookshotFinished;

        [Header("Links")]
        [SerializeField] private CharacterMovement _charMovement;
        [SerializeField] private LineRenderer _lineRenCircle;
        [SerializeField] private LineRenderer _lineRenShot;
        
        [Header("Debugging")]
        public bool Debugging;
        [SerializeField] private GameObject _testPrefab;

        private void Start()
        {
            _charMovement.StartExecutingHookshot += VisualizeFireHookshotServerRpc;
            _charMovement.StopExecutingHookshot += VisualizeStopExecuteHookshotServerRpc;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _charMovement.StartExecutingHookshot -= VisualizeFireHookshotServerRpc;
            _charMovement.StopExecutingHookshot -= VisualizeStopExecuteHookshotServerRpc;
        }

        private void Update()
        {
            if (IsOwner)
            {
                if (Input.GetKeyDown(KeyCode.F) && CanFire)
                {
                    _charMovement.SwitchHookshot();
                }
                
                _lineRenShot.SetPosition(0, transform.position);
                //_lineRenShot.SetPosition(1, _hookShotTargetPos);
                
            }
        }

        public Vector3 CheckFireHookshot()
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));

            if (Physics.Raycast(ray, out hit, MaxDistanceToTarget, _layerMask))
            {
                if (Debugging)
                {
                    Debug.Log(
                        $"FireWeapons. Object: {hit.transform.gameObject.name} ray.origin: {ray.origin}, hit.point: {hit.point}");
                    //Debug.DrawRay(ray.origin, Camera.main.transform.TransformDirection(Vector3.forward) * hit.distance, Color.red, 3.0f);
                    //Instantiate(_testPrefab, hit.point, Quaternion.identity);
                }
                var hookshotDir = (hit.point - transform.position).normalized;
                _momentumAterHookshotFinished = hookshotDir * 1.5f;
                _momentumAterHookshotFinished.y = Mathf.Clamp(_momentumAterHookshotFinished.y + 0.5f, 0.1f, 0.6f);
                
                return hit.point;
            }
            else
            {
                return Vector3.zero;
            }
        }

        public Vector3 ExecuteHookshot(Vector3 hookshotTargetPos)
        {
            VisualizeStopExecutingHookshotServerRpc();
            if (Vector3.Distance(hookshotTargetPos, transform.position) < 1.5f)
            {
                VisualizeStopExecuteHookshotServerRpc();
                
                return Vector3.zero;
            }
            
            Vector3 hookshotDir = (hookshotTargetPos - transform.position).normalized;
            hookshotDir.y = _momentumAterHookshotFinished.y / 2;

            float effectiveSpeed = Mathf.Clamp(Vector3.Distance(hookshotTargetPos, transform.position) * _hooksotSpeed,
                _minHooksotSpeed, _maxHooksotSpeed) * Time.deltaTime;
            
            return hookshotDir * effectiveSpeed;
        }

        public Vector3 CalculateMomentumAfterHookshot(bool aborted)
        {
            if (aborted)
            {
                _momentumAterHookshotAborted = Camera.main.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f)).direction.normalized * 2.5f;
                return _momentumAterHookshotAborted;
            }
            else
            {
                return _momentumAterHookshotFinished;
            }
        }

        [Rpc(SendTo.Server)]
        private void VisualizeFireHookshotServerRpc(Vector3 hitPos)
        {
            var hookshotNetObj = Instantiate(_hookshotObject, hitPos, Quaternion.identity).GetComponent<NetworkObject>();
            hookshotNetObj.Spawn();
            
            VisualizeFireHookshotClientRpc(hitPos);
        }
        
        [Rpc(SendTo.ClientsAndHost)]
        private void VisualizeFireHookshotClientRpc(Vector3 hitPos)
        {
            _lineRenCircle.enabled = true;
            _lineRenShot.enabled = true;
            _lineRenShot.SetPosition(0, transform.position);
            _lineRenShot.SetPosition(1, hitPos);
        }
        
        [Rpc(SendTo.Server)]
        private void VisualizeStopExecuteHookshotServerRpc()
        {
            VisualizeStopExecuteHookshotClientRpc();
        }
        
        [Rpc(SendTo.ClientsAndHost)]
        private void VisualizeStopExecuteHookshotClientRpc()
        {
            _lineRenCircle.enabled = false;
            _lineRenShot.enabled = false;
        }
        
        [Rpc(SendTo.Server)]
        private void VisualizeStopExecutingHookshotServerRpc()
        {
            VisualizeStopExecutingHookshotClientRpc();
        }
        
        [Rpc(SendTo.ClientsAndHost)]
        private void VisualizeStopExecutingHookshotClientRpc()
        {
            _lineRenShot.SetPosition(0, transform.position);
            //_lineRenShot.SetPosition(1, _hookShotTargetPos);
        }
    }
    
    
}