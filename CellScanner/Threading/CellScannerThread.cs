namespace CellScanner.Threading
{
	using System;
	using System.Threading;
	using System.Windows.Threading;

	/// <summary>
	/// CellScanner requires a thread that processes Windows messages.
	/// Without this, some API methods will hang.
	/// </summary>
	public class CellScannerThread
	{
		private readonly Thread _thread;
		private readonly Dispatcher _dispatcher;

		public CellScannerThread()
		{
			Dispatcher dispatcher = null;
			var mre = new ManualResetEvent(false);

			var thread = new Thread(() =>
			{
				dispatcher = Dispatcher.CurrentDispatcher;
				mre.Set();
				Dispatcher.Run();
			});
			thread.Name = "CellScanner Thread";
			thread.Start();

			// wait until the thread runs
			mre.WaitOne();

			_thread = thread;
			_dispatcher = dispatcher;
		}

		public Dispatcher Dispatcher => _dispatcher;

		public void Invoke(Action action)
		{
			_dispatcher.Invoke(action);
		}

		public TResult Invoke<TResult>(Func<TResult> func)
		{
			return _dispatcher.Invoke(func);
		}
	}
}
