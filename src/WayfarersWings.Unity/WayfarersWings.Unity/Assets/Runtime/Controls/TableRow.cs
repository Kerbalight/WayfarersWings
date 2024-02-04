using UnityEngine.UIElements;

namespace WayfarersWings.Unity.WayfarersWings.Unity.Assets.Runtime.Controls
{
    public class TableRow : VisualElement
    {
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _text = new()
            {
                name = "text"
            };

            private readonly UxmlStringAttributeDescription _value = new()
            {
                name = "value"
            };

            // Use the Init method to assign the value of the progress UXML attribute to the C# progress property.
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                ((TableRow)ve).text = _text.GetValueFromBag(bag, cc);
                ((TableRow)ve).value = _value.GetValueFromBag(bag, cc);
                // (ve as RadialProgress).progress = m_ProgressAttribute.GetValueFromBag(bag, cc);
            }
        }

        // Define a factory class to expose this control to UXML.
        public new class UxmlFactory : UxmlFactory<TableRow, UxmlTraits> { }

        private const string USSClassName = "table-row";
        private const string USSLabelClassName = "table-label";
        private const string USSValueClassName = "table-value";

        private string _text;

        public string text
        {
            get => _text;
            set
            {
                _text = value;
                _labelElement.text = "<color=#595DD5>*</color> " + _text;
            }
        }

        public string value
        {
            get => _valueElement.text;
            set => _valueElement.text = value;
        }

        private Label _labelElement;
        private Label _valueElement;


        public TableRow()
        {
            AddToClassList(USSClassName);

            _labelElement = new Label();
            _labelElement.AddToClassList(USSLabelClassName);
            Add(_labelElement);
            _valueElement = new Label();
            _valueElement.AddToClassList(USSValueClassName);
            Add(_valueElement);
        }
    }
}