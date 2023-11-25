using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using NLog;
using SimpleClocks.Utils.Extensions;

namespace SimpleClocks.Utils
{
	[Flags]
	public enum CatchKinds
	{
		DispatcherUnhandledException = 0,
		DomainUnhandledException = 1,
		AppDispatcherUnhandledException = 2,
		UnobservedTaskException = 4,
		All = DispatcherUnhandledException | DomainUnhandledException | AppDispatcherUnhandledException | UnobservedTaskException
	}

	public interface IAppCatcher
	{
		CatchKinds CatchKinds { get; set; }
		bool JoinConsole { get; set; }
	}

	internal class AppCatcher: IAppCatcher
	{
		readonly Application _application;
		readonly Action<Exception, string> _errorAction;

		public AppCatcher(Application application, Action<Exception, string> errorAction)
		{
			_application = application;
			_errorAction = errorAction;
			CatchKinds = CatchKinds.All;
			Attach();
		}

		public CatchKinds CatchKinds { get; set; }
		public bool JoinConsole { get; set; } = true;

		void Attach()
		{
			if (HasKind(CatchKinds.DispatcherUnhandledException))
				_application.DispatcherUnhandledException += OnDispatcherUnhandledException;
			if (HasKind(CatchKinds.AppDispatcherUnhandledException))
				_application.Dispatcher.UnhandledException += OnApplicationDispatcherUnhandledException;
			if (HasKind(CatchKinds.DomainUnhandledException))
				AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
			if (HasKind(CatchKinds.UnobservedTaskException))
				TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
		}

		bool HasKind(CatchKinds catchKind)
		{
			return (CatchKinds & catchKind) == catchKind;
		}

		void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			PrepareException(e.Exception, "OnDispatcherUnhandledException");
		}

		void OnApplicationDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			PrepareException(e.Exception, "OnApplicationDispatcherUnhandledException");
		}

		void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			PrepareException((Exception)e.ExceptionObject, "OnAppDomainUnhandledException");
		}

		void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
		{
			PrepareException(e.Exception, "OnUnobservedTaskException");
		}

		void PrepareException(Exception error, string source)
		{
			if (_application.Dispatcher.HasShutdownFinished && error is TaskCanceledException)
				return;
			if (JoinConsole)
				Win32.JoinConsole(false);
			log.Error(source);
			log.Error(error.Format(ErrorFormat.Verbose));
			Console.WriteLine(source);
			Console.WriteLine(error.Format(ErrorFormat.Verbose));
			_errorAction(error, source);
		}

		static readonly Logger log = LogManager.GetCurrentClassLogger();
	}
}
