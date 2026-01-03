using System;
using System.Collections.Generic;
using SmartDebugger;
using UnityEngine;

public class Demo : MonoBehaviour
{
    void Start()
    {
        Application.targetFrameRate = 60;
        SmartDebug.Instance.AddFieldLayout(new ShapeFields());
        SmartDebug.Instance.AddFieldLayout(new CameraFields());
        SmartDebug.Instance.AddFieldLayout(new SupportedTypeFields());
    }

    private void Update()
    {
        var frameCount = Time.frameCount;
        if (frameCount % 400 == 0)
        {
            Debug.LogError($"Error log {frameCount}");
        }
        else if (frameCount % 300 == 0)
        {
            Debug.LogWarning($"Warning log {frameCount}");
        }
        else if (frameCount % 200 == 0)
        {
            Debug.Log($"Info log {frameCount}");
        }
    }

    public class SupportedTypeFields : IFieldLayout
    {
        public string Title => "Supported Types";

        public readonly IntVariable Int = new("Int", defaultValue: 50, maxValue: 100, serializeKey: "sd.int");
        public readonly FloatVariable Float = new("Float");
        public readonly BoolVariable Bool = new("Bool");
        public readonly TextVariable Text = new("Text");
        public readonly EnumVariable<DeviceType> DeviceType = new("DeviceType");
        public readonly SelectionVariable Selection = new("Selection", () => new[] { "Option1", "Option2", "Option3" });

        public void OnLayout(FieldGroups groups)
        {
            groups.AddGroup("Int", group =>
            {
                group.AddField(Int);
                group.AddSlider(Int, width: 300f);
            });

            groups.AddGroup("Float", group =>
            {
                group.AddField(Float);
                group.AddSlider(Float, width: 300f);
            });

            groups.AddGroup("Bool", group => { group.AddField(Bool); });
            groups.AddGroup("Text", group => { group.AddField(Text); });
            groups.AddGroup("Enum", group => { group.AddField(DeviceType); });
            groups.AddGroup("Selection", group => { group.AddField(Selection); });
            groups.AddGroup("Action", group => { group.AddButton("Button", () => Debug.Log("Button Pressed")); });
        }
    }

    public class CameraFields : IFieldLayout
    {
        public string Title => "Camera";

        public readonly FloatVariable Fog = new("FOV", 60f, 30f, 120f);
        public readonly FloatVariable PosX = new("Position X", 0f, -5f, 5f);
        public readonly FloatVariable PosY = new("Position Y", 5f, 1f, 10f);
        public readonly FloatVariable PosZ = new("Position Z", -10f, -15f, -5f);

        public CameraFields()
        {
            Fog.OnValueChanged += OnFovChanged;
            PosX.OnValueChanged += OnPositionChanged;
            PosY.OnValueChanged += OnPositionChanged;
            PosZ.OnValueChanged += OnPositionChanged;
        }

        public void OnLayout(FieldGroups groups)
        {
            groups.AddGroup("Camera", group =>
            {
                group.AddSlider(Fog);
                group.AddSlider(PosX);
                group.AddSlider(PosY);
                group.AddSlider(PosZ);
            });
        }

        private void OnFovChanged(SerializeVariable<float> variable)
        {
            Camera.main.fieldOfView = variable.Value;
        }

        private void OnPositionChanged(SerializeVariable<float> _)
        {
            var pos = Camera.main.transform.localPosition;
            pos.x = PosX.Value;
            pos.y = PosY.Value;
            pos.z = PosZ.Value;
            Camera.main.transform.localPosition = pos;
        }
    }

    public class ShapeFields : IFieldLayout
    {
        public string Title => "Shape";

        private readonly EnumVariable<ShapeType> _shapeType = new("Shape Type");

        private readonly List<GameObject> _shapes = new();

        public void OnLayout(FieldGroups groups)
        {
            groups.AddGroup("Shape", group =>
            {
                group.AddField(_shapeType);
                group.AddButton("Spawn", Spawn);
                group.AddButton("Clear", () =>
                {
                    _shapes.ForEach(Destroy);
                    _shapes.Clear();
                });
            });
        }

        private void Spawn()
        {
            var prefab = Resources.Load<GameObject>($"Prefabs/{_shapeType.EnumValue}");
            var go = Instantiate(prefab);
            _shapes.Add(go);
            Debug.Log("Spawned " + _shapeType.EnumValue);
        }

        private enum ShapeType
        {
            Sphere,
            Cube,
            Capsule,
            Cylinder,
        }
    }
}