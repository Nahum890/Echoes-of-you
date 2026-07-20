using System.Collections.Generic;
using UnityEngine;

namespace Echoes.EnvironmentPass
{
    [CreateAssetMenu(menuName = "Echoes/Environment Pass/Room Data", fileName = "RoomData_")]
    public class RoomDataSO : ScriptableObject
    {
        public string roomId;
        public RoomType roomType;
        public List<PropPlacementSO> placements = new();
        public List<PropPlacementSO> decals = new();
        public bool validateRequiredProps = true;

        public List<string> GetRequiredPropsForType()
        {
            return roomType switch
            {
                RoomType.Classroom => new() { "Pizarra", "MesaProfesor", "PupitreDoble", "SillaEscolar", "Basurero", "Libros" },
                RoomType.Corridor => new() { "BancoMadera", "Locker", "Basurero", "Cartelera", "Extintor" },
                RoomType.Library => new() { "Estanteria", "MesaKenney", "SillaEscolar", "LamparaTecho", "Libros" },
                RoomType.Gym => new() { "Balon", "BancoMadera", "Extintor", "Basurero" },
                RoomType.Patio => new() { "BancoMadera", "PlantaMaceta", "Balon", "CarritoConserje" },
                RoomType.Office => new() { "MesaProfesor", "SillaOficina", "TazaCafe", "Libros", "Radio" },
                RoomType.Storage => new() { "CajaCartonCerrada", "EstanteriaCerrada", "Basurero" },
                RoomType.Hall => new() { "BancoMadera", "Cartelera", "PlantaMaceta" },
                RoomType.VoidFragment => new() { "Locker", "BancoMadera", "Libros" },
                _ => new()
            };
        }
    }
}