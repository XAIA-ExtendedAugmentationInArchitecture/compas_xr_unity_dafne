using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace MixedReality.Toolkit.SpatialManipulation
{
    public class BoxColliderRotateLogic : ManipulationLogic<Quaternion>
    {
        private Vector3 startHandlebar;
        private Quaternion startInputRotation;
        private Quaternion startRotation;

        private bool ShouldMatchAttachRotation => SelectedBySocket;

        public override void Setup(List<IXRSelectInteractor> interactors, IXRSelectInteractable interactable, MixedRealityTransform currentTarget)
        {
            base.Setup(interactors, interactable, currentTarget);

            if (NumInteractors >= 2)
            {
                startHandlebar = GetHandlebarDirection(interactors, interactable);
            }

            startInputRotation = interactors[0].GetAttachTransform(interactable).rotation;
            startRotation = currentTarget.Rotation;
        }

        public override Quaternion Update(List<IXRSelectInteractor> interactors, IXRSelectInteractable interactable, MixedRealityTransform currentTarget, bool centeredAnchor)
        {
            base.Update(interactors, interactable, currentTarget, centeredAnchor);

            if (ShouldMatchAttachRotation)
            {
                return interactors[0].GetAttachTransform(interactable).rotation;
            }

            Quaternion deltaRotation = NumInteractors == 1
                ? interactors[0].GetAttachTransform(interactable).rotation * Quaternion.Inverse(startInputRotation)
                : Quaternion.FromToRotation(startHandlebar, GetHandlebarDirection(interactors, interactable));

            if (centeredAnchor && interactable is ObjectManipulator manipulator)
            {
                var host = manipulator.HostTransform;
                var box = host.GetComponent<BoxCollider>();

                if (box != null)
                {
                    Vector3 pivot = box.transform.TransformPoint(box.center);
                    Debug.Log($"Pivot: {pivot}");
                    Vector3 offset = currentTarget.Position - pivot;
                    offset = deltaRotation * offset;
                    Vector3 newPosition = pivot + offset;

                    // Instead of applying directly to transform, return updated transform via ObjectManipulator
                    manipulator.HostTransform.position = newPosition;
                }
            }

            return deltaRotation * startRotation;
        }

        private static Vector3 GetHandlebarDirection(List<IXRSelectInteractor> interactors, IXRSelectInteractable interactable)
        {
            Debug.Assert(interactors.Count >= 2, $"GetHandlebarDirection called with less than 2 interactors ({interactors.Count}).");
            return interactors[1].GetAttachTransform(interactable).position - interactors[0].GetAttachTransform(interactable).position;
        }
    }
}
