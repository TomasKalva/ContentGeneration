﻿using Noesis;
using System;

namespace NoesisApp
{
    /// <summary>
    /// Represents the base class for specifying attachable objects.
    /// </summary>
    public abstract class AttachableObject : Animatable, IAttachedObject
    {
        protected AttachableObject(Type associatedType)
        {
            _associatedType = associatedType;
            _associatedObject = IntPtr.Zero;
            _view = IntPtr.Zero;
        }

        protected Type AssociatedType
        {
            get { return _associatedType; }
        }

        protected DependencyObject AssociatedObject
        {
            get { return (DependencyObject)GetProxy(_associatedObject); }
        }

        DependencyObject IAttachedObject.AssociatedObject
        {
            get { return this.AssociatedObject; }
        }

        public void Attach(DependencyObject associatedObject)
        {
            if (AssociatedObject != associatedObject)
            {
                if (AssociatedObject != null)
                {
                    throw new InvalidOperationException(string.Format(
                        "'{0}' already attached to another object '{1}'",
                        GetType(), AssociatedObject.GetType()));
                }

                if (associatedObject == null)
                {
                    throw new InvalidOperationException("Cannot attach to a null object");
                }

                if (!_associatedType.IsAssignableFrom(associatedObject.GetType()))
                {
                    throw new InvalidOperationException(string.Format(
                        "Invalid associated element type '{0}' for '{1}'",
                        associatedObject.GetType(), GetType()));
                }

                _associatedObject = GetPtr(associatedObject);
                _view = GetPtr(View.Find(this));

                BindingOperations.SetBinding(this, AttachmentProperty, new Binding("Visibility")
                {
                    RelativeSource = new RelativeSource { AncestorType = typeof(UIElement) }
                });

                InitObject();

                OnAttached();
            }
        }

        public void Detach()
        {
            if (AssociatedObject != null)
            {
                OnDetaching();

                _associatedObject = IntPtr.Zero;
                _view = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Called after the object is attached to an AssociatedObject
        /// </summary>
        protected virtual void OnAttached()
        {
        }

        /// <summary>
        /// Called just before the object is detached from its AssociatedObject
        /// </summary>
        protected virtual void OnDetaching()
        {
        }

        /// <summary>
        /// Gets the View where this object is connected to
        /// </summary>
        public View View
        {
            get { return (View)GetProxy(_view); }
        }

        // We use a binding to get notified when the associated object is disconnected from the tree
        // and is going to be destroyed, so we can properly detach from it
        private static readonly DependencyProperty AttachmentProperty = DependencyProperty.Register(
            ".Attachment", typeof(Visibility), typeof(AttachableObject),
            new PropertyMetadata(Detached, OnAttachmentChanged));

        private static void OnAttachmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((Visibility)e.NewValue == Detached)
            {
                AttachableObject attachable = (AttachableObject)d;
                attachable.Detach();
            }
        }

        private const Visibility Detached = (Visibility)(-1);
        private Type _associatedType;
        IntPtr _associatedObject;
        IntPtr _view;
    }
}
