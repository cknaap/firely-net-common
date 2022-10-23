using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Specification;
using System.Collections.Generic;
using System.Linq;

namespace Hl7.Fhir.ElementModel
{
    internal class CachedElement : BaseTypedElementWrapper
    {
        public CachedElement(ITypedElement original) : base(original)
        {
        }

        private bool _nameRead = false;
        private string _name = null;
        public override string Name
        {
            get
            {
                _name = _nameRead ? _name : base.Name;
                _nameRead = true;
                return _name;
            }
        }

        private bool _valueRead = false;
        private object _value = null;
        public override object Value
        {
            get
            {
                _value = _valueRead ? _value : base.Value;
                _valueRead = true;
                return _value;
            }
        }

        private bool _locationRead = false;
        private string _location = null;
        public override string Location
        {
            get
            {
                _location = _locationRead ? _location : base.Location;
                _locationRead = true;
                return _location;
            }
        }

        private bool _instanceTypeRead = false;
        private string _instanceType = null;
        public override string InstanceType
        {
            get
            {
                _instanceType = _instanceTypeRead ? _instanceType : base.InstanceType;
                _instanceTypeRead = true;
                return _instanceType;
            }
        }

        private bool _definitionRead = false;
        private IElementDefinitionSummary _definition = null;
        public override IElementDefinitionSummary Definition
        {
            get
            {
                _definition = _definitionRead ? _definition : base.Definition;
                _definitionRead = true;
                return _definition;
            }
        }

        private bool _childrenRead = false;
        private ILookup<string, ITypedElement> _children;

        public override IEnumerable<ITypedElement> Children(string name = null)
        {
            _children = _childrenRead ? _children : wrapped.Children().ToLookup(child => child.Name, child => child.Cache());
            _childrenRead = true;
            return (name is null) ? _children.SelectMany(group => group) : _children[name];
        }
    }

    public static partial class ITypedElementExtensions
    {
        /// <summary>
        /// Prevent recalculation of the Children upon every access.
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static ITypedElement Cache(this ITypedElement original)
        {
            return original as CachedElement ?? new CachedElement(original);
        }
    }
}
