using UnityEngine;

public static class LevelCameraProfiles
{
    public struct Profile
    {
        public EchoesCameraIdentity identity;
        public Vector3 followOffset;
        public float fov;
        public float playerWeight;
        public float goalWeight;
        public float dutchMax;
        public float noiseAmplitude;
        public float followResponsiveness;
        public float velocityFovBoost;
        public float tiltOnJump;
        public float driftDelay;
    }

    public static bool TryGet(string sceneName, out Profile profile)
    {
        switch (sceneName)
        {
            case "Level_01":
                profile = Build(EchoesCameraIdentity.WideLiminal,
                    new Vector3(-7f, 2.4f, -11f), 54f, 1.1f, 0.7f, 0.6f, 0.12f, 2.2f, 2f, 4f);
                return true;
            case "Level_02":
                profile = Build(EchoesCameraIdentity.DynamicFollow,
                    new Vector3(-5f, 3.4f, -8f), 56f, 1.55f, 0.35f, 2.4f, 0.22f, 5.5f, 6f, 8f);
                return true;
            case "Level_03":
                profile = Build(EchoesCameraIdentity.SideCinematic,
                    new Vector3(-12f, 2.6f, 0.5f), 50f, 1.35f, 0.5f, 3f, 0.18f, 4f, 3f, 5f);
                return true;
            case "Level_04":
                profile = Build(EchoesCameraIdentity.DynamicFollow,
                    new Vector3(-6f, 3.8f, -9f), 55f, 1.5f, 0.45f, 2.2f, 0.2f, 5f, 6.5f, 7f);
                return true;
            case "Level_05":
                profile = Build(EchoesCameraIdentity.Memory,
                    new Vector3(-8f, 4.2f, -10f), 48f, 1.15f, 0.85f, 2.8f, 0.34f, 2.8f, 2f, 3f, 0.35f);
                return true;
            case "Level_06":
                profile = Build(EchoesCameraIdentity.TopDescent,
                    new Vector3(-4f, 9f, -6f), 46f, 1.2f, 0.9f, 3.2f, 0.28f, 3.5f, 3f, 4f);
                return true;
            case "Level_07":
                profile = Build(EchoesCameraIdentity.SideCinematic,
                    new Vector3(-11f, 3f, 1f), 51f, 1.4f, 0.55f, 2.6f, 0.2f, 4.2f, 4f, 6f);
                return true;
            case "Level_08":
                profile = Build(EchoesCameraIdentity.DynamicFollow,
                    new Vector3(-5.5f, 3.6f, -8.5f), 57f, 1.6f, 0.4f, 2.5f, 0.24f, 6f, 7f, 9f);
                return true;
            case "Level_09":
                profile = Build(EchoesCameraIdentity.WideLiminal,
                    new Vector3(-10f, 5.5f, -13f), 44f, 1.05f, 0.95f, 1.2f, 0.3f, 2.5f, 2.5f, 3f);
                return true;
            case "Level_10":
                profile = Build(EchoesCameraIdentity.Memory,
                    new Vector3(-9f, 5f, -12f), 47f, 1.2f, 0.82f, 3f, 0.36f, 3f, 2.5f, 3.5f, 0.42f);
                return true;
            case "Level_11":
                profile = Build(EchoesCameraIdentity.DynamicFollow,
                    new Vector3(-6f, 4.2f, -9f), 58f, 1.65f, 0.35f, 2.8f, 0.24f, 6.2f, 8f, 9f);
                return true;
            case "Level_12":
                profile = Build(EchoesCameraIdentity.WideLiminal,
                    new Vector3(-10f, 7f, -12f), 45f, 1.1f, 0.9f, 1.4f, 0.28f, 2.8f, 2.5f, 4f);
                return true;
            case "Level_13":
                profile = Build(EchoesCameraIdentity.Memory,
                    new Vector3(-10f, 6f, -12f), 46f, 1.25f, 0.95f, 3.4f, 0.38f, 3.2f, 4f, 5f, 0.48f);
                return true;
            case "Level_14":
                profile = Build(EchoesCameraIdentity.DynamicFollow,
                    new Vector3(-5.5f, 3.4f, -8f), 59f, 1.75f, 0.3f, 3.2f, 0.28f, 7f, 9f, 10f);
                return true;
            case "Level_15":
                profile = Build(EchoesCameraIdentity.WideLiminal,
                    new Vector3(-11f, 7f, -14f), 43f, 1.08f, 1f, 2.6f, 0.36f, 2.6f, 3.5f, 5f, 0.35f);
                return true;
            default:
                profile = default;
                return false;
        }
    }

    static Profile Build(
        EchoesCameraIdentity identity,
        Vector3 offset,
        float fov,
        float playerWeight,
        float goalWeight,
        float dutchMax,
        float noise,
        float responsiveness,
        float fovBoost,
        float jumpTilt,
        float driftDelay = 0f)
    {
        return new Profile
        {
            identity = identity,
            followOffset = offset,
            fov = fov,
            playerWeight = playerWeight,
            goalWeight = goalWeight,
            dutchMax = dutchMax,
            noiseAmplitude = noise,
            followResponsiveness = responsiveness,
            velocityFovBoost = fovBoost,
            tiltOnJump = jumpTilt,
            driftDelay = driftDelay
        };
    }
}
