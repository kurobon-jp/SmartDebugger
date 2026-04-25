using System.Collections.Generic;
using SmartDebugger;
using UnityEngine;
using Random = UnityEngine.Random;

public class Demo : MonoBehaviour
{
    private ShapeFields _shapeFields = new ShapeFields();
    private CameraFields _cameraFields = new CameraFields();
    private SupportedTypeFields _supportedTypeFields = new SupportedTypeFields();

    [SerializeField] private TextMesh _textMesh;
    
    void Start()
    {
        Application.targetFrameRate = 60;
        SmartDebug.Instance.AddFieldLayout(_shapeFields);
        SmartDebug.Instance.AddFieldLayout(_cameraFields);
        SmartDebug.Instance.AddFieldLayout(_supportedTypeFields);
        
        _textMesh.text = $"Press {SDSettings.Instance.OpenShortcut}";
    }

    private void Update()
    {
        if (Time.frameCount % 100 == 0)
        {
            _shapeFields.Spawn();
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

        private readonly EnumVariable<ShapeType> _shapeType = new("Shape Type", ShapeType.Cube);

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

        public void Spawn()
        {
            var prefab = Resources.Load<GameObject>($"Prefabs/{_shapeType.EnumValue}");
            var go = Instantiate(prefab, Random.insideUnitSphere + Vector3.up * 10, Random.rotation);
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