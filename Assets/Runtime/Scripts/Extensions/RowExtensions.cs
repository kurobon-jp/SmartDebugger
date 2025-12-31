using System;

namespace SmartDebugger
{
    public static class RowExtensions
    {
        public static void AddField(this FieldGroup group, TextVariable variable, float width = 200f)
        {
            group.AddField(variable, new TextVariable.FieldFactory(width));
        }

        public static void AddField(this FieldGroup group, BoolVariable variable, float width = 200f)
        {
            group.AddField(variable, new BoolVariable.ToggleFactory(width));
        }

        public static void AddField(this FieldGroup group, IntVariable variable, float width = 200f)
        {
            group.AddField(variable, new IntVariable.FieldFactory(width));
        }

        public static void AddSlider(this FieldGroup group, IntVariable variable, float width = 200f)
        {
            group.AddField(variable, new IntVariable.SliderFactory(width));
        }

        public static void AddField(this FieldGroup group, FloatVariable variable, float width = 200f)
        {
            group.AddField(variable, new FloatVariable.FieldFactory(width));
        }

        public static void AddSlider(this FieldGroup group, FloatVariable variable, float width = 200f)
        {
            group.AddField(variable, new FloatVariable.SliderFactory(width));
        }

        public static void AddField(this FieldGroup group, SelectionVariable variable, float width = 200f)
        {
            group.AddField(variable, new SelectionVariable.DropdownFactory(width));
        }

        public static void AddButton(this FieldGroup group, ActionVariable variable, float width = 200f)
        {
            group.AddField(variable, new ActionVariable.ButtonFactory(width));
        }

        public static void AddButton(this FieldGroup group, string title, Action action, float width = 200f)
        {
            group.AddField(new ActionVariable(title, action), new ActionVariable.ButtonFactory(width));
        }

        public static void AddLabel(this FieldGroup group, string title, float width = 200f)
        {
            group.AddField(new LabelVariable(title), new LabelVariable.Factory(width));
        }

        public static void LineBreak(this FieldGroup group)
        {
            group.AddField(new LineBreak(), new LineBreak.Factory());
        }

        public static void AddSeparator(this FieldGroup group, string title, float width = 200f)
        {
            group.LineBreak();
            group.AddLabel(title, width);
            group.LineBreak();
        }
    }
}