using System;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleClocks.Models
{
	public delegate Task AsyncRefreshTimeHandler(Action action, CancellationToken token);
}
