﻿//#define USE_MOUNT_POINTS

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.ootii.Actors.AnimationControllers;
using com.ootii.Actors.Combat;
using com.ootii.Actors.LifeCores;
using com.ootii.Geometry;
using com.ootii.Helpers;
using com.ootii.Input;
using com.ootii.Messages;

namespace com.ootii.Actors.Inventory
{
    /// <summary>
    /// Creates a simple inventory system. That is both the "InventorySource" and the
    /// character inventory.
    /// 
    /// If you use a more advanced inventory system, simply create an "InventorySource" 
    /// that represents a bridge for your system.
    /// </summary>
    public class BasicInventory : MonoBehaviour, IInventorySource, IWeaponSetSource
    {
        /// <summary>
        /// The default Basic Inventory slot names (used in official Motion Packs, etc)
        /// </summary>
        public static class DefaultSlotNames
        {
            public static string RightHand = "RIGHT_HAND";
            public static string LeftHand = "LEFT_HAND";
            public static string LeftLowerArm = "LEFT_LOWER_ARM";
            public static string ReadyProjectile = "READY_PROJECTILE";
        }

        /// <summary>
        /// Properties that are critical for other packages
        /// </summary>
        public static string[] Properties = new string[] { "resourcepath", "instance", "quantity" };

        /// <summary>
        /// Provides an easy way to get the bone names
        /// </summary>
        public static string[] UnityBones = null;

        /// <summary>
        /// List of inventory items
        /// </summary>
        public List<BasicInventoryItem> Items = new List<BasicInventoryItem>();

        /// <summary>
        /// List of slots with items
        /// </summary>
        public List<BasicInventorySlot> Slots = new List<BasicInventorySlot>();

        /// <summary>
        /// Item sets that represent weapons that could be readied at the same time. 
        /// Ex: sword + shield, bow + arrow, dagger (rh) + dagger (lh), etc.
        /// </summary>
        public List<BasicInventorySet> WeaponSets = new List<BasicInventorySet>();

        /// <summary>
        /// Determines if we'll process the updates and input changes
        /// </summary>
        public bool _IsEnabled = true;
        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set { _IsEnabled = value; }
        }

        /// <summary>
        /// GameObject that owns the IInputSource we really want
        /// </summary>
        public GameObject _InputSourceOwner = null;
        public GameObject InputSourceOwner
        {
            get { return _InputSourceOwner; }
            set { _InputSourceOwner = value; }
        }

        /// <summary>
        /// Defines the source of the input that we'll use to control
        /// the character movement, rotations, and animations.
        /// </summary>
        [NonSerialized]
        public IInputSource _InputSource = null;
        public IInputSource InputSource
        {
            get { return _InputSource; }
            set { _InputSource = value; }
        }

        /// <summary>
        /// Determines if we'll auto find the input source if one doesn't exist
        /// </summary>
        public bool _AutoFindInputSource = true;
        public bool AutoFindInputSource
        {
            get { return _AutoFindInputSource; }
            set { _AutoFindInputSource = value; }
        }

        /// <summary>
        /// Determines if we use number keys to activate the specific weapon sets
        /// </summary>
        public bool _UseNumberKeys = true;
        public bool UseNumberKeys
        {
            get { return _UseNumberKeys; }
            set { _UseNumberKeys = value; }
        }

        /// <summary>
        /// Action alias to equip the current weapon set
        /// </summary>
        public string _ToggleWeaponSetAlias = "Inventory Toggle";
        public string ToggleWeaponSetAlias
        {
            get { return _ToggleWeaponSetAlias; }
            set { _ToggleWeaponSetAlias = value; }
        }

        /// <summary>
        /// Action alias to equip the prev/next weapon set in the list
        /// </summary>
        public string _ShiftWeaponSetAlias = "Inventory Shift";
        public string ShiftWeaponSetAlias
        {
            get { return _ShiftWeaponSetAlias; }
            set { _ShiftWeaponSetAlias = value; }
        }

        /// <summary>
        /// Some motions will use this to determine if they should test
        /// for activation or allow the inventory source to drive activation.
        /// </summary>
        public bool _AllowMotionSelfActivation = false;
        public virtual bool AllowMotionSelfActivation
        {
            get { return _AllowMotionSelfActivation; }
            set { _AllowMotionSelfActivation = value; }
        }

        /// <summary>
        /// Defines the weapon set that is active. In this case, active doesn't mean equipped.
        /// It could mean the weapons are equipped or it could mean the weapons are ready to be equipped
        /// when the equipped motion is played.
        /// </summary>
        public int _ActiveWeaponSet = 0;
        public virtual int ActiveWeaponSet
        {
            get { return _ActiveWeaponSet; }
            set { _ActiveWeaponSet = value; }
        }

        /// <summary>
        ///  Determines if we start by equiping the active weapon set (without running animations)
        /// </summary>
        public bool _EquipOnStart = false;
        public bool EquipOnStart
        {
            get { return _EquipOnStart; }
            set { _EquipOnStart = value; }
        }

        /// <summary>
        /// Prefent us from changing weapons if we are currently changing them.
        /// </summary>
        protected bool mIsEquippingItem = false;
        public bool IsEquippingItem
        {
            get { return mIsEquippingItem; }
        }

        /// <summary>
        /// Determine if the time is right to activate the equip/store motion
        /// </summary>
        public virtual bool IsEquipStoreAllowed
        {
            get
            {
                if (mMotionController != null)
                {
                    MotionControllerMotion lActiveMotion = mMotionController.ActiveMotion;
                    if (lActiveMotion == null) { return true; }
                    if (lActiveMotion.Category == EnumMotionCategories.IDLE) { return true; }
                    if (lActiveMotion.Category == EnumMotionCategories.WALK) { return true; }
                    if (lActiveMotion.Category == EnumMotionCategories.RUN) { return true; }
                    if (lActiveMotion is IWalkRunMotion) { return true; }

                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Event for when the component finishes an action
        /// </summary>
        public MessageEvent ItemEquippedEvent = null;

        /// <summary>
        /// Event for when the component finishes an action
        /// </summary>
        public MessageEvent ItemStoredEvent = null;

        /// <summary>
        /// Event for when the component finishes an action
        /// </summary>
        public MessageEvent WeaponSetEquippedEvent = null;

        /// <summary>
        /// Event for when the component finishes an action
        /// </summary>
        public MessageEvent WeaponSetStoredEvent = null;

        /// <summary>
        /// Motion controller associated with the character
        /// </summary>
        protected MotionController mMotionController = null;

        /// <summary>
        /// Actor core associated with the character
        /// </summary>
        protected IActorCore mActorCore = null;

#if USE_MOUNT_POINTS || OOTII_MP
        /// <summary>
        /// Mount Points associated with the character
        /// </summary>
        protected MountPoints mMountPoints = null;
#endif

        /// <summary>
        /// Once the objects are instanciated, awake is called before start. Use it
        /// to setup references to other objects
        /// </summary>
        protected virtual void Awake()
        {
            if (BasicInventory.UnityBones == null)
            {
                BasicInventory.UnityBones = System.Enum.GetNames(typeof(HumanBodyBones));
                for (int i = 0; i < BasicInventory.UnityBones.Length; i++)
                {
                    BasicInventory.UnityBones[i] = StringHelper.CleanString(BasicInventory.UnityBones[i]);
                }
            }

            // Object that will provide access to the keyboard, mouse, etc
            if (_InputSourceOwner != null) { _InputSource = InterfaceHelper.GetComponent<IInputSource>(_InputSourceOwner); }

            // If the input source is still null, see if we can grab a local input source
            if (_AutoFindInputSource && _InputSource == null)
            {
                _InputSource = InterfaceHelper.GetComponent<IInputSource>(gameObject);
                if (_InputSource != null) { _InputSourceOwner = gameObject; }
            }

            // If that's still null, see if we can grab one from the scene. This may happen
            // if the MC was instanciated from a prefab which doesn't hold a reference to the input source
            if (_AutoFindInputSource && _InputSource == null)
            {
                IInputSource[] lInputSources = InterfaceHelper.GetComponents<IInputSource>();
                for (int i = 0; i < lInputSources.Length; i++)
                {
                    GameObject lInputSourceOwner = ((MonoBehaviour)lInputSources[i]).gameObject;
                    if (lInputSourceOwner.activeSelf && lInputSources[i].IsEnabled)
                    {
                        _InputSource = lInputSources[i];
                        _InputSourceOwner = lInputSourceOwner;
                    }
                }
            }

            // Grab the motion controller
            mMotionController = gameObject.GetComponent<MotionController>();

            // Grab the actor core
            mActorCore = gameObject.GetComponent<IActorCore>(); 

#if USE_MOUNT_POINTS || OOTII_MP
            // Grab the mount points
            mMountPoints = gameObject.GetComponent<MountPoints>();
#endif
        }

        /// <summary>
        /// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time.
        /// </summary>
        protected virtual void Start()
        {
            if (WeaponSets.Count == 0) { _ActiveWeaponSet = -1; }
            if (_ActiveWeaponSet >= WeaponSets.Count) { _ActiveWeaponSet = 0; }

            // Cycle through all the items with GameObjects attached and store the
            // parent information.
            for (int i = 0; i < Items.Count; i++)
            {
                BasicInventoryItem lItem = Items[i];
                if (lItem.Instance != null)
                {
                    lItem.DestroyOnStore = false;
                    lItem.StoredParent = lItem.Instance.transform.parent;
                    lItem.StoredPosition = lItem.Instance.transform.localPosition;
                    lItem.StoredRotation = lItem.Instance.transform.localRotation;
                }
            }

            // Equip the default weapon set
            if (_ActiveWeaponSet >= 0 && _EquipOnStart)
            {
                EquipWeaponSet(_ActiveWeaponSet);
            }
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        protected virtual void Update()
        {
            bool lTestInput = (WeaponSets != null && WeaponSets.Count > 0);
            if (lTestInput) { lTestInput = mMotionController.IsGrounded; }
            if (lTestInput) { lTestInput = IsEquipStoreAllowed; }

            if (_InputSource != null && _InputSource.IsEnabled && !mIsEquippingItem)
            {
                if (lTestInput && _UseNumberKeys)
                {
                    for (int i = 0; i < Mathf.Min(WeaponSets.Count, 8); i++)
                    {
                        if (i < WeaponSets.Count && WeaponSets[i].IsEnabled)
                        {
                            if (_InputSource.IsJustPressed(KeyCode.Alpha1 + i))
                            {
                                lTestInput = false;
                                _ActiveWeaponSet = i;
                                StartCoroutine(SwapWeaponSet(i));
                            }
                        }
                    }
                }

                if (lTestInput && _ToggleWeaponSetAlias.Length > 0)
                {
                    float lToggle = _InputSource.GetValue(_ToggleWeaponSetAlias);
                    if (lToggle != 0f)
                    {
                        lTestInput = false;
                        StartCoroutine(SwapWeaponSet(_ActiveWeaponSet));
                    }
                }

                if (lTestInput && _ShiftWeaponSetAlias.Length > 0)
                {
                    float lShift = _InputSource.GetValue(_ShiftWeaponSetAlias);
                    if (lShift != 0f)
                    {
                        int lWeaponSetIndex = _ActiveWeaponSet;

                        for (int i = 0; i < 10; i++)
                        {
                            lWeaponSetIndex = lWeaponSetIndex + (lShift < -0.1f ? -1 : (lShift > 0.1f ? 1 : 0));
                            if (lWeaponSetIndex < 0) { lWeaponSetIndex = WeaponSets.Count - 1; }
                            else if (lWeaponSetIndex >= WeaponSets.Count) { lWeaponSetIndex = 0; }

                            if (WeaponSets[lWeaponSetIndex].IsEnabled) { break; }
                            if (lWeaponSetIndex == _ActiveWeaponSet) { break; }
                        }

                        if (lWeaponSetIndex != _ActiveWeaponSet)
                        {
                            lTestInput = false;
                            _ActiveWeaponSet = lWeaponSetIndex;
                            StartCoroutine(SwapWeaponSet(_ActiveWeaponSet));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Instantiates the specified item and equips it. We return the instantiated item.
        /// </summary>
        /// <param name="rItemID">String representing the name or ID of the item to equip</param>
        /// <param name="rSlotID">String representing the name or ID of the slot to equip</param>
        /// <param name="rResourcePath">Alternate resource path to override the ItemID's</param>
        /// <returns>GameObject that is the instance or null if it could not be created</returns>
        public virtual GameObject EquipItem(string rItemID, string rSlotID, string rResourcePath = "")
        {
            BasicInventoryItem lItem = GetInventoryItem(rItemID);
            if (lItem == null) { return null; }

            string lResourcePath = rResourcePath;
            if (lResourcePath.Length == 0) { lResourcePath = lItem.ResourcePath; }

            Quaternion lLocalRotation = Quaternion.Euler(lItem.LocalRotationEuler);
            if (lLocalRotation == Quaternion.identity && lItem.LocalRotation != Quaternion.identity)
            {
                lLocalRotation = lItem.LocalRotation;
                lItem.LocalRotationEuler = lItem.LocalRotation.eulerAngles;
            }

            if (lItem.Instance == null)
            {
                GameObject lGameObject = CreateAndMountItem(gameObject, lResourcePath, lItem.LocalPosition, lLocalRotation, rSlotID);
                if (lGameObject != null) { lItem.Instance = lGameObject; }
            }
            else
            {
                MountItem(gameObject, lItem.Instance, lItem.LocalPosition, lLocalRotation, rSlotID);
            }

            if (lItem.Instance != null)
            {
                IItemCore lItemCore = lItem.Instance.GetComponent<IItemCore>();
                if (lItemCore != null) { lItemCore.OnEquipped(); }

                BasicInventorySlot lSlot = GetInventorySlot(rSlotID);
                if (lSlot != null) { lSlot.ItemID = rItemID; }

                // Send the message
                InventoryMessage lMessage = InventoryMessage.Allocate();
                lMessage.ID = EnumMessageID.MSG_INVENTORY_ITEM_EQUIPPED;
                lMessage.InventorySource = this;
                lMessage.ItemID = rItemID;
                lMessage.SlotID = rSlotID;
                lMessage.Form = (lItem != null ? lItem.EquipStyle : 0);
                lMessage.WeaponSetID = null;

                if (ItemEquippedEvent != null)
                {
                    ItemEquippedEvent.Invoke(lMessage);
                }

                if (mActorCore != null)
                {
                    mActorCore.SendMessage(lMessage);
                }

#if USE_MESSAGE_DISPATCHER || OOTII_MD
                MessageDispatcher.SendMessage(lMessage);
#endif

                InventoryMessage.Release(lMessage);
            }

            return lItem.Instance;
        }

        /// <summary>
        /// Instantiates the specified item and equips it. We return the instantiated item.
        /// </summary>
        /// <param name="rSlotID">String representing the name or ID of the slot to clear</param>
        public virtual void StoreItem(string rSlotID)
        {
            int lSlotIndex = -1;
            for (int i = 0; i < Slots.Count; i++)
            {
                if (Slots[i].ID == rSlotID)
                {
                    lSlotIndex = i;
                    break;
                }
            }

            if (lSlotIndex < 0) { return; }

            BasicInventorySlot lSlot = Slots[lSlotIndex];
            if (lSlot == null) { return; }

            BasicInventoryItem lItem = null;
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].ID == lSlot.ItemID)
                {
                    lItem = Items[i];
                    break;
                }
            }

            // We need to disconnect the item, but we may need to destroy it as well
            if (lItem != null && lItem.Instance != null)
            {
                IItemCore lItemCore = lItem.Instance.GetComponent<IItemCore>();
                if (lItemCore != null) { lItemCore.OnStored(); }

                // If we know about a combatant, disconnect the weapon
                ICombatant lCombatant = gameObject.GetComponent<ICombatant>();
                if (lCombatant != null)
                {
                    IWeaponCore lWeaponCore = lItem.Instance.GetComponent<IWeaponCore>();
                    if (lWeaponCore != null)
                    {
                        if (lCombatant.PrimaryWeapon == lWeaponCore) { lCombatant.PrimaryWeapon = null; }
                        if (lCombatant.SecondaryWeapon == lWeaponCore) { lCombatant.SecondaryWeapon = null; }
                    }
                }

#if USE_MOUNT_POINTS || OOTII_MP

                if (mMountPoints != null)
                {
                    MountPoint lParentMountPoint = mMountPoints.GetMountPoint(lSlot.ID);
                    if (lParentMountPoint != null) { mMountPoints.DisconnectMountPoints(lParentMountPoint, lItem.Instance); }
                }

#endif

                // Without a stored parent, we destroy it
                if (lItem.StoredParent == null)
                {
                    if (lItem.DestroyOnStore)
                    {
                        GameObject.Destroy(lItem.Instance);
                        lItem.Instance = null;
                    }
                    else
                    {
                        lItem.Instance.SetActive(false);
                        lItem.Instance.hideFlags = HideFlags.HideInHierarchy;
                    }
                }
                else
                {
                    bool lIsAttached = false;

#if USE_MOUNT_POINTS || OOTII_MP

                    // See if we can attach it using a mount point
                    if (mMountPoints != null)
                    {
                        MountPoint lParentMountPoint = mMountPoints.GetMountPoint(lItem.StoredParent.name);
                        if (lParentMountPoint == null) { lParentMountPoint = mMountPoints.GetMountPoint(lItem.StoredParent); }
                        if (lParentMountPoint != null)
                        {
                            lIsAttached = mMountPoints.ConnectMountPoints(lParentMountPoint, lItem.Instance, "Handle");
                        }
                    }

#endif

                    if (!lIsAttached)
                    {
                        lItem.Instance.transform.parent = lItem.StoredParent;
                        lItem.Instance.transform.localPosition = lItem.StoredPosition;
                        lItem.Instance.transform.localRotation = lItem.StoredRotation;
                    }
                }

                // Send the message
                InventoryMessage lMessage = InventoryMessage.Allocate();
                lMessage.ID = EnumMessageID.MSG_INVENTORY_ITEM_STORED;
                lMessage.InventorySource = this;
                lMessage.ItemID = (lItem != null ? lItem.ID : null);
                lMessage.SlotID = rSlotID;
                lMessage.Form = (lItem != null ? lItem.StoreStyle : 0);
                lMessage.WeaponSetID = null;

                if (ItemStoredEvent != null)
                {
                    ItemStoredEvent.Invoke(lMessage);
                }

                if (mActorCore != null)
                {
                    mActorCore.SendMessage(lMessage);
                }

#if USE_MESSAGE_DISPATCHER || OOTII_MD
                MessageDispatcher.SendMessage(lMessage);
#endif

                InventoryMessage.Release(lMessage);
            }

            lSlot.ItemID = "";
        }

        /// <summary>
        /// Retrieves the item id for the item that is in the specified slot. If no item is slotted, returns an empty string.
        /// </summary>
        /// <param name="rSlotID">String representing the name or ID of the slot we're checking</param>
        /// <returns>ID of the item that is in the slot or the empty string</returns>
        public virtual string GetItemID(string rSlotID, bool rRequireIsEquipped = true)
        {
            for (int i = 0; i < Slots.Count; i++)
            {
                if (Slots[i].ID == rSlotID)
                {
                    return Slots[i].ItemID;
                }
            }

            return "";
        }

        /// <summary>
        /// Retrieves a specific item's property value.
        /// </summary>
        /// <typeparam name="T">Type of property being retrieved</typeparam>
        /// <param name="rItemID">String representing the name or ID of the item whose property we want.</param>
        /// <param name="rPropertyID">String representing the name or ID of the property whose value we want.</param>
        /// <returns>Value of the property or the type's default</returns>
        public virtual T GetItemPropertyValue<T>(string rItemID, string rPropertyID)
        {
            string lPropertyID = rPropertyID.Replace(" ", string.Empty);

            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].ID == rItemID)
                {
                    // Resource Path
                    if (string.Compare(lPropertyID, BasicInventory.Properties[0], true) == 0)
                    {
                        if (typeof(T) == typeof(string))
                        {
                            string lValue = Items[i].ResourcePath;
                            return (T)(object)lValue;
                        }
                    }
                    // Instance
                    else if (string.Compare(lPropertyID, BasicInventory.Properties[1], true) == 0)
                    {
                        if (typeof(T) == typeof(GameObject))
                        {
                            GameObject lValue = Items[i].Instance;
                            if (lValue != null) { return (T)(object)lValue; }
                        }
                    }
                    // Quantity
                    else if (string.Compare(lPropertyID, BasicInventory.Properties[2], true) == 0)
                    {
                        if (typeof(T) == typeof(int))
                        {
                            int lValue = Items[i].Quantity;
                            return (T)(object)lValue;
                        }
                    }

                    return default(T);
                }
            }

            return default(T);
        }

        /// <summary>
        /// Sets a specific item's property value.
        /// </summary>
        /// <typeparam name="T">Type of property being retrieved</typeparam>
        /// <param name="rItemID">String representing the name or ID of the item whose property we want.</param>
        /// <param name="rPropertyID">String representing the name or ID of the property whose value we want.</param>
        /// <param name="rValue">Value that we're going to set</param>
        public virtual void SetItemPropertyValue<T>(string rItemID, string rPropertyID, T rValue)
        {
            string lPropertyID = rPropertyID.Replace(" ", string.Empty);

            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].ID == rItemID)
                {
                    // Resource Path
                    if (string.Compare(lPropertyID, BasicInventory.Properties[0], true) == 0)
                    {
                        if (typeof(T) == typeof(string))
                        {
                            Items[i].ResourcePath = Convert.ToString(rValue);
                        }
                    }
                    // Instance
                    else if (string.Compare(lPropertyID, BasicInventory.Properties[1], true) == 0)
                    {
                        if (typeof(T) == typeof(GameObject))
                        {
                            Items[i].Instance = Convert.ChangeType(rValue, typeof(GameObject)) as GameObject;
                        }
                    }
                    // Quantity
                    else if (string.Compare(lPropertyID, BasicInventory.Properties[2], true) == 0)
                    {
                        if (typeof(T) == typeof(int))
                        {
                            Items[i].Quantity = Convert.ToInt32(rValue);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Given the specified slot ID, grab the slot associated with it.
        /// </summary>
        /// <param name="rSlotID">String representing the name or ID of the slot we want</param>
        /// <returns>BasicInventorySlot matching the slot ID or null.</returns>
        public virtual BasicInventorySlot GetInventorySlot(string rSlotID)
        {
            for (int i = 0; i < Slots.Count; i++)
            {
                if (Slots[i].ID == rSlotID)
                {
                    return Slots[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Given the specified item ID, grab the item associated with it.
        /// </summary>
        /// <param name="rItemID">String representing the name or ID of the item we want</param>
        /// <returns>BasicInventoryItem matching the itme ID or null</returns>
        public virtual BasicInventoryItem GetInventoryItem(string rItemID)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].ID == rItemID)
                {
                    return Items[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Given the specified set ID, grab the set associated with it.
        /// </summary>
        /// <param name="rItemID">String representing the name or ID of the set we want</param>
        /// <returns>BasicInventorySet matching the ID or null</returns>
        public virtual BasicInventorySet GetWeaponSet(string rSetID)
        {
            rSetID = StringHelper.CleanString(rSetID);
            for (int i = 0; i < WeaponSets.Count; i++)
            {
                if (StringHelper.CleanString(WeaponSets[i].ID) == rSetID)
                {
                    return WeaponSets[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if the specified weapon set is equipped
        /// </summary>
        /// <param name="rIndex">Index of the weapon set to check</param>
        /// <returns>Boolean that determines if all the items are equipped</returns>
        public virtual bool IsWeaponSetEquipped(int rIndex = -1)
        {
            if (rIndex < 0) { rIndex = _ActiveWeaponSet; }
            if (rIndex < 0 || rIndex >= WeaponSets.Count) { return false; }

            BasicInventorySet lSet = WeaponSets[rIndex];
            for (int i = 0; i < lSet.Items.Count; i++)
            {
                for (int j = 0; j < Slots.Count; j++)
                {
                    if (Slots[j].ID == lSet.Items[i].SlotID)
                    {
                        if (Slots[j].ItemID != lSet.Items[i].ItemID) { return false; }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Equips the specified weapon set. If it is already equipped, nothing happens.
        /// </summary>
        /// <param name="rIndex">Index of the weapon set to toggle or empty to toggle the active one.</param>
        public virtual void EquipWeaponSet(int rIndex = -1)
        {
            if (rIndex < 0) { rIndex = _ActiveWeaponSet; }
            StartCoroutine(Internal_EquipWeaponsSet(rIndex));
        }

        /// <summary>
        /// Stores the specified weapon set. If it isn't equipped, nothing happens.
        /// </summary>
        /// <param name="rIndex">Index of the weapon set to toggle or empty to toggle the active one.</param>
        public virtual void StoreWeaponSet(int rIndex = -1)
        {
            if (rIndex < 0) { rIndex = _ActiveWeaponSet; }
            StartCoroutine(Internal_StoreWeaponsSet(rIndex));
        }

        /// <summary>
        /// Equips/stores the specified weapon set. If it isn't equipped, it will be equipped. If it
        /// is equipped, it will be stored.
        /// </summary>
        /// <param name="rIndex">Index of the weapon set to toggle or empty to toggle the active one.</param>
        public virtual void ToggleWeaponSet(int rIndex = -1)
        {
            if (rIndex < 0) { rIndex = _ActiveWeaponSet; }
            StartCoroutine(SwapWeaponSet(rIndex));
        }

        /// <summary>
        /// Swaps the current weapons set items out for a new set
        /// </summary>
        /// <param name="rIndex">New weapont set index</param>
        protected virtual IEnumerator SwapWeaponSet(int rIndex)
        {
            if (!mIsEquippingItem && WeaponSets != null && WeaponSets.Count > rIndex)
            {
                mIsEquippingItem = true;

                bool lEquipItems = false;

                // Cycle through each item in the set and store each item that isn't part of the new set
                for (int i = 0; i < WeaponSets[rIndex].Items.Count; i++)
                {
                    BasicInventorySlot lSlot = GetInventorySlot(WeaponSets[rIndex].Items[i].SlotID);
                    if (lSlot != null)
                    {
                        // If no weapon is equipped, we need to equip it
                        if (lSlot.ItemID.Length == 0 && WeaponSets[rIndex].Items[i].ItemID.Length > 0)
                        {
                            lEquipItems = true;
                        }
                        // If we are changing weapons, unequip the weapon
                        else if (lSlot.ItemID != WeaponSets[rIndex].Items[i].ItemID)
                        {
                            // Flag the fact that we want to equip the group
                            lEquipItems = true;

                            // Store the item in the slot
                            yield return StartCoroutine(Internal_StoreItem(lSlot.ID, true));

                            // Clear the item
                            lSlot.ItemID = "";
                        }
                    }
                }

                // Equip all the set items
                if (lEquipItems)
                {
                    yield return StartCoroutine(Internal_EquipWeaponsSet(rIndex));
                }
                // Store all the set items
                else
                {
                    yield return StartCoroutine(Internal_StoreWeaponsSet(rIndex));
                }

                mIsEquippingItem = false;

                _ActiveWeaponSet = rIndex;
            }
        }

        /// <summary>
        /// Clears a slot by the storing the current item. We'll use the
        /// store motion if it exists
        /// </summary>
        /// <param name="rSlotID">Slot that is being cleared</param>
        /// <returns></returns>
        protected virtual IEnumerator Internal_StoreItem(string rSlotID, bool rClearStates)
        {
            bool lStanceSet = false;

            BasicInventorySlot lSlot = GetInventorySlot(rSlotID);
            if (lSlot != null)
            {
                BasicInventoryItem lItem = GetInventoryItem(lSlot.ItemID);
                if (lItem != null)
                {
                    // Run the unequip motion
                    if (lItem.StoreMotion.Length > 0)
                    {
                        // If we have a motion to unequip, activate it
                        MotionControllerMotion lMotion = mMotionController.GetMotion(lItem.StoreMotion);
                        if (lMotion != null)
                        {
                            // This is an extra test so we don't try to sheathe a weapons we just
                            // unsheathed... until we're totally done with the transitions
                            while (lMotion.MotionLayer._AnimatorTransitionID != 0)
                            {
                                yield return null;
                            }

                            IEquipStoreMotion lEquipStoreMotion = lMotion as IEquipStoreMotion;
                            if (lEquipStoreMotion != null)
                            {
                                lEquipStoreMotion.OverrideItemID = lItem.ID;
                                lEquipStoreMotion.OverrideSlotID = lSlot.ID;
                            }

                            // Now store
                            lMotion.Form = lItem.StoreStyle;
                            mMotionController.ActivateMotion(lMotion);
                            while (lMotion.IsActive || lMotion.QueueActivation)
                            {
                                yield return null;

                                // Set the stance so when we come out of the motion, we go to the right idle
                                if (!lStanceSet && rClearStates && lMotion.IsActive && lMotion.Age > 0.3f)
                                {
                                    lStanceSet = true;
                                    mMotionController.Stance = 0; 
                                    mMotionController.CurrentForm = mMotionController.DefaultForm; 
                                }
                            }
                        }
                    }
                    // Otherwise, simply unequip
                    else
                    {
                        StoreItem(lSlot.ID);
                    }
                }

                // Clear the slot
                lSlot.ItemID = "";
            }
        }

        /// <summary>
        /// Equip all items in the weapon set
        /// </summary>
        /// <param name="rIndex">Index of the weapon set whose items will be stored</param>
        protected virtual IEnumerator Internal_EquipWeaponsSet(int rIndex)
        {
            mIsEquippingItem = true;

            bool lStanceSet = false;

            // First, find all the entries with no equip motions and spawn them first
            for (int i = 0; i < WeaponSets[rIndex].Items.Count; i++)
            {
                BasicInventoryItem lItem = GetInventoryItem(WeaponSets[rIndex].Items[i].ItemID);
                if (lItem != null && lItem.EquipMotion.Length == 0)
                {
                    BasicInventorySlot lSlot = GetInventorySlot(WeaponSets[rIndex].Items[i].SlotID);
                    if (lSlot != null)
                    {
                        if (WeaponSets[rIndex].Items[i].Instantiate)
                        {
                            GameObject lInstance = EquipItem(lItem.ID, lSlot.ID);
                            if (lInstance != null)
                            {
                                lSlot.ItemID = lItem.ID;
                            }
                        }
                        else
                        {
                            lSlot.ItemID = lItem.ID;
                        }
                    }
                }
            }

            // Now, find all the entries that do have an equip motion
            for (int i = 0; i < WeaponSets[rIndex].Items.Count; i++)
            {
                BasicInventoryItem lItem = GetInventoryItem(WeaponSets[rIndex].Items[i].ItemID);
                if (lItem != null && lItem.EquipMotion.Length > 0)
                {
                    BasicInventorySlot lSlot = GetInventorySlot(WeaponSets[rIndex].Items[i].SlotID);
                    if (lSlot != null && lSlot.ItemID.Length == 0)
                    {
                        // If we have a motion to equip, activate it
                        MotionControllerMotion lMotion = mMotionController.GetMotion(lItem.EquipMotion);
                        if (lMotion != null)
                        {
                            IEquipStoreMotion lEquipStoreMotion = lMotion as IEquipStoreMotion;
                            if (lEquipStoreMotion != null)
                            {
                                lEquipStoreMotion.OverrideItemID = lItem.ID;
                                lEquipStoreMotion.OverrideSlotID = lSlot.ID;
                            }

                            // Now equip
                            lMotion.Form = lItem.EquipStyle;
                            mMotionController.ActivateMotion(lMotion);
                            while (lMotion.IsActive || lMotion.QueueActivation)
                            {
                                yield return null;

                                // Set the stance so when we come out of the motion, we go to the right idle
                                if (!lStanceSet && lMotion.IsActive && lMotion.Age > 0.3f)
                                {
                                    lStanceSet = true;
                                    if (WeaponSets[rIndex].Stance > 0) { mMotionController.Stance = WeaponSets[rIndex].Stance; }
                                    if (WeaponSets[rIndex].DefaultForm > 0) { mMotionController.CurrentForm = WeaponSets[rIndex].DefaultForm; }
                                }
                            }

                            // Set the item
                            lSlot.ItemID = lItem.ID;
                        }
                    }
                }
            }

            // Set the stance so when we come out of the motion, we go to the right idle
            if (!lStanceSet)
            {
                lStanceSet = true;
                if (WeaponSets[rIndex].Stance > 0) { mMotionController.Stance = WeaponSets[rIndex].Stance; }
                if (WeaponSets[rIndex].DefaultForm > 0) { mMotionController.CurrentForm = WeaponSets[rIndex].DefaultForm; }
            }

            // Set the final properties
            _ActiveWeaponSet = rIndex;

            // Send the message
            InventoryMessage lMessage = InventoryMessage.Allocate();
            lMessage.ID = EnumMessageID.MSG_INVENTORY_WEAPON_SET_EQUIPPED;
            lMessage.InventorySource = this;
            lMessage.ItemID = null;
            lMessage.SlotID = null;
            lMessage.WeaponSetID = WeaponSets[_ActiveWeaponSet].ID;
            lMessage.Form = WeaponSets[_ActiveWeaponSet].DefaultForm;

            if (WeaponSetEquippedEvent != null)
            {
                WeaponSetEquippedEvent.Invoke(lMessage);
            }

            if (mActorCore != null)
            {
                mActorCore.SendMessage(lMessage);
            }

#if USE_MESSAGE_DISPATCHER || OOTII_MD
            MessageDispatcher.SendMessage(lMessage);
#endif

            InventoryMessage.Release(lMessage);

            mIsEquippingItem = false;
        }

        /// <summary>
        /// Store all items in the weapon set
        /// </summary>
        /// <param name="rIndex">Index of the weapon set whose items will be stored</param>
        protected virtual IEnumerator Internal_StoreWeaponsSet(int rIndex)
        {
            mIsEquippingItem = true;

            bool lStanceSet = false;

            // First, find all the entries that do have a store motion
            for (int i = 0; i < WeaponSets[rIndex].Items.Count; i++)
            {
                BasicInventorySlot lSlot = GetInventorySlot(WeaponSets[rIndex].Items[i].SlotID);
                if (lSlot != null && lSlot.ItemID.Length > 0)
                {
                    BasicInventoryItem lItem = GetInventoryItem(lSlot.ItemID);
                    if (lItem != null && lItem.StoreMotion.Length > 0)
                    {
                        // If we have a motion to unequip, activate it
                        MotionControllerMotion lMotion = mMotionController.GetMotion(lItem.StoreMotion);
                        if (lMotion != null)
                        {
                            // This is an extra test so we don't try to sheathe a weapons we just
                            // unsheathed... until we're totally done with the transitions
                            while (lMotion.MotionLayer._AnimatorTransitionID != 0)
                            {
                                yield return null;
                            }

                            // Ensure we override the item and slot
                            IEquipStoreMotion lEquipStoreMotion = lMotion as IEquipStoreMotion;
                            if (lEquipStoreMotion != null)
                            {
                                lEquipStoreMotion.OverrideItemID = lItem.ID;
                                lEquipStoreMotion.OverrideSlotID = lSlot.ID;
                            }

                            // Now store
                            lMotion.Form = lItem.StoreStyle;
                            mMotionController.ActivateMotion(lMotion);
                            while (lMotion.IsActive || lMotion.QueueActivation)
                            {
                                yield return null;

                                // Set the stance so when we come out of the motion, we go to the right idle
                                if (!lStanceSet && lMotion.IsActive && lMotion.Age > 0.3f)
                                {
                                    lStanceSet = true;
                                    if (WeaponSets[rIndex].Stance >= 0) { mMotionController.Stance = 0; }
                                    if (WeaponSets[rIndex].DefaultForm >= 0) { mMotionController.CurrentForm = mMotionController.DefaultForm; }
                                }
                            }

                            // Clear the item
                            lSlot.ItemID = "";
                        }
                    }
                }
            }

            // Second, find all the entries with no store motions destroy them
            for (int i = 0; i < WeaponSets[rIndex].Items.Count; i++)
            {
                BasicInventorySlot lSlot = GetInventorySlot(WeaponSets[rIndex].Items[i].SlotID);
                if (lSlot != null && lSlot.ItemID.Length > 0)
                {
                    StoreItem(lSlot.ID);
                }
            }

            // Set the stance so when we come out of the motion, we go to the right idle
            if (!lStanceSet)
            {
                if (WeaponSets[rIndex].Stance >= 0) { mMotionController.Stance = 0; }
                if (WeaponSets[rIndex].DefaultForm >= 0) { mMotionController.CurrentForm = mMotionController.DefaultForm; }
            }

            // Send the message
            InventoryMessage lMessage = InventoryMessage.Allocate();
            lMessage.ID = EnumMessageID.MSG_INVENTORY_WEAPON_SET_STORED;
            lMessage.InventorySource = this;
            lMessage.ItemID = null;
            lMessage.SlotID = null;
            lMessage.WeaponSetID = WeaponSets[rIndex].ID;
            lMessage.Form = WeaponSets[rIndex].DefaultForm;

            if (WeaponSetStoredEvent != null)
            {
                WeaponSetStoredEvent.Invoke(lMessage);
            }

            if (mActorCore != null)
            {
                mActorCore.SendMessage(lMessage);
            }

#if USE_MESSAGE_DISPATCHER || OOTII_MD
            MessageDispatcher.SendMessage(lMessage);
#endif

            InventoryMessage.Release(lMessage);

            mIsEquippingItem = false;
        }

        /// <summary>
        /// Creates the item and attaches it to the parent mount point
        /// </summary>
        /// <param name="rParent">GameObject that is the parent (typically a character)</param>
        /// <param name="rResourcePath">String that is the resource path to the item</param>
        /// <param name="rLocalPosition">Position the item will have relative to the parent mount point</param>
        /// <param name="rLocalRotation">Rotation the item will have relative to the parent mount pont</param>
        /// <returns></returns>
        protected virtual GameObject CreateAndMountItem(GameObject rParent, string rResourcePath, Vector3 rLocalPosition, Quaternion rLocalRotation, string rParentMountPoint = "Left Hand", string rItemMountPoint = "Handle")
        {
            GameObject lItem = null;

            if (rResourcePath.Length > 0)
            {

#if USE_MOUNT_POINTS || OOTII_MP

                if (mMountPoints != null)
                {
                    lItem = mMountPoints.ConnectMountPoints(rParentMountPoint, rResourcePath, rItemMountPoint);
                }

#endif

                // Create and mount if we need to
                if (lItem == null)
                {
                    Animator lAnimator = rParent.GetComponentInChildren<Animator>();
                    if (lAnimator != null)
                    {
                        UnityEngine.Object lResource = Resources.Load(rResourcePath);
                        if (lResource != null)
                        {
                            lItem = GameObject.Instantiate(lResource) as GameObject;
                            MountItem(rParent, lItem, rLocalPosition, rLocalRotation, rParentMountPoint);
                        }
                        else
                        {
                            Debug.LogWarning("Resource not found. Resource Path: " + rResourcePath);
                        }
                    }
                }
                // Inform the combatant of the change
                else
                {
                    ICombatant lCombatant = gameObject.GetComponent<ICombatant>();
                    if (lCombatant != null)
                    {
                        IWeaponCore lWeaponCore = lItem.GetComponent<IWeaponCore>();
                        if (lWeaponCore != null)
                        {
                            string lCleanParentMountPoint = StringHelper.CleanString(rParentMountPoint);
                            if (lCleanParentMountPoint == "righthand")
                            {
                                lCombatant.PrimaryWeapon = lWeaponCore;
                            }
                            else if (lCleanParentMountPoint == "lefthand" || lCleanParentMountPoint == "leftlowerarm")
                            {
                                lCombatant.SecondaryWeapon = lWeaponCore;
                            }
                        }
                    }
                }
            }

            return lItem;
        }

        /// <summary>
        /// Mounts the item to the specified position based on the ItemCore
        /// </summary>
        /// <param name="rParent">Parent GameObject</param>
        /// <param name="rItem">Child GameObject that is this item</param>
        /// <param name="rLocalPosition">Vector3 that is the local position to set when the item is parented.</param>
        /// <param name="rLocalRotation">Quaternion that is the local rotation to set when the item is parented.</param>
        /// <param name="rParentMountPoint">Name of the parent mount point we're tying the item to</param>
        /// <param name="rItemMountPoint">Name of the child mount point we're tying the item to</param>
        protected virtual void MountItem(GameObject rParent, GameObject rItem, Vector3 rLocalPosition, Quaternion rLocalRotation, string rParentMountPoint, string rItemMountPoint = "Handle")
        {
            if (rParent == null || rItem == null) { return; }

            bool lIsConnected = false;

#if USE_MOUNT_POINTS || OOTII_MP

            if (mMountPoints != null)
            {
                lIsConnected = mMountPoints.ConnectMountPoints(rParentMountPoint, rItem, rItemMountPoint);

                if (lIsConnected)
                {
                    IItemCore lItemCore = rItem.GetComponent<IItemCore>();
                    if (lItemCore != null)
                    {
                        lItemCore.Owner = gameObject;
                    }
                }
            }

#endif

            if (!lIsConnected)
            {
                Transform lParentBone = FindTransform(rParent.transform, rParentMountPoint);
                rItem.transform.parent = lParentBone;

                //IItemCore lItemCore = InterfaceHelper.GetComponent<IItemCore>(rItem);
                IItemCore lItemCore = rItem.GetComponent<IItemCore>();
                if (lItemCore != null)
                {
                    lItemCore.Owner = gameObject;

                    if (rLocalPosition.sqrMagnitude == 0f && QuaternionExt.IsIdentity(rLocalRotation))
                    {
                        rItem.transform.localPosition = (lItemCore != null ? lItemCore.LocalPosition : Vector3.zero);
                        rItem.transform.localRotation = (lItemCore != null ? lItemCore.LocalRotation : Quaternion.identity);
                    }
                    else
                    {
                        rItem.transform.localPosition = rLocalPosition;
                        rItem.transform.localRotation = rLocalRotation;
                    }
                }
                else
                {
                    rItem.transform.localPosition = rLocalPosition;
                    rItem.transform.localRotation = rLocalRotation;
                }
            }

            if (rItem != null)
            {
                // Reenable the item as needed
                rItem.SetActive(true);
                rItem.hideFlags = HideFlags.None;

                // Inform the combatant of the change
                ICombatant lCombatant = gameObject.GetComponent<ICombatant>();
                if (lCombatant != null)
                {
                    IWeaponCore lWeaponCore = rItem.GetComponent<IWeaponCore>();
                    if (lWeaponCore != null)
                    {
                        string lCleanParentMountPoint = StringHelper.CleanString(rParentMountPoint);
                        if (lCleanParentMountPoint == "righthand")
                        {
                            lCombatant.PrimaryWeapon = lWeaponCore;
                        }
                        else if (lCleanParentMountPoint == "lefthand" || lCleanParentMountPoint == "leftlowerarm")
                        {
                            lCombatant.SecondaryWeapon = lWeaponCore;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to find a matching transform
        /// </summary>
        /// <param name="rParent">Parent transform where we'll start the search</param>
        /// <param name="rName">Name or identifier of the transform we want</param>
        /// <returns>Transform matching the name or the parent if not found</returns>
        protected Transform FindTransform(Transform rParent, string rName)
        {
            Transform lTransform = null;

            // Check by HumanBone name
            if (lTransform == null)
            {
                Animator lAnimator = rParent.GetComponentInChildren<Animator>();
                if (lAnimator != null)
                {
                    string lCleanName = StringHelper.CleanString(rName);
                    for (int i = 0; i < BasicInventory.UnityBones.Length; i++)
                    {
                        if (BasicInventory.UnityBones[i] == lCleanName)
                        {
                            lTransform = lAnimator.GetBoneTransform((HumanBodyBones)i);
                            break;
                        }
                    }
                }
            }

            // Check if by exact name
            if (lTransform == null)
            {
                lTransform = rParent.transform.FindTransform(rName);
            }

            // Default to the root
            if (lTransform == null)
            {
                lTransform = rParent.transform;
            }

            return lTransform;
        }

#region Editor Functions

#if UNITY_EDITOR

        /// <summary>
        /// Allows us to re-open the last selected item
        /// </summary>
        public int EditorItemIndex = -1;

        /// <summary>
        /// Allows us to re-open the last selected slot
        /// </summary>
        public int EditorSlotIndex = -1;

        /// <summary>
        /// Allows us to re-open the lst selected weapon set
        /// </summary>
        public int EditorWeaponSetIndex = -1;

        /// <summary>
        /// Allows us to re-open the lst selected weapon set item
        /// </summary>
        public int EditorWeaponSetItemIndex = -1;

        /// <summary>
        /// Determines if we show events to process
        /// </summary>
        public bool EditorShowEvents = false;

#endif

#endregion
    }
}
