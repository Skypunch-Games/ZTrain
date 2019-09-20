#if OOTII_AYMP || OOTII_SSMP
using com.ootii.Actors.AnimationControllers;
using com.ootii.Actors.Combat;
using com.ootii.Actors.Inventory;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace com.ootii.Demos
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(MotionController))]
    [RequireComponent(typeof(Combatant))]
    [RequireComponent(typeof(BasicInventory))]
    //[RequireComponent(typeof(NavMeshAgentBridge))]
    public class DemoNPCController : MonoBehaviour
    {
        public bool IsActive = true;

        [Tooltip("The target to attack")]
        public Combatant Target = null;

        public float StopDistance = 2.0f;
        public float SlowDistance = 4.0f;

        
        [Header("Attack")]
        /// <summary>
        /// Determines if our character attacks
        /// </summary>
        [Tooltip("Can the NPC attack?")]
        public bool Attack = true;        

        /// <summary>
        /// Minimum delay between attacks
        /// </summary>
        [Tooltip("The minimum delay between attacks")]
        public float AttackDelay = 2.5f;

        [Header("Defence")]
        [Tooltip("Can the NPC block?")]
        public bool Block = false;

        [Tooltip("How long to hold the block for")]
        public float BlockHold = 2f;

        protected NavMeshAgent mNavMeshAgent = null;
        protected MotionController mMotionController = null;        
        protected Combatant mCombatant = null;
        protected BasicInventory mBasicInventory = null;

        protected bool mDoMove = false;

       
        // Add a delay for the equip so it isn't called repeatedly       
        private float mLastEquipTime = -5f;
        // Time since the last attack
        private float mLastAttackTime = -5f;

        protected virtual void Awake()
        {
            mNavMeshAgent = GetComponent<NavMeshAgent>();
            mMotionController = GetComponent<MotionController>();            
            mCombatant = GetComponent<Combatant>();
            mBasicInventory = GetComponent<BasicInventory>();
        }

        protected virtual void Start()
        {
            if (Target == null)
            {
                var lPlayer = GameObject.FindGameObjectWithTag("Player");
                Target = lPlayer.GetComponent<Combatant>();
            }
            MoveToTarget();
        }

        protected virtual void Update()
        {
            if (!IsActive) return;
            MotionControllerMotion lMotion = mMotionController.ActiveMotion;
            if (lMotion == null) { return; }

            // Ensure the weapon is equipped
            if (mCombatant.PrimaryWeapon == null)
            {
                EquipWeapon();
                return;              
            }
            

            Vector3 lToTarget = Target._Transform.position - transform.position;
            lToTarget.y = 0f;

            Vector3 lToTargetDirection = lToTarget.normalized;
            float lToTargetDistance = lToTarget.magnitude;
            float lRange = mCombatant.MinMeleeReach + mCombatant.PrimaryWeapon.MaxRange;

            if (lToTargetDistance < StopDistance)
            {
                mDoMove = false;
            }
            else
            {
                mDoMove = true;
                
            }

            if (mDoMove)
            {
                MoveToTarget();
            }
            else
            {                
                mNavMeshAgent.isStopped = true;
            }    
            
          
                // Attack with the sword
            if (Attack && lMotion.Category == EnumMotionCategories.IDLE && (mLastAttackTime + AttackDelay < Time.time))//&& lMotion.Category == EnumMotionCategories.IDLE
            {
                   
                    CombatMessage lMessage = CombatMessage.Allocate();
                    lMessage.ID = CombatMessage.MSG_COMBATANT_ATTACK;
                    lMessage.Attacker = gameObject;
                    lMessage.Defender = Target.gameObject;

                    mMotionController.SendMessage(lMessage);
                    CombatMessage.Release(lMessage);

                    mLastAttackTime = Time.time;                   
            }
            // Block with shield
            else if (Block && lMotion.Category == EnumMotionCategories.IDLE && lMotion.Age > 0.5f)
            {
                CombatMessage lMessage = CombatMessage.Allocate();
                lMessage.ID = CombatMessage.MSG_COMBATANT_BLOCK;
                lMessage.Attacker = null;
                lMessage.Defender = gameObject;

                mMotionController.SendMessage(lMessage);
                CombatMessage.Release(lMessage);
            }
            // Release the shield block
            else if (lMotion.Category == EnumMotionCategories.COMBAT_MELEE_BLOCK && (lToTargetDistance > lRange + 1f || lMotion.Age > BlockHold))
            {
                CombatMessage lMessage = CombatMessage.Allocate();
                lMessage.ID = CombatMessage.MSG_COMBATANT_CANCEL;
                lMessage.Attacker = null;
                lMessage.Defender = gameObject;

                mMotionController.SendMessage(lMessage);
                CombatMessage.Release(lMessage);
            }



            // If we're dead, we can just stop
            if (lMotion.Category == EnumMotionCategories.DEATH)
            {
                IsActive = false;
            }
            // Clear the target if they are dead
            else if (Target != null && !Target.enabled)
            {
                Target = null;
                StartCoroutine(WaitAndStoreEquipment(2f));
            }

        }

        protected virtual void MoveToTarget()
        {
            if (Target == null || Target._Transform == null) return;

            //mNavMeshAgent.speed = 1.0f;
            mNavMeshAgent.isStopped = false;
            mNavMeshAgent.SetDestination(Target._Transform.position);
        }

        protected virtual void EquipWeapon()
        {
            if (mLastEquipTime + AttackDelay < Time.time)
            {
                mBasicInventory.EquipWeaponSet();
                mLastEquipTime = Time.time;
            }
        }

        /// <summary>
        /// Time to wait before storing the weapon
        /// </summary>
        /// <param name="rSeconds">Seconds to wait</param>
        private IEnumerator WaitAndStoreEquipment(float rSeconds)
        {
            if (mCombatant.PrimaryWeapon != null)
            {
                yield return new WaitForSeconds(rSeconds);
                mBasicInventory.StoreWeaponSet();
            }
        }
    }
}
#endif