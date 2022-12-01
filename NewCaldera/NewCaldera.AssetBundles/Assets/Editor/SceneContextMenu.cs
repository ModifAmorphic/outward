//using UnityEngine;
//using System.Collections;
//using UnityEditor;

//[InitializeOnLoad]
//public class SceneContextMenu : Editor
//{

//    static SceneContextMenu()
//    {
//        SceneView.onSceneGUIDelegate += OnSceneGUI;
//    }

//    static void OnSceneGUI(SceneView sceneview)
//    {
//        if (Event.current.button == 1)
//        {
//            if (Event.current.type == EventType.MouseDown)
//            {
//                GenericMenu menu = new GenericMenu();
//                menu.AddItem(new GUIContent("Publish Pack"), false, Callback, 1);
//                menu.ShowAsContext();
//            }
//        }
//    }

//    static void Callback(object obj)
//    {
//        // Do something
//    }
//}