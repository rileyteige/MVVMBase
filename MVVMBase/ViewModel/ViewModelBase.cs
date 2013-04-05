using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Threading;
using System.Threading;

namespace MVVMBase.ViewModel
{
	/// <summary>
	/// Base class for all ViewModel classes in the app.
	/// It provides support for property change notifications
	/// and has a DisplayName property. This class is abstract.
	/// </summary>
	public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        #region Fields

        private bool _cancelled;
        private bool _working;
        private static readonly Thread _mainThread = Thread.CurrentThread;

        #endregion // Fields

        #region Constructor

        protected ViewModelBase()
		{
		}

		#endregion // Constructor

        #region Properties

        protected bool Cancelled
        {
            get { return _cancelled; }
            private set
            {
                _cancelled = value;
            }
        }

        public bool Working
        {
            get { return _working; }
            private set
            {
                _working = value;
                OnPropertyChanged("Working");
            }
        }

        #endregion // Properties

        #region Debugging Aides

        /// <summary>
		/// Returns whether an exception is thrown, or if a Debug.Fail() is used
		/// when an invalid property name is passed to the VerifyPropertyName method.
		/// The default value is false, but subclasses used by unit tests might
		/// override this property's getter to return true.
		/// </summary>
		protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

		/// <summary>
		/// Warns the developer if this object does not have
		/// a public property with the specified name. This
		/// method does not exist in a Release build.
		/// </summary>
		/// <param name="propertyName">The property name to check.</param>
		[Conditional("DEBUG")]
		[DebuggerStepThrough]
		public void VerifyPropertyName(string propertyName)
		{
			// Verify that the property name matches a real,
			// public, instance property on this object.
			if (TypeDescriptor.GetProperties(this)[propertyName] == null)
			{
				string msg = "Invalid property name: " + propertyName;
				if (this.ThrowOnInvalidPropertyName)
					throw new Exception(msg);
				else
					Debug.Fail(msg);
			}
		}

		#endregion // Debugging Aides

		#region INotifyPropertyChanged Members

		/// <summary>
		/// Raised when a property on this object has a new value.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Raises this object's PropertyChanged event.
		/// </summary>
		/// <param name="propertyName">The property that has a new value.</param>
		protected virtual void OnPropertyChanged(string propertyName)
		{
			this.VerifyPropertyName(propertyName);

			PropertyChangedEventHandler handler = this.PropertyChanged;
			if (handler != null)
			{
				PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
				handler(this, e);
			}
		}

		#endregion // INotifyPropertyChangedMembers

		#region IDisposable Members

		/// <summary>
		/// Invoked when this object is being removed from the app
		/// and will be subject to garbage collection.
		/// </summary>
		public void Dispose()
		{
			this.OnDispose();
		}

		/// <summary>
		/// Child classes can override this method to perform
		/// clean-up logic, such as removing event handlers.
		/// </summary>
		protected virtual void OnDispose()
		{
		}

		#endregion // IDisposable Members

        #region Async

        /// <summary>
        /// Atomically sets Cancelled = true. Used to signal background
        /// work that it needs to halt. Background code would be responsible
        /// for checking Cancelled status.
        /// </summary>
        protected void Cancel()
        {
            lock (this)
            {
                Cancelled = true;
            }
        }

        /// <summary>
        /// Executes code asynchronously on a background thread.
        /// </summary>
        /// <param name="func">The code to execute asynchronously</param>
        protected void Background(Action func)
        {
            new Thread((ThreadStart)delegate
                {
                    Dispatcher.CurrentDispatcher.Invoke((Action)delegate
                    {
                        Working = true;
                        Cancelled = false;

                        func();

                        Working = false;
                        Cancelled = false;
                    });
                }
            ).Start();
        }

        /// <summary>
        /// Executes code on the main (UI) thread.
        /// </summary>
        /// <param name="func">The code to execute</param>
        protected void UpdateViewModel(Action func)
        {
            Dispatcher.FromThread(_mainThread).Invoke(func);
        }

        #endregion // Async
    }
}
