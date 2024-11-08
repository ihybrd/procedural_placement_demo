using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Demo_ProceduralPlacement {

    [InitializeOnLoad()]
    public class EditorUpdater
    {
        public static InstancePlacer instancePlacer;
        static EditorUpdater()
        {
            EditorApplication.update += Update; // call this func (Update) whenever the editor is updated.
        }

        // Update is called once per frame
        static void Update()
        {
            if (instancePlacer == null) {
                instancePlacer = GameObject.Find("InstancePlacer").GetComponent<InstancePlacer>();
            }
            instancePlacer.UpdateFunctionOnGPU();
        }
    }

}