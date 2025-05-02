using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System;

namespace MixedReality.Toolkit.SpatialManipulation
{
    public class BoxColliderManipulator : ObjectManipulator
    {
        private Vector3 startPivot;
        private Vector3 startPos;
        private Quaternion startRot;

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            if (HostTransform.TryGetComponent<BoxCollider>(out var box))
            {
                // Record the world‐space pivot and start pose
                startPivot = box.transform.TransformPoint(box.center);
                startPos   = HostTransform.position;
                startRot   = HostTransform.rotation;
            }
        }

        protected override void ModifyTargetPose(ref MixedRealityTransform targetPose,
                                                 ref TransformFlags modifiedTransformFlags)
        {
            // Let MRTK do all its smoothing/constraints first
            base.ModifyTargetPose(ref targetPose, ref modifiedTransformFlags);

            // Only when our enum is set to use the box‐collider center
            var rotateType = CurrentInteractionType == InteractionFlags.Near
                                 ? RotationAnchorNear
                                 : RotationAnchorFar;
            if (rotateType == RotateAnchorType.RotateAboutBoxColliderCenter)
            {
                // Compute how much it’s rotated since we started
                var delta = targetPose.Rotation * Quaternion.Inverse(startRot);

                // Pivot around our box‐center
                targetPose.Position = startPivot
                                     + delta * (startPos - startPivot);

                // Mark that we’ve changed both move and rotate on the targetPose
                modifiedTransformFlags |= TransformFlags.Move | TransformFlags.Rotate;
            }
        }
    }
}
