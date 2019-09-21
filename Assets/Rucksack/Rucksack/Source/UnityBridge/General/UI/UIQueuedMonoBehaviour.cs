using Devdog.General2;
using Devdog.General2.UI;
using UnityEngine;

namespace Devdog.Rucksack.UI
{
    /// <summary>
    /// This component handles queueing of repaintable tasks.
    /// If a window is not visible the repaint task is queued and will repaint once the window becomes visible again.
    /// 
    /// <remarks>Multiple repaint requests will be ignored. The element will only be repainted once, when the window becomes visible.</remarks>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class UIQueuedMonoBehaviour<T> : UIMonoBehaviour
        where T : UnityEngine.Component
    {
        [Required]
        [SerializeField]
        private T _prefab;
        public T prefab
        {
            get { return _prefab; }
        }

        [SerializeField]
        private RectTransform _uiContainer;
        public RectTransform uiContainer
        {
            get { return _uiContainer; }
        }

        protected T[] repaintableElements = new T[0];

        public UIWindow window { get; protected set; }
        protected ILogger logger { get; private set; }
        protected System.Collections.Generic.HashSet<int> dirtySlots = new System.Collections.Generic.HashSet<int>();

        public UIQueuedMonoBehaviour()
        {
            logger = new UnityLogger("[UI] ");
        }

        protected virtual void Awake()
        {
            window = gameObject.GetOrAddComponent<UIWindow>();
            // TODO: Add option for manual indexing of collection slots (isManuallyDefined)
        }
        
        protected virtual void Start()
        {
            RegisterWindowEvents();
        }

        protected virtual void OnDestroy()
        {
            UnRegisterWindowEvents();
        }
        
        protected void RegisterWindowEvents()
        {
            window.OnShow += OnWindowShow;
            window.OnHide += OnWindowHide;
        }
        
        protected void UnRegisterWindowEvents()
        {
            if (window != null)
            {
                window.OnShow -= OnWindowShow;
                window.OnHide -= OnWindowHide;    
            }
        }
        
        protected virtual void OnWindowShow()
        {
            foreach (var dirtySlot in dirtySlots)
            {
                Repaint(dirtySlot);
            }

            dirtySlots.Clear();
        }
        
        protected virtual void OnWindowHide()
        { }
        
        protected virtual T CreateUIElement(int index)
        {
            var inst = Instantiate(_prefab, Vector3.zero, Quaternion.identity, uiContainer);
            inst.transform.localPosition = Vector3.zero;
            inst.transform.localRotation = Quaternion.identity;
            inst.transform.localScale = Vector3.one;
            
            return inst;
        }
        
        protected virtual void DestroyUISlot(int index)
        {
            if (index >= 0 && index < repaintableElements.Length)
            {
                Destroy(repaintableElements[index]?.gameObject);
                repaintableElements[index] = null;
                dirtySlots.Remove(index);
            }
        }

        protected abstract void Repaint(int index);
        protected virtual void RepaintOrQueue(int slot)
        {
            if (window.isVisible)
            {
                Repaint(slot);
                dirtySlots.Remove(slot);
            }
            else
            {
                dirtySlots.Add(slot);
                logger.LogVerbose($"Slot #{slot} marked dirty");
            }
        }
    }
}