using Ninject;
using Ninject.Modules;
using Ninject.Parameters;
using SimpleClocks.Models;
using SimpleClocks.Services.Classes;
using SimpleClocks.Services.Intefaces;

namespace SimpleClocks.Services
{
	class ServicesManager
	{
		static readonly IKernel resolver;
		static ServicesManager()
		{
			resolver = new StandardKernel(
				new CommonModule()
			);
		}

		static T Get<T>(params IParameter[] arguments)=>resolver.Get<T>(arguments);

		public static ClockModel ClockModel(AsyncRefreshTimeHandler onRefresh)
			=> Get<ClockModel>(new ConstructorArgument(nameof(onRefresh), onRefresh));

		class CommonModule : NinjectModule
		{
			public override void Load()
			{
				Bind<IColorProfileManager>().To<ColorProfileManager>().InSingletonScope();
				Bind<IDialogsService>().To<DialogsService>().InSingletonScope();
				Bind<ClockModel>().To<ClockModel>();
			}
		}
	}
}
