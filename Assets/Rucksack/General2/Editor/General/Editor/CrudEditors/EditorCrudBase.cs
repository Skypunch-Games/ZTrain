using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Devdog.General2.Editors
{
    public abstract partial class EditorCrudBase<T> : IEditorCrud where T : class
    {
        protected virtual T selectedItem { get; set; }
        protected abstract ICollection<T> crudList { get; }

        protected Vector2 scrollPosition;
        protected Vector2 scrollPositionDetail;
        protected string singleName;
        protected string pluralName;

        protected string searchQuery = "";
        private int _searchResultCount = -1;

        public bool canDeleteItems { get; set; }
        public bool canDuplicateItems { get; set; }
        public bool canReOrderItems { get; set; }
        public bool canCreateItems { get; set; }
        public bool hideCreateItem { get; set; }

        public EditorWindow window { get; protected set; }
        
        protected Rect sidebarRowElementOffset;
        private readonly Color _createColor = new Color(0.4f, 1.0f, 0.4f, 0.8f);
        private readonly List<Action> _deferredActions = new List<Action>();

        public bool isSearching
        {
            get { return searchQuery != "" && searchQuery != "Search..."; }
        }

        protected EditorCrudBase(string singleName, string pluralName, EditorWindow window)
        {
            this.singleName = singleName;
            this.pluralName = pluralName;
            this.window = window;

            canCreateItems = true;
            canDuplicateItems = true;
            canDeleteItems = true;
            canReOrderItems = false;
            hideCreateItem = false;
        }
        
        protected void DeferAction(Action action)
        {
            _deferredActions.Add(action);
        }

        /// <summary>
        /// Create a new item and add it to the database
        /// </summary>
        protected abstract void CreateNewItem();


        public virtual void Focus()
        {
            _searchResultCount = -1;
            searchQuery = "Search...";
        }

        public virtual void Draw()
        {
            EditorGUILayout.BeginHorizontal();
            BeginSidebar();
            DrawSidebar();
            EndSidebar();

            scrollPositionDetail = EditorGUILayout.BeginScrollView(scrollPositionDetail, GUILayout.MaxWidth(600));
            EditorGUIUtility.labelWidth = EditorStyles.labelWidth;

            if (selectedItem != null)
            {
                DrawDetail(selectedItem);
            }

            EditorGUIUtility.labelWidth = 0;
            EditorGUILayout.EndScrollView();
            
            foreach (var method in _deferredActions)
            {
                if (method != null)
                {
                    method.Invoke();
                }
            }

            _deferredActions.Clear();
            
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw a single item in the sidebar
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        protected abstract void DrawSidebarRow(T item, int index);


        /// <summary>
        /// Does a item match a search query, only called when actually searching.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="searchQuery"></param>
        /// <returns></returns>
        protected abstract bool MatchesSearch(T item, string searchQuery);


        /// <summary>
        /// Add an item to the crud list
        /// </summary>
        public virtual void AddItem(T item, bool editOnceAdded = true)
        {
            crudList.Add(item);
            if (editOnceAdded)
            {
                EditItem(item);
            }
            
            window.Repaint();
        }

        public virtual void RemoveItem(T item)
        {
            if (selectedItem != null && selectedItem.Equals(item))
            {
                selectedItem = null;
            }

            DeferAction(() =>
            {
                crudList.Remove(item);
                window.Repaint();
            });
        }

        public abstract T DuplicateItem(T index);

        public virtual void EditItem(T item)
        {
            GUI.FocusControl("SearchField");
            selectedItem = item;
        }

        /// <summary>
        /// Begin a sidebar row element
        /// </summary>
        /// <param name="item"></param>
        /// <param name="i"></param>
        protected virtual void BeginSidebarRow(T item, int i)
        {
            Rect rect = EditorGUILayout.GetControlRect(true, 30.0f);
            rect.x -= 7;
            rect.width = 260;

            GUI.backgroundColor = (item.Equals(selectedItem)) ? new Color(0, 1.0f, 0, 0.3f) : new Color(0, 0, 0, 0.0f);

            if (GUI.Button(rect, "", "MeTransitionSelectHead"))
                ClickedSidebarRowElement(item);

            GUI.backgroundColor = Color.white;

            sidebarRowElementOffset = rect;
            sidebarRowElementOffset.x += 7;
            sidebarRowElementOffset.y += 7; // For text
            GUI.color = (selectedItem == item) ? Color.white : new Color(1.0f, 1.0f, 1.0f, 0.6f);

            EditorGUILayout.BeginHorizontal(GUILayout.Height(30.0f));
        }

        protected virtual void ClickedSidebarRowElement(T item)
        {
            EditItem(item);
        }

        protected virtual void DrawSidebarRowElement(string text, int width, int height = -1)
        {
            sidebarRowElementOffset.width = width;
            sidebarRowElementOffset.height = (height == -1) ? sidebarRowElementOffset.height : height;
            EditorGUI.LabelField(sidebarRowElementOffset, text);

            sidebarRowElementOffset.x += width;
        }

        protected virtual void DrawSidebarRowSpace(int width)
        {
            sidebarRowElementOffset.x += width;
        }

        protected virtual void DrawSidebarRowElement(string text, GUIStyle guiStyle, int width, int height = -1)
        {
            sidebarRowElementOffset.width = width;
            sidebarRowElementOffset.height = (height == -1) ? sidebarRowElementOffset.height : height;
            EditorGUI.LabelField(sidebarRowElementOffset, text, guiStyle);

            sidebarRowElementOffset.x += width;
        }

        protected virtual bool DrawSidebarRowElementButton(string text, int width, int height = -1)
        {
            sidebarRowElementOffset.width = width;
            sidebarRowElementOffset.height = (height == -1) ? sidebarRowElementOffset.height : height;
            bool clicked = GUI.Button(sidebarRowElementOffset, text);

            sidebarRowElementOffset.x += width;
            return clicked;
        }

        protected virtual bool DrawSidebarRowElementButton(string text, GUIStyle guiStyle, int width, int height = -1)
        {
            sidebarRowElementOffset.width = width;
            sidebarRowElementOffset.height = (height == -1) ? sidebarRowElementOffset.height : height;
            bool clicked = GUI.Button(sidebarRowElementOffset, text, guiStyle);

            sidebarRowElementOffset.x += width;
            return clicked;
        }

        protected virtual bool DrawSidebarRowElementToggle(bool toggled, string text, int width, int height = -1)
        {
            sidebarRowElementOffset.width = width;
            sidebarRowElementOffset.height = (height == -1) ? sidebarRowElementOffset.height : height;
            bool t = GUI.Toggle(sidebarRowElementOffset, toggled, text);

            sidebarRowElementOffset.x += width;
            return t;
        }

        protected virtual bool DrawSidebarRowElementToggle(bool toggled, string text, GUIStyle guiStyle, int width, int height = -1)
        {
            sidebarRowElementOffset.width = width;
            sidebarRowElementOffset.height = (height == -1) ? sidebarRowElementOffset.height : height;
            bool t = GUI.Toggle(sidebarRowElementOffset, toggled, text, guiStyle);

            sidebarRowElementOffset.x += width;
            return t;
        }

        protected virtual void EndSidebarRow(T item, int i)
        {
            sidebarRowElementOffset.width = 20;

            int prevDepth = GUI.depth;
            GUI.depth = 50;
            if (canReOrderItems)
            {
                sidebarRowElementOffset.x -= 40;

                var crudListList = crudList as IList<T>;
                if (crudListList != null && i < crudList.Count - 1)
                {
                    if (GUI.Button(sidebarRowElementOffset, "", "Grad Down Swatch"))
                    {
                        
                        // Move element down
                        var temp = crudListList[i + 1];
                        crudListList[i + 1] = item;
                        crudListList[i] = temp;

                        GUI.changed = true; // To save
                        window.Repaint();
                    }
                }
                sidebarRowElementOffset.x += 20;


                if (crudListList != null && i > 0)
                {
                    if (GUI.Button(sidebarRowElementOffset, "", "Grad Up Swatch"))
                    {
                        // Move element up
                        var temp = crudListList[i - 1];
                        crudListList[i - 1] = item;
                        crudListList[i] = temp;

                        GUI.changed = true; // To save
                        window.Repaint();
                    }
                }
                sidebarRowElementOffset.x += 20;
            }

            if (canDuplicateItems)
            {
                GUI.color = Color.green;
                if (GUI.Button(sidebarRowElementOffset, "", (GUIStyle)"OL Plus"))
                {
                    DuplicateItem(item);
                }

                sidebarRowElementOffset.x += 20;
            }

            if (canDeleteItems)
            {
                GUI.color = Color.red;
                if (GUI.Button(sidebarRowElementOffset, "", (GUIStyle)"OL Minus"))
                {
                    if (UnityEditor.EditorUtility.DisplayDialog("Are you sure?", "Do you want to delete " + singleName, "Yes", "NO!"))
                    {
                        RemoveItem(item);
                    }
                }
            }

            GUI.depth = prevDepth;
            GUI.color = Color.white;

            EditorGUILayout.EndHorizontal();
        }

        protected virtual void BeginSidebar()
        {
            EditorGUILayout.BeginVertical(EditorStyles.boxStyle, GUILayout.Width(375.0f));            
        }

        /// <summary>
        /// Draw the list, where item can be selected to edit
        /// </summary>
        protected virtual void DrawSidebar()
        {
            searchQuery = EditorStyles.SearchBar(searchQuery, window, isSearching);

            if (hideCreateItem == false)
            {
                GUI.color = _createColor;
                GUI.enabled = canCreateItems;

                if (GUILayout.Button("Create " + singleName, (GUIStyle) "LargeButton"))
                {
                    CreateNewItem();
                }

                GUI.color = Color.white;
                GUI.enabled = true;
            }


            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // BEGIN ROW
            EditorGUILayout.BeginHorizontal();

            if (isSearching)
            {
                GUILayout.Label(_searchResultCount + " " + pluralName + " (search result)"); // , InventoryEditorStyles.titleStyle
            }
            else
            {
                GUILayout.Label(crudList.Count + " " + pluralName);
            }

            EditorGUILayout.EndHorizontal();
            // END ROW

            int x = 0;
            _searchResultCount = 0;
            foreach (var item in crudList)
            {
                if (item == null)
                {
                    continue;                    
                }

                if (item.Equals(selectedItem))
                {
                    GUI.color = Color.green;
                }

                if (isSearching)
                {
                    if (MatchesSearch(item, searchQuery))
                    {
                        _searchResultCount++;
                        DrawSidebarRow(item, x);
                    }
                }
                else
                {
                    DrawSidebarRow(item, x);
                }

                GUI.color = Color.white;
                x++;
            }

            if (_searchResultCount == 0 && isSearching)
            {
                selectedItem = null;
                window.Repaint();
            }

            EditorGUILayout.EndScrollView();
        }

        public virtual void EndSidebar()
        {
            EditorGUILayout.EndHorizontal();            
        }
        
        /// <summary>
        /// Draw a single item in detail
        /// </summary>
        protected abstract void DrawDetail(T item);


        /// <summary>
        /// Name of the Editor
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return singleName + " editor";
        }
    }
}
