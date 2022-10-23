using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Specification;
using Hl7.Fhir.Utility;
using System;
using System.Collections.Generic;

namespace Hl7.Fhir.ElementModel
{
    public abstract class BaseTypedElementWrapper : ITypedElement, IAnnotated, IExceptionSource
    {
        protected readonly ITypedElement wrapped;

        public BaseTypedElementWrapper(ITypedElement original)
        {
            wrapped = original;

            if (original is IExceptionSource ies && ies.ExceptionHandler == null)
                ies.ExceptionHandler = (o, a) => exHandler.NotifyOrThrow(o, a); // Only call the exception handler that might have been added to the wrapped element, don't reach out to the original element. Otherwise we'll create a recursion.
        }

        public virtual string Name => wrapped.Name;

        public virtual string InstanceType => wrapped.InstanceType;

        public virtual object Value => wrapped.Value;

        public virtual string Location => wrapped.Location;

        public virtual IElementDefinitionSummary Definition => wrapped.Definition;

        public virtual IEnumerable<ITypedElement> Children(string name = null)
        {
            return wrapped.Children(name);
        }

        #region IExceptionSource implementation
        protected ExceptionNotificationHandler exHandler = null;
        public virtual ExceptionNotificationHandler ExceptionHandler
        {
            get => exHandler ?? (wrapped as IExceptionSource)?.ExceptionHandler;
            set => exHandler = value;
        }

        protected virtual void NotifyOrThrow(ExceptionNotification notification)
        {
            ExceptionHandler.NotifyOrThrow(this, notification);
        }
        #endregion

        #region IAnnoted implementation
        public virtual IEnumerable<object> Annotations(Type type)
        {
            if (type == typeof(ITypedElement))
            {
                return new[] { this }; //Override previous implementations of ITypedElement, since an override of this class probably changed something to one of the properties.
            }
            return (wrapped as IAnnotated)?.Annotations(type) ?? new object[] { };
        }
        #endregion
    }
}
