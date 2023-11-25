using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Threading;

namespace SimpleClocks.Utils
{

	static class NeverEndingTaskFactory
	{
		public static ITargetBlock<DateTimeOffset> CreateNeverEndingTask(
			Func<DateTimeOffset, Task> action,
			TimeSpan delay,
			CancellationToken cancellationToken
		)
		{
			if (action == null) throw new ArgumentNullException(nameof(action));
			ActionBlock<DateTimeOffset> block = null;
			block = new ActionBlock<DateTimeOffset>(async now => {
				await action(now);
				await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
				block.Post(DateTimeOffset.Now);
			}, new ExecutionDataflowBlockOptions {
				CancellationToken = cancellationToken
			});
			return block;
		}

		static async Task AwaitUi()
		{
			//var @do = Application.Current.Dispatcher.InvokeAsync((Action)(() => { }), DispatcherPriority.ContextIdle);
			//return await Task.Factory.StartNew(async async => await @do, CancellationToken.None);
			await Application.Current.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ContextIdle);
		}
	}
}
