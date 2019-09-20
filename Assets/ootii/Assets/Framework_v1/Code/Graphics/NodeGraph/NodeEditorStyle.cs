using UnityEngine;
using com.ootii.Helpers;

namespace com.ootii.Graphics.NodeGraph
{
    /// <summary>
    /// Contains style information used to render or the node editor
    /// </summary>
    public class NodeEditorStyle
    {
        /// <summary>
        /// Canvas Background
        /// </summary>
        public static Texture2D mCanvasBackground = null;
        public static Texture2D CanvasBackground
        {
            get
            {
                if (mCanvasBackground == null)
                {
#if UNITY_EDITOR
                    mCanvasBackground = Resources.Load<Texture2D>(UnityEditor.EditorGUIUtility.isProSkin ? "Textures/Background_dark" : "Textures/Background_dark");
#endif
                }

                return mCanvasBackground;
            }
        }

        /// <summary>
        /// Label
        /// </summary>
        public static GUIStyle mPanelTitle = null;
        public static GUIStyle PanelTitle
        {
            get
            {
                //if (mNodeText == null)
                {
                    mPanelTitle = new GUIStyle(GUI.skin.label);
                    mPanelTitle.normal.textColor = Color.white;
                    //mPanelTitle.fontSize = 11;
                    mPanelTitle.fontStyle = FontStyle.Bold;
                    mPanelTitle.alignment = TextAnchor.MiddleLeft;
                }

                return mPanelTitle;
            }
        }

        /// <summary>
        /// Label
        /// </summary>
        public static GUIStyle mPanelSelectedTitle = null;
        public static GUIStyle PanelSelectedTitle
        {
            get
            {
                //if (mNodeText == null)
                {
                    mPanelSelectedTitle = new GUIStyle(GUI.skin.label);
                    mPanelSelectedTitle.normal.textColor = Color.black;
                    //mPanelTitle.fontSize = 11;
                    mPanelSelectedTitle.fontStyle = FontStyle.Bold;
                    mPanelSelectedTitle.alignment = TextAnchor.MiddleLeft;
                }

                return mPanelSelectedTitle;
            }
        }

        /// <summary>
        /// Box used to group standard GUI elements
        /// </summary>
        private static GUIStyle mPanel = null;
        public static GUIStyle Panel
        {
            get
            {
                //if (mPanel == null)
                {
                    mPanel = new GUIStyle(GUI.skin.box);
#if UNITY_EDITOR
                    Texture2D lTexture = Resources.Load<Texture2D>(UnityEditor.EditorGUIUtility.isProSkin ? "Textures/PanelDefault_pro" : "Textures/PanelDefault");
                    mPanel.normal.background = lTexture;
#endif
                    mPanel.alignment = TextAnchor.MiddleCenter;
                    mPanel.border = new RectOffset(10, 10, 38, 10);
                }

                return mPanel;
            }
        }

        /// <summary>
        /// Box used to group standard GUI elements
        /// </summary>
        private static GUIStyle mPanelSelected = null;
        public static GUIStyle PanelSelected
        {
            get
            {
                //if (mPanelSelected == null)
                {
                    mPanelSelected = new GUIStyle(GUI.skin.box);
#if UNITY_EDITOR
                    Texture2D lTexture = Resources.Load<Texture2D>(UnityEditor.EditorGUIUtility.isProSkin ? "Textures/PanelSelected_pro" : "Textures/PanelSelected");
                    mPanelSelected.normal.background = lTexture;
#endif
                    mPanelSelected.alignment = TextAnchor.MiddleCenter;
                    mPanelSelected.border = new RectOffset(10, 10, 38, 10);
                }

                return mPanelSelected;
            }
        }

        /// <summary>
        /// Label
        /// </summary>
        public static GUIStyle mNodeText = null;
        public static GUIStyle NodeText
        {
            get
            {
                //if (mNodeText == null)
                {
                    mNodeText = new GUIStyle(GUI.skin.label);
                    mNodeText.fixedWidth = 130f;
                    mNodeText.fixedHeight = 40f;

#if UNITY_EDITOR
                    mNodeText.normal.textColor = EditorHelper.CreateColor(167f, 174f, 189f, 255f);
#endif

                    //mNodeText.fontSize = 11;
                    mNodeText.alignment = TextAnchor.MiddleCenter;
                    mNodeText.wordWrap = true;
                    mNodeText.padding = new RectOffset(25, 0, -2, 0);
                }

                return mNodeText;
            }
        }

        /// <summary>
        /// Default node box
        /// </summary>
        public static GUIStyle mNodeBox = null;
        public static GUIStyle NodeBox
        {
            get
            {
                //if (mNodeBox == null)
                {
                    mNodeBox = new GUIStyle(GUI.skin.box);
                }

                return mNodeBox;
            }
        }

        /// <summary>
        /// Default node box
        /// </summary>
        public static GUIStyle mSelectedNodeBox = null;
        public static GUIStyle SelectedNodeBox
        {
            get
            {
                //if (mSelectedNodeBox == null)
                {
                    mSelectedNodeBox = new GUIStyle(GUI.skin.window);
                }

                return mSelectedNodeBox;
            }
        }

        /// <summary>
        /// Box used to group standard GUI elements
        /// </summary>
        private static GUIStyle mNode = null;
        public static GUIStyle Node
        {
            get
            {
                //if (mNode == null)
                {
                    mNode = new GUIStyle(GUI.skin.box);
#if UNITY_EDITOR
                    Texture2D lTexture = Resources.Load<Texture2D>(UnityEditor.EditorGUIUtility.isProSkin ? "Textures/NodeDefault" : "Textures/NodeDefault");
                    mNode.normal.background = lTexture;
#endif
                    mNode.alignment = TextAnchor.MiddleCenter;
                    mNode.border = new RectOffset(10, 10, 18, 10);
                }

                return mNode;
            }
        }

        /// <summary>
        /// Box used to group standard GUI elements
        /// </summary>
        private static GUIStyle mNodeSelected = null;
        public static GUIStyle NodeSelected
        {
            get
            {
                //if (mNodeSelected == null)
                {
                    mNodeSelected = new GUIStyle(GUI.skin.box);
#if UNITY_EDITOR
                    Texture2D lTexture = Resources.Load<Texture2D>(UnityEditor.EditorGUIUtility.isProSkin ? "Textures/NodeSelected" : "Textures/NodeSelected");
                    mNodeSelected.normal.background = lTexture;
#endif
                    mNodeSelected.alignment = TextAnchor.MiddleCenter;
                    mNodeSelected.border = new RectOffset(10, 10, 18, 10);
                }

                return mNodeSelected;
            }
        }

        /// <summary>
        /// Segment that the header appears
        /// </summary>
        public static int LinkHeaderIndex = 11;

        /// <summary>
        /// Standard color for links
        /// </summary>
        public static Color LinkColor = new Color(32f / 255f, 36f / 255f, 43f / 255f, 1f);

        /// <summary>
        /// Standard color for links
        /// </summary>
        public static Color LinkSelectedColor = new Color(255f / 255f, 180f / 255f, 74f / 255f, 1f);

        /// <summary>
        /// Standard color for links
        /// </summary>
        public static Color LinkDisabledColor = new Color(64f / 255f, 72f / 255f, 86f / 255f, 1f);

        /// <summary>
        /// Standard color for link shadows
        /// </summary>
        public static Color LinkShadowColor = new Color(13f / 255f, 15f / 255f, 18f / 255f, 0.1f);

        /// <summary>
        /// Link Arrow
        /// </summary>
        public static Texture2D mLinkHead = null;
        public static Texture2D LinkHead
        {
            get
            {
                //if (mLinkHead == null)
                {
#if UNITY_EDITOR
                    mLinkHead = Resources.Load<Texture2D>(UnityEditor.EditorGUIUtility.isProSkin ? "Textures/LinkDefault" : "Textures/LinkDefault");
#endif
                }

                return mLinkHead;
            }
        }

        /// <summary>
        /// Link Arrow
        /// </summary>
        public static Texture2D mLinkHeadSelected = null;
        public static Texture2D LinkHeadSelected
        {
            get
            {
                //if (mLinkHeadSelected == null)
                {
#if UNITY_EDITOR
                    mLinkHeadSelected = Resources.Load<Texture2D>(UnityEditor.EditorGUIUtility.isProSkin ? "Textures/LinkSelected" : "Textures/LinkSelected");
#endif
                }

                return mLinkHeadSelected;
            }
        }

        /// <summary>
        /// Link Arrow
        /// </summary>
        public static Texture2D mLinkHeadDisabled = null;
        public static Texture2D LinkHeadDisabled
        {
            get
            {
                //if (mLinkHead == null)
                {
#if UNITY_EDITOR
                    mLinkHeadDisabled = Resources.Load<Texture2D>(UnityEditor.EditorGUIUtility.isProSkin ? "Textures/LinkDisabled" : "Textures/LinkDisabled");
#endif
                }

                return mLinkHeadDisabled;
            }
        }


        /// <summary>
        /// Link Arrow
        /// </summary>
        public static Texture2D mLinkAction = null;
        public static Texture2D LinkAction
        {
            get
            {
                //if (mLinkAction == null)
                {
#if UNITY_EDITOR
                    mLinkAction = Resources.Load<Texture2D>(UnityEditor.EditorGUIUtility.isProSkin ? "Textures/LinkActionDefault" : "Textures/LinkActionDefault");
#endif
                }

                return mLinkAction;
            }
        }

        /// <summary>
        /// Link Arrow
        /// </summary>
        public static Texture2D mLinkActionSelected = null;
        public static Texture2D LinkActionSelected
        {
            get
            {
                //if (mLinkSelectedAction == null)
                {
#if UNITY_EDITOR
                    mLinkActionSelected = Resources.Load<Texture2D>(UnityEditor.EditorGUIUtility.isProSkin ? "Textures/LinkActionSelected" : "Textures/LinkActionSelected");
#endif
                }

                return mLinkActionSelected;
            }
        }

        /// <summary>
        /// Link Arrow
        /// </summary>
        public static Texture2D mLinkActionDisabled = null;
        public static Texture2D LinkActionDisabled
        {
            get
            {
                //if (mLinkActionDisabled == null)
                {
#if UNITY_EDITOR
                    mLinkActionDisabled = Resources.Load<Texture2D>(UnityEditor.EditorGUIUtility.isProSkin ? "Textures/LinkActionDisabled" : "Textures/LinkActionDisabled");
#endif
                }

                return mLinkActionDisabled;
            }
        }

        /// <summary>
        /// Box used to group standard GUI elements
        /// </summary>
        private static GUIStyle mPanelTitleBox = null;
        public static GUIStyle PanelTitleBox
        {
            get
            {
                //if (mPanelTitleBox == null)
                {
                    mPanelTitleBox = new GUIStyle(GUI.skin.box);
#if UNITY_EDITOR
                    //Texture2D lTexture = Resources.Load<Texture2D>(UnityEditor.EditorGUIUtility.isProSkin ? "Textures/TitleBoxBlue" : "Textures/TitleBoxBlue");
                    //mPanelTitleBox.normal.background = lTexture;
#endif
                    mPanelTitleBox.padding = new RectOffset(0, 0, 0, 0);
                    mPanelTitleBox.fixedHeight = 20f;
                }

                return mPanelTitleBox;
            }
        }

        /// <summary>
        /// Box used to group standard GUI elements
        /// </summary>
        private static GUIStyle mTitleBox = null;
        public static GUIStyle TitleBox
        {
            get
            {
                //if (mTitleBox == null)
                {
                    mTitleBox = new GUIStyle(GUI.skin.box);
#if UNITY_EDITOR
                    Texture2D lTexture = Resources.Load<Texture2D>(UnityEditor.EditorGUIUtility.isProSkin ? "Textures/TitleBoxBlue" : "Textures/TitleBoxBlue");
                    mTitleBox.normal.background = lTexture;
#endif
                    mTitleBox.padding = new RectOffset(0, 0, 0, 0);
                    mTitleBox.fixedHeight = 20f;
                }

                return mTitleBox;
            }
        }

        /// <summary>
        /// Renders a simple line to the inspector
        /// </summary>
        public static void DrawLine(GUIStyle rLineStyle)
        {
            GUILayout.BeginHorizontal(rLineStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Box used to draw a solid line
        /// </summary>
        private static GUIStyle mLineGold = null;
        public static GUIStyle LineGold
        {
            get
            {
                if (mLineGold == null)
                {
                    mLineGold = new GUIStyle(GUI.skin.box);

#if UNITY_EDITOR
                    Texture2D lTexture = Resources.Load<Texture2D>(UnityEditor.EditorGUIUtility.isProSkin ? "Textures/LineGold" : "Textures/LineGold");
                    mLineGold.normal.background = lTexture;
#endif

                    mLineGold.border.top = 0;
                    mLineGold.border.left = 0;
                    mLineGold.border.right = 0;
                    mLineGold.border.bottom = 0;
                    mLineGold.padding.top = 0;
                    mLineGold.padding.left = 0;
                    mLineGold.padding.right = 0;
                    mLineGold.padding.bottom = 0;
                    mLineGold.fixedHeight = 8f;
                }

                return mLineGold;
            }
        }

        /// <summary>
        /// Box used to draw a solid line
        /// </summary>
        private static GUIStyle mLineBlue = null;
        public static GUIStyle LineBlue
        {
            get
            {
                //if (mLineBlue == null)
                {
                    mLineBlue = new GUIStyle(GUI.skin.box);

#if UNITY_EDITOR
                    Texture2D lTexture = Resources.Load<Texture2D>(UnityEditor.EditorGUIUtility.isProSkin ? "Textures/LineBlue" : "Textures/LineBlue");
                    mLineBlue.normal.background = lTexture;
#endif

                    mLineBlue.border.top = 0;
                    mLineBlue.border.left = 0;
                    mLineBlue.border.right = 0;
                    mLineBlue.border.bottom = 0;
                    mLineBlue.padding.top = 0;
                    mLineBlue.padding.left = 0;
                    mLineBlue.padding.right = 0;
                    mLineBlue.padding.bottom = 0;
                    mLineBlue.fixedHeight = 8f;
                }

                return mLineBlue;
            }
        }

        /// <summary>
        /// Tag background
        /// </summary>
        public static GUIStyle mTag = null;
        public static GUIStyle Tag
        {
            get
            {
                //if (mTag == null)
                {
                    mTag = new GUIStyle();

#if UNITY_EDITOR
                    Texture2D lTexture = Resources.Load<Texture2D>(UnityEditor.EditorGUIUtility.isProSkin ? "Textures/TagDefault" : "Textures/TagDefault");
                    mTag.normal.background = lTexture;
#endif

                    mTag.normal.textColor = Color.black;
                    mTag.alignment = TextAnchor.MiddleCenter;
                    mTag.border = new RectOffset(8, 8, 8, 8);
                    mTag.fontSize = 9;
                }

                return mTag;
            }
        }


        /// <summary>
        /// Selected Tag background
        /// </summary>
        public static GUIStyle mTagSelected = null;
        public static GUIStyle TagSelected
        {
            get
            {
                //if (mTagSelected == null)
                {
                    mTagSelected = new GUIStyle();

#if UNITY_EDITOR
                    mTagSelected.normal.background = Resources.Load<Texture2D>(UnityEditor.EditorGUIUtility.isProSkin ? "Textures/TagSelected" : "Textures/TagSelected");
#endif

                    mTagSelected.normal.textColor = Color.black;
                    mTagSelected.alignment = TextAnchor.MiddleCenter;
                    mTagSelected.border = new RectOffset(8, 8, 8, 8);
                    mTagSelected.fontSize = 9;
                }

                return mTagSelected;
            }
        }

        /// <summary>
        /// Button background
        /// </summary>
        public static GUIStyle mButton = null;
        public static GUIStyle Button
        {
            get
            {
                //if (mButton == null)
                {
                    mButton = new GUIStyle();

#if UNITY_EDITOR
                    Texture2D lTexture = Resources.Load<Texture2D>(UnityEditor.EditorGUIUtility.isProSkin ? "Textures/ButtonDefault" : "Textures/ButtonDefault");
                    mButton.normal.background = lTexture;
#endif

                    mButton.border = new RectOffset(8, 8, 8, 8);
                }

                return mButton;
            }
        }

        /// <summary>
        /// Button - Green
        /// </summary>
        public static GUIStyle mButtonGreen = null;
        public static GUIStyle ButtonGreen
        {
            get
            {
                //if (mButton == null)
                {
                    mButtonGreen = new GUIStyle();

#if UNITY_EDITOR
                    mButtonGreen.normal.background = Resources.Load<Texture2D>(UnityEditor.EditorGUIUtility.isProSkin ? "Textures/ButtonGreen" : "Textures/ButtonGreen");
#endif

                    mButtonGreen.border = new RectOffset(8, 8, 8, 8);
                }

                return mButtonGreen;
            }
        }

        /// <summary>
        /// ButtonX
        /// </summary>
        public static GUIStyle mButtonX = null;
        public static GUIStyle ButtonX
        {
            get
            {
                //if (mButtonX == null)
                {
                    mButtonX = new GUIStyle();

#if UNITY_EDITOR
                    Texture2D lTexture = Resources.Load<Texture2D>(UnityEditor.EditorGUIUtility.isProSkin ? "Textures/Icon_Delete" : "Textures/Icon_Delete");
                    mButtonX.normal.background = lTexture;
#endif

                    mButtonX.margin = new RectOffset(0, 0, 2, 0);
                }

                return mButtonX;
            }
        }

        /// <summary>
        /// New icon
        /// </summary>
        public static Texture2D mIconNew = null;
        public static Texture2D IconNew
        {
            get
            {
                //if (mIconNew == null)
                {
#if UNITY_EDITOR
                    mIconNew = Resources.Load<Texture2D>(UnityEditor.EditorGUIUtility.isProSkin ? "Textures/Icon_New" : "Textures/Icon_New");
#endif
                }

                    return mIconNew;
            }
        }

        /// <summary>
        /// Open icon
        /// </summary>
        public static Texture2D mIconOpen = null;
        public static Texture2D IconOpen
        {
            get
            {
                //if (mIconOpen == null)
                {
#if UNITY_EDITOR
                    mIconOpen = Resources.Load<Texture2D>(UnityEditor.EditorGUIUtility.isProSkin ? "Textures/Icon_Open" : "Textures/Icon_Open");
#endif
                }

                return mIconOpen;
            }
        }

        /// <summary>
        /// Save icon
        /// </summary>
        public static Texture2D mIconSave = null;
        public static Texture2D IconSave
        {
            get
            {
                //if (mIconSave == null)
                {
#if UNITY_EDITOR
                    mIconSave = Resources.Load<Texture2D>(UnityEditor.EditorGUIUtility.isProSkin ? "Textures/Icon_Save" : "Textures/Icon_Save");
#endif
                }

                return mIconSave;
            }
        }

        /// <summary>
        /// Trash icon
        /// </summary>
        public static Texture2D mIconTrash = null;
        public static Texture2D IconTrash
        {
            get
            {
                //if (mIconTrash == null)
                {
#if UNITY_EDITOR
                    mIconTrash = Resources.Load<Texture2D>(UnityEditor.EditorGUIUtility.isProSkin ? "Textures/Icon_Trash" : "Textures/Icon_Trash");
#endif
                }

                return mIconTrash;
            }
        }

        /// <summary>
        /// Box used to group standard GUI elements
        /// </summary>
        private static GUIStyle mBox = null;
        public static GUIStyle Box
        {
            get
            {
                //if (mBox == null)
                {
                    mBox = new GUIStyle(GUI.skin.box);

#if UNITY_EDITOR
                    Texture2D lTexture = Resources.Load<Texture2D>(UnityEditor.EditorGUIUtility.isProSkin ? "Textures/Box_pro" : "Textures/Box");
                    mBox.normal.background = lTexture;
#endif

                    mBox.padding = new RectOffset(2, 2, 6, 6);
                    //mBox.margin = new RectOffset(0, 0, 0, 0);
                }

                return mBox;
            }
        }

        /// <summary>
        /// Box used to group standard GUI elements
        /// </summary>
        private static GUIStyle mGroupBox = null;
        public static GUIStyle GroupBox
        {
            get
            {
                //if (mGroupBox == null)
                {
                    mGroupBox = new GUIStyle(GUI.skin.box);

#if UNITY_EDITOR
                    Texture2D lTexture = Resources.Load<Texture2D>(UnityEditor.EditorGUIUtility.isProSkin ? "Textures/GroupBox_pro" : "Textures/GroupBox");
                    mGroupBox.normal.background = lTexture;
#endif

                    mGroupBox.padding = new RectOffset(3, 3, 3, 3);
                }

                return mGroupBox;
            }
        }

        /// <summary>
        /// Label
        /// </summary>
        public static GUIStyle mBoldLabel = null;
        public static GUIStyle BoldLabel
        {
            get
            {
                //if (mSmallBoldLabel == null)
                {
                    mBoldLabel = new GUIStyle(GUI.skin.label);
                    mBoldLabel.fontStyle = FontStyle.Bold;
                }

                return mBoldLabel;
            }
        }

#if UNITY_EDITOR

        /// <summary>
        /// Renders the inspector title for our asset
        /// </summary>
        public static void DrawInspectorDescription(string rDescription, UnityEditor.MessageType rMessageType)
        {
            Color lGUIColor = GUI.color;

            GUI.color = EditorHelper.CreateColor(255f, 255f, 255f, 1f);
            UnityEditor.EditorGUILayout.HelpBox(rDescription, rMessageType);

            GUI.color = lGUIColor;
        }

#endif
    }
}
