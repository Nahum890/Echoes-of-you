using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Echoes.EnvironmentPass;

public static class EnvironmentPassDataGenerator
{
    [MenuItem("Tools/Environment Pass/6 - Generate All LevelDataSO (Run Once)")]
    public static void GenerateAllData()
    {
        Debug.Log("[EnvPassDataGen] Starting data generation...");

        // Ensure folders exist
        EnsureFolder("Assets/ScriptableObjects/EnvironmentPass");
        for (int i = 1; i <= 15; i++)
            EnsureFolder($"Assets/ScriptableObjects/EnvironmentPass/Level{i:D2}");

        // Load canonical materials
        var matMemory = EchoesMaterialLibrary.GetOrCreateEmissiveMaterial("Mat_Memory", EchoesMaterialLibrary.HexColor("8A5A2E"), EchoesMaterialLibrary.HexColor("FFBF00") * 0.9f);
        var matWallTeal = EchoesMaterialLibrary.GetOrCreateMaterial("Mat_WallTeal", EchoesMaterialLibrary.HexColor("2B4A4A"));
        var matWallMustard = EchoesMaterialLibrary.GetOrCreateMaterial("Mat_WallMustard", EchoesMaterialLibrary.HexColor("5A4A2E"));
        var matWallSage = EchoesMaterialLibrary.GetOrCreateMaterial("Mat_WallSage", EchoesMaterialLibrary.HexColor("3A4A38"));
        var matWallRose = EchoesMaterialLibrary.GetOrCreateMaterial("Mat_WallRose", EchoesMaterialLibrary.HexColor("4A3438"));

        int createdCount = 0;

        // ============================================================
        // LEVEL 1 - Desorientación - Cap I Persistencia (Minimalista, silencioso, casi vacío)
        // Coordenadas locales a zona (vestíbulo Entrada@-4, PasilloA@6, PasilloB@24,
        // AulaAusente@38, Hall_Salida@44); medidas contra greybox real (layer Ground,
        // corredor interior x±1.8, franja de paso x∈[-1.0,1.0], lockers a x±1.45,
        // barrera EchoOnly@13.1..14.9, PuertaAula@29.8, vestíbulo z -8.1..-4.1).
        // ============================================================
        {
            var r1 = CreateRoom(1, "Entrada", RoomType.Hall, new[]
            {
                PP("MesaProfesor", new(1.45f, 0f, 1.4f), new(0, -90, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("Cartelera", new(-1.45f, 1.2f, 1.4f), new(0, 90, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("Extintor", new(-1.45f, 0.8f, 3.0f), new(0, 90, 0), Vector3.one, PropSize.Small, required: true),
                PP("RelojPared", new(-1.45f, 2.9f, 4.5f), new(0, 90, 0), Vector3.one, PropSize.Small),
                PP("BancoMadera", new(1.45f, 0f, 3.5f), new(0, -90, 0), Vector3.one, PropSize.Medium, required: true),
                PP("PlantaMaceta", new(1.15f, 0f, 5.4f), Vector3.zero, Vector3.one, PropSize.Small),
                PP("Perchero", new(1.45f, 0f, 4.2f), Vector3.zero, Vector3.one, PropSize.Small),
                PP("Basurero", new(1.45f, 0f, 4.8f), Vector3.zero, Vector3.one, PropSize.Small, required: true),
            }, new[]
            {
                DD("dec_aviso_corcho", new(-1.65f, 1.5f, 2.1f), new(0, 90, 0)),
                DD("dec_humedad", new(1.65f, 0.5f, 2.8f), new(0, -90, 0)),
            });

            var r2 = CreateRoom(1, "PasilloA", RoomType.Corridor, new[]
            {
                PP("Radiador", new(1.45f, 0f, -4f), new(0, -90, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("Radiador", new(-1.45f, 0f, 2f), new(0, 90, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("Radiador", new(1.45f, 0f, 4f), new(0, -90, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("Cartelera", new(-1.45f, 1.2f, -6f), new(0, 90, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("Extintor", new(-1.45f, 0.8f, 6f), new(0, 90, 0), Vector3.one, PropSize.Small, required: true),
                PP("BancoMadera", new(-1.3f, 0f, -2f), new(0, 90, 0), Vector3.one, PropSize.Medium, required: true),
                PP("RelojPared", new(-1.45f, 2.6f, -4f), new(0, 90, 0), Vector3.one, PropSize.Small),
                PP("Basurero", new(1.2f, 0f, 0f), Vector3.zero, Vector3.one, PropSize.Small, required: true),
            }, new[]
            {
                DD("dec_grieta", new(-1.45f, 1f, -10f), new(0, 90, 0)),
                DD("dec_papel_suelo", new(0.3f, 0.01f, -5f), new(90, 0, 0)),
            });

            var r3 = CreateRoom(1, "PasilloB", RoomType.Corridor, new[]
            {
                PP("Locker", new(-1.45f, 0f, 2.8f), new(0, 90, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("Radiador", new(1.45f, 0f, -3.2f), new(0, -90, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("Cartelera", new(-1.45f, 1.2f, -5.2f), new(0, 90, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("BancoMadera", new(-1.3f, 0f, -1.2f), new(0, 90, 0), Vector3.one, PropSize.Medium, required: true),
                PP("Extintor", new(1.45f, 0.8f, 4.8f), new(0, -90, 0), Vector3.one, PropSize.Small, required: true),
                PP("PlantaMaceta", new(1.15f, 0f, 0.8f), Vector3.zero, Vector3.one, PropSize.Small),
                PP("Basurero", new(-1.2f, 0f, 4.5f), Vector3.zero, Vector3.one, PropSize.Small, required: true),
            }, new[]
            {
                DD("dec_tiza_borrada", new(-1.45f, 1f, 4.0f), new(0, 90, 0)),
                DD("dec_humedad", new(1.45f, 0.8f, -4.5f), new(0, -90, 0)),
            });

            // AulaAusente: el "aula" real es la franja del corredor (x±1.8, z 30.3..42.3);
            // paredes/piso del contenedor están degenerados (escala 0). Los pupitres grises
            // ya existen; la MochilaLyra cae sobre la mesa central de la 2ª fila
            // (raycast golpea la mesa, y=0 => snap a su superficie, top y=0.6).
            var r4 = CreateRoom(1, "AulaAusente", RoomType.Classroom, new[]
            {
                PP("Pizarra", new(1.45f, 1.3f, -1.8f), new(0, -90, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("Radiador", new(-1.45f, 0f, -0.2f), new(0, 90, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("Estanteria", new(1.45f, 0f, 2.4f), new(0, -90, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("MochilaLyra", new(0f, 0f, 0.3f), new(0, 45, 0), Vector3.one, PropSize.Small, matMemory),
                PP("Basurero", new(1.3f, 0f, 0.8f), Vector3.zero, Vector3.one, PropSize.Small, required: true),
            }, new[]
            {
                DD("dec_tiza_borrada", new(1.65f, 1f, -3.2f), new(0, -90, 0)),
                DD("dec_papel_suelo", new(1.2f, 0.01f, 2.0f), new(90, 0, 0)),
                DD("dec_humedad", new(-1.65f, 0.8f, 0.9f), new(0, 90, 0)),
            });

            var r5 = CreateRoom(1, "Hall_Salida", RoomType.Hall, new[]
            {
                PP("BancoMadera", new(-1.35f, 0f, -1.5f), new(0, 90, 0), Vector3.one, PropSize.Medium, required: true),
                PP("Basurero", new(1.35f, 0f, -1.8f), Vector3.zero, Vector3.one, PropSize.Small),
            }, new[]
            {
                DD("dec_arrastre", new(-1.0f, 0.01f, -1.3f), new(90, 0, 0)),
            });

            var level1 = CreateLevel(1, "Level_01 -- Desorientacion", "Assets/Scenes/Level_01.unity", Chapter.I_Persistencia,
                new[] { r1, r2, r3, r4, r5 },
                CreateCluster(1, "Llegada", new[] { "MochilaLyra", "PupitreDoble" }, matMemory, new[] { "Mat_Memory" }));
            createdCount++;
        }

        // ============================================================
        // LEVEL 2 - Repetición - Cap I Persistencia (Aulas estructuradas pero amplias)
        // ============================================================
        {
            var r1 = CreateRoom(2, "Entrada", RoomType.Hall, new[]
            {
                PP("BancoMadera", new(-1.5f, 0f, 0f), new(0, 90, 0), Vector3.one, PropSize.Medium, required: true),
                PP("Cartelera", new(1.7f, 1.2f, 0f), new(0, -90, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("Extintor", new(1.7f, 0.8f, -1.5f), new(0, -90, 0), Vector3.one, PropSize.Small, required: true),
            }, new[]
            {
                DD("dec_aviso_corcho", new(1.7f, 1.2f, 1.2f), new(0, -90, 0)),
            });

            var r2 = CreateRoom(2, "CorredorCentral", RoomType.Corridor, new[]
            {
                PP("Locker", new(-1.7f, 0f, -7f), new(0, 90, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("Radiador", new(1.7f, 0f, -5f), new(0, -90, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("BancoMadera", new(-1.5f, 0f, 0f), new(0, 90, 0), Vector3.one, PropSize.Medium, required: true),
                PP("Cartelera", new(1.8f, 1.2f, 2.5f), new(0, -90, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("Locker", new(-1.7f, 0f, 5f), new(0, 90, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("Radiador", new(1.7f, 0f, 7f), new(0, -90, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("Extintor", new(-1.8f, 0.8f, 8.5f), new(0, 90, 0), Vector3.one, PropSize.Small, required: true),
            }, new[]
            {
                DD("dec_grieta", new(-1.8f, 1f, -2f), new(0, 90, 0)),
                DD("dec_papel_suelo", new(0.4f, 0.01f, 2f), new(90, 0, 0)),
            });

            var r3 = CreateRoom(2, "AulaIzquierda", RoomType.Classroom, new[]
            {
                PP("Pizarra", new(0f, 1.3f, 4.6f), new(0, 180, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("MesaProfesor", new(-1.5f, 0f, 3.2f), new(0, 180, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("SillaOficina", new(-1.5f, 0f, 4.0f), Vector3.zero, Vector3.one, PropSize.Medium, required: true),
                PP("Estanteria", new(-2.6f, 0f, 2f), new(0, 90, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("Radiador", new(-2.6f, 0f, -2.5f), new(0, 90, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("PupitreDoble", new(-1.2f, 0f, -2.5f), Vector3.zero, Vector3.one, PropSize.Dominant, required: true),
                PP("SillaEscolar", new(-1.2f, 0f, -3.1f), Vector3.zero, Vector3.one, PropSize.Medium, required: true),
                PP("PupitreDoble", new(1.2f, 0f, -2.5f), Vector3.zero, Vector3.one, PropSize.Dominant, required: true),
                PP("SillaEscolar", new(1.2f, 0f, -3.1f), Vector3.zero, Vector3.one, PropSize.Medium, required: true),
                PP("PapeleraKenney", new(-2.5f, 0f, 4.2f), Vector3.zero, Vector3.one, PropSize.Small, required: true),
            }, new[]
            {
                DD("dec_tiza_borrada", new(-2.0f, 1f, 4.5f), new(0, 180, 0)),
            });

            var r4 = CreateRoom(2, "AulaDerecha", RoomType.Classroom, new[]
            {
                PP("Pizarra", new(0f, 1.3f, 4.6f), new(0, 180, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("MesaProfesor", new(1.5f, 0f, 3.2f), new(0, 180, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("SillaOficina", new(1.5f, 0f, 4.0f), Vector3.zero, Vector3.one, PropSize.Medium, required: true),
                PP("Estanteria", new(2.6f, 0f, 2f), new(0, -90, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("Radiador", new(2.6f, 0f, -2.5f), new(0, -90, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("PupitreDoble", new(-1.2f, 0f, -2.5f), Vector3.zero, Vector3.one, PropSize.Dominant, required: true),
                PP("SillaEscolar", new(-1.2f, 0f, -3.1f), Vector3.zero, Vector3.one, PropSize.Medium, required: true),
                PP("PupitreDoble", new(1.2f, 0f, -2.5f), Vector3.zero, Vector3.one, PropSize.Dominant, required: true),
                PP("SillaEscolar", new(1.2f, 0f, -3.1f), Vector3.zero, Vector3.one, PropSize.Medium, required: true),
                PP("Basurero", new(2.5f, 0f, 4.2f), Vector3.zero, Vector3.one, PropSize.Small, required: true),
            }, new[]
            {
                DD("dec_tiza_borrada", new(-2.0f, 1f, 4.5f), new(0, 180, 0)),
            });

            var r5 = CreateRoom(2, "Hall_Salida", RoomType.Hall, new[]
            {
                PP("BancoMadera", new(-3.2f, 0f, 0f), new(0, 90, 0), Vector3.one, PropSize.Medium, required: true),
                PP("Cartelera", new(3.6f, 1.2f, 1.5f), new(0, -90, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("Basurero", new(3.2f, 0f, -2f), Vector3.zero, Vector3.one, PropSize.Small, required: true),
            }, new[]
            {
                DD("dec_arrastre", new(-1.5f, 0.01f, -1.0f), new(90, 0, 0)),
            });

            var level2 = CreateLevel(2, "Level_02 -- Repeticion", "Assets/Scenes/Level_02.unity", Chapter.I_Persistencia,
                new[] { r1, r2, r3, r4, r5 },
                CreateCluster(2, "Primera Clase", new[] { "SillaEscolar", "Pizarra", "PupitreDoble" }, null, null));
            createdCount++;
        }

        // ============================================================
        // LEVEL 3 - Indecisión - Cap I Persistencia (Bifurcación arquitectónica limpia)
        // ============================================================
        {
            var r1 = CreateRoom(3, "Entrada", RoomType.Hall, new[]
            {
                PP("BancoMadera", new(-2.5f, 0f, 0f), new(0, 90, 0), Vector3.one, PropSize.Medium, required: true),
                PP("Cartelera", new(-2.8f, 1.2f, 1.5f), new(0, 90, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("Extintor", new(-2.8f, 0.8f, -1.5f), new(0, 90, 0), Vector3.one, PropSize.Small, required: true),
            }, new[]
            {
                DD("dec_aviso_corcho", new(-2.8f, 1.5f, 0f), new(0, 90, 0)),
            });

            var r2 = CreateRoom(3, "CorredorBifurcacion", RoomType.Corridor, new[]
            {
                PP("Cartelera", new(-2.7f, 1.2f, -1f), new(0, 90, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("Cartelera", new(2.7f, 1.2f, -1f), new(0, -90, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("Locker", new(-2.6f, 0f, -3f), new(0, 90, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("Locker", new(2.6f, 0f, -3f), new(0, -90, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("BancoMadera", new(-2.5f, 0f, 2f), new(0, 90, 0), Vector3.one, PropSize.Medium, required: true),
                PP("BancoMadera", new(2.5f, 0f, 2f), new(0, -90, 0), Vector3.one, PropSize.Medium, required: true),
            }, new[]
            {
                DD("dec_aviso_corcho", new(-2.7f, 1.5f, 0f), new(0, 90, 0)),
                DD("dec_grieta", new(2.7f, 0.8f, 0f), new(0, -90, 0)),
            });

            var r3 = CreateRoom(3, "AulaLyra", RoomType.Classroom, new[]
            {
                PP("Pizarra", new(0f, 1.3f, 3.6f), new(0, 180, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("PupitreDoble", new(-1.5f, 0f, -2.5f), Vector3.zero, Vector3.one, PropSize.Dominant, required: true),
                PP("SillaEscolar", new(-1.5f, 0f, -3.1f), Vector3.zero, Vector3.one, PropSize.Medium, required: true),
                PP("Radiador", new(-2.2f, 0f, 2f), new(0, 90, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("MochilaLyra", new(-1.5f, 0f, -2.0f), new(0, 30, 0), Vector3.one, PropSize.Small, matMemory),
                PP("Basurero", new(-2.1f, 0f, 3.2f), Vector3.zero, Vector3.one, PropSize.Small, required: true),
            }, new[]
            {
                DD("dec_tiza_borrada", new(-2.0f, 1f, 3.5f), new(0, 180, 0)),
            });

            var r4 = CreateRoom(3, "AulaEco", RoomType.Classroom, new[]
            {
                PP("Pizarra", new(0f, 1.3f, 3.6f), new(0, 180, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("PupitreDoble", new(1.5f, 0f, -2.5f), Vector3.zero, Vector3.one, PropSize.Dominant, required: true),
                PP("SillaEscolar", new(1.5f, 0f, -3.1f), Vector3.zero, Vector3.one, PropSize.Medium, required: true),
                PP("Estanteria", new(2.1f, 0f, 2f), new(0, -90, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("Cronometro", new(1.5f, 0.75f, -2.5f), Vector3.zero, Vector3.one * 0.5f, PropSize.Small),
                PP("PapeleraKenney", new(2.1f, 0f, 3.2f), Vector3.zero, Vector3.one, PropSize.Small, required: true),
            }, new[]
            {
                DD("dec_tiza_borrada", new(-2.0f, 1f, 3.5f), new(0, 180, 0)),
            });

            var r5 = CreateRoom(3, "Hall_Estatua", RoomType.Hall, new[]
            {
                PP("Arch_Column", new(-3.5f, 0f, -3.5f), Vector3.zero, Vector3.one, PropSize.Dominant),
                PP("Arch_Column", new(3.5f, 0f, -3.5f), Vector3.zero, Vector3.one, PropSize.Dominant),
                PP("Arch_Column", new(-3.5f, 0f, 3.5f), Vector3.zero, Vector3.one, PropSize.Dominant),
                PP("Arch_Column", new(3.5f, 0f, 3.5f), Vector3.zero, Vector3.one, PropSize.Dominant),
                PP("BancoMadera", new(-4.2f, 0f, 0f), new(0, 90, 0), Vector3.one, PropSize.Medium, required: true),
                PP("BancoMadera", new(4.2f, 0f, 0f), new(0, -90, 0), Vector3.one, PropSize.Medium, required: true),
                PP("Cartelera", new(-4.6f, 1.5f, 2f), new(0, 90, 0), Vector3.one, PropSize.Dominant, required: true),
                PP("Radiador", new(4.5f, 0f, -2f), new(0, -90, 0), Vector3.one, PropSize.Dominant, required: true),
            }, new[]
            {
                DD("dec_arrastre", new(0f, 0.01f, 1f), new(90, 0, 0)),
            });

            var r6 = CreateRoom(3, "Hall_Salida", RoomType.Hall, new[]
            {
                PP("BancoMadera", new(-3.2f, 0f, 0f), new(0, 90, 0), Vector3.one, PropSize.Medium, required: true),
                PP("Basurero", new(3.2f, 0f, -2f), Vector3.zero, Vector3.one, PropSize.Small, required: true),
            }, new[]
            {
                DD("dec_arrastre", new(-1.5f, 0.01f, -1.0f), new(90, 0, 0)),
            });

            var level3 = CreateLevel(3, "Level_03 -- Indecision", "Assets/Scenes/Level_03.unity", Chapter.I_Persistencia,
                new[] { r1, r2, r3, r4, r5, r6 },
                CreateCluster(3, "Bifurcacion", new[] { "Cronometro", "PupitreDoble" }, null, new[] { "Mat_Memory", "Mat_WallRose" }));
            createdCount++;
        }

        // ============================================================
        // LEVEL 4 - Espera - Cap II Coordinación
        // ============================================================
        {
            var r1 = CreateRoom("ZonaA_PreObservacion", RoomType.Hall, new[]
            {
                PP("Cartelera", new(2,1.2f,0.5f), new(0,180,0), Vector3.one, PropSize.Dominant, required:true),
                PP("BancoMadera", new(-2,0,2), Vector3.zero, Vector3.one, PropSize.Medium, required:true),
                PP("SillaEscolar", new(1.5f,0,3), new(0,45,0), Vector3.one, PropSize.Medium, required:true),
                PP("RelojPared", new(-2.9f,1.5f,1), new(0,90,0), Vector3.one, PropSize.Small),
                PP("Libros", new(2,0,2), new(0,15,0), Vector3.one*0.7f, PropSize.Small),
                PP("Basurero", new(-2,0,4), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("Extintor", new(2.9f,0,1), new(0,270,0), Vector3.one, PropSize.Small, required:true),
                PP("PapeleraKenney", new(2,0,4), Vector3.zero, Vector3.one, PropSize.Small, required:true),
            }, new[]
            {
                DD("dec_aviso_corcho", new(1.9f,1.2f,0.5f), new(0,180,0)),
                DD("dec_nota_adhesiva", new(-0.5f,1.2f,2.5f), Vector3.zero),
            });

            var r2 = CreateRoom("ZonaB_AulaConDesnivel", RoomType.Classroom, new[]
            {
                PP("Pizarra", new(0,1.5f,10), Vector3.zero, Vector3.one, PropSize.Dominant, required:true),
                PP("MesaProfesor", new(0,1,11.5f), Vector3.zero, Vector3.one, PropSize.Dominant, required:true),
                PP("Estanteria", new(-3.5f,0,13), new(0,90,0), Vector3.one, PropSize.Dominant, required:true),
                PP("PupitreDoble", new(-1.5f,0,8), Vector3.zero, Vector3.one, PropSize.Medium, required:true),
                PP("PupitreDoble", new(1.5f,0,8), Vector3.zero, Vector3.one, PropSize.Medium, required:true),
                PP("SillaEscolar", new(-1.5f,0,7.5f), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("SillaEscolar", new(1.5f,0,7.5f), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("Libros", new(0,1.8f,11.5f), Vector3.zero, Vector3.one*0.7f, PropSize.Small),
                PP("Libros", new(-3.5f,1.8f,13), new(0,0,5), Vector3.one*0.7f, PropSize.Small),
                PP("Basurero", new(3.5f,0,8), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("PlantaMaceta", new(3.5f,1,13), Vector3.zero, Vector3.one, PropSize.Small),
            }, new[]
            {
                DD("dec_tiza_borrada", new(0,1.2f,9.9f), Vector3.zero),
                DD("dec_nota_adhesiva", new(-0.5f,1.8f,11.5f), Vector3.zero),
            });

            var level4 = CreateLevel(4, "Level_04 -- Espera", "Assets/Scenes/Level_04.unity", Chapter.II_Coordinacion,
                new[] { r1, r2 },
                CreateCluster(4, "Tiempo Detenido", new[] { "RelojPared", "Libros" }, null, null));
            createdCount++;
        }

        // ============================================================
        // LEVEL 5 - Culpa - Cap II Coordinación
        // ============================================================
        {
            var r1 = CreateRoom("ZonaA_EntradaTecnica", RoomType.Storage, new[]
            {
                PP("Extintor", new(-1.5f,0,1), Vector3.zero, Vector3.one, PropSize.Dominant, required:true),
                PP("CarritoConserje", new(1.5f,0,2), new(0,180,0), Vector3.one, PropSize.Medium, required:true),
                PP("Casco", new(0,0,0.5f), new(0,45,0), Vector3.one, PropSize.Medium),
                PP("CajaCartonCerrada", new(-1.5f,0,3), new(0,20,0), Vector3.one, PropSize.Small),
                PP("CajaCartonAbierta", new(-1.5f,0.5f,3), Vector3.zero, Vector3.one, PropSize.Small),
                PP("Basurero", new(1.5f,0,4), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("Libros", new(0.5f,0,3), new(0,80,90), Vector3.one*0.5f, PropSize.Small),
                PP("PapeleraKenney", new(-0.5f,0,4), Vector3.zero, Vector3.one, PropSize.Small, required:true),
            }, new[]
            {
                DD("dec_grieta", new(-1.6f,1,2), Vector3.zero),
                DD("dec_humedad", new(1.6f,0.5f,1.5f), new(0,180,0)),
            });

            var r2 = CreateRoom("ZonaC_LaberintoMantenimiento", RoomType.Corridor, new[]
            {
                PP("SillaEscolar", new(-5,0,10), new(0,45,0), Vector3.one, PropSize.Medium),
                PP("TazaCafe", new(5,0,10), Vector3.zero, Vector3.one*0.4f, PropSize.Small),
                PP("Extintor", new(0,0,8), new(0,90,0), Vector3.one, PropSize.Dominant, required:true),
                PP("CajaCartonCerrada", new(-3,0,7), new(0,20,0), Vector3.one, PropSize.Medium),
                PP("CajaCartonAbierta", new(-3,0.5f,7), Vector3.zero, Vector3.one, PropSize.Small),
                PP("Casco", new(3,0,7), new(0,30,0), Vector3.one, PropSize.Small),
                PP("Basurero", new(0,0,5), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("PapeleraKenney", new(0,0,12), Vector3.zero, Vector3.one, PropSize.Small, required:true),
            }, new[]
            {
                DD("dec_foto_borrosa", new(0,1,6), Vector3.zero),
                DD("dec_arrastre", new(0,0.01f,8.5f), new(90,0,0)),
                DD("dec_humedad", new(-5.1f,0.8f,10), new(0,90,0)),
            });

            var r3 = CreateRoom("ZonaD_SalaCentral", RoomType.Lab, new[]
            {
                PP("LamparaTecho", new(0,2.8f,20), Vector3.zero, Vector3.one, PropSize.Dominant, required:true),
                PP("SillaEscolar", new(-1,0,20), new(0,90,0), Vector3.one, PropSize.Medium),
                PP("MesaKenney", new(1,0,20), Vector3.zero, Vector3.one, PropSize.Medium),
                PP("Libros", new(1,0.8f,20), new(0,10,0), Vector3.one*0.6f, PropSize.Small),
                PP("TazaCafe", new(1.3f,0.8f,20), new(0,30,0), Vector3.one*0.4f, PropSize.Small),
                PP("Radio", new(0.8f,0.8f,20), Vector3.zero, Vector3.one*0.5f, PropSize.Small),
                PP("CajaCartonCerrada", new(-2,0,21), new(0,15,0), Vector3.one, PropSize.Small),
                PP("Basurero", new(2,0,19), Vector3.zero, Vector3.one, PropSize.Small, required:true),
            }, new[]
            {
                DD("dec_humedad", new(-1.6f,0.5f,20), Vector3.zero),
                DD("dec_nota_adhesiva", new(1.6f,1.2f,20), new(0,180,0)),
            });

            var level5 = CreateLevel(5, "Level_05 -- Culpa", "Assets/Scenes/Level_05.unity", Chapter.II_Coordinacion,
                new[] { r1, r2, r3 },
                CreateCluster(5, "Voz Grabada", new[] { "Extintor", "Casco", "TazaCafe" }, null, null));
            createdCount++;
        }

        // ============================================================
        // LEVEL 6 - Negación - Cap III Confianza
        // ============================================================
        {
            var r1 = CreateRoom("ZonaA_EntradaBiblioteca", RoomType.Library, new[]
            {
                PP("MesaKenney", new(-2,0,2), Vector3.zero, Vector3.one, PropSize.Dominant, required:true),
                PP("SillaOficina", new(-2,0,3), Vector3.zero, Vector3.one, PropSize.Medium, required:true),
                PP("SillaEscolar", new(-3,0,2), new(0,90,0), Vector3.one, PropSize.Medium, required:true),
                PP("SillaEscolar", new(-1,0,2), new(0,270,0), Vector3.one, PropSize.Medium, required:true),
                PP("Libros", new(-2,0.8f,2), Vector3.zero, Vector3.one*0.8f, PropSize.Small),
                PP("PlantaMaceta", new(2,0,1), Vector3.zero, Vector3.one, PropSize.Small),
                PP("RelojPared", new(2.9f,1.5f,0.5f), new(0,90,0), Vector3.one, PropSize.Small),
                PP("Basurero", new(2,0,3), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("PapeleraKenney", new(-3.5f,0,4), Vector3.zero, Vector3.one, PropSize.Small, required:true),
            }, new[]
            {
                DD("dec_aviso_corcho", new(2.9f,1.2f,1), new(0,90,0)),
                DD("dec_humedad", new(-3.4f,0.8f,3), Vector3.zero),
            });

            var r2 = CreateRoom("ZonaB_PasilloEstanterias", RoomType.Library, new[]
            {
                PP("Estanteria", new(-1.5f,0,8), Vector3.zero, Vector3.one, PropSize.Dominant, required:true),
                PP("Estanteria", new(1.5f,0,8), Vector3.zero, Vector3.one, PropSize.Dominant, required:true),
                PP("Estanteria", new(-1.5f,0,12), Vector3.zero, Vector3.one, PropSize.Dominant, required:true),
                PP("Estanteria", new(1.5f,0,12), Vector3.zero, Vector3.one, PropSize.Dominant, required:true),
                PP("SillaEscolar", new(0,0,10), new(0,45,0), Vector3.one, PropSize.Medium, required:true),
                PP("Libros", new(-1.5f,1,8), new(0,5,0), Vector3.one*0.8f, PropSize.Small),
                PP("Libros", new(1.5f,1,8), new(0,0,5), Vector3.one*0.8f, PropSize.Small),
                PP("Libros", new(-1.5f,2,12), new(0,10,0), Vector3.one*0.8f, PropSize.Small),
                PP("Libros", new(1.5f,2,12), new(0,0,10), Vector3.one*0.8f, PropSize.Small),
                PP("BancoMadera", new(0,0,6.5f), Vector3.zero, Vector3.one, PropSize.Medium, required:true),
                PP("Basurero", new(1.8f,0,6.5f), Vector3.zero, Vector3.one, PropSize.Small, required:true),
            }, new[]
            {
                DD("dec_papel_suelo", new(0,0.01f,9), new(90,0,0)),
                DD("dec_grieta", new(-1.6f,1.5f,11), Vector3.zero),
            });

            var r3 = CreateRoom("ZonaC_SalaAbismo", RoomType.Library, new[]
            {
                PP("EstanteriaCerrada", new(-4,0,20), new(0,90,0), Vector3.one, PropSize.Dominant, required:true),
                PP("EstanteriaCerrada", new(4,0,20), new(0,270,0), Vector3.one, PropSize.Dominant, required:true),
                PP("MesaKenney", new(-2,0,18), Vector3.zero, Vector3.one, PropSize.Medium, required:true),
                PP("MesaKenney", new(-3.5f,0,18), Vector3.zero, Vector3.one, PropSize.Medium, required:true),
                PP("Libros", new(-2,0.8f,18), Vector3.zero, Vector3.one*0.8f, PropSize.Small),
                PP("Libros", new(-4,1,20), new(0,5,0), Vector3.one*0.8f, PropSize.Small),
                PP("Libros", new(4,2,20), new(0,0,5), Vector3.one*0.8f, PropSize.Small),
                PP("Basurero", new(3.5f,0,18), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("PlantaMaceta", new(-4,0,22), Vector3.zero, Vector3.one, PropSize.Small),
            }, new[]
            {
                DD("dec_humedad", new(-4.1f,0.5f,19), Vector3.zero),
                DD("dec_grieta", new(4.1f,1,21), new(0,180,0)),
            });

            var r4 = CreateRoom("ZonaD_SalaLlegada", RoomType.Library, new[]
            {
                PP("Estanteria", new(-1.5f,0,30), Vector3.zero, Vector3.one, PropSize.Dominant, required:true),
                PP("Estanteria", new(1.5f,0,30), Vector3.zero, Vector3.one, PropSize.Dominant, required:true),
                PP("MesaKenney", new(0,0,33), Vector3.zero, Vector3.one, PropSize.Medium, required:true),
                PP("SillaEscolar", new(-1,0,33), new(0,30,0), Vector3.one, PropSize.Medium, required:true),
                PP("Libros", new(-1.5f,1.5f,30), new(0,0,15), Vector3.one*0.8f, PropSize.Small),
                PP("Libros", new(1.5f,0.5f,30), Vector3.zero, Vector3.one*0.7f, PropSize.Small),
                PP("Libros", new(0,0.8f,33), new(0,10,0), Vector3.one*0.6f, PropSize.Small),
                PP("Basurero", new(-2,0,32), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("PlantaMaceta", new(2,0,32), Vector3.zero, Vector3.one, PropSize.Small),
            }, new[]
            {
                DD("dec_papel_suelo", new(-0.3f,0.01f,31), new(90,0,15)),
                DD("dec_papel_suelo", new(0.5f,0.01f,30), new(90,0,0)),
            });

            var level6 = CreateLevel(6, "Level_06 -- Negacion", "Assets/Scenes/Level_06.unity", Chapter.III_Confianza,
                new[] { r1, r2, r3, r4 },
                CreateCluster(6, "Libro en Blanco", new[] { "Libros", "SillaEscolar" }, null, null));
            createdCount++;
        }

        // ============================================================
        // LEVEL 7 - Evasión - Cap III Confianza
        // ============================================================
        {
            var r1 = CreateRoom("ZonaA_CorredorEmergencia", RoomType.Corridor, new[]
            {
                PP("Extintor", new(-1.5f,0,2), Vector3.zero, Vector3.one, PropSize.Dominant, required:true),
                PP("CarritoConserje", new(1.5f,0,3), new(0,180,0), Vector3.one, PropSize.Medium, required:true),
                PP("CajaCartonCerrada", new(-1,0,5), new(0,10,0), Vector3.one, PropSize.Medium),
                PP("Casco", new(0,0,1), new(0,45,0), Vector3.one, PropSize.Small),
                PP("Basurero", new(1.5f,0,5), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("PapeleraKenney", new(-1.5f,0,6), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("Libros", new(1,0,4), new(0,80,90), Vector3.one*0.5f, PropSize.Small),
                PP("CajaCartonAbierta", new(-1.5f,0.5f,5), Vector3.zero, Vector3.one, PropSize.Small),
            }, new[]
            {
                DD("dec_aviso_corcho", new(-1.6f,2,1), Vector3.zero),
                DD("dec_grieta", new(1.6f,1,3), new(0,180,0)),
            });

            var r2 = CreateRoom("ZonaB_PatioTrasero", RoomType.Patio, new[]
            {
                PP("BancoMadera", new(-3,0,12), Vector3.zero, Vector3.one, PropSize.Dominant, required:true),
                PP("Balon", new(2,0,14), Vector3.zero, Vector3.one, PropSize.Medium, required:true),
                PP("PlantaMaceta", new(-4,0,15), Vector3.zero, Vector3.one, PropSize.Medium, required:true),
                PP("PlantaMaceta", new(4,0,15), Vector3.zero, Vector3.one, PropSize.Medium, required:true),
                PP("Libros", new(-3,0.5f,12), new(0,15,0), Vector3.one*0.7f, PropSize.Small),
                PP("Basurero", new(3,0,10), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("Extintor", new(-4,0,10), new(0,90,0), Vector3.one, PropSize.Small, required:true),
                PP("PapeleraKenney", new(3.5f,0,14), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("CajaCartonCerrada", new(-3,0,10), new(0,20,0), Vector3.one, PropSize.Small),
            }, new[]
            {
                DD("dec_arrastre", new(0,0.01f,12), new(90,0,0)),
                DD("dec_humedad", new(-4.5f,0.5f,12), new(0,90,0)),
            });

            var r3 = CreateRoom("ZonaC_Almacenamiento", RoomType.Storage, new[]
            {
                PP("EstanteriaCerrada", new(-2,0,22), new(0,90,0), Vector3.one, PropSize.Dominant, required:true),
                PP("CajaCartonCerrada", new(1,0,22), new(0,10,0), Vector3.one, PropSize.Medium, required:true),
                PP("CajaCartonCerrada", new(1,0.55f,22), new(0,5,0), Vector3.one, PropSize.Medium, required:true),
                PP("CajaCartonAbierta", new(1,1.1f,22), Vector3.zero, Vector3.one, PropSize.Small),
                PP("Libros", new(-2,1.5f,22), Vector3.zero, Vector3.one*0.7f, PropSize.Small),
                PP("Mochila", new(-1,0,23), new(0,45,0), Vector3.one, PropSize.Small),
                PP("Basurero", new(2,0,21), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("Paraguas", new(-2.5f,0,21), new(0,30,0), Vector3.one, PropSize.Small),
            }, new[]
            {
                DD("dec_humedad", new(-2.1f,0.5f,23), Vector3.zero),
                DD("dec_papel_suelo", new(-0.5f,0.01f,22), new(90,15,0)),
            });

            var level7 = CreateLevel(7, "Level_07 -- Evasion", "Assets/Scenes/Level_07.unity", Chapter.III_Confianza,
                new[] { r1, r2, r3 },
                CreateCluster(7, "Salida Emergencia", new[] { "Balon", "CajaCartonCerrada" }, null, null));
            createdCount++;
        }

        // ============================================================
        // LEVEL 8 - Autosabotaje - Cap II Coordinación avanzada
        // ============================================================
        {
            var r1 = CreateRoom("ZonaA_Antesala", RoomType.Hall, new[]
            {
                PP("Cartelera", new(1.9f,1.2f,1), new(0,180,0), Vector3.one, PropSize.Dominant, required:true),
                PP("BancoMadera", new(-1.5f,0,2), Vector3.zero, Vector3.one, PropSize.Medium, required:true),
                PP("Perchero", new(1.5f,0,0.5f), Vector3.zero, Vector3.one, PropSize.Medium, required:true),
                PP("AbrigoColgado", new(1.5f,1.3f,0.5f), Vector3.zero, Vector3.one, PropSize.Small, matWallMustard),
                PP("Basurero", new(-1.8f,0,3), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("Paraguas", new(1.8f,0,3), new(0,30,0), Vector3.one, PropSize.Small),
                PP("Mochila", new(-1,0,3.5f), new(0,60,0), Vector3.one, PropSize.Small),
                PP("PapeleraKenney", new(-1.8f,0,1), Vector3.zero, Vector3.one, PropSize.Small, required:true),
            }, new[]
            {
                DD("dec_aviso_corcho", new(1.9f,1.2f,1), new(0,180,0)),
                DD("dec_nota_adhesiva", new(1.9f,1,0.5f), new(0,180,0)),
            });

            var r2 = CreateRoom("ZonaB_SalaProfesores", RoomType.Office, new[]
            {
                PP("Pizarra", new(0,1.2f,10), Vector3.zero, Vector3.one, PropSize.Dominant, required:true),
                PP("MesaProfesor", new(-3,0,8), Vector3.zero, Vector3.one, PropSize.Dominant, required:true),
                PP("MesaProfesor", new(3,0,8), Vector3.zero, Vector3.one, PropSize.Dominant, required:true),
                PP("MesaProfesor", new(-3,0,12), new(0,180,0), Vector3.one, PropSize.Dominant, required:true),
                PP("SillaOficina", new(-3,0,9), Vector3.zero, Vector3.one, PropSize.Medium, required:true),
                PP("SillaOficina", new(3,0,9), Vector3.zero, Vector3.one, PropSize.Medium, required:true),
                PP("SillaOficina", new(-3,0,11), new(0,180,0), Vector3.one, PropSize.Medium, required:true),
                PP("TazaCafe", new(-3,0.8f,8), new(0,30,0), Vector3.one*0.4f, PropSize.Small),
                PP("TazaCafe", new(3,0.8f,8), new(0,120,0), Vector3.one*0.4f, PropSize.Small),
                PP("Libros", new(-3,0.8f,12), Vector3.zero, Vector3.one*0.7f, PropSize.Small),
                PP("Radio", new(3,0.8f,12), Vector3.zero, Vector3.one*0.5f, PropSize.Small),
                PP("RelojPared", new(-5.9f,1.5f,10), new(0,90,0), Vector3.one, PropSize.Small),
                PP("PlantaMaceta", new(-5,0,6.5f), Vector3.zero, Vector3.one, PropSize.Small),
                PP("Basurero", new(5,0,6.5f), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("Extintor", new(5.5f,0,13), new(0,270,0), Vector3.one, PropSize.Small, required:true),
            }, new[]
            {
                DD("dec_tiza_borrada", new(0,1,9.9f), Vector3.zero),
                DD("dec_foto_borrosa", new(-5.9f,2,8), new(0,90,0)),
                DD("dec_nota_adhesiva", new(-3,0.9f,8.3f), Vector3.zero),
            });

            var r3 = CreateRoom("ZonaC_Fotocopiadora", RoomType.Office, new[]
            {
                PP("MesaKenney", new(0,0,18), Vector3.zero, Vector3.one, PropSize.Dominant, required:true),
                PP("EstanteriaCerrada", new(-2,0,18), new(0,90,0), Vector3.one, PropSize.Medium, required:true),
                PP("SillaOficina", new(1,0,18), new(0,90,0), Vector3.one, PropSize.Medium, required:true),
                PP("Libros", new(0,0.8f,18), Vector3.zero, Vector3.one*0.7f, PropSize.Small),
                PP("CajaCartonAbierta", new(0,0.8f,18.4f), Vector3.zero, Vector3.one, PropSize.Small),
                PP("Basurero", new(1.5f,0,19), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("PapeleraKenney", new(-2,0,19.5f), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("Extintor", new(2,0,17), new(0,90,0), Vector3.one, PropSize.Small, required:true),
            }, new[]
            {
                DD("dec_papel_suelo", new(0.5f,0.01f,18.5f), new(90,0,0)),
                DD("dec_humedad", new(-2.1f,0.8f,18), Vector3.zero),
            });

            var level8 = CreateLevel(8, "Level_08 -- Autosabotaje", "Assets/Scenes/Level_08.unity", Chapter.II_Coordinacion,
                new[] { r1, r2, r3 },
                CreateCluster(8, "Dos Tazas", new[] { "TazaCafe", "TazaCafe", "Libros" }, matMemory, null));
            createdCount++;
        }

        // ============================================================
        // LEVEL 9 - Control - Cap III Confianza avanzada
        // ============================================================
        {
            var r1 = CreateRoom("ZonaB_PatioExterior", RoomType.Patio, new[]
            {
                PP("PlantaMaceta", new(-12,0,-12), Vector3.zero, new Vector3(2,2,2), PropSize.Dominant),
                PP("BancoMadera", new(-10,0,0), Vector3.zero, Vector3.one, PropSize.Medium, required:true),
                PP("BancoMadera", new(10,0,0), new(0,180,0), Vector3.one, PropSize.Medium, required:true),
                PP("BancoMadera", new(0,0,-10), new(0,90,0), Vector3.one, PropSize.Medium, required:true),
                PP("CarritoConserje", new(8,0,8), new(0,45,0), Vector3.one, PropSize.Medium, required:true),
                PP("Balon", new(-5,0,5), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("Basurero", new(10,0,8), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("Basurero", new(-10,0,8), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("PapeleraKenney", new(0,0,12), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("Extintor", new(13,0,0), new(0,90,0), Vector3.one, PropSize.Small, required:true),
            }, new[]
            {
                DD("dec_arrastre", new(0,0.01f,0), new(90,0,0)),
                DD("dec_arrastre", new(5,0.01f,-3), new(90,45,0)),
            });

            var r2 = CreateRoom("ZonaC_GaleriaPerimetral", RoomType.Corridor, new[]
            {
                PP("BancoMadera", new(14,0,-5), new(0,90,0), Vector3.one, PropSize.Dominant, required:true),
                PP("PlantaMaceta", new(14,0,-8), Vector3.zero, Vector3.one, PropSize.Medium, required:true),
                PP("PlantaMaceta", new(14,0,-2), Vector3.zero, Vector3.one, PropSize.Medium, required:true),
                PP("Extintor", new(14,0,2), new(0,270,0), Vector3.one, PropSize.Small, required:true),
                PP("Basurero", new(14,0,-10), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("Paraguas", new(14,0,-12), new(0,30,0), Vector3.one, PropSize.Small),
                PP("PapeleraKenney", new(14,0,4), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("Casco", new(14,0,-6), Vector3.zero, Vector3.one, PropSize.Small),
            }, new[]
            {
                DD("dec_humedad", new(14.5f,0.5f,-5), new(0,180,0)),
                DD("dec_grieta", new(14.5f,1,-8), new(0,180,0)),
            });

            var level9 = CreateLevel(9, "Level_09 -- Control", "Assets/Scenes/Level_09.unity", Chapter.III_Confianza,
                new[] { r1, r2 },
                CreateCluster(9, "El Respiro", new[] { "Balon", "CarritoConserje", "PlantaMaceta" }, null, null));
            createdCount++;
        }

        // ============================================================
        // LEVEL 10 - Recuerdos - Cap IV Optimización
        // ============================================================
        {
            var r1 = CreateRoom("ZonaA_UmbralAulaLyra", RoomType.Hall, new[]
            {
                PP("Cartelera", new(-1.9f,1.2f,1.5f), new(0,90,0), Vector3.one, PropSize.Dominant, required:true),
                PP("PlantaMaceta", new(-1,0,0), Vector3.zero, Vector3.one, PropSize.Medium, required:true),
                PP("PlantaMaceta", new(1,0,0), Vector3.zero, Vector3.one, PropSize.Medium, required:true),
                PP("Libros", new(-1,0,0.5f), new(0,15,0), Vector3.one*0.5f, PropSize.Small),
                PP("Basurero", new(1.5f,0,1), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("Extintor", new(-1.5f,0,1), new(0,90,0), Vector3.one, PropSize.Small, required:true),
                PP("PapeleraKenney", new(0,0,1.5f), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("Paraguas", new(-1.8f,0,2), new(0,30,0), Vector3.one, PropSize.Small),
            }, new[]
            {
                DD("dec_nota_adhesiva", new(-1.9f,1.2f,0.5f), Vector3.zero),
            });

            var r2 = CreateRoom("ZonaB_AulaLyra", RoomType.Classroom, new[]
            {
                PP("Pizarra", new(0,1.2f,5), Vector3.zero, Vector3.one, PropSize.Dominant, required:true),
                PP("PupitreDoble", new(-2,0,8), Vector3.zero, Vector3.one, PropSize.Dominant, required:true),
                PP("PupitreDoble", new(0,0,8), Vector3.zero, Vector3.one, PropSize.Dominant, required:true),
                PP("PupitreDoble", new(2,0,8), Vector3.zero, Vector3.one, PropSize.Dominant, matMemory),
                PP("SillaEscolar", new(-2,0,7.5f), Vector3.zero, Vector3.one, PropSize.Medium, required:true),
                PP("SillaEscolar", new(0,0,7.5f), Vector3.zero, Vector3.one, PropSize.Medium, required:true),
                PP("SillaEscolar", new(2,0,7.5f), new(0,30,0), Vector3.one, PropSize.Medium, required:true),
                PP("MochilaLyra", new(2,0,9), new(0,45,0), Vector3.one, PropSize.Small, matMemory),
                PP("PlantaMaceta", new(4,0,5.5f), Vector3.zero, Vector3.one, PropSize.Small),
                PP("RelojPared", new(-4.9f,1.5f,5), new(0,90,0), Vector3.one, PropSize.Small),
                PP("Libros", new(-2,0.8f,8), Vector3.zero, Vector3.one*0.7f, PropSize.Small),
                PP("Basurero", new(-4,0,9), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("PapeleraKenney", new(4,0,9), Vector3.zero, Vector3.one, PropSize.Small, required:true),
            }, new[]
            {
                DD("dec_tiza_borrada", new(0,1,4.9f), Vector3.zero),
                DD("dec_foto_borrosa", new(-4.9f,2,6), new(0,90,0)),
                DD("dec_nota_adhesiva", new(2,0.9f,9.2f), Vector3.zero),
            });

            var r3 = CreateRoom("ZonaC_DespachoLateral", RoomType.Office, new[]
            {
                PP("MesaKenney", new(0,0,15), Vector3.zero, Vector3.one, PropSize.Dominant, required:true),
                PP("SillaOficina", new(0,0,16), new(0,180,0), Vector3.one, PropSize.Medium, required:true),
                PP("EstanteriaCerrada", new(-2,0,15), new(0,90,0), Vector3.one, PropSize.Medium, required:true),
                PP("Radio", new(0,0.8f,15), Vector3.zero, Vector3.one*0.5f, PropSize.Small),
                PP("Libros", new(-2,1.5f,15), Vector3.zero, Vector3.one*0.7f, PropSize.Small),
                PP("TazaCafe", new(0.4f,0.8f,15), new(0,60,0), Vector3.one*0.4f, PropSize.Small),
                PP("Basurero", new(1.5f,0,14), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("PapeleraKenney", new(-1.5f,0,14), Vector3.zero, Vector3.one, PropSize.Small, required:true),
            }, new[]
            {
                DD("dec_humedad", new(-2.1f,0.5f,16), Vector3.zero),
                DD("dec_nota_adhesiva", new(0,0.9f,15.2f), Vector3.zero),
            });

            var level10 = CreateLevel(10, "Level_10 -- Recuerdos", "Assets/Scenes/Level_10.unity", Chapter.IV_Optimizacion,
                new[] { r1, r2, r3 },
                CreateCluster(10, "Aula de Lyra", new[] { "MochilaLyra", "PupitreDoble", "PlantaMaceta", "Radio" }, matMemory, new[] { "Mat_Memory" }));
            createdCount++;
        }

        // ============================================================
        // LEVEL 11 - Conexión - Cap IV Optimización
        // ============================================================
        {
            var r1 = CreateRoom("ZonaA_BaseEscalera", RoomType.Hall, new[]
            {
                PP("Cartelera", new(2,1.2f,1), new(0,180,0), Vector3.one, PropSize.Dominant, required:true),
                PP("PlantaMaceta", new(-2,0,2), Vector3.zero, Vector3.one, PropSize.Medium, required:true),
                PP("BancoMadera", new(1.5f,0,3), Vector3.zero, Vector3.one, PropSize.Medium, required:true),
                PP("Paraguas", new(-1.8f,0,0.5f), new(0,30,0), Vector3.one, PropSize.Small),
                PP("Basurero", new(2.5f,0,3), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("Extintor", new(-2.5f,0,3), new(0,90,0), Vector3.one, PropSize.Small, required:true),
                PP("Mochila", new(-1.5f,0,4), new(0,45,0), Vector3.one, PropSize.Small),
                PP("PapeleraKenney", new(2,0,4), Vector3.zero, Vector3.one, PropSize.Small, required:true),
            }, new[]
            {
                DD("dec_aviso_corcho", new(1.9f,1.2f,1), new(0,180,0)),
                DD("dec_humedad", new(-2.9f,0.5f,2), new(0,90,0)),
            });

            var r2 = CreateRoom("ZonaC_DescansoIntermedio", RoomType.Hall, new[]
            {
                PP("BancoMadera", new(-1.5f,0,12), Vector3.zero, Vector3.one, PropSize.Dominant, required:true),
                PP("PlantaMaceta", new(1.5f,0,12), Vector3.zero, Vector3.one, PropSize.Medium, required:true),
                PP("RelojPared", new(-1.9f,1.5f,12), new(0,90,0), Vector3.one, PropSize.Medium),
                PP("Libros", new(-1.5f,0.5f,12), new(0,15,0), Vector3.one*0.6f, PropSize.Small),
                PP("Basurero", new(1.5f,0,11), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("PapeleraKenney", new(-1.5f,0,11), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("Extintor", new(1.9f,0,13), new(0,270,0), Vector3.one, PropSize.Small, required:true),
                PP("Mochila", new(0,0,13), new(0,90,0), Vector3.one, PropSize.Small),
            }, new[]
            {
                DD("dec_foto_borrosa", new(-1.9f,2,12), new(0,90,0)),
                DD("dec_grieta", new(1.9f,1,11), new(0,270,0)),
            });

            var level11 = CreateLevel(11, "Level_11 -- Conexion", "Assets/Scenes/Level_11.unity", Chapter.IV_Optimizacion,
                new[] { r1, r2 },
                CreateCluster(11, "Escalera", new[] { "Mochila", "RelojPared", "BancoMadera" }, null, null));
            createdCount++;
        }

        // ============================================================
        // LEVEL 12 - Conflicto - Cap V Consecuencia
        // ============================================================
        {
            var r1 = CreateRoom("ZonaA_AccesoGimnasio", RoomType.Gym, new[]
            {
                PP("Extintor", new(-1.5f,0,1), Vector3.zero, Vector3.one, PropSize.Medium, required:true),
                PP("CarritoConserje", new(1.5f,0,2), new(0,180,0), Vector3.one, PropSize.Medium, required:true),
                PP("Casco", new(0,0,0.5f), new(0,30,0), Vector3.one, PropSize.Small),
                PP("Basurero", new(1.8f,0,3), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("CajaCartonCerrada", new(-1,0,3), new(0,10,0), Vector3.one, PropSize.Small),
                PP("Libros", new(-1.5f,0,4), new(0,80,90), Vector3.one*0.5f, PropSize.Small),
                PP("PapeleraKenney", new(1.8f,0,4), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("Cartelera", new(-1.9f,1.2f,1), Vector3.zero, Vector3.one, PropSize.Dominant, required:true),
            }, new[]
            {
                DD("dec_grieta", new(-1.9f,1,2), Vector3.zero),
                DD("dec_humedad", new(1.9f,0.5f,1.5f), new(0,180,0)),
            });

            var r2 = CreateRoom("ZonaB_GimnasioMain", RoomType.Gym, new[]
            {
                PP("Cronometro", new(0,5,10), Vector3.zero, Vector3.one, PropSize.Dominant, required:true),
                PP("Cartelera", new(-7.9f,2,10), new(0,90,0), Vector3.one, PropSize.Dominant, required:true),
                PP("Balon", new(-3,0,7), Vector3.zero, Vector3.one, PropSize.Medium, required:true),
                PP("Balon", new(4,0,14), Vector3.zero, Vector3.one, PropSize.Medium, required:true),
                PP("BancoMadera", new(-7,0,8), new(0,90,0), Vector3.one, PropSize.Medium, required:true),
                PP("BancoMadera", new(-7,0,12), new(0,90,0), Vector3.one, PropSize.Medium, required:true),
                PP("Extintor", new(7.5f,0,6), new(0,270,0), Vector3.one, PropSize.Small, required:true),
                PP("Extintor", new(-7.5f,0,14), new(0,90,0), Vector3.one, PropSize.Small, required:true),
                PP("Basurero", new(6,0,6), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("Basurero", new(-6,0,14), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("PapeleraKenney", new(7,0,14), Vector3.zero, Vector3.one, PropSize.Small, required:true),
            }, new[]
            {
                DD("dec_arrastre", new(0,0.01f,10), new(90,0,0)),
                DD("dec_arrastre", new(2,0.01f,8), new(90,45,0)),
                DD("dec_aviso_corcho", new(-7.9f,2.5f,10), new(0,90,0)),
            });

            var r3 = CreateRoom("ZonaC_AlmacenMaterial", RoomType.Storage, new[]
            {
                PP("EstanteriaCerrada", new(-1,0,22), new(0,90,0), Vector3.one, PropSize.Dominant, required:true),
                PP("CajaCartonCerrada", new(1,0,22), new(0,10,0), Vector3.one, PropSize.Medium, required:true),
                PP("CajaCartonCerrada", new(1,0.55f,22), new(0,5,0), Vector3.one, PropSize.Medium, required:true),
                PP("CajaCartonAbierta", new(1,1.1f,22), Vector3.zero, Vector3.one, PropSize.Small),
                PP("Balon", new(-0.5f,0,23), Vector3.zero, Vector3.one, PropSize.Small),
                PP("Basurero", new(2,0,21), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("Casco", new(-1,1.5f,22), new(0,30,0), Vector3.one, PropSize.Small),
                PP("PapeleraKenney", new(-2,0,23), Vector3.zero, Vector3.one, PropSize.Small, required:true),
            }, new[]
            {
                DD("dec_humedad", new(-1.1f,0.5f,23), Vector3.zero),
                DD("dec_papel_suelo", new(0,0.01f,22.5f), new(90,0,0)),
            });

            var level12 = CreateLevel(12, "Level_12 -- Conflicto", "Assets/Scenes/Level_12.unity", Chapter.V_Consecuencia,
                new[] { r1, r2, r3 },
                CreateCluster(12, "Récords", new[] { "Cronometro", "Cartelera", "Balon" }, null, null));
            createdCount++;
        }

        // ============================================================
        // LEVEL 13 - Verdad - Cap V Consecuencia
        // ============================================================
        {
            var r1 = CreateRoom("ZonaA_UmbralRoto", RoomType.Hall, new[]
            {
                PP("Cartelera", new(-1.9f,1.2f,2.5f), new(0,90,0), Vector3.one, PropSize.Dominant, required:true),
                PP("Extintor", new(1.5f,0,1), new(0,90,0), Vector3.one, PropSize.Medium, required:true),
                PP("Basurero", new(-1.5f,0,2), Vector3.zero, Vector3.one, PropSize.Medium, required:true),
                PP("Libros", new(0,0,1.5f), new(0,20,90), Vector3.one*0.5f, PropSize.Small),
                PP("Mochila", new(-1,0,3), new(0,70,0), Vector3.one, PropSize.Small),
                PP("PapeleraKenney", new(1.5f,0,3), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("CajaCartonAbierta", new(0,0,3.5f), new(0,10,0), Vector3.one, PropSize.Small),
                PP("Paraguas", new(-1.5f,0,4), new(0,30,0), Vector3.one, PropSize.Small),
            }, new[]
            {
                DD("dec_grieta", new(-1.6f,1,2), Vector3.zero),
                DD("dec_humedad", new(1.6f,0.5f,1), new(0,180,0)),
            });

            var r2 = CreateRoom("ZonaB_AulaLyraFragmentada", RoomType.Classroom, new[]
            {
                PP("Pizarra", new(0,1.2f,8), new(0,0,15), Vector3.one, PropSize.Dominant, required:true),
                PP("PupitreDoble", new(-2,0.15f,11), new(5,0,0), Vector3.one, PropSize.Dominant, required:true),
                PP("PupitreDoble", new(0,0.1f,11), Vector3.zero, Vector3.one, PropSize.Dominant, required:true),
                PP("PupitreDoble", new(2,0.2f,11), new(3,5,0), Vector3.one, PropSize.Dominant, matMemory),
                PP("SillaEscolar", new(-2,0.15f,10.5f), new(5,0,0), Vector3.one, PropSize.Medium, required:true),
                PP("SillaEscolar", new(0,0,10.5f), new(0,45,0), Vector3.one, PropSize.Medium, required:true),
                PP("MochilaLyra", new(0,0,13), Vector3.zero, Vector3.one, PropSize.Small, matMemory),
                PP("Radio", new(-3,0,8), Vector3.zero, Vector3.one*0.5f, PropSize.Small),
                PP("Cronometro", new(3,0,8), new(0,30,0), Vector3.one*0.5f, PropSize.Small),
                PP("Libros", new(-2,0.95f,11), new(0,0,30), Vector3.one*0.7f, PropSize.Small),
                PP("Basurero", new(-4,0,9), Vector3.zero, Vector3.one, PropSize.Small, required:true),
            }, new[]
            {
                DD("dec_grieta", new(-4.9f,1,10), Vector3.zero),
                DD("dec_humedad", new(4.9f,0.5f,10), new(0,180,0)),
                DD("dec_papel_suelo", new(0,0.01f,12), new(90,30,0)),
            });

            var level13 = CreateLevel(13, "Level_13 -- Verdad", "Assets/Scenes/Level_13.unity", Chapter.V_Consecuencia,
                new[] { r1, r2 },
                CreateCluster(13, "Conversacion", new[] { "MochilaLyra", "PupitreDoble", "Radio", "Cronometro" }, matMemory, new[] { "Mat_Memory" }));
            createdCount++;
        }

        // ============================================================
        // LEVEL 14 - Aceptación - Cap VI Aceptación
        // ============================================================
        {
            var r1 = CreateRoom("ZonaA_EntradaVacio", RoomType.Corridor, new[]
            {
                PP("Locker", new(-1.8f,0,2.5f), new(0,90,0), Vector3.one, PropSize.Dominant, required:true),
                PP("Extintor", new(-1.5f,0,1), Vector3.zero, Vector3.one, PropSize.Medium, required:true),
                PP("Basurero", new(1.5f,0,1), Vector3.zero, Vector3.one, PropSize.Medium, required:true),
                PP("Libros", new(-1,0,2), new(0,15,90), Vector3.one*0.5f, PropSize.Small),
                PP("Mochila", new(1,0,3), new(0,30,0), Vector3.one, PropSize.Small),
                PP("PapeleraKenney", new(-1.5f,0,3), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("CajaCartonCerrada", new(1.5f,0,4), new(0,10,0), Vector3.one, PropSize.Small),
                PP("Paraguas", new(-1.8f,0,4), new(0,30,0), Vector3.one, PropSize.Small),
            }, new[]
            {
                DD("dec_arrastre", new(0,0.01f,2), new(90,0,0)),
            });

            var r2 = CreateRoom("ZonaB_FragmentoIzquierdo", RoomType.VoidFragment, new[]
            {
                PP("Locker", new(-8,0,8), Vector3.zero, Vector3.one, PropSize.Dominant, required:true),
                PP("LockerPuertaAbierta", new(-8,0,5), Vector3.zero, Vector3.one, PropSize.Medium),
                PP("BancoMadera", new(-6,0,5), new(0,90,0), Vector3.one, PropSize.Medium, required:true),
                PP("Extintor", new(-10,0,6), new(0,90,0), Vector3.one, PropSize.Small, required:true),
                PP("Basurero", new(-5,0,7), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("Libros", new(-8,1.8f,8), new(0,0,5), Vector3.one*0.7f, PropSize.Small),
                PP("PapeleraKenney", new(-5,0,9), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("CajaCartonCerrada", new(-10,0,9), new(0,15,0), Vector3.one, PropSize.Small),
            }, new[]
            {
                DD("dec_grieta", new(-10.1f,1,7), new(0,90,0)),
                DD("dec_humedad", new(-10.1f,0.5f,8), new(0,90,0)),
            });

            var r3 = CreateRoom("ZonaC_FragmentoDerecho", RoomType.VoidFragment, new[]
            {
                PP("Locker", new(8,0,8), new(0,180,0), Vector3.one, PropSize.Dominant, required:true),
                PP("LockerPuertaAbierta", new(8,0,5), new(0,180,0), Vector3.one, PropSize.Medium),
                PP("BancoMadera", new(6,0,5), new(0,270,0), Vector3.one, PropSize.Medium, required:true),
                PP("Extintor", new(10,0,6), new(0,270,0), Vector3.one, PropSize.Small, required:true),
                PP("Basurero", new(5,0,7), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("Libros", new(8,1.8f,8), new(0,0,5), Vector3.one*0.7f, PropSize.Small),
                PP("PapeleraKenney", new(5,0,9), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("CajaCartonCerrada", new(10,0,9), new(0,15,0), Vector3.one, PropSize.Small),
            }, new[]
            {
                DD("dec_grieta", new(10.1f,1,7), new(0,270,0)),
                DD("dec_humedad", new(10.1f,0.5f,8), new(0,270,0)),
            });

            var level14 = CreateLevel(14, "Level_14 -- Aceptacion", "Assets/Scenes/Level_14.unity", Chapter.VI_Aceptacion,
                new[] { r1, r2, r3 },
                CreateCluster(14, "Vacio", new string[0], null, new[] { "Mat_Memory", "Mat_WallRose" }));
            createdCount++;
        }

        // ============================================================
        // LEVEL 15 - Integración - Cap VI Aceptación
        // ============================================================
        {
            var r1 = CreateRoom("ZonaA_PasilloNivel1", RoomType.Corridor, new[]
            {
                PP("Locker", new(-1.8f,0,5), Vector3.zero, Vector3.one, PropSize.Dominant, required:true),
                PP("Locker", new(-1.8f,0,10), Vector3.zero, Vector3.one, PropSize.Dominant, required:true),
                PP("LockerPuertaAbierta", new(-1.8f,0,15), Vector3.zero, Vector3.one, PropSize.Medium),
                PP("BancoMadera", new(1.5f,0,8), new(0,180,0), Vector3.one, PropSize.Medium, required:true),
                PP("Libros", new(-1.6f,0,15.3f), Vector3.zero, Vector3.one*0.6f, PropSize.Small),
                PP("Extintor", new(1.8f,0,6), new(0,90,0), Vector3.one, PropSize.Small, required:true),
                PP("Basurero", new(-1.5f,0,3), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("PapeleraKenney", new(1.8f,0,12), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("Mochila", new(1,0,4), new(0,20,0), Vector3.one, PropSize.Small),
                PP("CajaCartonCerrada", new(1.5f,0,16), new(0,30,0), Vector3.one, PropSize.Small),
            }, new[]
            {
                DD("dec_grieta", new(-1.9f,1,7), Vector3.zero),
                DD("dec_papel_suelo", new(0,0.01f,9.5f), new(90,0,0)),
            });

            var r2 = CreateRoom("ZonaB_TresPuzzles", RoomType.Classroom, new[]
            {
                PP("Cartelera", new(4,1.2f,5), new(0,270,0), Vector3.one, PropSize.Dominant, required:true),
                PP("BancoMadera", new(4,0,8), new(0,270,0), Vector3.one, PropSize.Medium, required:true),
                PP("PlantaMaceta", new(4,0,10), Vector3.zero, Vector3.one, PropSize.Medium, required:true),
                PP("Libros", new(4,0.5f,8), new(0,10,0), Vector3.one*0.6f, PropSize.Small),
                PP("Extintor", new(5.5f,0,5), new(0,270,0), Vector3.one, PropSize.Small, required:true),
                PP("Basurero", new(5.5f,0,10), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("TazaCafe", new(4,0.8f,8.2f), new(0,30,0), Vector3.one*0.4f, PropSize.Small),
                PP("PapeleraKenney", new(5.5f,0,8), Vector3.zero, Vector3.one, PropSize.Small, required:true),
                PP("MochilaLyra", new(3,0,12), Vector3.zero, Vector3.one, PropSize.Small, matMemory),
            }, new[]
            {
                DD("dec_aviso_corcho", new(3.9f,1.2f,5), new(0,270,0)),
                DD("dec_nota_adhesiva", new(3.9f,1,8.2f), new(0,270,0)),
            });

            var level15 = CreateLevel(15, "Level_15 -- Integracion", "Assets/Scenes/Level_15.unity", Chapter.VI_Aceptacion,
                new[] { r1, r2 },
                CreateCluster(15, "Circulo Cierra", new[] { "MochilaLyra", "Libros", "Locker" }, matMemory, new[] { "Mat_Memory" }));
            createdCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[EnvPassDataGen] ✅ Generated {createdCount} LevelDataSO assets with all rooms, props, decals, and narrative clusters.");
    }

    // ===== HELPERS =====

    static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            var parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }

    static PropPlacementSO PP(string prefab, Vector3 pos, Vector3 rot, Vector3 scale, PropSize size,
        Material mat = null, bool required = false)
    {
        var pp = ScriptableObject.CreateInstance<PropPlacementSO>();
        pp.prefabName = prefab;
        pp.localPosition = pos;
        pp.localRotationEuler = rot;
        pp.scale = scale;
        pp.size = size;
        pp.materialOverride = mat;
        pp.requiredForRoomType = required;
        return pp;
    }

    static PropPlacementSO DD(string prefab, Vector3 pos, Vector3 rot)
    {
        var pp = ScriptableObject.CreateInstance<PropPlacementSO>();
        pp.prefabName = prefab;
        pp.localPosition = pos;
        pp.localRotationEuler = rot;
        pp.scale = Vector3.one;
        pp.size = PropSize.Small;
        return pp;
    }

    static RoomDataSO CreateRoom(string roomId, RoomType type, PropPlacementSO[] props, PropPlacementSO[] decals)
    {
        // Infer level from caller context if possible, default to Level01
        return CreateRoom(1, roomId, type, props, decals);
    }

    static RoomDataSO CreateRoom(int levelNum, string roomId, RoomType type, PropPlacementSO[] props, PropPlacementSO[] decals)
    {
        string folder = $"Assets/ScriptableObjects/EnvironmentPass/Level{levelNum:D2}/";
        EnsureFolder(folder);
        string assetPath = $"{folder}RoomData_{roomId}.asset";

        if (AssetDatabase.LoadAssetAtPath<RoomDataSO>(assetPath) != null)
            AssetDatabase.DeleteAsset(assetPath);

        var room = ScriptableObject.CreateInstance<RoomDataSO>();
        room.roomId = roomId;
        room.roomType = type;
        room.placements = new List<PropPlacementSO>(props);
        room.decals = new List<PropPlacementSO>(decals);
        room.validateRequiredProps = true;
        AssetDatabase.CreateAsset(room, assetPath);

        for (int i = 0; i < props.Length; i++)
        {
            if (props[i] != null)
            {
                props[i].name = $"Prop_{roomId}_{props[i].prefabName}_{i:D2}";
                AssetDatabase.AddObjectToAsset(props[i], room);
            }
        }
        for (int i = 0; i < decals.Length; i++)
        {
            if (decals[i] != null)
            {
                decals[i].name = $"Decal_{roomId}_{decals[i].prefabName}_{i:D2}";
                AssetDatabase.AddObjectToAsset(decals[i], room);
            }
        }

        EditorUtility.SetDirty(room);
        return room;
    }

    static LevelDataSO CreateLevel(int num, string name, string scenePath, Chapter chapter,
        RoomDataSO[] rooms, NarrativeClusterSO cluster)
    {
        string folder = $"Assets/ScriptableObjects/EnvironmentPass/Level{num:D2}/";
        EnsureFolder(folder);
        string levelPath = $"{folder}LevelData_{num:D2}.asset";

        if (AssetDatabase.LoadAssetAtPath<LevelDataSO>(levelPath) != null)
            AssetDatabase.DeleteAsset(levelPath);

        var level = ScriptableObject.CreateInstance<LevelDataSO>();
        level.levelNumber = num;
        level.levelName = name;
        level.scenePath = scenePath;
        level.chapter = chapter;
        level.rooms = new List<RoomDataSO>(rooms);
        level.narrativeCluster = cluster;
        AssetDatabase.CreateAsset(level, levelPath);
        
        if (cluster != null)
        {
            string clusterPath = $"{folder}NarrativeCluster_{num:D2}.asset";
            if (AssetDatabase.LoadAssetAtPath<NarrativeClusterSO>(clusterPath) != null)
                AssetDatabase.DeleteAsset(clusterPath);

            cluster.levelNumber = num;
            cluster.clusterName = name.Contains("--") ? name.Split(new[] { "--" }, StringSplitOptions.None)[1].Trim() : name;
            AssetDatabase.CreateAsset(cluster, clusterPath);
        }
        EditorUtility.SetDirty(level);
        return level;
    }

    static NarrativeClusterSO CreateCluster(int levelNum, string name, string[] requiredProps, Material reqMat, string[] forbidden)
    {
        var cluster = ScriptableObject.CreateInstance<NarrativeClusterSO>();
        cluster.levelNumber = levelNum;
        cluster.clusterName = name;
        if (requiredProps != null)
            cluster.requiredPrefabNames = new List<string>(requiredProps);
        cluster.requiredMaterial = reqMat;
        if (forbidden != null)
            cluster.forbiddenMaterials = new List<string>(forbidden);
        cluster.tag = NarrativeTag.Lyra;
        return cluster;
    }
}