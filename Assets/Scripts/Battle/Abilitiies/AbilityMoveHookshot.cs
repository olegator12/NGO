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

        [Header("Links")]
        [SerializeField] private CharacterMovement _charMovement;
        [SerializeField] private LineRenderer _lineRenCircle;
        [SerializeField] private LineRenderer _lineRenShot;
        
        [Header("Debugging")]
        public bool Debugging;
        [SerializeField] private GameObject _testPrefab;

        [SerializeField] private bool _executing;
        [SerializeField] private Vector3 _hookShotTargetPos;

        private void Update()
        {
            if (IsOwner)
            {
                if (Input.GetKeyDown(KeyCode.F) && CanFire)
                {
                    _hookShotTargetPos = FireWeapons();
                }
                
                _lineRenShot.SetPosition(0, transform.position);
                //_lineRenShot.SetPosition(1, _hookShotTargetPos);

                if (_hookShotTargetPos != Vector3.zero)
                {
                    Vector3 moveVector3 = ExecuteHookshot(_hookShotTargetPos);
                    _charMovement._controller.Move(moveVector3);
                }
            }
        }

        public Vector3 FireWeapons()
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
                Instantiate(_hookshotObject, hit.point, Quaternion.identity);
                
                VisualizeFireHookshot(hit);

                _executing = true;
                return hit.point;
            }
            else
            {
                _executing = false;
                return Vector3.zero;
            }
        }

        public Vector3 ExecuteHookshot(Vector3 hookshotTargetPos)
        {
            if (Vector3.Distance(hookshotTargetPos, transform.position) < 1.5f)
            {
                _hookShotTargetPos = Vector3.zero;

                StopeExecuteHookshot();
                
                return Vector3.zero;
            }
            
            Vector3 hookshotDir = (hookshotTargetPos - transform.position).normalized;

            float effectiveSpeed = Mathf.Clamp(Vector3.Distance(hookshotTargetPos, transform.position) * _hooksotSpeed,
                _minHooksotSpeed, _maxHooksotSpeed) * Time.deltaTime;
            
            return hookshotDir * effectiveSpeed;
        }
        
        private void VisualizeFireHookshot(RaycastHit hit)
        {
            _lineRenCircle.enabled = true;
            _lineRenShot.enabled = true;
            _lineRenShot.SetPosition(0, transform.position);
            _lineRenShot.SetPosition(1, hit.point);
        }
        
        private void StopeExecuteHookshot()
        {
            _lineRenCircle.enabled = false;
            _lineRenShot.enabled = false;
        }
    }
    
    
}