﻿using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Model;
using Hl7.Fhir.Specification;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Hl7.Fhir.ElementModel
{
    public static class DynamicExtensions
    {
        public static DynamicObject Dynamic(this ElementNode n, IStructureDefinitionSummaryProvider prov)
            => new DynamicElementNode(n, prov);
    }

    internal class DynamicElementNode : DynamicObject, ITypedElement
    {
        private readonly ElementNode _wrapped;
        private readonly IStructureDefinitionSummaryProvider _prov;

        public string Name => _wrapped.Name;
        public string InstanceType => _wrapped.InstanceType;
        public object Value => _wrapped.Value;
        public string Location => _wrapped.Location;
        public IEnumerable<ITypedElement> Children(string name = null) => _wrapped.Children(name);
        public IElementDefinitionSummary Definition => _wrapped.Definition;

        public DynamicElementNode(ElementNode wrapped, IStructureDefinitionSummaryProvider prov)
        {
            _wrapped = wrapped ?? throw new ArgumentNullException(nameof(wrapped));
            _prov = prov ?? throw new ArgumentNullException(nameof(prov));
        }

        public override IEnumerable<string> GetDynamicMemberNames()
           => base.GetDynamicMemberNames().Union(_wrapped.Children().Select(c => c.Name));

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _wrapped.Remove(binder.Name);
            if (value == null) return true;

            var valueType = value.GetType();
            IEnumerable<object> values;

            if (!IsCollection(valueType))
                values = new object[] { value };
            else
                values = ((IEnumerable)value).OfType<object>();

            foreach (var newChild in values.Select(v => ToElementNode(v, binder.Name)))
                _wrapped.Add(_prov, newChild);

            return true;
        }

        internal static bool IsCollection(Type t) => typeof(ICollection).IsAssignableFrom(t) &&
            t != typeof(string) && t != typeof(byte[]);

        internal ElementNode ToElementNode(object v, string name)
        {
            var result = v switch
            {
                ElementNode en => en,
                ITypedElement ite => ElementNode.FromElement(ite, true),
                Base b => ElementNode.FromElement(b.ToTypedElement(_prov), true),
                _ => ElementNode.FromElement(ElementNode.ForPrimitive(v))
            };

            result.Name = name;
            return result;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = getMemberByName(binder.Name);

            return result != null;
        }

        public override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result)
            => base.TryBinaryOperation(binder, arg, out result);


        internal object getMemberByName(string name)
        {
            var children = _wrapped.Children(name).Cast<ElementNode>().ToList();
            if (!children.Any()) return null;

            if (children[0].Definition?.IsCollection == true || children.Count > 1)
                return children.Select(c => (ITypedElement)c.Dynamic(_prov)).ToReadOnlyCollection();
            else
                return children.Single().Dynamic(_prov);
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (indexes.Length == 1 && indexes[0] is string key)
            {
                result = getMemberByName(key);
                return true; // if not found, just return null
            }

            //else if (indexes.Length == 1 && IsCollection(   indexes[0]) is int ix))
            return base.TryGetIndex(binder, indexes, out result);
        }


        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            // Case 1: convert this dynamic to ITypedElement/ElementNode -> 
            // return the wrapped instance
            if (typeof(ITypedElement).IsAssignableFrom(binder.Type))
            {
                result = _wrapped;
                return true;
            }
            else if (_wrapped.Value != null && binder.Type.IsAssignableFrom(_wrapped.Value.GetType()))
            {
                result = _wrapped.Value;
                return true;
            }
            else if (typeof(Base).IsAssignableFrom(binder.Type))
            {
                result = _wrapped.ToPoco(_prov);
                return true;
            }

            return base.TryConvert(binder, out result);
        }        
    }
}
