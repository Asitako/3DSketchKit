using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ThreeDSketchKit.Editor.Tools
{
    /// <summary>
    /// One-shot generator for level-building prefabs under <c>Prefabs/BuildingBlocks</c>.
    /// Run from menu: <b>3D Sketch Kit → Generate Building Block Prefabs</b>.
    /// Ramp: place <c>RampModel.fbx</c> (or <c>Ramp.fbx</c> / OBJ variants) under <c>Prefabs/BuildingBlocks/Source</c>;
    /// otherwise a procedural wedge is saved to <c>Meshes/Ramp_Mesh.asset</c> (updated in place).
    /// Procedural wedge is authored in ~[0,1]³; prefab root scale is <see cref="RampPrefabLocalScale"/> (1 × 0.5 × 0.5 on X/Y/Z).
    /// </summary>
    public static class BuildingBlockPrefabGenerator
    {
        /// <summary>Kit ramp size after generation: length X, height Y, depth Z (procedural mesh is scaled from unit wedge).</summary>
        static readonly Vector3 RampPrefabLocalScale = new(1f, 0.5f, 0.5f);

        const string Root = "Assets/3DSketchKit/Prefabs/BuildingBlocks";
        const string MaterialsPath = Root + "/Materials";
        const string RampSourceFolder = Root + "/Source";
        const string ProceduralRampMeshPath = Root + "/Meshes/Ramp_Mesh.asset";

        static readonly string[] RampSourceModelPaths =
        {
            RampSourceFolder + "/RampModel.fbx",
            RampSourceFolder + "/Ramp.fbx",
            RampSourceFolder + "/RampModel.obj",
            RampSourceFolder + "/Ramp.obj",
            RampSourceFolder + "/RampModel.blend",
            RampSourceFolder + "/Ramp.blend",
        };

        static readonly Color DarkGray = new(0.22f, 0.23f, 0.24f, 1f);
        static readonly Color Gray = new(0.48f, 0.48f, 0.5f, 1f);
        static readonly Color White = new(0.95f, 0.95f, 0.95f, 1f);

        [MenuItem("3D Sketch Kit/Generate Building Block Prefabs", priority = 20)]
        public static void Generate()
        {
            Directory.CreateDirectory(MaterialsPath);
            AssetDatabase.Refresh();

            var matDark = GetOrCreateLitMaterial("M_SketchKit_Block_DarkGray.mat", DarkGray);
            var matGray = GetOrCreateLitMaterial("M_SketchKit_Block_Gray.mat", Gray);
            var matWhite = GetOrCreateLitMaterial("M_SketchKit_Block_White.mat", White);

            SavePrimitivePrefab("Floor", PrimitiveType.Cube, new Vector3(1f, 0.25f, 1f), matDark);
            SavePrimitivePrefab("Wall", PrimitiveType.Cube, new Vector3(0.25f, 1f, 1f), matDark);
            SavePrimitivePrefab("Pillar", PrimitiveType.Cylinder, new Vector3(0.25f, 0.5f, 0.25f), matGray);
            SavePrimitivePrefab("Beam", PrimitiveType.Cube, new Vector3(1f, 0.25f, 0.25f), matGray);
            SavePrimitivePrefab("Door", PrimitiveType.Cube, new Vector3(0.25f, 1f, 1f), matWhite);

            GenerateRampPrefab(matGray);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log("[3D Sketch Kit] Building block prefabs generated under " + Root);
        }

        static Material GetOrCreateLitMaterial(string fileName, Color baseColor)
        {
            var path = Path.Combine(MaterialsPath, fileName).Replace('\\', '/');
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                ApplyBaseColor(existing, baseColor);
                EditorUtility.SetDirty(existing);
                return existing;
            }

            var shader = FindLitShader();
            var material = new Material(shader);
            ApplyBaseColor(material, baseColor);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        static Shader FindLitShader()
        {
            var s = Shader.Find("Universal Render Pipeline/Lit");
            if (s != null)
                return s;
            s = Shader.Find("HDRP/Lit");
            if (s != null)
                return s;
            return Shader.Find("Standard");
        }

        static void ApplyBaseColor(Material material, Color color)
        {
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            else if (material.HasProperty("_Color"))
                material.SetColor("_Color", color);
            if (material.HasProperty("_Surface"))
                material.SetFloat("_Surface", 0f);
        }

        static void SavePrimitivePrefab(string objectName, PrimitiveType primitive, Vector3 localScale, Material material)
        {
            var gameObject = GameObject.CreatePrimitive(primitive);
            gameObject.name = objectName;
            gameObject.transform.localScale = localScale;

            var renderer = gameObject.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;

            var prefabPath = $"{Root}/{objectName}.prefab";
            PrefabUtility.SaveAsPrefabAsset(gameObject, prefabPath);
            Object.DestroyImmediate(gameObject);
        }

        static void GenerateRampPrefab(Material material)
        {
            if (TryFindRampSourceMesh(out var sourceMesh, out var sourcePath))
            {
                UnityEngine.Debug.Log("[3D Sketch Kit] Ramp prefab uses imported mesh from: " + sourcePath + " (\"" + sourceMesh.name + "\").");
                RemoveProceduralRampMeshAssetIfPresent();
                SaveRampPrefab(sourceMesh, material);
            }
            else
            {
                var meshAsset = GetOrUpdateProceduralRampMeshAsset();
                SaveRampPrefab(meshAsset, material);
            }
        }

        static void RemoveProceduralRampMeshAssetIfPresent()
        {
            if (AssetDatabase.LoadAssetAtPath<Mesh>(ProceduralRampMeshPath) == null)
                return;
            AssetDatabase.DeleteAsset(ProceduralRampMeshPath);
        }

        /// <summary>
        /// Looks under <see cref="RampSourceFolder"/> for common Blender/Unity export names and returns a mesh sub-asset.
        /// Prefers a mesh whose name contains "Ramp" (case-insensitive).
        /// </summary>
        static bool TryFindRampSourceMesh(out Mesh mesh, out string assetPath)
        {
            mesh = null;
            assetPath = null;
            Directory.CreateDirectory(RampSourceFolder);

            foreach (var path in RampSourceModelPaths)
            {
                if (string.IsNullOrEmpty(path) || AssetDatabase.LoadMainAssetAtPath(path) == null)
                    continue;

                var meshes = AssetDatabase.LoadAllAssetsAtPath(path)
                    .OfType<Mesh>()
                    .Where(m => m != null && m.vertexCount > 0)
                    .ToList();
                if (meshes.Count == 0)
                    continue;

                var pick = meshes.FirstOrDefault(m => m.name.IndexOf("ramp", System.StringComparison.OrdinalIgnoreCase) >= 0)
                           ?? meshes[0];
                mesh = pick;
                assetPath = path;
                return true;
            }

            return false;
        }

        static Mesh GetOrUpdateProceduralRampMeshAsset()
        {
            var meshesFolder = $"{Root}/Meshes";
            Directory.CreateDirectory(meshesFolder);

            var template = CreateRampWedgeMesh();
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(ProceduralRampMeshPath);
            if (existing != null)
            {
                CopyMeshContents(existing, template);
                Object.DestroyImmediate(template);
                existing.name = "Ramp_Mesh";
                EditorUtility.SetDirty(existing);
                return existing;
            }

            template.name = "Ramp_Mesh";
            AssetDatabase.CreateAsset(template, ProceduralRampMeshPath);
            return AssetDatabase.LoadAssetAtPath<Mesh>(ProceduralRampMeshPath);
        }

        static void CopyMeshContents(Mesh target, Mesh source)
        {
            target.Clear();
            target.vertices = source.vertices;
            target.triangles = source.triangles;
            target.normals = source.normals;
            target.tangents = source.tangents;
            target.uv = source.uv;
            if (source.uv2 != null && source.uv2.Length == source.vertexCount)
                target.uv2 = source.uv2;
            if (target.normals == null || target.normals.Length != target.vertexCount)
            {
                target.RecalculateNormals();
                target.RecalculateTangents();
            }
            else if (target.tangents == null || target.tangents.Length != target.vertexCount)
                target.RecalculateTangents();

            target.RecalculateBounds();
        }

        static void SaveRampPrefab(Mesh sharedMesh, Material material)
        {
            var gameObject = new GameObject("Ramp");
            try
            {
                gameObject.transform.localScale = RampPrefabLocalScale;

                var meshFilter = gameObject.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = sharedMesh;

                var meshRenderer = gameObject.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = material;

                var meshCollider = gameObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = sharedMesh;
                meshCollider.convex = false;

                if (!sharedMesh.isReadable)
                    UnityEngine.Debug.LogWarning(
                        "[3D Sketch Kit] Ramp mesh \"" + sharedMesh.name + "\" is not CPU-readable. " +
                        "Enable **Read/Write** on the model import settings (Inspector) if MeshCollider does not work.");

                PrefabUtility.SaveAsPrefabAsset(gameObject, $"{Root}/Ramp.prefab");
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        /// <summary>
        /// Unit right triangular prism: base triangle in the horizontal plane (y=0) with vertices (0,0,0), (1,0,0), (0,0,1);
        /// copy at y=1 — walkable slope on the hypotenuse quad between (1,0,0)-(0,0,1) and (1,1,0)-(0,1,1).
        /// </summary>
        static Mesh CreateRampWedgeMesh()
        {
            var mesh = new Mesh { name = "RampWedge_1m" };

            var v0 = new Vector3(0f, 0f, 0f);
            var v1 = new Vector3(1f, 0f, 0f);
            var v2 = new Vector3(0f, 0f, 1f);
            var v3 = new Vector3(0f, 1f, 0f);
            var v4 = new Vector3(1f, 1f, 0f);
            var v5 = new Vector3(0f, 1f, 1f);

            var vertices = new[] { v0, v1, v2, v3, v4, v5 };
            mesh.vertices = vertices;

            mesh.triangles = new[]
            {
                0, 1, 2,
                3, 5, 4,
                0, 3, 5,
                0, 5, 2,
                0, 1, 4,
                0, 4, 3,
                1, 2, 5,
                1, 5, 4
            };

            var uv = new Vector2[vertices.Length];
            for (var i = 0; i < vertices.Length; i++)
                uv[i] = new Vector2(vertices[i].x + vertices[i].z * 0.25f, vertices[i].y);
            mesh.uv = uv;

            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
