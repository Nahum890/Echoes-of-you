using System;
using System.Reflection;
using UnityEditor;

[InitializeOnLoad]
public class FixAnimationsAuto
{
    const string RepairVersionKey = "EchoesAnimationsFixed_20260521";

    static FixAnimationsAuto()
    {
        EditorApplication.delayCall += RunFixOnce;
    }

    static void RunFixOnce()
    {
        if (EditorPrefs.GetBool(RepairVersionKey, false))
            return;

        RunFix();
    }

    [MenuItem("Echoes/Force Fix Character and Animations")]
    public static void RunFix()
    {
        Type repairType = Type.GetType("PlayerAnimationRepair");
        if (repairType == null)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length && repairType == null; i++)
                repairType = assemblies[i].GetType("PlayerAnimationRepair");
        }

        MethodInfo method = repairType?.GetMethod("RepairProjectAnimationSetup", BindingFlags.Public | BindingFlags.Static);
        if (method != null)
        {
            method.Invoke(null, null);
        }
        else
        {
            RigSetup.EnsureGenericAnimationRigs();
            SetupPlayerAnimator.Setup();
        }

        EditorPrefs.SetBool(RepairVersionKey, true);
    }
}
