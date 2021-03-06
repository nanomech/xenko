﻿using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.UI;

namespace SiliconStudio.Xenko.Engine
{
    public class UIElementTransformLink : TransformLink
    {
        private readonly UIComponent parentUIComponent;
        private readonly CameraComponent camera;
        private UIElement rootElement;
        private readonly bool forceRecursive;
        private string elementName;
        private UIElement followedElement;

        public UIElementTransformLink(UIComponent parentUIComponent, CameraComponent camera, string elementName, bool forceRecursive)
        {
            this.parentUIComponent = parentUIComponent;
            this.camera = camera;
            this.elementName = elementName;
            this.forceRecursive = forceRecursive;
        }

        public TransformTRS Transform;

        private UIElement FindElementByName(string name, UIElement element)
        {
            if (element == null || name == null)
                return null;

            if (name.Equals(element.Name))
                return element;

            foreach (var child in element.VisualChildren)
            {
                var childElement = FindElementByName(name, child);
                if (childElement != null)
                    return childElement;
            }

            return null;
        }


        protected CameraComponent GetUICameraComponent(UIComponent uiComponent)
        {
            var virtualResolution = uiComponent.Resolution;

            var nearPlane = virtualResolution.Z / 2;
            var farPlane = nearPlane + virtualResolution.Z;
            var zOffset = nearPlane + virtualResolution.Z / 2;
            var aspectRatio = virtualResolution.X / virtualResolution.Y;
            var verticalFov = (float)Math.Atan2(virtualResolution.Y / 2, zOffset) * 2;

            var cameraComponent = new CameraComponent(nearPlane, farPlane)
            {
                UseCustomAspectRatio = true,
                AspectRatio = aspectRatio,
                VerticalFieldOfView = MathUtil.RadiansToDegrees(verticalFov),
                ViewMatrix = Matrix.LookAtRH(new Vector3(0, 0, zOffset), Vector3.Zero, Vector3.UnitY),
                ProjectionMatrix = Matrix.PerspectiveFovRH(verticalFov, aspectRatio, nearPlane, farPlane),
            };

            return cameraComponent;
        }

        /// <inheritdoc/>
        public override void ComputeMatrix(bool recursive, out Matrix matrix)
        {
            // If model is not in the parent, we might want to force recursive update (since parentModelComponent might not be updated yet)
            if (forceRecursive || recursive)
            {
                parentUIComponent.Entity.Transform.UpdateWorldMatrix();
            }

            if (parentUIComponent.RootElement != rootElement)
            {
                rootElement = parentUIComponent.RootElement;
                followedElement = FindElementByName(elementName, rootElement);
            }

            matrix = Matrix.Identity;
            var worldMatrix = parentUIComponent.Entity.Get<TransformComponent>().WorldMatrix;

            // Updated? (rare slow path)
            if (followedElement != null)
            {
                Matrix parentWorldMatrix = Matrix.Identity;
                Matrix parentInverseMatrix = Matrix.Identity;

                if (parentUIComponent.IsFullScreen)
                {
                    // The resulting matrix should be in pixel units

                    // The fullscreen ui component is always in the center
                    worldMatrix = Matrix.Identity;

                    // The resulting matrix should be in world units
                    parentWorldMatrix.Row2 = -parentWorldMatrix.Row2;
                    parentWorldMatrix.Row3 = -parentWorldMatrix.Row3;

                    parentInverseMatrix.Row2 = -parentInverseMatrix.Row2;
                    parentInverseMatrix.Row3 = -parentInverseMatrix.Row3;

                    matrix = parentWorldMatrix * followedElement.WorldMatrix * parentInverseMatrix;
                }
                else
                {
                    if (parentUIComponent.IsBillboard && camera != null)
                    {
                        Matrix viewInverse;
                        Matrix.Invert(ref camera.ViewMatrix, out viewInverse);

                        // remove scale of the camera
                        viewInverse.Row1 /= viewInverse.Row1.XYZ().Length();
                        viewInverse.Row2 /= viewInverse.Row2.XYZ().Length();

                        // set the scale of the object
                        viewInverse.Row1 *= worldMatrix.Row1.XYZ().Length();
                        viewInverse.Row2 *= worldMatrix.Row2.XYZ().Length();

                        // set the adjusted world matrix
                        worldMatrix.Row1 = viewInverse.Row1;
                        worldMatrix.Row2 = viewInverse.Row2;
                        worldMatrix.Row3 = viewInverse.Row3;
                    }

                    // The resulting matrix should be in world units
                    parentWorldMatrix.Row2 = -parentWorldMatrix.Row2;
                    parentWorldMatrix.Row3 = -parentWorldMatrix.Row3;
                    parentWorldMatrix = Matrix.Scaling(parentUIComponent.Resolution / parentUIComponent.Size) * parentWorldMatrix;

                    parentInverseMatrix.Row2 = -parentInverseMatrix.Row2;
                    parentInverseMatrix.Row3 = -parentInverseMatrix.Row3;
                    parentInverseMatrix = Matrix.Scaling(parentUIComponent.Size / parentUIComponent.Resolution) * parentInverseMatrix;
                   // Matrix.Invert(ref parentWorldMatrix, out parentInverseMatrix);

                    matrix = parentWorldMatrix * followedElement.WorldMatrix * parentInverseMatrix;
                }

            }

            // Fallback to TransformComponent
            matrix = matrix * worldMatrix;
        }

        public bool NeedsRecreate(Entity parentEntity, string targetNodeName)
        {
            return parentUIComponent.Entity != parentEntity
                || !object.ReferenceEquals(elementName, targetNodeName); // note: supposed to use same string instance so no need to compare content
        }
    }
}
