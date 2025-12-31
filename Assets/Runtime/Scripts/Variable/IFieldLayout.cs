namespace SmartDebugger
{
    public interface IFieldLayout
    {
        string Title { get; }
        void OnLayout(FieldGroups groups);
    }
}