using System;
using System.Collections.Generic;
using UnityEngine;

namespace Echoes.EnvironmentPass
{
    public enum RoomType
    {
        Hall,
        Corridor,
        Classroom,
        Exit,
        Courtyard,
        Staircase,
        Special,
        Gym,
        Patio,
        Library,
        Office,
        Storage,
        Lab,
        VoidFragment
    }

    public enum PropSize
    {
        Small,
        Medium,
        Dominant
    }

    public enum Chapter
    {
        I_Persistencia,
        II_Coordinacion,
        III_Confianza,
        IV_Optimizacion,
        V_Consecuencia,
        VI_Aceptacion,
        Epilogue
    }

    public enum NarrativeTag
    {
        Lyra,
        Aiden,
        Memory,
        Echo,
        Environmental
    }

    [CreateAssetMenu(fileName = "PropPlacement", menuName = "Echoes/Environment/Prop Placement")]
    public class PropPlacementSO : ScriptableObject
    {
        public string prefabName;
        public Vector3 localPosition;
        public Vector3 localRotationEuler;
        public Vector3 scale = Vector3.one;
        public PropSize size;
        public Material materialOverride;
        public bool requiredForRoomType;
        public NarrativeTag narrativeTag;
        public float minClearanceFromPuzzle;
    }

    [CreateAssetMenu(fileName = "RoomData", menuName = "Echoes/Environment/Room Data")]
    public class RoomDataSO : ScriptableObject
    {
        public string roomId;
        public RoomType roomType;
        public List<PropPlacementSO> placements = new List<PropPlacementSO>();
        public List<PropPlacementSO> decals = new List<PropPlacementSO>();
        public bool validateRequiredProps;

        public List<string> GetRequiredPropsForType()
        {
            var list = new List<string>();
            switch (roomType)
            {
                case RoomType.Classroom:
                    list.Add("Pizarra");
                    list.Add("MesaProfesor");
                    list.Add("SillaEscolar");
                    list.Add("PupitreDoble");
                    break;
                case RoomType.Corridor:
                case RoomType.Hall:
                    list.Add("Lockers");
                    list.Add("Extintor");
                    list.Add("Cartelera");
                    list.Add("Radiador");
                    break;
                case RoomType.Exit:
                    list.Add("CartelSalida");
                    list.Add("Extintor");
                    break;
                case RoomType.Library:
                    list.Add("Estanteria");
                    list.Add("MesaEstudio");
                    break;
                case RoomType.Gym:
                    list.Add("Espaldera");
                    list.Add("BancoGimnasio");
                    break;
                default:
                    list.Add("BancoMadera");
                    list.Add("Papelera");
                    break;
            }
            return list;
        }
    }

    [CreateAssetMenu(fileName = "NarrativeCluster", menuName = "Echoes/Environment/Narrative Cluster")]
    public class NarrativeClusterSO : ScriptableObject
    {
        public int levelNumber;
        public string clusterName;
        public List<string> requiredPrefabNames = new List<string>();
        public Material requiredMaterial;
        public List<string> forbiddenMaterials = new List<string>();
        public NarrativeTag tag;
    }

    [CreateAssetMenu(fileName = "LevelData", menuName = "Echoes/Environment/Level Data")]
    public class LevelDataSO : ScriptableObject
    {
        public int levelNumber;
        public string levelName;
        public string scenePath;
        public Chapter chapter;
        public List<RoomDataSO> rooms = new List<RoomDataSO>();
        public NarrativeClusterSO narrativeCluster;
    }
}