using System;
using System.Collections.Generic;
using RenderWareIo.Structs.Ifp;
using UnityEngine;

using RwQuaternion = System.Numerics.Quaternion;
using RwVector3 = System.Numerics.Vector3;

namespace GTA3Unity.Utility
{
    public static class IfpAnimationConverter
    {
        private const float MinimumFrameStep = 1.0f / 30.0f;

        public static AnimationClip CreateLegacyClip(
            IfpAnimation animation,
            Transform root,
            bool makeInPlace = false)
        {
            if (animation == null)
            {
                throw new ArgumentNullException(nameof(animation));
            }

            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            AnimationClip clip = new AnimationClip
            {
                name = animation.Name,
                legacy = true,
                wrapMode = WrapMode.Loop
            };

            Dictionary<string, Transform> transformsByName =
                CollectTransformsByName(root);

            foreach (IfpObjectAnimation obj in animation.Objects)
            {
                if (string.IsNullOrEmpty(obj.Name) ||
                    obj.Frames == null ||
                    obj.Frames.Count == 0 ||
                    !transformsByName.TryGetValue(obj.Name, out Transform target))
                {
                    continue;
                }

                string path = GetRelativePath(root, target);
                AddRotationCurves(clip, path, obj.Frames);

                if (HasPositionFrames(obj.Frames))
                {
                    AddPositionCurves(
                        clip,
                        path,
                        obj.Frames,
                        makeInPlace && IsRootMotionTransform(root, target),
                        target.localPosition);
                }

                if (HasScaleFrames(obj.Frames))
                {
                    AddScaleCurves(clip, path, obj.Frames);
                }
            }

            clip.EnsureQuaternionContinuity();
            return clip;
        }

        private static Dictionary<string, Transform> CollectTransformsByName(Transform root)
        {
            Dictionary<string, Transform> transforms =
                new Dictionary<string, Transform>(StringComparer.OrdinalIgnoreCase);

            foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
            {
                if (!transforms.ContainsKey(transform.name))
                {
                    transforms.Add(transform.name, transform);
                }
            }

            return transforms;
        }

        private static void AddRotationCurves(
            AnimationClip clip,
            string path,
            List<IfpFrame> frames)
        {
            AnimationCurve x = new AnimationCurve();
            AnimationCurve y = new AnimationCurve();
            AnimationCurve z = new AnimationCurve();
            AnimationCurve w = new AnimationCurve();

            for (int frameIndex = 0; frameIndex < frames.Count; frameIndex++)
            {
                float time = GetFrameTime(frames, frameIndex);
                Quaternion rotation = ConvertQuaternion(frames[frameIndex].Rotation);

                x.AddKey(time, rotation.x);
                y.AddKey(time, rotation.y);
                z.AddKey(time, rotation.z);
                w.AddKey(time, rotation.w);
            }

            clip.SetCurve(path, typeof(Transform), "m_LocalRotation.x", x);
            clip.SetCurve(path, typeof(Transform), "m_LocalRotation.y", y);
            clip.SetCurve(path, typeof(Transform), "m_LocalRotation.z", z);
            clip.SetCurve(path, typeof(Transform), "m_LocalRotation.w", w);
        }

        private static void AddPositionCurves(
            AnimationClip clip,
            string path,
            List<IfpFrame> frames,
            bool makeInPlace,
            Vector3 bindPosition)
        {
            AnimationCurve x = new AnimationCurve();
            AnimationCurve y = new AnimationCurve();
            AnimationCurve z = new AnimationCurve();

            for (int frameIndex = 0; frameIndex < frames.Count; frameIndex++)
            {
                float time = GetFrameTime(frames, frameIndex);
                Vector3 position = ConvertVector(frames[frameIndex].Position);

                if (makeInPlace)
                {
                    position.x = bindPosition.x;
                    position.z = bindPosition.z;
                }

                x.AddKey(time, position.x);
                y.AddKey(time, position.y);
                z.AddKey(time, position.z);
            }

            clip.SetCurve(path, typeof(Transform), "m_LocalPosition.x", x);
            clip.SetCurve(path, typeof(Transform), "m_LocalPosition.y", y);
            clip.SetCurve(path, typeof(Transform), "m_LocalPosition.z", z);
        }

        private static bool IsRootMotionTransform(Transform root, Transform target)
        {
            return target == root || target.parent == root;
        }

        private static void AddScaleCurves(
            AnimationClip clip,
            string path,
            List<IfpFrame> frames)
        {
            AnimationCurve x = new AnimationCurve();
            AnimationCurve y = new AnimationCurve();
            AnimationCurve z = new AnimationCurve();

            for (int frameIndex = 0; frameIndex < frames.Count; frameIndex++)
            {
                float time = GetFrameTime(frames, frameIndex);
                Vector3 scale = ConvertVector(frames[frameIndex].Scale);

                x.AddKey(time, scale.x);
                y.AddKey(time, scale.y);
                z.AddKey(time, scale.z);
            }

            clip.SetCurve(path, typeof(Transform), "m_LocalScale.x", x);
            clip.SetCurve(path, typeof(Transform), "m_LocalScale.y", y);
            clip.SetCurve(path, typeof(Transform), "m_LocalScale.z", z);
        }

        private static bool HasPositionFrames(List<IfpFrame> frames)
        {
            foreach (IfpFrame frame in frames)
            {
                if (frame.HasPosition)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasScaleFrames(List<IfpFrame> frames)
        {
            foreach (IfpFrame frame in frames)
            {
                if (frame.HasScale)
                {
                    return true;
                }
            }

            return false;
        }

        private static float GetFrameTime(List<IfpFrame> frames, int frameIndex)
        {
            float time = frames[frameIndex].Time;

            if (frameIndex > 0 && time <= frames[frameIndex - 1].Time)
            {
                time = frameIndex * MinimumFrameStep;
            }

            return Math.Max(0.0f, time);
        }

        private static string GetRelativePath(Transform root, Transform target)
        {
            if (root == target)
            {
                return string.Empty;
            }

            Stack<string> parts = new Stack<string>();
            Transform current = target;

            while (current != null && current != root)
            {
                parts.Push(current.name);
                current = current.parent;
            }

            return string.Join("/", parts);
        }

        private static Vector3 ConvertVector(RwVector3 vector)
        {
            return new Vector3(
                vector.X,
                vector.Z,
                vector.Y);
        }

        private static Quaternion ConvertQuaternion(RwQuaternion quaternion)
        {
            Quaternion converted = new Quaternion(
                quaternion.X,
                quaternion.Z,
                quaternion.Y,
                quaternion.W);

            if (converted.x == 0.0f &&
                converted.y == 0.0f &&
                converted.z == 0.0f &&
                converted.w == 0.0f)
            {
                return Quaternion.identity;
            }

            return Normalize(converted);
        }

        private static Quaternion Normalize(Quaternion quaternion)
        {
            float magnitude = Mathf.Sqrt(
                quaternion.x * quaternion.x +
                quaternion.y * quaternion.y +
                quaternion.z * quaternion.z +
                quaternion.w * quaternion.w);

            if (magnitude <= 0.0001f)
            {
                return Quaternion.identity;
            }

            return new Quaternion(
                quaternion.x / magnitude,
                quaternion.y / magnitude,
                quaternion.z / magnitude,
                quaternion.w / magnitude);
        }
    }
}
