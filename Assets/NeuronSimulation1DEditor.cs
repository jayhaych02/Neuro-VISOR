﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

namespace C2M2.NeuronalDynamics.Simulation
{
    /// <summary>
    /// Provides editor features so that users can more easily select neuron cells and options for them
    /// </summary>
    [CustomEditor(typeof(NeuronSimulation1D), true)]
    public class NeuronSimulation1DEditor : Editor
    {

        private readonly char slash = Path.DirectorySeparatorChar;
        private readonly string activeCellFolder = "ActiveCell";
        private readonly string neuronCellFolder = "NeuronalDynamics";
        private readonly string ugxExt = ".ugx";
        private readonly string spec1D = "_1d";
        private readonly string specTris = "_tris";
        private string[] cellFileNames;
        private int _cellIndex = 0;

        public override void OnInspectorGUI()
        {
            // Skip all of this if we're in runtime
            if (!Application.isPlaying)
            {
                var neuronSimulation = target as NeuronSimulation1D;

                string basePath = Application.streamingAssetsPath + slash + neuronCellFolder + slash + activeCellFolder + slash;

                string[] _cellOptions = new[] { "No cells found" };
                _cellOptions = BuildCellOptions(basePath);

                // Build dropdown menu
                _cellIndex = EditorGUILayout.Popup("Neuron Cell Source", _cellIndex, _cellOptions);

                // Build path from menu selection
                string cellVizPath = basePath + slash + _cellOptions[_cellIndex] + slash;
                Debug.Log(cellVizPath);

                // Find diameter selections for rendering and interaction
                string cellColPath = cellVizPath;
                switch (neuronSimulation.meshColScale)
                {
                    case NeuronSimulation1D.MeshColScaling.x1:
                        cellColPath += "1xDiameter" + slash;
                        break;
                    case NeuronSimulation1D.MeshColScaling.x2:
                        cellColPath += "2xDiameter" + slash;
                        break;
                    case NeuronSimulation1D.MeshColScaling.x3:
                        cellColPath += "3xDiameter" + slash;
                        break;
                    case NeuronSimulation1D.MeshColScaling.x4:
                        cellColPath += "4xDiameter" + slash;
                        break;
                    case NeuronSimulation1D.MeshColScaling.x5:
                        cellColPath += "5xDiameter" + slash;
                        break;
                    default:
                        Debug.LogError("ERROR");
                        break;
                }
                cellVizPath += "1xDiameter";


                // Find refinement levels for rendering and interaction
                string identifier = "x";
                switch (neuronSimulation.refinementLevel)
                {
                    case NeuronSimulation1D.RefinementLevel.x0:
                        identifier = "0ref";
                        break;
                    case NeuronSimulation1D.RefinementLevel.x1:
                        identifier = "1ref";
                        break;
                    case NeuronSimulation1D.RefinementLevel.x2:
                        identifier = "2ref";
                        break;
                    case NeuronSimulation1D.RefinementLevel.x3:
                        identifier = "3ref";
                        break;
                    case NeuronSimulation1D.RefinementLevel.x4:
                        identifier = "4ref";
                        break;
                    default:
                        Debug.LogError("ERROR");
                        break;
                }
                string[] rendRefinementOptions = Directory.GetDirectories(cellVizPath);
                string[] colRefinementOptions = Directory.GetDirectories(cellColPath);

                for (int i = 0; i < rendRefinementOptions.Length; i++)
                {
                    if (rendRefinementOptions[i].EndsWith(identifier))
                    {
                        cellVizPath = rendRefinementOptions[i];
                    }
                }
                for (int i = 0; i < colRefinementOptions.Length; i++)
                {
                    if (colRefinementOptions[i].EndsWith(identifier))
                    {
                        cellColPath = colRefinementOptions[i];
                    }
                }

                // Get 1D, 3D, triangle files for rendering
                string[] cellNames = new string[5];
                string[] files = Directory.GetFiles(cellVizPath);
                foreach (string file in files)
                {
                    // If this isn't a non-metadata ugx file,
                    if (!file.EndsWith(".meta") && file.EndsWith(ugxExt))
                    {
                        if (file.EndsWith(spec1D + ugxExt)) cellNames[1] = file;  // 1D cell
                        else if (file.EndsWith(specTris + ugxExt)) cellNames[2] = file;    // Triangles
                        else if (file.EndsWith(ugxExt)) cellNames[0] = file;     // If it isn't specified as 1D or triangles, it's most likely 3D
                    }
                }

                // Get interaction files
                cellNames[3] = "NULL"; // Blown up mesh default
                cellNames[4] = "NULL"; // Blown up mesh triangles default
                files = Directory.GetFiles(cellColPath);

                foreach (string file in files)
                {
                    // If this isn't a non-metadata ugx file,
                    if (!file.EndsWith(".meta"))
                    {
                        if (file.EndsWith(spec1D + ugxExt)) ; // 1D cell isn't needed for the blownup mesh
                        else if (file.EndsWith(specTris + ugxExt)) cellNames[4] = file;    // Triangles
                        else if (file.EndsWith(ugxExt)) cellNames[3] = file;    // If it isn't specified as 1D or triangles, it's most likely 3D
                    }
                }

                // Set file names in simulation script
                neuronSimulation.cellFile3D = cellNames[0];
                neuronSimulation.cellFile1D = cellNames[1];
                neuronSimulation.cellFileTriangles = cellNames[2];
                neuronSimulation.cellColliderFile3D = cellNames[3];
                neuronSimulation.cellColliderFileTriangles = cellNames[4];
            }

            // Draw the default inspector
            DrawDefaultInspector();

            return;

            string[] BuildCellOptions(string basePath)
            {
                // Separate cell option names from full paths
                string[] allPaths = Directory.GetDirectories(basePath);
                string[] allPathEnds = new string[allPaths.Length];
                for (int i = 0; i < allPaths.Length; i++)
                {
                    int pos = allPaths[i].LastIndexOf(slash) + 1;
                    allPathEnds[i] = allPaths[i].Substring(pos, allPaths[i].Length - pos);
                }
                return allPathEnds;
            }

        }
    }
}