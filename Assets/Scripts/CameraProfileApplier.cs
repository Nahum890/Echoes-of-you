using Unity.Cinemachine;
using UnityEngine;

public static class CameraProfileApplier
{
    public static void Apply(LevelCameraProfiles.Profile profile)
    {
        float world = EchoesWorldMetrics.LevelGeometryScale;
        profile.followOffset *= world;

        var vcam = GameObject.Find("PlayerVCam")?.GetComponent<CinemachineCamera>();
        if (vcam == null)
            vcam = Object.FindAnyObjectByType<CinemachineCamera>();
        if (vcam == null)
            return;

        var follow = vcam.GetComponent<CinemachineFollow>();
        if (follow != null)
            follow.FollowOffset = profile.followOffset;

        var lens = vcam.Lens;
        lens.FieldOfView = profile.fov;
        vcam.Lens = lens;

        var dynamics = vcam.gameObject.GetComponent<CinemachineGameplayDynamics>();
        if (dynamics == null)
            dynamics = vcam.gameObject.AddComponent<CinemachineGameplayDynamics>();
        dynamics.ApplyProfile(profile);
    }
}