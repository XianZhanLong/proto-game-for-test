using UnityEngine;

using UnityEditor;
using UnityEngine.AI;

namespace Unity.Tutorials
{
    /// <summary>
    /// Implement your Tutorial callbacks here.
    /// </summary>
    public class TutorialCallbacks : ScriptableObject
    {

        NavMeshSurface navMeshSurface = default;

        public bool NavMeshIsBuilt()
        {
            return navMeshSurface.navMeshData != null;
        }

        public void ClearAllNavMeshes()
        {
            if (!navMeshSurface)
            {
                navMeshSurface = GameObject.FindObjectOfType<NavMeshSurface>();
            }
            UnityEditor.AI.NavMeshBuilder.ClearAllNavMeshes();
            navMeshSurface.navMeshData = null;
        }

        /// <summary>
        /// Keeps the Room selected during a tutorial. 
        /// </summary>


        /// <summary>
        /// Keeps the Room selected during a tutorial. 
        /// </summary>



        /// <summary>
        /// Selects a GameObject in the scene, marking it as the active object for selection
        /// </summary>


        public void SelectMoveTool()
        {
            Tools.current = Tool.Move;
        }

        public void SelectRotateTool()
        {
            Tools.current = Tool.Rotate;
        }

    }
}