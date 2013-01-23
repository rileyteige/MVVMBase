using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Diagnostics;

namespace MVVMBase.Command
{
	/// <summary>
	/// A command whose sole purpose is to
	/// relay its functionality to other
	/// objects by invoking delegates. The
	/// default return value for CanExecute()
	/// is true.
	/// </summary>
	public class AppCommand : ICommand
	{
		#region Fields

		Action<object> mExecute;
		Predicate<object> mCanExecute;

		#endregion // Fields

		#region Constructors

		public AppCommand(Action<object> execute)
			: this(execute, null)
		{
		}

		public AppCommand(Action<object> execute, Predicate<object> canExecute)
		{
			if (execute == null)
				throw new ArgumentNullException("execute");

			mExecute = execute;
			mCanExecute = canExecute;
		}

		#endregion // Constructors

		#region ICommand Members

		[DebuggerStepThrough]
		public bool CanExecute(object parameter)
		{
			return mCanExecute == null ? true : mCanExecute(parameter);
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public void Execute(object parameter)
		{
			mExecute(parameter);
		}

		#endregion
	}
}
