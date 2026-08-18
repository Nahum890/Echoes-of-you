#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// QA Validator for Pressure Plate actor acceptance:
/// - Player stepping on EchoOnly plate => FAIL (plate remains unpressed, actor physically rejected)
/// - Echo stepping on EchoOnly plate => PASS (plate activates)
/// - Player stepping on standard plate => PASS (plate activates)
/// - Echo stepping on standard plate => PASS (plate activates)
/// </summary>
public static class EchoButtonTestValidator
{
    public struct ValidationResult
    {
        public string testName;
        public bool passed;
        public string details;
    }

    [MenuItem("Echoes of You/QA/Validate Echo Plates (EchoButtonTestValidator)")]
    public static void RunValidationMenu()
    {
        var results = RunAllTests();
        int passedCount = 0;
        foreach (var res in results)
        {
            if (res.passed)
            {
                passedCount++;
                Debug.Log($"[EchoButtonTestValidator] PASS: {res.testName} - {res.details}");
            }
            else
            {
                Debug.LogError($"[EchoButtonTestValidator] FAIL: {res.testName} - {res.details}");
            }
        }

        Debug.Log($"[EchoButtonTestValidator] Completed {results.Count} tests. Passed: {passedCount}/{results.Count}");
    }

    public static List<ValidationResult> RunAllTests()
    {
        var results = new List<ValidationResult>();

        // 1. Test standard pressure plate accepts player
        results.Add(TestStandardPlateWithPlayer());

        // 2. Test standard pressure plate accepts echo
        results.Add(TestStandardPlateWithEcho());

        // 3. Test EchoOnly plate rejects player (PLAYER -> EchoOnly = FAIL to activate)
        results.Add(TestEchoOnlyPlateWithPlayer());

        // 4. Test EchoOnly plate accepts echo (ECHO -> EchoOnly = PASS)
        results.Add(TestEchoOnlyPlateWithEcho());

        // 5. Test Blueprints N01-N03 plate configuration
        results.Add(TestBlueprintPlateConfigurations());

        return results;
    }

    private static ValidationResult TestStandardPlateWithPlayer()
    {
        GameObject plateObj = new GameObject("Test_StandardPlate");
        GameObject playerObj = new GameObject("Test_Player");
        try
        {
            playerObj.tag = "Player";
            BoxCollider playerCol = playerObj.AddComponent<BoxCollider>();
            playerCol.size = Vector3.one;

            BoxCollider plateCol = plateObj.AddComponent<BoxCollider>();
            plateCol.size = new Vector3(2f, 0.12f, 2f);
            plateCol.isTrigger = true;

            PressurePlate plate = plateObj.AddComponent<PressurePlate>();
            plate.ConfigureAcceptedActors(true, true, true);

            bool accepted = plate.acceptPlayer;
            return new ValidationResult
            {
                testName = "Standard Plate accepts Player",
                passed = accepted,
                details = accepted ? "Player is accepted by standard plate." : "Player was unexpectedly rejected."
            };
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(plateObj);
            UnityEngine.Object.DestroyImmediate(playerObj);
        }
    }

    private static ValidationResult TestStandardPlateWithEcho()
    {
        GameObject plateObj = new GameObject("Test_StandardPlate_Echo");
        GameObject echoObj = new GameObject("Test_Echo");
        try
        {
            echoObj.tag = "Echo";
            BoxCollider echoCol = echoObj.AddComponent<BoxCollider>();
            echoCol.size = Vector3.one;

            BoxCollider plateCol = plateObj.AddComponent<BoxCollider>();
            plateCol.size = new Vector3(2f, 0.12f, 2f);
            plateCol.isTrigger = true;

            PressurePlate plate = plateObj.AddComponent<PressurePlate>();
            plate.ConfigureAcceptedActors(true, true, true);

            bool accepted = plate.acceptEcho;
            return new ValidationResult
            {
                testName = "Standard Plate accepts Echo",
                passed = accepted,
                details = accepted ? "Echo is accepted by standard plate." : "Echo was unexpectedly rejected."
            };
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(plateObj);
            UnityEngine.Object.DestroyImmediate(echoObj);
        }
    }

    private static ValidationResult TestEchoOnlyPlateWithPlayer()
    {
        GameObject plateObj = new GameObject("Test_EchoOnlyPlate_Player");
        GameObject playerObj = new GameObject("Test_Player");
        try
        {
            playerObj.tag = "Player";
            playerObj.AddComponent<CharacterController>();

            BoxCollider plateCol = plateObj.AddComponent<BoxCollider>();
            plateCol.size = new Vector3(2f, 0.12f, 2f);
            plateCol.isTrigger = true;

            PressurePlate plate = plateObj.AddComponent<PressurePlate>();
            PressurePlateEchoOnly echoOnly = plateObj.AddComponent<PressurePlateEchoOnly>();
            echoOnly.Initialize();

            bool playerAccepted = plate.acceptPlayer;
            Transform barrier = plateObj.transform.Find("EchoOnly_PlayerBarrier");
            bool barrierExists = barrier != null && barrier.CompareTag("PlayerOnlyBarrier");

            bool pass = (!playerAccepted) && barrierExists;
            return new ValidationResult
            {
                testName = "PLAYER -> EchoOnly = FAIL (rejection active)",
                passed = pass,
                details = pass
                    ? "Player cannot activate EchoOnly plate (acceptPlayer=false) and physical barrier is active."
                    : $"Failed. acceptPlayer={playerAccepted}, barrierExists={barrierExists}"
            };
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(plateObj);
            UnityEngine.Object.DestroyImmediate(playerObj);
        }
    }

    private static ValidationResult TestEchoOnlyPlateWithEcho()
    {
        GameObject plateObj = new GameObject("Test_EchoOnlyPlate_Echo");
        GameObject echoObj = new GameObject("Test_Echo");
        try
        {
            echoObj.tag = "Echo";
            BoxCollider echoCol = echoObj.AddComponent<BoxCollider>();
            echoCol.size = Vector3.one;

            BoxCollider plateCol = plateObj.AddComponent<BoxCollider>();
            plateCol.size = new Vector3(2f, 0.12f, 2f);
            plateCol.isTrigger = true;

            PressurePlate plate = plateObj.AddComponent<PressurePlate>();
            PressurePlateEchoOnly echoOnly = plateObj.AddComponent<PressurePlateEchoOnly>();
            echoOnly.Initialize();

            bool echoAccepted = plate.acceptEcho;
            bool projectionAccepted = plate.acceptEchoProjection;
            bool pass = echoAccepted && projectionAccepted;

            return new ValidationResult
            {
                testName = "ECHO -> EchoOnly = PASS",
                passed = pass,
                details = pass
                    ? "Echo and EchoProjection are accepted by EchoOnly plate."
                    : $"Failed. acceptEcho={echoAccepted}, acceptEchoProjection={projectionAccepted}"
            };
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(plateObj);
            UnityEngine.Object.DestroyImmediate(echoObj);
        }
    }

    private static ValidationResult TestBlueprintPlateConfigurations()
    {
        var bp3 = AssetDatabase.LoadAssetAtPath<LevelBlueprint>("Assets/Data/Levels/Level_03_Blueprint.asset");
        if (bp3 == null)
        {
            return new ValidationResult
            {
                testName = "Blueprint N03 EchoOnly verification",
                passed = false,
                details = "Could not load Level_03_Blueprint.asset"
            };
        }

        bool foundEchoPlate = false;
        foreach (var mod in bp3.modules)
        {
            if (mod.name == "PlacaEco_RamaDerecha")
            {
                if (!string.IsNullOrEmpty(mod.customData) && mod.customData.Contains("EchoOnly"))
                {
                    foundEchoPlate = true;
                    break;
                }
            }
        }

        return new ValidationResult
        {
            testName = "Blueprint N03 EchoOnly Module Setup",
            passed = foundEchoPlate,
            details = foundEchoPlate
                ? "PlacaEco_RamaDerecha is properly tagged with customData: EchoOnly in Level_03_Blueprint."
                : "PlacaEco_RamaDerecha missing EchoOnly customData in Level_03_Blueprint."
        };
    }
}
#endif
